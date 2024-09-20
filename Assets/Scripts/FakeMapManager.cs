using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeMapManager : MonoBehaviour
{
    public GameObject overlayPrefab;
    public GameObject overlayContainer;

    public Dictionary<Vector2Int, FakeOverlayTile> map;

    internal Vector2 minMapBounds = new Vector2(0, 0);
    public bool isEnemyLand;
    internal Vector2 maxMapBounds = new Vector2(7, 4);

    public int width = 12; // Width of the map
    private List<FakeOverlayTile> overlayTiles = new List<FakeOverlayTile>();
    public int depth = 2; // Depth of the map (previously height)
    public bool InitializeOnStart;
    public List<MaterialTipColorChanger> materialTipColorChangerList;

    private void OnEnable()
    {

    }

    public void ToggleRectangles()
    {
        foreach (var overlayTile in overlayTiles)
        {
            overlayTile.ToggleRectangle(true);
        }
    }

    public void ToggleRectanglesFalse()
    {
        foreach (var overlayTile in overlayTiles)
        {
            overlayTile.ToggleRectangle(false);
        }
    }

    private void Start()
    {
        if (InitializeOnStart)
        {
            GenerateFakeMap();
        }
    }

    internal void GenerateFakeMap()
    {
        map = new Dictionary<Vector2Int, FakeOverlayTile>();

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                var overlayTile = Instantiate(overlayPrefab, overlayContainer.transform);
                if (isEnemyLand)
                {
                    overlayTile.layer = LayerMask.NameToLayer("LandEnemy");
                    SetLayerRecursively(overlayTile.transform, "LandEnemy");
                    Vector3 cellWorldPosition = new Vector3(x, 0, z); // Position based on x and z
                    overlayTile.transform.position = cellWorldPosition;
                    var tileScript = overlayTile.GetComponent<EnemyTile>();
                    materialTipColorChangerList.Add(tileScript.materialTipColorChanger);
                    tileScript.gridLocation = new Vector3Int(x, 0, z);
                }
                else
                {
                    Vector3 cellWorldPosition = new Vector3(x, 0, z); // Position based on x and z
                    overlayTile.transform.position = cellWorldPosition;
                    var tileScript = overlayTile.GetComponent<FakeOverlayTile>();
                    materialTipColorChangerList.Add(tileScript.materialTipColorChanger);
                    overlayTiles.Add(tileScript);
                    tileScript.gridLocation = new Vector3Int(x, 0, z);

                    map.Add(new Vector2Int(x, z), tileScript);
                }
            }
        }

        ToggleRectanglesFalse();
    }

    void SetLayerRecursively(Transform root, string layerName)
    {
        // Set the layer of the current root
        root.gameObject.layer = LayerMask.NameToLayer(layerName);

        // Iterate over all children and apply the function recursively
        for (int i = 0; i < root.childCount; i++)
        {
            SetLayerRecursively(root.GetChild(i), layerName);
        }
    }

}