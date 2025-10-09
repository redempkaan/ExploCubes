using System.Collections.Generic;
using UnityEngine;


public class ItemsToExplode // Helper class to store near objects like cubes, obstacles and rockets
{
    public HashSet<GridItem> connectedCubes = new HashSet<GridItem>();
    public HashSet<GridItem> nearObstacles = new HashSet<GridItem>();
    public HashSet<GridItem> nearRockets = new HashSet<GridItem>();

    public HashSet<int> DestroyItemsToExplode(GridItem[,] matrix)
    {
        HashSet<int> affectedColumns = new HashSet<int>(); // Marking affected columns to run CollapseColumn on them in the end

        foreach (GridItem cube in connectedCubes) // Destroying near cubes
        {
            if (cube == null) continue;
            affectedColumns.Add(cube.gridX);
            cube.TakeHit(matrix);
        }

        foreach (GridItem obs in nearObstacles) // Destroying near obstacles
        {
            if (obs == null) continue;
            affectedColumns.Add(obs.gridX);
            obs.TakeHit(matrix);
        }

        return affectedColumns; // Returning affected columns to use in collapse function
    }
}

