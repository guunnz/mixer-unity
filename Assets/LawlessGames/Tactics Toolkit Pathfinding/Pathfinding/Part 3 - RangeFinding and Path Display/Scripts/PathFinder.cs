﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace finished3
{
    public class PathFinder
    {
        private Dictionary<Vector2Int, OverlayTile> searchableTiles;

        public List<OverlayTile> FindPath(OverlayTile start, OverlayTile end, List<OverlayTile> inRangeTiles)
        {
            searchableTiles = new Dictionary<Vector2Int, OverlayTile>();
            List<OverlayTile> openList = new List<OverlayTile>();
            HashSet<OverlayTile> closedList = new HashSet<OverlayTile>();

            // Track the closest reachable tile to the end
            OverlayTile closestTile = start;
            int closestDistance = GetManhattanDistance(start, end);

            if (inRangeTiles.Count > 0)
            {
                foreach (var item in inRangeTiles)
                {
                    searchableTiles.Add(item.grid2DLocation, MapManager.Instance.map[item.grid2DLocation]);
                }
            }
            else
            {
                searchableTiles = MapManager.Instance.map;
            }

            openList.Add(start);

            while (openList.Count > 0)
            {
                OverlayTile currentOverlayTile = openList.OrderBy(x => x.F).First();

                openList.Remove(currentOverlayTile);
                closedList.Add(currentOverlayTile);

                if (currentOverlayTile == end)
                {
                    return GetFinishedList(start, end);
                }

                foreach (var tile in GetNeightbourOverlayTiles(currentOverlayTile))
                {
                    if (tile.isBlocked || closedList.Contains(tile))
                    {
                        continue;
                    }

                    tile.G = GetManhattanDistance(start, tile);
                    tile.H = GetManhattanDistance(end, tile);

                    // Check if this tile is a new closest tile to the end
                    int distanceToEnd = GetManhattanDistance(tile, end);
                    if (distanceToEnd < closestDistance)
                    {
                        closestDistance = distanceToEnd;
                        closestTile = tile;
                    }

                    tile.Previous = currentOverlayTile;

                    if (!openList.Contains(tile))
                    {
                        openList.Add(tile);
                    }
                }
            }

            // If end is unreachable, return the path to the closest reachable tile
            return closestTile == start ? new List<OverlayTile>() : GetFinishedList(start, closestTile);
        }

        private int GetManhattanDistance(OverlayTile tile1, OverlayTile tile2)
        {
            return Mathf.Abs(tile1.gridLocation.x - tile2.gridLocation.x) +
                   Mathf.Abs(tile1.gridLocation.z - tile2.gridLocation.z);
        }


        private List<OverlayTile> GetFinishedList(OverlayTile start, OverlayTile end)
        {
            List<OverlayTile> finishedList = new List<OverlayTile>();
            OverlayTile currentTile = end;

            while (currentTile != start)
            {
                finishedList.Add(currentTile);
                currentTile = currentTile.Previous;
            }

            finishedList.Reverse();

            return finishedList;
        }

        private List<OverlayTile> GetNeightbourOverlayTiles(OverlayTile currentOverlayTile)
        {
            var map = MapManager.Instance.map;

            List<OverlayTile> neighbours = new List<OverlayTile>();

            //right
            Vector2Int locationToCheck = new Vector2Int(
                currentOverlayTile.gridLocation.x + 1,
                currentOverlayTile.gridLocation.z
            );

            if (searchableTiles.ContainsKey(locationToCheck))
            {
                neighbours.Add(searchableTiles[locationToCheck]);
            }

            //left
            locationToCheck = new Vector2Int(
                currentOverlayTile.gridLocation.x - 1,
                currentOverlayTile.gridLocation.z
            );

            if (searchableTiles.ContainsKey(locationToCheck))
            {
                neighbours.Add(searchableTiles[locationToCheck]);
            }

            //top
            locationToCheck = new Vector2Int(
                currentOverlayTile.gridLocation.x,
                currentOverlayTile.gridLocation.z + 1
            );

            if (searchableTiles.ContainsKey(locationToCheck))
            {
                neighbours.Add(searchableTiles[locationToCheck]);
            }

            //bottom
            locationToCheck = new Vector2Int(
                currentOverlayTile.gridLocation.x,
                currentOverlayTile.gridLocation.z - 1
            );

            if (searchableTiles.ContainsKey(locationToCheck))
            {
                neighbours.Add(searchableTiles[locationToCheck]);
            }

            return neighbours;
        }
    }
}