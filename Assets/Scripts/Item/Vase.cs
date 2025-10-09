using UnityEngine;

public class Vase : GridItem // GridItem -> Vase
{
    [Header("Vase Settings")]
    public GameObject brokenVasePrefab;

    public override void TakeHit(GridItem[,] matrix)
    {
        // If has 2hps turn the prefab into the broken one
        if (hitPoints == 2)
        {
            hitPoints = 1;

            if (brokenVasePrefab != null)
            {
                GameObject broken = Instantiate(
                    brokenVasePrefab,
                    transform.position,
                    Quaternion.identity,
                    transform.parent
                );

                Vase brokenScript = broken.GetComponent<Vase>(); // Getting its vase script
                if (brokenScript != null)
                {
                    brokenScript.gridX = gridX;
                    brokenScript.gridY = gridY;
                    brokenScript.itemCode = itemCode;
                    brokenScript.hitPoints = 1;
                }

                matrix[gridX, gridY] = brokenScript;
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("Broken vase prefab not assigned!");
            }
        }
        else
        {
            // Else destroy it by triggering the base classes function
            base.TakeHit(matrix);
        }
    }
}