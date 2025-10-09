using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelButtonUI : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    private Button button;

private void OnEnable()
{
    SceneController.OnSceneLoadedEvent += OnSceneLoaded;
    UpdateLevelDisplay();
}

private void OnDisable()
{
    SceneController.OnSceneLoadedEvent -= OnSceneLoaded;
}

private void OnSceneLoaded(Scene scene)
{
    if (scene.name == "MainScene")
        UpdateLevelDisplay();
}

    private void UpdateLevelDisplay()
    {
        button = GetComponent<Button>() ?? GetComponentInParent<Button>();

        if (button == null || levelText == null)
        {
            Debug.LogWarning("[LevelButtonUI] Missing components!");
            return;
        }

        int lastLevel = LevelProgress.GetLastLevel();

        if (lastLevel <= 10)
        {
            levelText.text = "Level " + lastLevel;
            levelText.color = Color.white;
            button.interactable = true;
        }
        else
        {
            levelText.text = "Finished!";
            levelText.color = Color.gray; 
            button.interactable = false;
        }
    }
}