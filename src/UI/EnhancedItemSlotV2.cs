using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Chat;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.UI;
using Terraria.UI.Chat;
using Microsoft.Xna.Framework;

namespace SerousCommonLib.UI {
	/// <summary>
	/// A delegate representing a function that can be used to determine whether an item can be inserted into an <see cref="EnhancedItemSlotV2"/>
	/// </summary>
	/// <param name="slot">The item slot object</param>
	/// <param name="item">The item being inserted into <paramref name="slot"/></param>
	/// <returns>Whether <paramref name="item"/> can be placed in <paramref name="slot"/></returns>
	public delegate bool CanAcceptItemDelegate(EnhancedItemSlotV2 slot, Item item);

	/// <summary>
	/// A delegate representing a function that is called whenever the item in an <see cref="EnhancedItemSlotV2"/> is updated
	/// </summary>
	/// <param name="slot">The item slot object</param>
	/// <param name="oldItem">The item that used to be in <paramref name="slot"/></param>
	/// <param name="newItem">The item currently in <paramref name="slot"/></param>
	public delegate void StoredItemUpdatedDelegate(EnhancedItemSlotV2 slot, Item oldItem, Item newItem);

	/// <summary>
	/// An enhanced version of <see cref="ItemSlot"/> containing various functions used when inserting items, removing items, etc.
	/// </summary>
	public class EnhancedItemSlotV2 : UIElement {
		/// <summary>
		/// The <see cref="ItemSlot.Context"/> to draw this item slot with, which affects how the slot background is rendered
		/// </summary>
		public int Context { get; set; }

		/// <summary>
		/// The scale to draw this item slot at
		/// </summary>
		public float Scale { get; set; }

		/// <summary>
		/// The public property used to retrieve the item in this item slot.
		/// By default, this property simply retrieves the <see cref="Item"/> instance bound to this item slot
		/// </summary>
		public virtual Item StoredItem => storedItem;

		/// <summary>
		/// The <see cref="Item"/> instance bound to this item slot
		/// </summary>
		private Item storedItem = new Item();

		/// <summary>
		/// A function indicating whether the item on the player's mouse can be inserted into this item slot or can be swapped with this item slot's bound item
		/// </summary>
		public CanAcceptItemDelegate IsItemAllowed;

		/// <summary>
		/// A function that is executed when the item in this item slot is updated
		/// </summary>
		public StoredItemUpdatedDelegate OnStoredItemUpdated;

		/// <summary>
		/// Whether this item slot should ignore left and right click actions.  Defaults to <see langword="false"/>
		/// </summary>
		public bool IgnoreClicks { get; set; }

		/// <summary>
		/// Whether this item slot should not run its item handling logic the next time it is attempted to be executed.  Defaults to <see langword="false"/>
		/// </summary>
		public bool IgnoreNextHandleAction { get; set; }

		/// <summary>
		/// Whether holding the favorite key will favorite the item (<see langword="false"/>) or share it to the chat (<see langword="true"/>) when the favorite key is held. Defaults to <see langword="false"/>.
		/// </summary>
		public bool CanShareItemToChat { get; set; }

		/// <summary>
		/// An integer ID that can be used to identify this item slot
		/// </summary>
		public readonly int id;

		private readonly Item[] _dummyItemCollection = new Item[11];

		/// <summary>
		/// Creates a new <see cref="EnhancedItemSlotV2"/> instance
		/// </summary>
		/// <param name="id">The identifier</param>
		/// <param name="context">The <see cref="ItemSlot.Context"/>.  Defaults to <see cref="ItemSlot.Context.InventoryItem"/></param>
		/// <param name="scale">The draw scale</param>
		public EnhancedItemSlotV2(int id, int context = ItemSlot.Context.InventoryItem, float scale = 1f) {
			this.id = id;
			Context = context;
			Scale = scale;

			Width.Set(TextureAssets.InventoryBack9.Value.Width * scale, 0f);
			Height.Set(TextureAssets.InventoryBack9.Value.Height * scale, 0f);
		}

		/// <inheritdoc/>
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			float oldScale = Main.inventoryScale;
			Main.inventoryScale = Scale;

			if (!IgnoreNextHandleAction)
				Handle();

			IgnoreNextHandleAction = false;

			Render(spriteBatch);

			Main.inventoryScale = oldScale;
		}

		private void Handle() {
			if (!PlayerInput.IgnoreMouseInterface && IsMouseHovering)
				Handle_MouseInteraction();
		}

		private void Handle_MouseInteraction() {
			Main.LocalPlayer.mouseInterface = true;

			Hovering();

			if (IgnoreClicks) {
				using (MouseCapture.CaptureAndReset())
					Handle_ItemInteractionWeak();
			} else
				Handle_ItemInteraction();
		}

		private void Handle_ItemInteraction() {
			Item item = StoredItem;
			Item old = item.Clone();
			_dummyItemCollection[10] = item;
			// Handle handles all the click and hover actions based on the context
			ItemSlot.Handle(_dummyItemCollection, Context, 10);
			storedItem = _dummyItemCollection[10];

			if (storedItem.IsNotSameTypePrefixAndStack(old))
				OnStoredItemUpdated?.Invoke(this, old, storedItem);

			Handle_ShareInteraction();
		}

		private void Handle_ShareInteraction() {
			Item item = StoredItem;

			if (CanShareItemToChat && Main.keyState.IsKeyDown(Main.FavoriteKey) && !item.IsAir && Main.drawingPlayerChat) {
				Main.cursorOverride = CursorOverrideID.Magnifiers;

				// Handle sharing the item to chat when clicking
				if (Main.mouseLeft && Main.mouseLeftRelease) {
					// Copied from Terraria.UI.ItemSlot
					if (ChatManager.AddChatText(FontAssets.MouseText.Value, ItemTagHandler.GenerateTag(item), Vector2.One))
						SoundEngine.PlaySound(SoundID.MenuTick);
				}
			} else if (Main.cursorOverride == CursorOverrideID.Magnifiers) {
				// Force the cursor to be the favorite star instead of the magnifying glass since item sharing isn't allowed or isn't being attempted
				Main.cursorOverride = CursorOverrideID.FavoriteStar;
			}
		}

		// Lightweight version of Handle that doesn't handle clicks, since they are ignored
		// Hence, cloning the item is not necessary
		private void Handle_ItemInteractionWeak() {
			_dummyItemCollection[10] = StoredItem;
			// ItemSlot.Handle(), but without the click handling
			ItemSlot.OverrideHover(_dummyItemCollection, Context, 10);
			ItemSlot.MouseHover(_dummyItemCollection, Context, 10);

			Handle_ShareInteractionWeak();
		}

		private void Handle_ShareInteractionWeak() {
			if (CanShareItemToChat && Main.keyState.IsKeyDown(Main.FavoriteKey) && !StoredItem.IsAir && Main.drawingPlayerChat) {
				// Only force the cursor to be the magnifying glass; no click handling is necessary
				Main.cursorOverride = CursorOverrideID.Magnifiers;
			} else if (Main.cursorOverride == CursorOverrideID.Magnifiers) {
				// Force the cursor to be the favorite star instead of the magnifying glass since item sharing isn't allowed or isn't being attempted
				Main.cursorOverride = CursorOverrideID.FavoriteStar;
			}
		}

		private void Render(SpriteBatch spriteBatch) {
			// Draw draws the slot itself and its item.  Depending on context, the color will change, as will drawing other things like stack counts
			_dummyItemCollection[10] = StoredItem;
			ItemSlot.Draw(spriteBatch, _dummyItemCollection, Context, 10, GetDimensions().Position());
		}

		/// <summary>
		/// Invoked when the mouse is hovering over this item slot
		/// </summary>
		public event Action<EnhancedItemSlotV2> OnHovering;

		/// <summary>
		/// Called when the mouse is hovering over this item slot
		/// </summary>
		public virtual void Hovering() {
			OnHovering?.Invoke(this);
		}

		/// <summary>
		/// Forcibly overwrites the item bound to this item slot with the given item
		/// </summary>
		/// <param name="newItem">The new item instance.  If <see langword="null"/>, this parameter is set to an empty item</param>
		public void SetBoundItem(Item newItem) {
			newItem ??= new Item();

			var oldItem = storedItem;
			storedItem = newItem;
			OnStoredItemUpdated?.Invoke(this, oldItem, newItem);
		}
	}
}
