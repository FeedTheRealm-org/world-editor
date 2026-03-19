using System.Collections.Generic;
using Enums;
using FeedTheRealm.Core.DataPersistence;
using FeedTheRealm.Core.EventChannels.WorldEvents;
using FeedTheRealm.Core.WorldObjects.CreatorObjects;
using FeedTheRealm.Core.WorldObjects.Shop;
using FeedTheRealm.Gameplay.WorldLoader;
using FTRShared.Runtime.Models;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopManager", menuName = "Scriptable Objects/ShopManager")]
public class ShopManagerSO : ScriptableObject, ILoadable, IPersistent
{
    [SerializeField]
    private Logging.Logger logger;

    [SerializeField]
    private WorldSelectedEvent worldSelectedEvent;

    private readonly List<ShopObject> shops = new();

    void OnEnable()
    {
        worldSelectedEvent.OnRaised += LoadWorld;
    }

    void OnDisable()
    {
        worldSelectedEvent.OnRaised -= LoadWorld;
    }

    public ShopObject CreateShop(string displayName)
    {
        var shop = new ShopObject(displayName);
        shops.Add(shop);
        return shop;
    }

    public void DeleteShop(string shopId)
    {
        var shop = FindShop(shopId);
        if (shop != null)
            shops.Remove(shop);
    }

    public ShopObject GetShop(string shopId)
    {
        return FindShop(shopId);
    }

    public List<ShopObject> GetShops()
    {
        return shops;
    }

    public List<ProductObject> GetProducts(string shopId)
    {
        var shop = FindShop(shopId);
        return shop?.products;
    }

    public void AddProduct(
        string shopId,
        CreatorObject item,
        int price,
        CurrencyType currency = CurrencyType.Gold
    )
    {
        var shop = FindShop(shopId);
        if (shop == null)
            return;
        shop.products.Add(new ProductObject(item, price, currency));
    }

    public void RemoveProduct(string shopId, string productId)
    {
        var shop = FindShop(shopId);
        if (shop == null)
            return;
        var product = shop.products.Find(p => p.id == productId);
        if (product != null)
            shop.products.Remove(product);
        else
            logger.Log(
                $"Product with ID {productId} not found in shop {shopId}.",
                this,
                Logging.LogType.Warning
            );
    }

    public void LoadWorld(WorldData worldData)
    {
        shops.Clear();
        if (worldData == null)
        {
            logger.Log("WorldData is null.", this, Logging.LogType.Warning);
            return;
        }

        foreach (ShopData shopData in worldData.worldShopsData.shops)
        {
            var shop = new ShopObject(shopData.id, shopData.displayName);
            foreach (ProductData productData in shopData.products)
            {
                shop.products.Add(
                    new ProductObject(productData.itemId, productData.price, productData.currency)
                );
            }
            shops.Add(shop);
        }
    }

    // TODO: this needs a validation in cases when a CreatorObject was deleted
    public void SaveData(ref WorldData worldData)
    {
        worldData.worldShopsData.shops.Clear();
        foreach (var shop in shops)
        {
            ShopData shopData = new() { id = shop.id, displayName = shop.displayName };
            foreach (var product in shop.products)
                shopData.products.Add(
                    new ProductData(
                        product.item?.ObjectId ?? product.itemId,
                        product.price,
                        product.currency
                    )
                );
            worldData.worldShopsData.shops.Add(shopData);
        }
    }

    private ShopObject FindShop(string shopId)
    {
        var shop = shops.Find(s => s.id == shopId);
        if (shop == null)
            logger.Log($"Shop with ID {shopId} not found.", this, Logging.LogType.Warning);
        return shop;
    }
}
