using UnityEngine;
using System.Collections.Generic;

public class GridItem : MonoBehaviour // Base class of all grid items
{
    public int hitPoints = 1;
    public int gridX;
    public int gridY;
    public string itemCode;

    public virtual void TakeHit(GridItem[,] matrix)
    {
        hitPoints--;
        if (hitPoints <= 0)
        {
            DestroyItem(matrix);
        }
    }

    public virtual void DestroyItem(GridItem[,] matrix) // Marked as virtual as vase is going to have a different destroy function
    {
        LevelManager.Instance.UnregisterObstacle(new HashSet<GridItem> { this }); // Unregistering obstacle from our nearObstacle list before destroying it
        
        matrix[gridX, gridY] = null; // Marking its location as null
        Destroy(gameObject); // Killing it
    }
}