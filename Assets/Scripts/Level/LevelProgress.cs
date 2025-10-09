using UnityEngine;

public static class LevelProgress
{
    private const string KEY = "last_level";
    private static int cachedLevel = -1; // Temporary variable to store level info accurately

    public static int GetLastLevel()
    {
        if (cachedLevel == -1)
            cachedLevel = PlayerPrefs.GetInt(KEY, 1);

        return cachedLevel;
    }

    public static void SetLastLevel(int levelNumber)
    {
        cachedLevel = levelNumber; // Write it to temp variable first
        PlayerPrefs.SetInt(KEY, levelNumber); 
        PlayerPrefs.Save();
    }
}