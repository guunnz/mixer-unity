using UnityEngine;

public class EnemyTile : MonoBehaviour
{
    public int G;
    public int H;
    //
    public int F
    {
        get { return G + H; }
    }

    public Vector3Int gridLocation;

    public Vector2Int grid2DLocation
    {
        get { return new Vector2Int(gridLocation.x, gridLocation.z); }
    }

    public MaterialTipColorChanger materialTipColorChanger;
}