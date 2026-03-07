namespace UI.EditorBar.ElementOption.Scripts.ShopMenu
{
    /// <summary>
    /// Static context holder for passing a ShopObject between the shop list menu and the shop
    /// creator/editor menu. Mirrors EditContext but for ShopObject.
    /// </summary>
    public static class ShopEditContext
    {
        private static ShopObject shopToEdit;

        /// <summary>
        /// Set the shop that should be edited. Called by the list menu before opening the editor.
        /// </summary>
        public static void SetShopToEdit(ShopObject shop)
        {
            shopToEdit = shop;
        }

        /// <summary>
        /// Get the shop to edit and clear the context. Called by the editor menu on open.
        /// Returns null if no shop is pending edit (i.e. create-new mode).
        /// </summary>
        public static ShopObject GetAndClear()
        {
            var shop = shopToEdit;
            shopToEdit = null;
            return shop;
        }

        /// <summary>
        /// Check whether a shop is pending edit.
        /// </summary>
        public static bool HasShopToEdit() => shopToEdit != null;

        /// <summary>
        /// Clear any pending edit context.
        /// </summary>
        public static void Clear() => shopToEdit = null;
    }
}
