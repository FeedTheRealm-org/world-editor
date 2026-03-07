using System.Collections.Generic;
using Enums;
using FeedTheRealm.Core.WorldObjects.CreatorObjects;

namespace FeedTheRealm.Core.WorldObjects.Shop
{
    public class ShopObject
    {
        public string id;
        public string displayName;
        public List<ProductObject> products = new();

        public ShopObject(string displayName)
        {
            id = System.Guid.NewGuid().ToString();
            this.displayName = displayName;
        }

        public ShopObject(string id, string displayName)
        {
            this.id = id;
            this.displayName = displayName;
        }
    }

    public class ProductObject
    {
        public string id;
        public string itemId;
        public CreatorObject item;
        public int price;
        public CurrencyType currency;

        public ProductObject(
            CreatorObject item,
            int price,
            CurrencyType currency = CurrencyType.Gold
        )
        {
            id = System.Guid.NewGuid().ToString();
            this.item = item;
            this.itemId = item?.ObjectId;
            this.price = price;
            this.currency = currency;
        }

        public ProductObject(string itemId, int price, CurrencyType currency = CurrencyType.Gold)
        {
            id = System.Guid.NewGuid().ToString();
            this.itemId = itemId;
            this.price = price;
            this.currency = currency;
        }
    }
}
