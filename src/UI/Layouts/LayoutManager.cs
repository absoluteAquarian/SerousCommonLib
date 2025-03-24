using Microsoft.Xna.Framework;
using SerousCommonLib.API;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

#nullable enable
namespace SerousCommonLib.UI.Layouts {
	/// <summary>
	/// An object which handles the position, size and alignment of a UI element.<br/>
	/// Usage of this object overrides the default logic for calculating the dimensions of the UI element and its children.
	/// </summary>
	[Obsolete("This class is part of a new API that is not yet complete.", error: true)]
	public class LayoutManager {
		private class Loadable : ILoadable {
			void ILoadable.Load(Mod mod) { }

			void ILoadable.Unload() {
				_managers.Clear();
			}
		}

		private readonly WeakReference<UIElement> _elementReference;
		private CalculatedLayout _layout;
		private CalculatedLayout _screenRoot;
		private int _constraintStack;

		/// <summary>
		/// The attributes resposible for aligning the element within its parent.  If <see langword="null"/>, the manager will not affect the element's layout.
		/// </summary>
		public LayoutAttributes? Attributes { get; set; }

		/// <summary>
		/// Whether this layout manager is considered read-only.  That is, the layout manager is not allowed to modify the layout of the element.
		/// </summary>
		public bool IsReadOnly => _layout is ReadOnlyCalculatedLayout;

#if DEBUG
		/// <summary>
		/// Whether the layout manager should break when the element is recalculated.  This is useful for debugging layout issues.<br/>
		/// This property is only available in debug builds of <i>absoluteAquarian Utilities</i>.
		/// </summary>
		public bool BreakOnRecalculate { get; set; }
#endif

		private LayoutManager(UIElement element) {
			ArgumentNullException.ThrowIfNull(element);

			_elementReference = new WeakReference<UIElement>(element);

			_layout = null!;
			_screenRoot = null!;
		}

		internal void Lock() {
			if (!_elementReference.TryGetTarget(out _))
				return;

			_constraintStack++;
		}

		internal void Unlock() {
			if (!_elementReference.TryGetTarget(out _))
				return;

			if (_constraintStack == 0)
				throw new InvalidOperationException("Constraints are already unlocked");

			_constraintStack--;
		}

		private static readonly ConditionalWeakTable<UIElement, LayoutManager> _managers = [];

		/// <summary>
		/// Gets the layout manager for the specified element.  If the element does not have a layout manager, a read-only manager is created instead.
		/// </summary>
		public static LayoutManager GetManager(UIElement element) => _managers.TryGetValue(element, out LayoutManager? manager) ? manager : CreateWatchingManager(element);

		/// <summary>
		/// Gets the layout manager for the specified element.  If the element does not have a layout manager, a new manager is created.
		/// </summary>
		public static LayoutManager GetOrCreateManager(UIElement element) => _managers.GetValue(element, static e => new LayoutManager(e));

		private static LayoutManager CreateWatchingManager(UIElement element) {
			// Note: There's no point in recursing to the parent's parent since that would be extra work for use case, so just set it to null
			CalculatedLayout parentLayout = element.Parent is UIElement parent ? new ReadOnlyCalculatedLayout(parent, null!) : CalculatedLayout.GetScreenLayout();

			return new LayoutManager(element) {
				_layout = new ReadOnlyCalculatedLayout(element, parentLayout)
			};
		}

		/// <summary>
		/// Destroys this layout manager and forces the eleemnt to use a read-only layout manager until a new manager is created.
		/// </summary>
		public void Destroy() {
			if (_elementReference.TryGetTarget(out UIElement? element))
				_managers.Remove(element);
		}

		internal bool ApplyConstraints() {
			if (_constraintStack > 0)
				return true;  // Layout was already calculated

			if (!_elementReference.TryGetTarget(out UIElement? element))
				return false;

			#if DEBUG
			if (BreakOnRecalculate)
				System.Diagnostics.Debugger.Break();
			#endif

			ApplyConstraints_Impl(element, GetParentSize(element));
			return true;
		}

		private static Vector2 GetParentSize(UIElement element) {
			if (element.Parent is UIElement parent) {
				var dims = parent._innerDimensions;
				return new Vector2(dims.Width, dims.Height);
			}

			// If the element has no parent, use the screen size
			return new Vector2(Main.screenWidth, Main.screenHeight);
		}

		private void ApplyConstraints_Impl(UIElement element, Vector2 parentSize) {
			_screenRoot = CalculatedLayout.GetScreenLayout();

			// Note: There's no point in recursing to the parent's parent since that would be extra work for use case, so just set it to null
			var parentLayout = element.Parent is UIElement parent ? new ReadOnlyCalculatedLayout(parent, null!) : _screenRoot;

			ApplyConstraints_Reset(element, parentLayout);
			ApplyConstraints_Init(element);
			ApplyConstraints_MoveElementWithinParent();
			ApplyConstraints_CheckConstraints(element, parentSize);
		}

		internal void MirrorToVanilla() {
			if (!_elementReference.TryGetTarget(out UIElement? element))
				return;

			// Vanilla info must be delayed to here in case they're set in Recalculate/RecalculateChildren
			ApplyConstraints_SetVanillaInfo(element, GetParentSize(element));

			foreach (UIElement child in element.Elements)
				child.GetLayoutManager(LayoutCreationMode.View).MirrorToVanilla();

			if (_constraintStack > 0)
				Unlock();
		}

		private void ApplyConstraints_Reset(UIElement element, CalculatedLayout parentLayout) {
			if (_constraintStack > 0)
				return;

			if (!IsReadOnly) {
				// Ensure that the layout has the updated attributes
				_layout = new CalculatedLayout(element, Attributes, parentLayout);
				Attributes?.CheckInheritance();
			} else {
				// Ensure that no leftover data from the previous calculation bleeds over
				_layout = new ReadOnlyCalculatedLayout(element, parentLayout);
				// ReadOnlyCalculatedLayout doesn't use Attributes, so force it to null to be sure
				Attributes = null;
			}

			foreach (UIElement child in element.Elements) {
				var manager = child.GetLayoutManager(LayoutCreationMode.View);
				// Child elements will share the root of the topmost element being processed
				manager._screenRoot = _screenRoot;
				manager.ApplyConstraints_Reset(child, _layout);
			}
		}

		private void ApplyConstraints_Init(UIElement element) {
			if (_constraintStack > 0)
				return;

			if (AreModificationsAllowed()) {
				// Create any links to anchor dimensions
				foreach (LayoutConstraint constraint in Attributes.Constraints) {
					// Null/invalid anchor implies that the element is anchored to its parent / the screen rather than a specific element
					// If the anchor was set to the parent directly, we need to use the precalculated layout from Reset rather than a
					//   read-only view (in the case that the parent doesn't have a layout)
					CalculatedLayout layout = constraint.anchor?.TryGetTarget(out UIElement? anchor) is true && !object.ReferenceEquals(anchor, element.Parent)
						? anchor.GetLayoutManager(LayoutCreationMode.View)._layout
						: _layout.Parent;

					layout.LinkDimension(_layout, constraint);
				}
			}

			// Initialize the children
			foreach (UIElement child in element.Elements) {
				LayoutManager childManager = child.GetLayoutManager(LayoutCreationMode.View);
				childManager.ApplyConstraints_Init(child);
				_layout.AssignChild(childManager._layout);
			}
		}

		[MemberNotNullWhen(true, nameof(Attributes))]
		internal bool AreModificationsAllowed() => Attributes is not null && !IsReadOnly;

		private void ApplyConstraints_MoveElementWithinParent() {
			if (_constraintStack > 0)
				return;

			if (AreModificationsAllowed()) {
				// The parent only has the constraints related to the first element -- the one that called Recalculate() -- at this point
				CalculatedLayout parent = _layout.Parent;

				parent.EvaluateHorizontalConstraints();
				parent.EvaluateVerticalConstraints();
			}
		}

		private void ApplyConstraints_CheckConstraints(UIElement element, Vector2 parentSize) {
			if (_constraintStack > 0)
				return;

			if (AreModificationsAllowed()) {
				ApplyConstraints_Horizontal(element, parentSize);
				ApplyConstraints_Vertical(element, parentSize);
			}

			// Apply the constraints on the children
			Vector2 selfSize = _layout.GetChildContainerSize();

			foreach (UIElement child in element.Elements) {
				LayoutManager childManager = child.GetLayoutManager(LayoutCreationMode.View);
				childManager.ApplyConstraints_CheckConstraints(child, selfSize);
			}

			// Prevent repeated calculations
			if (AreModificationsAllowed())
				Lock();
		}

		private void ApplyConstraints_SetVanillaInfo(UIElement element, Vector2 parentSize) {
			if (_constraintStack > 1)
				return;

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
			
			if ((gravity.type & LayoutGravityType.CenterHorizontal) != 0 && parentSize.X > 0)
				element.HAlign = gravity.horizontal.GetValueRaw(parentSize.X) / parentSize.X;
			else
				element.HAlign = 0;
			
			if ((gravity.type & LayoutGravityType.CenterVertical) != 0 && parentSize.Y > 0)
				element.VAlign = gravity.vertical.GetValueRaw(parentSize.Y) / parentSize.Y;
			else
				element.VAlign = 0;

			Vector2 selfSize = _layout.GetChildContainerSize();

			foreach (UIElement child in element.Elements)
				child.GetLayoutManager(LayoutCreationMode.View).ApplyConstraints_SetVanillaInfo(child, selfSize);
		}

		private void ApplyConstraints_Horizontal(UIElement element, Vector2 parentSize) {
			if (AreModificationsAllowed()) {
				if (Attributes.Size is { } size) {
					if (size.width.IsDynamic())
						ApplyConstraints_HorizontalPass(element, parentSize, size);
					else
						_layout.AssignWidth(size.GetConstrainedWidth(parentSize.X), Attributes.Gravity, parentSize);
				}

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

				_layout.EvaluateVerticalConstraints();
			}
		}

		private void ApplyConstraints_HorizontalPass(UIElement element, Vector2 parentSize, SizeConstraint sizeConstraint) {
			// If no children exist, then the dynamic size should default to zero
			// Otherwise, the clamping ends up trying to do "Infinity - Infinity", which is "NaN"
			if (element.Elements.Count == 0) {
				ReportDynamicSizeWithNoChildren(element);
				_layout.AssignWidth(0, Attributes!.Gravity, parentSize);
				return;
			}

			// Apply the constraints on the children that affect the parent's width
			Vector2 selfSize = _layout.GetChildContainerSize();

			foreach (UIElement child in element.Elements) {
				LayoutManager childManager = child.GetLayoutManager(LayoutCreationMode.View);
				childManager.ApplyConstraints_Horizontal(child, selfSize);
			}

			// This element's width can now be calculated since all children have been processed
			float leftmost = float.PositiveInfinity;
			float rightmost = float.NegativeInfinity;
			foreach (UIElement child in element.Elements) {
				LayoutManager childManager = child.GetLayoutManager(LayoutCreationMode.View);
				leftmost = Math.Min(leftmost, childManager._layout.Left);
				rightmost = Math.Max(rightmost, childManager._layout.Right);
			}

			leftmost = Math.Max(0, leftmost);
			rightmost = Math.Max(leftmost, rightmost);

			_layout.AssignWidth(sizeConstraint.ClampWidth(rightmost - leftmost, parentSize.X), Attributes!.Gravity, parentSize);
		}

		private void ApplyConstraints_VerticalPass(UIElement element, Vector2 parentSize, SizeConstraint sizeConstraint) {
			// If no children exist, then the dynamic size should default to zero
			// Otherwise, the clamping ends up trying to do "Infinity - Infinity", which is "NaN"
			if (element.Elements.Count == 0) {
				ReportDynamicSizeWithNoChildren(element);
				_layout.AssignHeight(0, Attributes!.Gravity, parentSize);
				return;
			}

			// Apply the constraints on the children that affect the parent's height
			Vector2 selfSize = _layout.GetChildContainerSize();

			foreach (UIElement child in element.Elements) {
				LayoutManager childManager = child.GetLayoutManager(LayoutCreationMode.View);
				childManager.ApplyConstraints_Vertical(child, selfSize);
			}

			// This element's height can now be calculated since all children have been processed
			float topmost = float.PositiveInfinity;
			float bottommost = float.NegativeInfinity;
			foreach (UIElement child in element.Elements) {
				LayoutManager childManager = child.GetLayoutManager(LayoutCreationMode.View);
				topmost = Math.Min(topmost, childManager._layout.Top);
				bottommost = Math.Max(bottommost, childManager._layout.Bottom);
			}

			topmost = Math.Max(0, topmost);
			bottommost = Math.Max(topmost, bottommost);

			_layout.AssignHeight(sizeConstraint.ClampHeight(bottommost - topmost, parentSize.Y), Attributes!.Gravity, parentSize);
		}

		private static void ReportDynamicSizeWithNoChildren(UIElement element) {
			StringBuilder sb = new StringBuilder()
				.AppendLine("Attempted to apply a dynamic size to a UI element with no children, defaulting to zero.")
				.AppendLine("Element hierarchy:")
				.AppendJoin("\n  ", GetHierarchyFromTopmostParent(element));

			SerousUtilities.Instance.Logger.Warn(sb.ToString());
		}

		private static IEnumerable<string> GetHierarchyFromTopmostParent(UIElement element) {
			UIElement current = element;
			Stack<UIElement> elements = new Stack<UIElement>();

			while (current is not null) {
				elements.Push(current);
				current = current.Parent;
			}

			while (elements.TryPop(out UIElement? popped))
				yield return popped.GetType().FullName!;
		}
	}

	internal class ElementManagerLink : Edit {
		public override void LoadEdits() {
			// API isn't ready for deployment yet
		//	On_UIElement.Recalculate += On_UIElement_Recalculate;
		}

		public override void UnloadEdits() {
		//	On_UIElement.Recalculate -= On_UIElement_Recalculate;
		}

		[Obsolete]
		private static void On_UIElement_Recalculate(On_UIElement.orig_Recalculate orig, UIElement self) {
			// Layout overrides the default Recalculate method to apply constraints only if the element has a LayoutManager and the manager has attributes to apply
			var manager = self.GetLayoutManager(LayoutCreationMode.View);
			if (!manager.IsReadOnly && manager.ApplyConstraints()) {
				self.RecalculateChildren();  // RecalculateChildren has special effects in Magic Storage, so this call is still necessary even though it's useless
				manager.MirrorToVanilla();
			} else
				orig(self);
		}
	}
}
