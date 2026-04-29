using System.Collections.Generic;
using System.IO;
using FeedTheRealm.Core.Utils;
using FeedTheRealm.Core.WorldObjects;
using FTRShared.Runtime.Models;

namespace FeedTheRealm.Gameplay.Creatables
{
    public class Cosmetic : Creatable
    {
        public CosmeticData data { get; private set; }

        public Cosmetic(CosmeticData data)
        {
            this.data = data;
        }

        public override string Id => data.id;

        public override void OnDelete()
        {
            foreach (var kvp in data.categories)
            {
                var spritePath = kvp.Value?.sprite_path;
                if (
                    !string.IsNullOrEmpty(spritePath)
                    && spritePath.StartsWith(config.SpritesDirectory)
                )
                {
                    FileSystemHandler.DeleteFile(spritePath);
                }
            }
        }

        public override void Save(ref CreatablesData creatablesData)
        {
            var savedPaths = new Dictionary<string, string>();
            var keys = new List<string>(data.categories.Keys);

            foreach (var key in keys)
            {
                var entry = data.categories[key];
                var spriteFilePath = entry?.sprite_path;

                if (string.IsNullOrEmpty(spriteFilePath) || !Path.IsPathRooted(spriteFilePath))
                    continue;

                if (savedPaths.TryGetValue(spriteFilePath, out string existingSavedName))
                {
                    entry.sprite_path = existingSavedName;
                }
                else
                {
                    string savedFileName = FileSystemHandler.SaveSprite(
                        spriteFilePath,
                        config.SpritesDirectory,
                        data.id
                    );

                    if (savedFileName != null)
                    {
                        entry.sprite_path = savedFileName;
                        savedPaths[spriteFilePath] = savedFileName;
                    }
                }
            }

            creatablesData.cosmetics.Add(this.data);
        }
    }
}
