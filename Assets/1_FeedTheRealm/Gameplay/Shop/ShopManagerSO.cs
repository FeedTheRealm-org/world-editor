using System.Collections.Generic;
using Models;
using UnityEngine;
using Utils;

[CreateAssetMenu(fileName = "ShopManager", menuName = "Scriptable Objects/ShopManager")]
public class ShopManagerSO : ScriptableObject, ILoadable, IPersistent
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private CreatorObjectLibrarySO creatorObjectLibrary;

    private readonly ShopObject shop = new();

    void OnEnable()
    {
        SelectionRaiser.WorldSelected += LoadWorld;
    }

    void OnDisable()
    {
        SelectionRaiser.WorldSelected -= LoadWorld;
    }

    public List<ProductObject> GetProducts()
    {
        return shop.products;
    }

    public void AddProduct(CreatorObject item, int price)
    {
        var product = new ProductObject(item, price);
        shop.products.Add(product);
    }

    public void RemoveProduct(string id)
    {
        var product = shop.products.Find(p => p.id == id);
        if (product != null)
            shop.products.Remove(product);
        else
            logger.Log($"Product with ID {id} not found.", this, Logging.LogType.Warning);
    }

    public void LoadWorld(WorldData worldData)
    {
        shop.products.Clear();
        if (worldData == null)
        {
            logger.Log("WorldData is null.", this, Logging.LogType.Warning);
            return;
        }

        foreach (ProductData productData in worldData.shopData.products)
        {
            List<CreatorObject> allCreatables = creatorObjectLibrary.GetAllCreatorObjects();

            CreatorObject creatorObject = allCreatables.Find(co =>
                co.ObjectId == productData.itemData.id
            );
            if (creatorObject != null)
                shop.products.Add(new ProductObject(creatorObject, productData.price));
            else
                logger.Log(
                    $"CreatorObject with ID {productData.itemData.id} not found.",
                    this,
                    Logging.LogType.Warning
                );
        }
    }

    // TODO: this needs a validation in cases when a CreatorObject was deleted
    public void SaveData(ref WorldData worldData)
    {
        worldData.shopData.products.Clear();
        foreach (var product in shop.products)
        {
            ProductData productData = new(product.item.ToItemData(), product.price);
            worldData.shopData.products.Add(productData);
        }
    }
}
