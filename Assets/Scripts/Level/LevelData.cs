using System.Collections.Generic;

[System.Serializable]
public class LevelData
{
    public int level_number;
    public int grid_width;
    public int grid_height;
    public int move_count;
    public string[] grid;
    public Dictionary<string, int> obstacleCounts = new Dictionary<string, int>(); // Dictionary to track obstacle count
}