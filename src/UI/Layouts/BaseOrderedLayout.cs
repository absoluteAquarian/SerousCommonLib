using System.Collections;
using System.Collections.Generic;
using Terraria.UI;

namespace SerousCommonLib.UI.Layouts {
	/// <summary>
	/// The base class for ordered layouts
	/// </summary>
	public abstract class BaseOrderedLayout : UIElement, IEnumerable<UIElement>, IEnumerable, IOrderedLayout {
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
		/// The number of elements in the layout
		/// </summary>
		public int Count => _trackedLayoutElements.Count;

		/// <inheritdoc/>
		public override void Recalculate() {
			// Ensure the layout manager is in a valid state
			if (this.GetLayoutManager(LayoutCreationMode.View) is { IsReadOnly: false } manager)
				manager.Attributes ??= new LayoutAttributes();

			// If any of the tracked elements are no longer children, remove them from the layout
			// Otherwise, ensure that the constraints are correct
			for (int i = _trackedLayoutElements.Count - 1; i >= 0; i--) {
				var element = _trackedLayoutElements[i];
				if (!object.ReferenceEquals(element.Parent, this))
					RemoveElement_Impl(element, i);
				else {
					var childManager = element.GetLayoutManager(LayoutCreationMode.View);
					if (!childManager.IsReadOnly) {
						childManager.Attributes ??= new LayoutAttributes();

						if (i == 0) {
							childManager.Attributes.RemoveConstraint(AlignmentToSibling);
							childManager.Attributes.AddConstraint(AlignmentToParent, null, LayoutUnit.Zero);
						} else {
							childManager.Attributes.AddConstraint(AlignmentToSibling, _trackedLayoutElements[i - 1], Spacing);
							childManager.Attributes.RemoveConstraint(AlignmentToParent);
						}
					}
				}
			}

			base.Recalculate();
		}

		/// <inheritdoc/>
		public LayoutAttributes AddElement(UIElement element) {
			Elements.Add(element);
			element.Parent = this;

			var manager = element.GetLayoutManager();
			manager.Attributes ??= new LayoutAttributes();

			if (_trackedLayoutElements.Count == 0)
				manager.Attributes.AddConstraint(AlignmentToParent, null, LayoutUnit.Zero);
			else
				manager.Attributes.AddConstraint(AlignmentToSibling, _trackedLayoutElements[^1], Spacing);

			_trackedLayoutElements.Add(element);

			return manager.Attributes;
		}

		/// <inheritdoc/>
		public LayoutAttributes InsertElement(int index, UIElement element) {
			if (index < 0 || index > _trackedLayoutElements.Count)
				return null;

			Elements.Insert(index, element);
			element.Parent = this;

			var manager = element.GetLayoutManager();
			manager.Attributes ??= new LayoutAttributes();

			if (index == 0) {
				manager.Attributes.AddConstraint(AlignmentToParent, null, LayoutUnit.Zero);
				if (_trackedLayoutElements.Count > 0) {
					var nextElement = _trackedLayoutElements[0];
					var nextManager = nextElement.GetLayoutManager(LayoutCreationMode.View);
					if (nextManager.AreModificationsAllowed()) {
						nextManager.Attributes.RemoveConstraint(AlignmentToParent);
						nextManager.Attributes.AddConstraint(AlignmentToSibling, element, Spacing);
					}
				}
			} else {
				manager.Attributes.AddConstraint(AlignmentToSibling, _trackedLayoutElements[index - 1], Spacing);
				if (index < _trackedLayoutElements.Count) {
					var nextElement = _trackedLayoutElements[index];
					var nextManager = nextElement.GetLayoutManager(LayoutCreationMode.View);
					if (nextManager.AreModificationsAllowed())
						nextManager.Attributes.ChangeConstraintAnchor(AlignmentToSibling, element);
				}
			}

			_trackedLayoutElements.Insert(index, element);

			return manager.Attributes;
		}

		/// <inheritdoc/>
		public bool RemoveElement(UIElement element) => RemoveElement_Impl(element, _trackedLayoutElements.IndexOf(element));

		private bool RemoveElement_Impl(UIElement element, int index) {
			if (index < 0) {
				// Ignore, the element was not in the layout
				return false;
			}

			Elements.Remove(element);
			element.Parent = null;

			// Adjust the constraints of the adjacent elements
			var manager = element.GetLayoutManager(LayoutCreationMode.View);

			if (manager.AreModificationsAllowed())
				manager.Attributes.ClearConstraints();

			if (index == 0) {
				// Only the second element is affected
				if (_trackedLayoutElements.Count > 1) {
					manager = _trackedLayoutElements[1].GetLayoutManager(LayoutCreationMode.View);
					if (manager.AreModificationsAllowed()) {
						manager.Attributes.RemoveConstraint(AlignmentToSibling);
						manager.Attributes.AddConstraint(AlignmentToParent, null, LayoutUnit.Zero);
					}
				}
			} else {
				// The element to the right needs to change its anchor element
				if (index < _trackedLayoutElements.Count - 1) {
					manager = _trackedLayoutElements[index + 1].GetLayoutManager(LayoutCreationMode.View);
					if (manager.AreModificationsAllowed())
						manager.Attributes.ChangeConstraintAnchor(AlignmentToSibling, _trackedLayoutElements[index - 1]);
				}
			}

			_trackedLayoutElements.RemoveAt(index);
			return true;
		}

		/// <inheritdoc/>
		public void Clear() {
			foreach (UIElement element in Elements) {
				element.Parent = null;

				var manager = element.GetLayoutManager(LayoutCreationMode.View);
				if (manager.AreModificationsAllowed())
					manager.Attributes.ClearConstraints();
			}

			Elements.Clear();
		}

		/// <inheritdoc/>
		public IEnumerator<UIElement> GetEnumerator() => _trackedLayoutElements.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _trackedLayoutElements.GetEnumerator();
	}
}
