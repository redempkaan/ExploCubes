using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelPopupUI : MonoBehaviour
{
    [Header("Popup Elements")]
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public TMP_Text levelRelatedButtonText;
    public Button mainMenuButton;
    public Button levelRelatedButton;

    public void SetupPopup(string title, string description, string levelButtonText, System.Action onLevelClick, System.Action onMenuClick) // Filling the popup window
    {
        if (titleText != null)
            titleText.text = title;

        if (description != null)
        {
            descriptionText.text = description;
        }

        if (levelRelatedButtonText != null)
            levelRelatedButtonText.text = levelButtonText;

        levelRelatedButton.onClick.RemoveAllListeners();
        levelRelatedButton.onClick.AddListener(() => onLevelClick?.Invoke()); // Putting input function in onLevelClick to the related button

        mainMenuButton.onClick.RemoveAllListeners();
        mainMenuButton.onClick.AddListener(() => onMenuClick?.Invoke()); // Putting input function in onMenuClick to the related button
    }
    
}