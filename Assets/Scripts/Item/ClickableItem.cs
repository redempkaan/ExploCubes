using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickableItem : GridItem, IPointerClickHandler // GridItem -> ClickableItem
{
    private Outline outline;

    private void Awake()
    {
        outline = GetComponent<Outline>();
        if (outline != null)
            outline.enabled = false; // Setting outline effect off initially
    }
    public virtual void OnPointerClick(PointerEventData eventData) // Called if clicked to an item that has ClickableItem script
    {
        if (outline != null)
        {
            outline.enabled = !outline.enabled;
        }

        GridManager gridManager = FindFirstObjectByType<GridManager>();

        if (gridManager != null)
        {
            gridManager.OnItemClicked(this); // Trigger onItemClicked event in grid manager
        }
    }
}