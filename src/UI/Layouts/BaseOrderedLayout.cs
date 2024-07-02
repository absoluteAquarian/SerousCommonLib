using System.Collections.Generic;
using Terraria.UI;

namespace SerousCommonLib.UI.Layouts {
	/// <summary>
	/// The base class for ordered layouts
	/// </summary>
	public abstract class BaseOrderedLayout : UIElement, IConstraintLayout, IOrderedLayout {
		/// <inheritdoc/>
		public LayoutManager Manager { get; }
		
		/// <inheritdoc/>
		public abstract LayoutConstraintType AlignmentToParent { get; }

		/// <inheritdoc/>
		public abstract LayoutConstraintType AlignmentToSibling { get; }

		/// <summary>
		/// The spacing between elements in pixels
		/// </summary>
		public LayoutUnit Spacing { get; set; }

		private readonly List<UIElement> _trackedLayoutElements = new();

		/// <summary>
		/// Creates a new instance of the <see cref="BaseOrderedLayout"/> class
		/// </summary>
		public BaseOrderedLayout() {
			Manager = LayoutManager.GetOrCreateManager(this);
			Manager.Attributes = new LayoutAttributes();
		}

		/// <inheritdoc/>
		public override void Recalculate() {
			// Ensure the layout manager is in a valid state
			Manager.Attributes ??= new LayoutAttributes();

			// If any of the tracked elements are no longer children, remove them from the layout
			// Otherwise, ensure that the constraints are correct
			for (int i = _trackedLayoutElements.Count - 1; i >= 0; i--) {
				var element = _trackedLayoutElements[i];
				if (!object.ReferenceEquals(element.Parent, this))
					RemoveElement_Impl(element, i);
				else {
					var manager = LayoutManager.GetManager(element);
					if (!manager.IsReadOnly) {
						manager.Attributes ??= new LayoutAttributes();

						if (i == 0) {
							manager.Attributes.RemoveConstraint(AlignmentToSibling);
							manager.Attributes.AddConstraint(AlignmentToParent, null, LayoutUnit.Zero);
						} else {
							manager.Attributes.AddConstraint(AlignmentToSibling, _trackedLayoutElements[i - 1], Spacing);
							manager.Attributes.RemoveConstraint(AlignmentToParent);
						}
					}
				}
			}

			base.Recalculate();
		}

		/// <summary>
		/// Appends a new element to the layout, aligning it to the right of the previous element or the left of the parent element if no elements have been added yet
		/// </summary>
		/// <returns>The layout attributes for the element</returns>
		public LayoutAttributes AddElement(UIElement element) {
			Elements.Add(element);
			element.Parent = this;

			var manager = LayoutManager.GetOrCreateManager(element);
			manager.Attributes ??= new LayoutAttributes();

			if (_trackedLayoutElements.Count == 0)
				manager.Attributes.AddConstraint(AlignmentToParent, null, LayoutUnit.Zero);
			else
				manager.Attributes.AddConstraint(AlignmentToSibling, _trackedLayoutElements[^1], Spacing);

			_trackedLayoutElements.Add(element);

			return manager.Attributes;
		}

		/// <summary>
		/// Removes an element from the layout and adjusts the constraints of the adjacent elements if necessary
		/// </summary>
		public void RemoveElement(UIElement element) => RemoveElement_Impl(element, _trackedLayoutElements.IndexOf(element));

		private void RemoveElement_Impl(UIElement element, int index) {
			if (index < 0) {
				// Ignore, the element was not in the layout
				return;
			}

			Elements.Remove(element);
			element.Parent = null;

			// Adjust the constraints of the adjacent elements
			var manager = LayoutManager.GetManager(element);

			if (!manager.IsReadOnly)
				manager.Attributes?.ClearConstraints();

			if (index == 0) {
				// Only the second element is affected
				if (_trackedLayoutElements.Count > 1) {
					manager = LayoutManager.GetManager(_trackedLayoutElements[1]);
					if (!manager.IsReadOnly) {
						manager.Attributes.RemoveConstraint(AlignmentToSibling);
						manager.Attributes.AddConstraint(AlignmentToParent, null, LayoutUnit.Zero);
					}
				}
			} else {
				// The element to the right needs to change its anchor element
				if (index < _trackedLayoutElements.Count - 1) {
					manager = LayoutManager.GetManager(_trackedLayoutElements[index + 1]);
					if (!manager.IsReadOnly)
						manager.Attributes.ChangeConstraintAnchor(AlignmentToSibling, _trackedLayoutElements[index - 1]);
				}
			}

			_trackedLayoutElements.RemoveAt(index);
		}
	}
}
