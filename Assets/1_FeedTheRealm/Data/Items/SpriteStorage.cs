using System;
using System.IO;
using System.Linq;
using UnityEngine;

public static class SpriteStorage {
    // Save a source image into persistent data Items/ folder and return the generated UUID (without extension)
    public static string SaveFileReturnId(string sourceAbsolutePath) {
        try {
            if (string.IsNullOrEmpty(sourceAbsolutePath) || !File.Exists(sourceAbsolutePath)) return null;
            string itemsDir = Path.Combine(Application.persistentDataPath, "Items");
            Directory.CreateDirectory(itemsDir);

            // Generate ID and copy file with ID as filename
            string id = Guid.NewGuid().ToString();
            string ext = Path.GetExtension(sourceAbsolutePath);
            string dest = Path.Combine(itemsDir, id + ext);

            File.Copy(sourceAbsolutePath, dest);
            return id;
        } catch (Exception ex) {
            Debug.LogWarning($"SpriteStorage: Failed to save file to persistent data: {ex.Message}");
            return null;
        }
    }

    // Given an id or a path, resolve to an absolute file path if present in persistent storage or as absolute path.
    public static string GetFilePathFromIdOrPath(string idOrPath) {
        if (string.IsNullOrEmpty(idOrPath)) return null;
        try {
            // If it's already an absolute path or file exists, return it
            if (Path.IsPathRooted(idOrPath) && File.Exists(idOrPath)) return Path.GetFullPath(idOrPath);
            if (File.Exists(idOrPath)) return Path.GetFullPath(idOrPath);

            // Otherwise treat as id and look for file named <id>.* in persistent Items folder
            string itemsDir = Path.Combine(Application.persistentDataPath, "Items");
            if (!Directory.Exists(itemsDir)) return null;

            // Possible file with the id as filename
            var match = Directory.EnumerateFiles(itemsDir)
                .FirstOrDefault(f => string.Equals(Path.GetFileNameWithoutExtension(f), idOrPath, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(match) && File.Exists(match)) return match;

            return null;
        } catch (Exception ex) {
            Debug.LogWarning($"SpriteStorage: Failed to resolve id/path '{idOrPath}': {ex.Message}");
            return null;
        }
    }

    public static byte[] LoadSpriteBytesFromPath(string absolutePath) {
        try {
            if (string.IsNullOrEmpty(absolutePath) || !File.Exists(absolutePath)) return null;
            return File.ReadAllBytes(absolutePath);
        } catch (Exception ex) {
            Debug.LogWarning($"SpriteStorage: Failed to load sprite bytes from path '{absolutePath}': {ex.Message}");
            return null;
        }
    }
}
