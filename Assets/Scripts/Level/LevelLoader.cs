using UnityEngine;



public class LevelLoader : MonoBehaviour
{

    public GridManager gridManager;
    public LevelData currentLevel;
    
    public void LoadLevel(LevelData level)
    {
        if (gridManager == null)
        {
            gridManager = FindFirstObjectByType<GridManager>(); // Finding gridManager object
        }

        LevelData levelData_ = LevelManager.Instance.CurrentLevel;

        if (levelData_ == null)
        {
            print("Level Data is NULL!");
        }
        else
        {
            gridManager.SetGridBackground(levelData_.grid_width, levelData_.grid_height); // Setting grid background

            gridManager.InitializeItemMatrix(levelData_.grid_width, levelData_.grid_height); // Initializing grid item matrix with the specified width and height

            gridManager.GenerateGrid(levelData_); // Putting items on the grid

            print("Entered level " + levelData_);
        }
    }
}