using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using Models;

public class WorldHandler {

    public void Save(WorldData worldData, string dataDirPath, string fileExtension) {

        dataDirPath = Path.Combine(Application.persistentDataPath, dataDirPath);

        try {
            string data = JsonUtility.ToJson(worldData, true);
            if (!Directory.Exists(dataDirPath)) {
                Directory.CreateDirectory(dataDirPath);
            }
            Debug.Log($"Saving data: {data}");

            if (string.IsNullOrEmpty(data) || data.Trim() == "{}") {
                Debug.LogWarning("No data to save.");
                return;
            }
            string dataFileName = CreateFileName(worldData.worldName, fileExtension);
            string fullPath = Path.Combine(dataDirPath, dataFileName);
            // using statement implements proper cleanup for resources,
            // this is used here to properly write and close the file
            using FileStream fs = new(fullPath, FileMode.Create);
            using StreamWriter writer = new(fs);
            writer.Write(data);

        } catch (Exception e) {
            Debug.LogError($"Error saving data to file: {e}");
        }
    }

    public WorldData Load(string dataFileName, string dataDirPath) {
        dataDirPath = Path.Combine(Application.persistentDataPath, dataDirPath);
        try {
            if (!File.Exists(Path.Combine(dataDirPath, dataFileName))) {
                return null;
            }
            string fullPath = Path.Combine(dataDirPath, dataFileName);
            using FileStream fs = new(fullPath, FileMode.Open);
            using StreamReader reader = new(fs);
            string dataToLoad = reader.ReadToEnd();
            return JsonUtility.FromJson<WorldData>(dataToLoad);

        } catch (Exception e) {
            Debug.LogError($"Error loading data from file: {e}");
            return null;
        }
    }

    public List<string> GetAllWorlds(string dataDirPath, string fileExtension) {
        List<string> worldFiles = new();
        dataDirPath = Path.Combine(Application.persistentDataPath, dataDirPath);
        try {
            if (!Directory.Exists(dataDirPath)) {
                Debug.Log($"No directory with {fileExtension} file extension found at: {dataDirPath}");
                return worldFiles;
            }

            var files = Directory.GetFiles(dataDirPath, $"*{fileExtension}");
            foreach (var file in files) {
                worldFiles.Add(Path.GetFileName(file));
            }
        } catch (Exception e) {
            Debug.LogError($"Error listing world files: {e}");
        }
        return worldFiles;
    }


    private string CreateFileName(string worldName, string fileExtension) {
        if (string.IsNullOrWhiteSpace(worldName)) {
            worldName = $"new_FTR_world_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
        }
        worldName = System.Text.RegularExpressions.Regex.Replace(worldName.Trim(), @"\s+", "_").ToLowerInvariant();
        return $"{worldName}{fileExtension}";
    }


}

