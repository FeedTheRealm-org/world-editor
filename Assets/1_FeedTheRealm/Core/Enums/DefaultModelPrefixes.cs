namespace FTR.Core.Enums
{
    public static class ModelFilePrefixes
    {
        public const string DefaultChestClosed = "DEFAULT_CHEST_CLOSED_";
        public const string DefaultChestOpen = "DEFAULT_CHEST_OPEN_";
        public const string NoCollider = "NO_COLLIDER_";

        private static readonly string[] All = { DefaultChestClosed, DefaultChestOpen, NoCollider };

        public static string StripPrefix(string name)
        {
            foreach (var prefix in All)
            {
                if (name.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
                    return name.Substring(prefix.Length);
            }
            return name;
        }
    }
}
