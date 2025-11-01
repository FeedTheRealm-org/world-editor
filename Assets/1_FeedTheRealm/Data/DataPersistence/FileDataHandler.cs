using System;
using System.IO;
using UnityEngine;

public class FileDataHandler {

    private string dataDirPath;
    private string dataFileName;

    public FileDataHandler(string dataDirPath, string dataFileName) {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
    }

    public void Save(WorldData worldData) {

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

            string fullPath = Path.Combine(dataDirPath, dataFileName);
            // using statement implements proper cleanup for resources, this is used here to properly write and close the file
            using FileStream fs = new(fullPath, FileMode.Create);
            using StreamWriter writer = new(fs);
            writer.Write(data);

        } catch (Exception e) {
            Debug.LogError($"Error saving data to file: {e}");
        }
    }

    public WorldData Load() {
        try {

            if (!File.Exists(Path.Combine(dataDirPath, dataFileName))) {
                return null;
            }

            string fullPath = Path.Combine(dataDirPath, dataFileName);
            using FileStream fs = new(fullPath, FileMode.Open);
            using StreamReader reader = new(fs);
            string dataToLoad = reader.ReadToEnd();
            Debug.Log($"Loaded Data: {dataToLoad}");
            return JsonUtility.FromJson<WorldData>(dataToLoad);

        } catch (Exception e) {
            Debug.LogError($"Error loading data from file: {e}");
            return null;
        }
    }
}

