using System;
using System.Collections.Generic;
using System.IO;
using Models;
using UnityEngine;

public class WorldFileHandler
{
    public static void Save(WorldData worldData, string dataDirPath, string fileExtension)
    {
        string saveFilePath = Application.persistentDataPath;
        dataDirPath = Path.Combine(saveFilePath, dataDirPath);

        try
        {
            string data = JsonUtility.ToJson(worldData, true);
            if (!Directory.Exists(dataDirPath))
            {
                Directory.CreateDirectory(dataDirPath);
            }
            if (string.IsNullOrEmpty(data) || data.Trim() == "{}")
            {
                Debug.LogWarning("No data to save.");
                return;
            }
            string dataFileName = CreateFileName(worldData.worldName, fileExtension);
            string fullPath = Path.Combine(dataDirPath, dataFileName);
            using FileStream fs = new(fullPath, FileMode.Create);
            using StreamWriter writer = new(fs);
            writer.Write(data);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving data to file: {e}");
        }
    }

    public static WorldData Load(string dataFileName, string dataDirPath)
    {
        try
        {
            string saveFilePath = Application.persistentDataPath;
            string fullDataDirPath = Path.Combine(saveFilePath, dataDirPath);
            if (!File.Exists(Path.Combine(fullDataDirPath, dataFileName)))
            {
                return null;
            }
            string fullPath = Path.Combine(fullDataDirPath, dataFileName);
            using FileStream fs = new(fullPath, FileMode.Open);
            using StreamReader reader = new(fs);
            string dataToLoad = reader.ReadToEnd();
            WorldData worldData = JsonUtility.FromJson<WorldData>(dataToLoad);
            return worldData;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading data from file: {e}");
            return null;
        }
    }

    public static string GetWorldFilePath(
        string worldName,
        string dataDirPath,
        string fileExtension
    )
    {
        string saveFilePath = Application.persistentDataPath;
        string dataFileName = CreateFileName(worldName, fileExtension);
        string fullDataDirPath = Path.Combine(saveFilePath, dataDirPath);
        return Path.Combine(fullDataDirPath, dataFileName);
    }

    public static List<string> GetAllWorlds(string dataDirPath, string fileExtension)
    {
        List<string> worldFiles = new();
        dataDirPath = Path.Combine(Application.persistentDataPath, dataDirPath);
        try
        {
            if (!Directory.Exists(dataDirPath))
            {
                Debug.Log(
                    $"No directory with {fileExtension} file extension found at: {dataDirPath}"
                );
                return worldFiles;
            }

            var files = Directory.GetFiles(dataDirPath, $"*{fileExtension}");
            foreach (var file in files)
            {
                worldFiles.Add(Path.GetFileName(file));
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error listing world files: {e}");
        }
        return worldFiles;
    }

    private static string CreateFileName(string worldName, string fileExtension)
    {
        if (string.IsNullOrWhiteSpace(worldName))
        {
            worldName = $"new_FTR_world_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
        }
        worldName = System
            .Text.RegularExpressions.Regex.Replace(worldName.Trim(), @"\s+", "_")
            .ToLowerInvariant();
        return $"{worldName}{fileExtension}";
    }
}
