using System.Collections.Generic;
using System.IO;
using System.Linq;
using Models;
using LootEntryData = Models.LootTableData.LootEntryData;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UIElements;

public static class LootTableMenuUI
{
    public static Sprite LoadSpriteFromDisk(string path)
    {
        if (!FileBrowserHelpers.FileExists(path))
            return null;

        byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(path);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

        if (!texture.LoadImage(bytes))
            return null;

        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        return Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f
        );
    }

    public static void UpdateSpritePreview(UnityEngine.UIElements.Image spritePreview, string spriteId)
    {
        if (string.IsNullOrEmpty(spriteId))
        {
            spritePreview.sprite = null;
            return;
        }

        string cleanId = Path.GetFileNameWithoutExtension(spriteId);

        string spritePath = Path.Combine(
            Application.streamingAssetsPath,
            "Sprites",
            cleanId + ".png"
        );
        Sprite sprite = LoadSpriteFromDisk(spritePath);
        spritePreview.sprite =
            (FileBrowserHelpers.FileExists(spritePath) && sprite != null) ? sprite : null;
    }

    public static VisualElement CreateLootItemElement(
        LootEntryData entry,
        string itemDisplayName,
        System.Action<LootEntryData> onRemove
    )
    {
        var itemContainer = new VisualElement();
        itemContainer.style.flexDirection = FlexDirection.Row;
        itemContainer.style.justifyContent = Justify.SpaceBetween;
        itemContainer.style.alignItems = Align.Center;
        itemContainer.style.marginBottom = 5;
        itemContainer.style.paddingLeft = 5;
        itemContainer.style.paddingRight = 5;
        itemContainer.style.paddingTop = 3;
        itemContainer.style.paddingBottom = 3;
        itemContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        itemContainer.style.borderBottomLeftRadius = 3;
        itemContainer.style.borderBottomRightRadius = 3;
        itemContainer.style.borderTopLeftRadius = 3;
        itemContainer.style.borderTopRightRadius = 3;

        var infoContainer = new VisualElement();
        infoContainer.style.flexDirection = FlexDirection.Column;

        var itemLabel = new Label(itemDisplayName);
        itemLabel.style.color = Color.white;
        itemLabel.style.fontSize = 14;

        var probabilityLabel = new Label($"Drop Probability: {entry.dropProbability}%");
        probabilityLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
        probabilityLabel.style.fontSize = 12;

        infoContainer.Add(itemLabel);
        infoContainer.Add(probabilityLabel);

        var removeButton = new Button(() => onRemove(entry));
        removeButton.text = "Remove";
        removeButton.style.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 0.8f);
        removeButton.style.color = Color.white;

        itemContainer.Add(infoContainer);
        itemContainer.Add(removeButton);

        return itemContainer;
    }
}
