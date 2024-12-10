using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameInput;
using Terraria.UI;

#nullable enable
namespace SerousCommonLib.UI.Layouts {
	internal class ReadOnlyCalculatedLayout : CalculatedLayout {
		public override LayoutDimension Left {
			get {
				if (source?.TryGetTarget(out UIElement? element) is not true)
					return new LayoutDimension(0);

				CalculatedStyle parentDims = element.Parent?._innerDimensions ?? UserInterface.ActiveInstance.GetDimensions();
				CalculatedStyle selfDims = element._outerDimensions;
				return new LayoutDimension(selfDims.X - parentDims.X);
			}
			set { }
		}

		public override LayoutDimension Top {
			get {
				if (source?.TryGetTarget(out UIElement? element) is not true)
					return new LayoutDimension(0);

				CalculatedStyle parentDims = element.Parent?._innerDimensions ?? UserInterface.ActiveInstance.GetDimensions();
				CalculatedStyle selfDims = element._outerDimensions;
				return new LayoutDimension(selfDims.Y - parentDims.Y);
			}
			set { }
		}

		public override LayoutDimension Right {
			get {
				if (source?.TryGetTarget(out UIElement? element) is not true)
					return new LayoutDimension(PlayerInput.OriginalScreenSize.X / Main.UIScale);

				CalculatedStyle parentDims = element.Parent?._innerDimensions ?? UserInterface.ActiveInstance.GetDimensions();
				CalculatedStyle selfDims = element._outerDimensions;
				return new LayoutDimension(selfDims.X + selfDims.Width - parentDims.X);
			}
			set { }
		}

		public override LayoutDimension Bottom {
			get {
				if (source?.TryGetTarget(out UIElement? element) is not true)
					return new LayoutDimension(PlayerInput.OriginalScreenSize.Y / Main.UIScale);

				CalculatedStyle parentDims = element.Parent?._innerDimensions ?? UserInterface.ActiveInstance.GetDimensions();
				CalculatedStyle selfDims = element._outerDimensions;
				return new LayoutDimension(selfDims.Y + selfDims.Height - parentDims.Y);
			}
			set { }
		}

		public ReadOnlyCalculatedLayout(UIElement? source) : base(source, null) { }

		public override void ToTerrariaDimensions(UIElement element, out CalculatedStyle innerDims, out CalculatedStyle dims, out CalculatedStyle outerDims) {
			if (source?.TryGetTarget(out UIElement? sourceElement) is not true) {
				Vector2 screenSize = PlayerInput.OriginalScreenSize;
				CalculatedStyle screenDims = new(0, 0, screenSize.X / Main.UIScale, screenSize.Y / Main.UIScale);
				outerDims = dims = innerDims = screenDims;
				return;
			}

			innerDims = sourceElement._innerDimensions;
			dims = sourceElement._dimensions;
			outerDims = sourceElement._outerDimensions;
		}
	}
}
