using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SerousCommonLib.UI.Layouts;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace SerousCommonLib.UI {
	/// <summary>
	/// A copy of <see cref="UIList"/> that uses a <see cref="NewUIScrollbar"/> instead of a <see cref="UIScrollbar"/>
	/// </summary>
	public class NewUIList : BaseClippedList {
		private class UIInnerList : UIElement {
			public override bool ContainsPoint(Vector2 point) => true;

			protected override void DrawChildren(SpriteBatch spriteBatch) => RenderClippedElement(this, spriteBatch);

			public override Rectangle GetViewCullingArea() => Parent.GetDimensions().ToRectangle();
		}

		/// <summary>
		/// The list of elements in the list
		/// </summary>
		public List<UIElement> _items = new();
		public float ListPadding = 5f;
		/// <summary>
		/// The delegate used to manually sort the elements in the list
		/// </summary>
		public Action<List<UIElement>> ManualSortMethod;

		/// <inheritdoc cref="GetElementCount"/>
		public int Count => GetElementCount();

		/// <summary/>
		[Obsolete($"Use the {nameof(ReversedOrder)} property instead", error: true)]
		public bool DisplayChildrenInReverseOrder;

		[Obsolete]
		private ref bool Obsolete_ReversedOrder() => ref DisplayChildrenInReverseOrder;

		/// <inheritdoc/>
		public override bool ReversedOrder {
			get => base.ReversedOrder || Obsolete_ReversedOrder();
			set {
				if (base.ReversedOrder != value) {
					base.ReversedOrder = value;
					Obsolete_ReversedOrder() = value;
					Recalculate();
				}
			}
		}

		/// <inheritdoc/>
		public override float Padding {
			get => ListPadding;
			set {
				if (ListPadding != value) {
					ListPadding = value;
					Recalculate();
				}
			}
		}

		/// <inheritdoc/>
		protected override UIElement CreateInnerList() => new UIInnerList();

		/// <inheritdoc/>
		public override void Add(UIElement item) {
			_items.Add(item);
			_innerList.Append(item);
			UpdateOrder();
			_innerList.Recalculate();
		}

		/// <inheritdoc/>
		public override void AddRange(IEnumerable<UIElement> items) {
			foreach (var item in items) {
				//TML bug fix:  duplicate enumerations resulting in separate object instances in "_items" and "_innerList.Children"
				_items.Add(item);
				_innerList.Append(item);
			}

			UpdateOrder();
			_innerList.Recalculate();
		}

		/// <inheritdoc/>
		public override void Clear() {
			_innerList.RemoveAllChildren();
			_items.Clear();
		}

		/// <inheritdoc/>
		protected override IEnumerable<UIElement> GetElements() => _items;

		/// <inheritdoc/>
		protected override int GetElementCount() => _items.Count;

		/// <inheritdoc/>
		public override void Insert(int index, UIElement element) => throw new NotSupportedException("Inserting elements is not supported in NewUIList");

		/// <inheritdoc/>
		public override bool Remove(UIElement item) {
			_innerList.RemoveChild(item);
			UpdateOrder();
			return _items.Remove(item);
		}

		/// <summary>
		/// Sorts the elements in the list
		/// </summary>
		public void UpdateOrder() {
			if (ManualSortMethod != null)
				ManualSortMethod(_items);
			else
				_items.Sort(static (e, e2) => e.CompareTo(e2));

			UpdateScrollbar();
		}

		/// <inheritdoc/>
		public override void Recalculate() {
		//	DestroyChildManagers();
			base.Recalculate();
		}

		[Obsolete]
		private void DestroyChildManagers() {
			// This element's children should never have alignment constraints
			foreach (var item in _items) {
				if (item.GetLayoutManager(LayoutCreationMode.View) is { IsReadOnly: false } manager)
					manager.Destroy();
			}
		}

		/// <inheritdoc/>
		public override void RecalculateChildren() {
			base.RecalculateChildren();

			bool vertical = AlignmentMode is ElementAlignmentMode.Vertical;

			if (ReversedOrder) {
				float num = 0;
				foreach (UIElement item in _items.Reverse<UIElement>()) {
					if (vertical) {
						item.Top.Set(num, 0f);
						item.Recalculate();
						num += item.GetOuterDimensions().Height + ListPadding;
					} else {
						item.Left.Set(num, 0f);
						item.Recalculate();
						num += item.GetOuterDimensions().Width + ListPadding;
					}
				}
			} else {
				float num2 = 0;
				foreach (UIElement item in _items) {
					if (vertical) {
						item.Top.Set(num2, 0f);
						item.Recalculate();
						num2 += item.GetOuterDimensions().Height + ListPadding;
					} else {
						item.Left.Set(num2, 0f);
						item.Recalculate();
						num2 += item.GetOuterDimensions().Width + ListPadding;
					}
				}
			}
		}
	}
}
