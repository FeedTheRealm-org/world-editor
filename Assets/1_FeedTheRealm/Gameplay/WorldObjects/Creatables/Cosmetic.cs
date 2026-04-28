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
            foreach (var kvp in data.category_sprites)
            {
                if (kvp.Value.StartsWith(config.SpritesDirectory))
                {
                    FileSystemHandler.DeleteFile(kvp.Value);
                }
            }
        }

        public override void Save(ref CreatablesData creatablesData)
        {
            var savedPaths = new Dictionary<string, string>();
            var keys = new List<string>(data.category_sprites.Keys);

            foreach (var key in keys)
            {
                var spriteFilePath = data.category_sprites[key];

                if (string.IsNullOrEmpty(spriteFilePath) || !Path.IsPathRooted(spriteFilePath))
                    continue;

                if (savedPaths.TryGetValue(spriteFilePath, out string existingSavedName))
                {
                    data.category_sprites[key] = existingSavedName;
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
                        data.category_sprites[key] = savedFileName;
                        savedPaths[spriteFilePath] = savedFileName;
                    }
                }
            }

            creatablesData.cosmetics.Add(this.data);
        }
    }
}
