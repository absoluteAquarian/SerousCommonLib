using System.Collections.Generic;
using System.Linq;
using Terraria.UI;

namespace SerousCommonLib.UI.Layouts {
	/// <summary>
	/// A variant of <see cref="NewUIList"/> that incorporates a <see cref="BaseOrderedLayout"/> to organize its elements
	/// </summary>
	public abstract class BaseOrderedLayoutList : BaseClippedList {
		internal BaseOrderedLayout _innerLayout;

		/// <inheritdoc/>
		public override bool ReversedOrder {
			get => base.ReversedOrder;
			set {
				if (base.ReversedOrder != value) {
					base.ReversedOrder = value;
					RecalculateConstraints();
				}
			}
		}

		/// <summary/>
		public BaseOrderedLayoutList() {
			_innerLayout = (BaseOrderedLayout)_innerList;
		}

		/// <inheritdoc/>
		protected sealed override UIElement CreateInnerList() => CreateInnerLayout();

		/// <inheritdoc cref="CreateInnerList"/>
		protected abstract BaseOrderedLayout CreateInnerLayout();

		/// <inheritdoc/>
		protected override int GetElementCount() => _innerLayout.Count;

		/// <inheritdoc/>
		protected override IEnumerable<UIElement> GetElements() => _innerLayout;

		/// <inheritdoc/>
		public override void Add(UIElement element) {
			if (ReversedOrder)
				_innerLayout.InsertElement(0, element);
			else
				_innerLayout.AddElement(element);
		}

		/// <inheritdoc/>
		public override void AddRange(IEnumerable<UIElement> elements) {
			foreach (var element in elements)
				Add(element);
		}

		/// <inheritdoc/>
		public override void Insert(int index, UIElement element) {
			if (ReversedOrder)
				_innerLayout.InsertElement(_innerLayout.Count - index, element);
			else
				_innerLayout.InsertElement(index, element);
		}

		/// <inheritdoc/>
		public override bool Remove(UIElement element) => _innerLayout.RemoveElement(element);

		/// <inheritdoc/>
		public override void Clear() => _innerLayout.Clear();

		private void RecalculateConstraints() {
			List<UIElement> elements = _innerLayout.Elements.ToList();

			_innerLayout.Clear();

			if (ReversedOrder) {
				foreach (var element in elements.Reverse<UIElement>())
					_innerLayout.AddElement(element);
			} else {
				foreach (var element in elements)
					_innerLayout.AddElement(element);
			}
		}
	}
}
