using UnityEngine;
public static class LevelProgress
{
    private const string KEY = "last_level";

    public static int GetLastLevel() // Reading user's last level
    {
        return PlayerPrefs.GetInt(KEY, 1);
    }

    public static void SetLastLevel(int levelNumber) // Setting user's last level
    {
        PlayerPrefs.SetInt(KEY, levelNumber);
        PlayerPrefs.Save();
    }
}