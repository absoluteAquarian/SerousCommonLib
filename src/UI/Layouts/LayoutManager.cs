using Microsoft.Xna.Framework;
using SerousCommonLib.API;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

#nullable enable
namespace SerousCommonLib.UI.Layouts {
	/// <summary>
	/// An object which handles the position, size and alignment of a UI element.<br/>
	/// Usage of this object overrides the default logic for calculating the dimensions of the UI element and its children.
	/// </summary>
	public class LayoutManager {
		private class Loadable : ILoadable {
			void ILoadable.Load(Mod mod) { }

			void ILoadable.Unload() {
				_managers.Clear();
			}
		}

		private readonly WeakReference<UIElement> _elementReference;
		private CalculatedLayout _layout;

		/// <summary>
		/// The attributes resposible for aligning the element within its parent.  If <see langword="null"/>, the manager will not affect the element's layout.
		/// </summary>
		public LayoutAttributes? Attributes { get; set; }

		/// <summary>
		/// Whether this layout manager is considered read-only.  That is, the layout manager is not allowed to modify the layout of the element.
		/// </summary>
		public bool IsReadOnly => _layout is ReadOnlyCalculatedLayout;

		private LayoutManager(UIElement element) {
			ArgumentNullException.ThrowIfNull(element);

			_elementReference = new WeakReference<UIElement>(element);
		}

		private static readonly ConditionalWeakTable<UIElement, LayoutManager> _managers = [];

		/// <summary>
		/// Gets the layout manager for the specified element.  If the element does not have a layout manager, a read-only manager is created instead.
		/// </summary>
		public static LayoutManager GetManager(UIElement element) {
			if (element is IConstraintLayout { Manager: LayoutManager imanager })
				return imanager;

			return _managers.TryGetValue(element, out LayoutManager? manager) ? manager : CreateWatchingManager(element);
		}

		/// <summary>
		/// Gets the layout manager for the specified element.  If the element does not have a layout manager, a new manager is created.
		/// </summary>
		public static LayoutManager GetOrCreateManager(UIElement element) => _managers.GetValue(element, static e => e is IConstraintLayout { Manager: LayoutManager manager } ? manager : new LayoutManager(e));

		private static LayoutManager CreateWatchingManager(UIElement element) {
			LayoutManager manager = new LayoutManager(element) {
				_layout = new ReadOnlyCalculatedLayout(element)
			};
			return manager;
		}

		internal bool ApplyConstraints() {
			if (!_elementReference.TryGetTarget(out UIElement? element))
				return false;

			Vector2 parentSize;
			if (element.Parent is UIElement parent) {
				var dims = parent.GetInnerDimensions();
				parentSize = new Vector2(dims.Width, dims.Height);
			} else {
				// If the element has no parent, use the screen size
				parentSize = new Vector2(Main.screenWidth, Main.screenHeight);
			}

			ApplyConstraints_Impl(element, parentSize);
			return true;
		}

		private void ApplyConstraints_Impl(UIElement element, Vector2 parentSize) {
			ApplyConstraints_Reset(element);
			ApplyConstraints_Init(element);
			ApplyConstraints_CheckConstraints(element, parentSize);
		}

		private void ApplyConstraints_Reset(UIElement element) {
			_layout = new(element);

			if (IsReadOnly)
				Attributes = null;

			foreach (UIElement child in element.Elements)
				GetManager(child).ApplyConstraints_Reset(child);
		}

		private void ApplyConstraints_Init(UIElement element) {
			if (AreModificationsAllowed()) {
				// Create any links to anchor dimensions
				foreach (LayoutConstraint constraint in Attributes.Constraints) {
					if (!constraint.TryGetAnchor(element, out UIElement? anchor))
						continue;

					GetAnchorLayout(anchor).LinkDimension(_layout, constraint);
				}
			}

			// Initialize the children
			foreach (UIElement child in element.Elements) {
				LayoutManager childManager = GetManager(child);
				childManager.ApplyConstraints_Init(child);
				_layout.AssignChild(childManager._layout);
			}
		}

		[MemberNotNullWhen(true, nameof(Attributes))]
		internal bool AreModificationsAllowed() => Attributes is not null && !IsReadOnly;

		private static CalculatedLayout GetAnchorLayout(UIElement? anchor) => anchor is null ? CalculatedLayout.Screen : GetManager(anchor)._layout;

		private void ApplyConstraints_CheckConstraints(UIElement element, Vector2 parentSize) {
			if (AreModificationsAllowed()) {
				ApplyConstraints_Horizontal(element, parentSize);
				ApplyConstraints_Vertical(element, parentSize);
			}

			// Apply the constraints on the children
			Vector2 selfSize = _layout.GetChildContainerSize();

			foreach (UIElement child in element.Elements) {
				LayoutManager childManager = GetManager(child);
				childManager.ApplyConstraints_CheckConstraints(child, selfSize);
			}
		}

		private void ApplyConstraints_SetVanillaInfo(UIElement element, Vector2 parentSize) {
			if (!AreModificationsAllowed())
				return;

			// Set the vanilla dimensions
			element.Left.Set(_layout.Left, 0);
			element.Top.Set(_layout.Top, 0);
			element.Width.Set(_layout.Width, 0);
			element.Height.Set(_layout.Height, 0);

			if (Attributes.Size is { } size) {
				element.MinWidth = size.minWidth;
				element.MaxWidth = size.maxWidth;
				element.MinHeight = size.minHeight;
				element.MaxHeight = size.maxHeight;
			}

			_layout.ToTerrariaDimensions(element, out element._innerDimensions, out element._dimensions, out element._outerDimensions);

			var gravity = Attributes.Gravity;
			
			if ((gravity.type & LayoutGravityType.CenterHorizontal) != 0)
				element.HAlign = gravity.horizontal.GetValueRaw(parentSize.X) / parentSize.X;
			else
				element.HAlign = 0;
			
			if ((gravity.type & LayoutGravityType.CenterVertical) != 0)
				element.VAlign = gravity.vertical.GetValueRaw(parentSize.Y) / parentSize.Y;
			else
				element.VAlign = 0;
		}

		private void ApplyConstraints_Horizontal(UIElement element, Vector2 parentSize) {
			if (AreModificationsAllowed()) {
				if (Attributes.Size is { } size) {
					if (size.width.IsDynamic())
						ApplyConstraints_HorizontalPass(element, parentSize, size);
					else
						_layout.AssignWidth(size.GetConstrainedWidth(parentSize.X), Attributes.Gravity, parentSize);
				}

				// Apply the constraints on the children that affect the parent's width
				_layout.EvaluateHorizontalConstraints();
			}	
		}

		private void ApplyConstraints_Vertical(UIElement element, Vector2 parentSize) {
			if (AreModificationsAllowed()) {
				if (Attributes.Size is { } size) {
					if (size.height.IsDynamic())
						ApplyConstraints_VerticalPass(element, parentSize, size);
					else
						_layout.AssignHeight(size.GetConstrainedHeight(parentSize.Y), Attributes.Gravity, parentSize);
				}

				// Apply the constraints on the children that affect the parent's height
				_layout.EvaluateVerticalConstraints();
			}
		}

		private void ApplyConstraints_HorizontalPass(UIElement element, Vector2 parentSize, SizeConstraint sizeConstraint) {
			// Apply the constraints on the children that affect the parent's width
			Vector2 selfSize = _layout.GetChildContainerSize();

			foreach (UIElement child in element.Elements) {
				LayoutManager childManager = GetManager(child);
				childManager.ApplyConstraints_Horizontal(child, selfSize);
			}

			// This element's width can now be calculated since all children have been processed
			float leftmost = float.PositiveInfinity;
			float rightmost = float.NegativeInfinity;
			foreach (UIElement child in element.Elements) {
				LayoutManager childManager = GetManager(child);
				leftmost = Math.Min(leftmost, childManager._layout.Left);
				rightmost = Math.Max(rightmost, childManager._layout.Right);
			}

			leftmost = Math.Max(0, leftmost);
			rightmost = Math.Max(leftmost, rightmost);

			_layout.AssignWidth(sizeConstraint.ClampWidth(rightmost - leftmost, parentSize.X), Attributes!.Gravity, parentSize);
		}

		private void ApplyConstraints_VerticalPass(UIElement element, Vector2 parentSize, SizeConstraint sizeConstraint) {
			// Apply the constraints on the children that affect the parent's height
			Vector2 selfSize = _layout.GetChildContainerSize();

			foreach (UIElement child in element.Elements) {
				LayoutManager childManager = GetManager(child);
				childManager.ApplyConstraints_Vertical(child, selfSize);
			}

			// This element's height can now be calculated since all children have been processed
			float topmost = float.PositiveInfinity;
			float bottommost = float.NegativeInfinity;
			foreach (UIElement child in element.Elements) {
				LayoutManager childManager = GetManager(child);
				topmost = Math.Min(topmost, childManager._layout.Top);
				bottommost = Math.Max(bottommost, childManager._layout.Bottom);
			}

			topmost = Math.Max(0, topmost);
			bottommost = Math.Max(topmost, bottommost);

			_layout.AssignHeight(sizeConstraint.ClampHeight(bottommost - topmost, parentSize.Y), Attributes!.Gravity, parentSize);
		}
	}

	internal class ElementManagerLink : Edit {
		public override void LoadEdits() {
			On_UIElement.Recalculate += On_UIElement_Recalculate;
		}

		public override void UnloadEdits() {
			On_UIElement.Recalculate -= On_UIElement_Recalculate;
		}

		private static void On_UIElement_Recalculate(On_UIElement.orig_Recalculate orig, UIElement self) {
			// Layout overrides the default Recalculate method to apply constraints only if the element has a LayoutManager and the manager has attributes to apply
			if (!LayoutManager.GetManager(self).ApplyConstraints())
				orig(self);
		}
	}
}
