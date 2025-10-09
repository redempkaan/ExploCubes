using UnityEngine;

public static class LevelReader
{
    public static LevelData ReadLevelData(int levelNumber)
    {
        string fileName = "Levels/level_0" + levelNumber; // Reading levelNumber's json file
        TextAsset jsonFile = Resources.Load<TextAsset>(fileName);

        if (jsonFile == null)
        {
            Debug.LogError("Level file not found: " + fileName);
            return null;
        }

        return JsonUtility.FromJson<LevelData>(jsonFile.text);
    }
}