using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;


public class LevelManager : MonoBehaviour
{
        [Header("Tracker UI Elements")]
        public TMP_Text moveText;
        public TMP_Text boxText;
        public TMP_Text vaseText;
        public TMP_Text stoneText;

        [Header("Popup Window")]
        public LevelPopupUI popupWindow;

        public static LevelManager Instance { get; private set; } // Singleton reference

        public LevelData CurrentLevel { get; private set; }
        public int remainingMoves;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);  // Marking as singleton
            SceneController.OnSceneLoadedEvent -= HandleSceneLoaded;
            SceneController.OnSceneLoadedEvent += HandleSceneLoaded; // Subscribing scene controller's scene loaded event

        }
        private void InitiateTrackerTextObjects()
        {
            var moveTextObj = GameObject.Find("NumberOfMovesText"); // Finding related text object

            if (moveTextObj != null)
            {
                moveText = moveTextObj.GetComponent<TMP_Text>();
                moveText.SetText(CurrentLevel.move_count.ToString());
                remainingMoves = CurrentLevel.move_count; // Initially remaining moves equals to the number we read from the level json
            }
            else
            {
                Debug.LogWarning("MoveText not found in LevelScene!");
            }
            //Finding related tracker text objects
            var boxTextObj = GameObject.Find("BoxText");
            var vaseTextObj = GameObject.Find("VaseText");
            var stoneTextObj = GameObject.Find("StoneText");

            if (boxTextObj != null && CurrentLevel.obstacleCounts.ContainsKey("bo"))
            {
                boxText = boxTextObj.GetComponent<TMP_Text>();
                boxText.SetText(CurrentLevel.obstacleCounts["bo"].ToString()); // Putting box count to the text
            }
            else
            {
                Debug.LogWarning("Box text object not found in LevelScene or the level does not contain any specified obstacle!");
                if (boxTextObj != null)
                    boxTextObj.SetActive(false); // If there is no box in the level, deactivate related text object
            }

            if (vaseTextObj != null && CurrentLevel.obstacleCounts.ContainsKey("v"))
            {
                vaseText = vaseTextObj.GetComponent<TMP_Text>();
                vaseText.SetText(CurrentLevel.obstacleCounts["v"].ToString()); // Putting vase count to the text
            }
            else
            {
                Debug.LogWarning("Vase text object not found in LevelScene or the level does not contain any specified obstacle!");
                if (vaseTextObj != null)
                    vaseTextObj.SetActive(false); // If there is no vase, deactivate
            }

            if (stoneTextObj != null && CurrentLevel.obstacleCounts.ContainsKey("s"))
            {
                stoneText = stoneTextObj.GetComponent<TMP_Text>();
                stoneText.SetText(CurrentLevel.obstacleCounts["s"].ToString()); // Set stone count text
            }
            else
            {
                Debug.LogWarning("Stone text object not found in LevelScene or the level does not contain any specified obstacle!");
                if (stoneTextObj != null)
                    stoneTextObj.SetActive(false); // If there is no stone, deactivate
            }
            

        }
    private void HandleSceneLoaded(Scene scene)
    {
        if (scene.name == "LevelScene")
        {
            StartCoroutine(DelayedSceneInit()); // Coroutine to waiting for the scene objects to fully load
        }
        else
        {
            ClearUIReferences(); // If the loaded scene is not LevelScene, clear tracker objects
        }
    }

    private IEnumerator DelayedSceneInit()
    {
        yield return new WaitUntil(() =>
            FindFirstObjectByType<LevelPopupUI>(FindObjectsInactive.Include) != null); // Wait until LevelPopupUI is loaded into the scene

        popupWindow = FindFirstObjectByType<LevelPopupUI>(FindObjectsInactive.Include); // Finding and storing popupWindow script

        if (popupWindow != null)
        {
            popupWindow.gameObject.SetActive(false); // Setting popupWindow passive initially
            Debug.Log("[LevelManager] PopupWindow has been found and stored.");
        }
        else
        {
            Debug.LogWarning("[LevelManager] PopupWindow not found!");
        }

        InitiateTrackerTextObjects(); // And lastly initiating the tracker's text objects
    }

    private void ClearUIReferences() // Setting tracker texts to null
    {
        moveText = null;
        boxText = null;
        vaseText = null;
        stoneText = null;
    }
    public void DecreaseMove()
    {
        remainingMoves = Mathf.Max(remainingMoves - 1, 0);
        UpdateMoveUI();

        if (remainingMoves <= 0)
        {
            OnOutOfMoves();
        }
    }
    private void OnOutOfMoves()
    {
        ShowPopup(false);
        Debug.Log("Out of moves — Level failed!");
    }

    public void RegisterObstacle(string code) // Registering found obstacle to our LevelManager's CurrentLevel's obstacleCounts dictionary
    {
        if (CurrentLevel.obstacleCounts.ContainsKey(code))
            CurrentLevel.obstacleCounts[code]++;
        else
            CurrentLevel.obstacleCounts[code] = 1;
    }

    public void UnregisterObstacle(HashSet<GridItem> nearObstacles) // Unregistering all of the obstacles from our nearObstacles dictionary
    {
        foreach (GridItem obs in nearObstacles)
        {
            if (obs == null)
                continue;

                string code = obs.itemCode;

            if (!CurrentLevel.obstacleCounts.ContainsKey(code))
                continue;

            CurrentLevel.obstacleCounts[code] = Mathf.Max(CurrentLevel.obstacleCounts[code] - 1, 0); // If obs exist, decrease the count of it

            if (CurrentLevel.obstacleCounts[code] <= 0)
            {
                UpdateTargetUI();
                CurrentLevel.obstacleCounts.Remove(code); // If obs's count is 0, remove it from the dictionary
            }
        }

        UpdateTargetUI(); // Update tracker's target side

        if (CurrentLevel.obstacleCounts.Count == 0) // If all obstacles are unregistered, run OnAllObstaclesDestroyed
        {
            OnAllObstaclesDestroyed(); // Target reached
        }
    }

    private void OnAllObstaclesDestroyed()
    {
        Debug.Log($"Level Completed! : {LevelManager.Instance.CurrentLevel.level_number}");
        LevelProgress.SetLastLevel(LevelManager.Instance.CurrentLevel.level_number + 1); // Increasing persisted level by 1
        Debug.Log($"Current last level: {LevelProgress.GetLastLevel()}");
        ShowPopup(true); // Showing success version of popup
    }

    private void UpdateMoveUI() // Updating tracker's move side
    {
        if (moveText != null)
        {
            moveText.text = remainingMoves.ToString();
        }
    }

    private void UpdateTargetUI() // Updating tracker's target side
    {
        if (CurrentLevel.obstacleCounts.ContainsKey("bo"))
        {
            boxText.text = CurrentLevel.obstacleCounts["bo"].ToString();
        }
        if (CurrentLevel.obstacleCounts.ContainsKey("v"))
        {
            vaseText.text = CurrentLevel.obstacleCounts["v"].ToString();
        }
        if (CurrentLevel.obstacleCounts.ContainsKey("s"))
        {
            stoneText.text = CurrentLevel.obstacleCounts["s"].ToString();
        }
    }

        private void ShowPopup(bool isWin) // Popup to show to the user at the end of the level; isWin=T : success, isWin=F : Failure
        {
            popupWindow.SetupPopup(
                title: isWin ? "Level Complete!" : "Out of Moves",
                description: isWin ?
                    "You cleared all obstacles!" :
                    "You ran out of moves! Try again.",
                levelButtonText: isWin ? "Next Level" : "Retry",
                onLevelClick: () => // Specifying functions to run when related button is clicked
                {
                    popupWindow.gameObject.SetActive(false);
                    SceneController.Instance.StartLevelScene();
                    
                },
                onMenuClick: () => // Specifying functions to run when menu button is clicked
                {
                    popupWindow.gameObject.SetActive(false);
                    SceneManager.LoadScene("MainScene");
                }
                
            );

            popupWindow.gameObject.SetActive(true); // After filling it, making popup window active
        }

        public void SetCurrentLevel(LevelData levelData)
        {
            CurrentLevel = levelData;
        }
}