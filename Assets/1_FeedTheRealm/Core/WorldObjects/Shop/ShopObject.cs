using System.Collections.Generic;
using Models;

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
    public int price;
    public CurrencyType currency;

    public ProductObject(CreatorObject item, int price, CurrencyType currency = CurrencyType.Gold)
    {
        id = System.Guid.NewGuid().ToString();
        this.item = item;
        this.price = price;
        this.currency = currency;
    }
}
