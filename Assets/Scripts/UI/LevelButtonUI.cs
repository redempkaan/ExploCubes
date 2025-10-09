using UnityEngine;
using TMPro;

public class LevelButtonUI : MonoBehaviour
{
    public TextMeshProUGUI levelText;

    void Start()
    {
        int lastLevel = LevelProgress.GetLastLevel();
        if (lastLevel <= 10)
        {
            levelText.text = "Level " + lastLevel;
        }
        else
        {
            levelText.text = "Finished !";
        }
    }
}