using Terraria.UI;

namespace SerousCommonLib.UI.Layouts {
	/// <summary>
	/// An interface representing an object that manages organizing UI elements within a parent element with specific alignment rules
	/// </summary>
	public interface IOrderedLayout {
		/// <summary>
		/// The alignment of the first element with respect to the parent element
		/// </summary>
		LayoutConstraintType AlignmentToParent { get; }

		/// <summary>
		/// The alignment of the elements after the first element with respect to the previously placed element
		/// </summary>
		LayoutConstraintType AlignmentToSibling { get; }

		/// <summary>
		/// The spacing between elements in the layout
		/// </summary>
		LayoutUnit Spacing { get; }

		/// <summary>
		/// Appends a new element to the layout, aligning it to the previous element or the parent element if no elements have been added yet
		/// </summary>
		/// <returns>The layout attributes for the element</returns>
		LayoutAttributes AddElement(UIElement element);

		/// <summary>
		/// Inserts an element into the layout at the specified index, adjusting the constraints of the adjacent elements if necessary
		/// </summary>
		/// <param name="index">The index to insert the element at</param>
		/// <param name="element">The element to insert</param>
		/// <returns>The layout attributes for the element</returns>
		LayoutAttributes InsertElement(int index, UIElement element);

		/// <summary>
		/// Removes an element from the layout and adjusts the constraints of the adjacent elements if necessary
		/// </summary>
		/// <returns>Whether the element was removed</returns>
		bool RemoveElement(UIElement element);

		/// <summary>
		/// Removes all elements from the layout
		/// </summary>
		void Clear();
	}
}
