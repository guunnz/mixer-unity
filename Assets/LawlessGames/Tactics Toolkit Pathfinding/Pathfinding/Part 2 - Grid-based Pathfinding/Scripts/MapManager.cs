using System.Collections.Generic;
using UnityEngine;

namespace finished2 
{
    public class MapManager : MonoBehaviour
    {
        private static MapManager _instance;
        public static MapManager Instance { get { return _instance; } }

        public GameObject overlayPrefab;
        public GameObject overlayContainer;

        public Dictionary<Vector2Int, OverlayTile> map;

        // Define the extents of your map
        public Vector2Int mapSize = new Vector2Int(12, 12); // Example size

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

        void Start()
        {
            map = new Dictionary<Vector2Int, OverlayTile>();
            GenerateOverlayTiles();
        }

        private void GenerateOverlayTiles()
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    var overlayTile = Instantiate(overlayPrefab, overlayContainer.transform);
                    Vector2Int gridPosition = new Vector2Int(x, y);
                    Vector3 worldPosition = GetWorldPosition(gridPosition);
                    overlayTile.transform.position = new Vector3(worldPosition.x, worldPosition.y, worldPosition.z + 1);

                    var overlayTileComponent = overlayTile.GetComponent<OverlayTile>();
                    overlayTileComponent.gridLocation = new Vector3Int(x, y, 0); // Assuming a flat grid

                    map.Add(gridPosition, overlayTileComponent);
                }
            }
        }

        private Vector3 GetWorldPosition(Vector2Int gridPosition)
        {
            // Convert grid position to world position
            // Adjust this method based on how you want to position your tiles in the world
            return new Vector3(gridPosition.x, gridPosition.y, 0);
        }
    }
}
