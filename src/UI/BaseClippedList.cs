using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using Terraria;
using Terraria.GameInput;
using Terraria.UI;

namespace SerousCommonLib.UI {
	/// <summary>
	/// The base class for lists of UI elements whose contents are clipped to the bounds of the list
	/// </summary>
	public abstract class BaseClippedList : UIElement, IEnumerable<UIElement>, IEnumerable {
		/// <summary/>
		public delegate bool ElementSearchMethod(UIElement element);

		/// <summary>
		/// The alignment modes of the list
		/// </summary>
		public enum ElementAlignmentMode {
			/// <summary>
			/// Elements are arranged vertically
			/// </summary>
			Vertical,
			/// <summary>
			/// Elements are arranged horizontally
			/// </summary>
			Horizontal
		}

		/// <summary>
		/// The scroll bar responsible for scrolling the list
		/// </summary>
		protected NewUIScrollbar _scrollbar;
		private float _innerListSize;
		/// <summary>
		/// The UI element responsible for containing the elements of the list
		/// </summary>
		protected readonly UIElement _innerList;

		/// <summary>
		/// The spacing between elements in pixels
		/// </summary>
		public virtual float Padding { get; set; }

		/// <summary>
		/// The scroll bar's location
		/// </summary>
		public float ViewPosition {
			get => _scrollbar.ViewPosition;
			set => _scrollbar.ViewPosition = value;
		}

		/// <summary>
		/// Whether children at the end of the list should be rendered at the top of the list
		/// </summary>
		public virtual bool ReversedOrder { get; set; }

		/// <summary>
		/// Whether the elements in the list are arranged vertically or horizontally
		/// </summary>
		public virtual ElementAlignmentMode AlignmentMode => ElementAlignmentMode.Vertical;

		/// <summary/>
		public BaseClippedList() {
			_innerList = CreateInnerList();
			_innerList.SetPadding(0);
			_innerList.MarginTop = _innerList.MarginBottom = _innerList.MarginLeft = _innerList.MarginRight = 0;
			_innerList.OverflowHidden = false;
			_innerList.Width.Set(0f, 1f);
			_innerList.Height.Set(0f, 1f);
			OverflowHidden = true;
			Append(_innerList);
		}

		/// <summary>
		/// Return the UI element responsible for containing the elements of the list
		/// </summary>
		protected abstract UIElement CreateInnerList();

		/// <summary>
		/// How many elements are in the list
		/// </summary>
		protected abstract int GetElementCount();

		/// <summary>
		/// The collection of elements in the list
		/// </summary>
		protected abstract IEnumerable<UIElement> GetElements();

		/// <summary>
		/// Adds an element to the end of the list
		/// </summary>
		/// <param name="element">The element to add</param>
		public abstract void Add(UIElement element);

		/// <summary>
		/// Adds a range of elements to the end of the list
		/// </summary>
		/// <param name="elements">The elements to add</param>
		public abstract void AddRange(IEnumerable<UIElement> elements);

		/// <summary>
		/// Inserts an element at the specified index in the list
		/// </summary>
		/// <param name="index">The index in the list</param>
		/// <param name="element">The element to add</param>
		public abstract void Insert(int index, UIElement element);

		/// <summary>
		/// Removes an element from the list.  If the element is not in the list, nothing happens.
		/// </summary>
		/// <param name="element">The element to remove</param>
		/// <returns>Whether the element was removed</returns>
		public abstract bool Remove(UIElement element);

		/// <summary>
		/// Removes all elements from the list
		/// </summary>
		public abstract void Clear();

		/// <summary>
		/// The total height of the list in pixels
		/// </summary>
		public float GetTotalSize() => _innerListSize;

		/// <summary>
		/// Scrolls the list to the first element that matches the search method
		/// </summary>
		public void Goto(ElementSearchMethod searchMethod) {
			foreach (UIElement element in GetElements()) {
				if (searchMethod(element)) {
					_scrollbar.ViewPosition = AlignmentMode is ElementAlignmentMode.Vertical ? element.Top.Pixels : element.Left.Pixels;
					break;
				}
			}
		}

		/// <summary>
		/// Updates the view size of the scrollbar
		/// </summary>
		protected void UpdateScrollbar() {
			if (_scrollbar != null) {
				float height = GetInnerDimensions().Height;
				_scrollbar.SetView(height, _innerListSize);
			}
		}

		/// <inheritdoc/>
		public override void MouseOver(UIMouseEvent evt) {
			base.MouseOver(evt);
			PlayerInput.LockVanillaMouseScroll("SerousCommonLib/BaseClippedList");
		}

		/// <inheritdoc/>
		public override void ScrollWheel(UIScrollWheelEvent evt) {
			base.ScrollWheel(evt);
			if (_scrollbar != null)
				_scrollbar.ViewPosition -= evt.ScrollWheelValue / _scrollbar.ScrollDividend;
		}

		/// <summary>
		/// Assigns a scrollbar to the list
		/// </summary>
		public void SetScrollbar(NewUIScrollbar scrollbar) {
			if (scrollbar.VerticalScrolling != (AlignmentMode is ElementAlignmentMode.Vertical))
				throw new ArgumentException("Scrollbar must be oriented in the same direction as the list");

			_scrollbar = scrollbar;
			UpdateScrollbar();
		}

		/// <inheritdoc/>
		public override List<SnapPoint> GetSnapPoints() {
			List<SnapPoint> list = new();
			if (GetSnapPoint(out SnapPoint point))
				list.Add(point);

			foreach (UIElement item in GetElements())
				list.AddRange(item.GetSnapPoints());

			return list;
		}

		/// <inheritdoc/>
		public override void Recalculate() {
			base.Recalculate();

			UpdateScrollbar();

			// Figure out the total height of the list
			float min = float.PositiveInfinity;
			float max = float.NegativeInfinity;

			bool vertical = AlignmentMode is ElementAlignmentMode.Vertical;

			foreach (UIElement element in GetElements()) {
				CalculatedStyle outerDimensions = element.GetOuterDimensions();
				min = Math.Min(min, vertical ? outerDimensions.Y : outerDimensions.X);
				max = Math.Max(max, vertical ? outerDimensions.Y + outerDimensions.Height : outerDimensions.X + outerDimensions.Width);
			}

			_innerListSize = max - min;
		}

		/// <inheritdoc/>
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			// Ensure that the clipping behavior is kept
			OverflowHidden = true;

			if (_scrollbar != null) {
				ref StyleDimension dim = ref AlignmentMode is ElementAlignmentMode.Vertical ? ref _innerList.Top : ref _innerList.Left;

				float old = dim.Pixels;
				dim.Set(-ViewPosition, 0f);
				if (dim.Pixels != old)
					Recalculate();
			}

			base.DrawSelf(spriteBatch);
		}

		/// <summary>
		/// Draws the element's children that are within the bounds of the element's parent
		/// </summary>
		protected static void RenderClippedElement(UIElement self, SpriteBatch spriteBatch) {
			var parentDims = self.Parent.GetDimensions();

			Vector2 position = parentDims.Position();
			Vector2 dimensions = new(parentDims.Width, parentDims.Height);

			foreach (UIElement element in self.Elements) {
				var elementDims = element.GetDimensions();

				Vector2 position2 = elementDims.Position();
				Vector2 dimensions2 = new(elementDims.Width, elementDims.Height);

				if (Collision.CheckAABBvAABBCollision(position, dimensions, position2, dimensions2))
					element.Draw(spriteBatch);
			}
		}

		/// <inheritdoc/>
		public IEnumerator<UIElement> GetEnumerator() => GetElements().GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetElements().GetEnumerator();
	}
}
