using System.Collections.Generic;
using Enums;

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
    public CreatorObject item;
    public string itemId;
    public int price;
    public CurrencyType currency;

    // Used when adding a new product via UI
    public ProductObject(CreatorObject item, int price, CurrencyType currency = CurrencyType.Gold)
    {
        id = System.Guid.NewGuid().ToString();
        this.item = item;
        this.itemId = item.ObjectId;
        this.price = price;
        this.currency = currency;
    }

    // Used when loading from saved data
    public ProductObject(string itemId, int price, CurrencyType currency = CurrencyType.Gold)
    {
        id = System.Guid.NewGuid().ToString();
        this.itemId = itemId;
        this.price = price;
        this.currency = currency;
    }
}
