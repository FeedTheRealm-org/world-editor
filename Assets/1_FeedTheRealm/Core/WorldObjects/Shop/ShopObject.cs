using System.Collections.Generic;

public class ShopObject
{
    public List<ProductObject> products = new();
}

public class ProductObject
{
    public string id;
    public CreatorObject item;
    public int price;

    public ProductObject(CreatorObject item, int price)
    {
        id = System.Guid.NewGuid().ToString();
        this.item = item;
        this.price = price;
    }
}
