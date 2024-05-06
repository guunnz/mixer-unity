using System.Collections.Generic;
using UnityEngine;


public class MapManager : MonoBehaviour
{
    private static MapManager _instance;

    public static MapManager Instance
    {
        get { return _instance; }
    }

    public GameObject overlayPrefab;
    public GameObject overlayContainer;

    public Dictionary<Vector2Int, OverlayTile> map;

    public int width = 12; // Width of the map
    private List<OverlayTile> overlayTiles = new List<OverlayTile>();
    public int depth = 2; // Depth of the map (previously height)

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public void ToggleRectangles()
    {
        foreach (var overlayTile in overlayTiles)
        {
            overlayTile.ToggleRectangle(true);
        }
    }

    public void GenerateMap()
    {
        map = new Dictionary<Vector2Int, OverlayTile>();

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                var overlayTile = Instantiate(overlayPrefab, overlayContainer.transform);
                Vector3 cellWorldPosition = new Vector3(x, 0, z); // Position based on x and z
                overlayTile.transform.position = cellWorldPosition;
                var tileScript = overlayTile.GetComponent<OverlayTile>();
                overlayTiles.Add(tileScript);
                tileScript.gridLocation = new Vector3Int(x, 0, z);

                map.Add(new Vector2Int(x, z), tileScript);
            }
        }
    }

    public List<OverlayTile> GetAdjacentTiles(OverlayTile originTile)
    {
        var surroundingTiles = new List<OverlayTile>();

        // Check surrounding tiles
        List<Vector2Int> offsets = new List<Vector2Int>
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0)
        };

        foreach (var offset in offsets)
        {
            Vector2Int tileToCheck = originTile.grid2DLocation + offset;
            if (map.ContainsKey(tileToCheck))
            {
                surroundingTiles.Add(map[tileToCheck]);
            }
        }

        return surroundingTiles;
    }

    public List<OverlayTile> GetSurroundingTiles(Vector2Int originTile)
    {
        var surroundingTiles = new List<OverlayTile>();

        // Check surrounding tiles
        List<Vector2Int> offsets = new List<Vector2Int>
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        foreach (var offset in offsets)
        {
            Vector2Int tileToCheck = originTile + offset;
            if (map.ContainsKey(tileToCheck))
            {
                surroundingTiles.Add(map[tileToCheck]);
            }
        }

        return surroundingTiles;
    }
}