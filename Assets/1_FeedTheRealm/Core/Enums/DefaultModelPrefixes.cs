using Enums;
using FTR.Core.Common.Config;
using FTRShared.Runtime.Models;

namespace FTR.Core.Enums
{
    public static class DefaultModelPrefix
    {
        public const string DefaultChestClosed = "DEFAULT_CHEST_CLOSED_";
        public const string DefaultChestOpen = "DEFAULT_CHEST_OPEN_";
        public const string NoCollider = "NO_COLLIDER_";
        public const string Slope = "SLOPE_";
        public static readonly string[] AllModelPrefixes =
        {
            DefaultChestClosed,
            DefaultChestOpen,
            NoCollider,
            Slope,
        };
    }
}
