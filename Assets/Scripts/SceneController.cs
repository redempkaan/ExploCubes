using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }
    public static event Action<Scene> OnSceneLoadedEvent;

    private void Awake()
    {
        if (Instance != null && Instance != this) // Making sure that Scene Controller only has 1 instance
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Marking as indestructable

        SceneManager.sceneLoaded -= HandleSceneLoaded; 
        SceneManager.sceneLoaded += HandleSceneLoaded; // Subscribing sceneLoaded event
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    public void StartLevelScene()
    {
        int lastLevel = LevelProgress.GetLastLevel(); // Getting user's current level number
        LevelManager.Instance.SetCurrentLevel(LevelReader.ReadLevelData(lastLevel)); // Reading specified level data
        SceneManager.LoadScene("LevelScene"); // Loading LevelScene
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[SceneController] Scene loaded: {scene.name}");

        if (scene.name == "LevelScene")
        {
            var level = LevelManager.Instance.CurrentLevel;
            if (level != null)
            {
                Debug.Log("Entered level " + level.level_number);

                LevelLoader loader = FindFirstObjectByType<LevelLoader>();
                if (loader != null)
                    loader.LoadLevel(level); // Loading level
                else
                    Debug.LogWarning("LevelLoader object not found!");
            }
            else
            {
                Debug.LogWarning("Level data is NULL!");
            }
        }
        OnSceneLoadedEvent?.Invoke(scene); // Publishing a message to other subscribed scripts
    }
}