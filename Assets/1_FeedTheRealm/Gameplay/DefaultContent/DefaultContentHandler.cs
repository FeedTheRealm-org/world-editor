using Enums;
using FTR.Core.Common.Config;
using FTR.Core.Enums;
using FTRShared.Runtime.Models;

namespace FTR.Gameplay.DefaultContent
{
    public static class DefaultContentHandler
    {
        public static void ApplyPrefixConfig(string fileName, StructureData data, Config config)
        {
            foreach (var prefix in DefaultModelPrefix.AllModelPrefixes)
            {
                if (!fileName.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
                    continue;

                switch (prefix)
                {
                    case DefaultModelPrefix.DefaultChestClosed:
                        config.defaultClosedChestId = data.id;
                        break;
                    case DefaultModelPrefix.DefaultChestOpen:
                        config.defaultOpenChestId = data.id;
                        break;
                    case DefaultModelPrefix.NoCollider:
                        data.hasColliders = false;
                        break;
                    case DefaultModelPrefix.Slope:
                        data.colliderType = ColliderType.Slope;
                        break;
                }
                return;
            }
        }

        public static string StripPrefix(string name)
        {
            foreach (var prefix in DefaultModelPrefix.AllModelPrefixes)
            {
                if (name.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
                    return name.Substring(prefix.Length);
            }
            return name;
        }
    }
}
