using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

namespace TilemapConverter
{
    public class Executor
    {
        [MenuItem("Assets/Convert Tilemap to Sprites")]
        public static void Execute()
        {
            foreach (var grid in GameObject.FindObjectsOfType<Grid>())
            {
                var gridGameObject = new GameObject(grid.name);

                foreach (var tilemap in grid.GetComponentsInChildren<Tilemap>())
                {
                    var tilemapGameObject = CreateTilemap(grid, tilemap);

                    tilemapGameObject.transform.parent = gridGameObject.transform;
                }
            }
        }

        static GameObject CreateTilemap(Grid grid, Tilemap tilemap)
        {
            var tilemapGameObject = new GameObject(tilemap.name);

            var sortingLayerName = tilemap.GetComponent<TilemapRenderer>().sortingLayerName;
            var tileAnchor = CalculateTilemapAnchor(tilemap);
            tileAnchor.Scale(grid.cellSize);

            foreach (var position in tilemap.cellBounds.allPositionsWithin)
            {
                if (tilemap.HasTile(position))
                {
                    var tileGameObject = CreateTile(tilemap, position, sortingLayerName, tileAnchor);

                    tileGameObject.transform.parent = tilemapGameObject.transform;
                }
            }

            return tilemapGameObject;
        }

        static GameObject CreateTile(Tilemap tilemap, Vector3Int position, string sortingLayerName, Vector3 tileAnchor)
        {
            var tileGameObject = new GameObject(position.ToString(), typeof(SpriteRenderer));

            tileGameObject.transform.position = tilemap.CellToWorld(position) + tileAnchor;
            tileGameObject.transform.rotation = (tilemap.orientationMatrix * tilemap.GetTransformMatrix(position)).rotation;
            tileGameObject.transform.localScale = (tilemap.orientationMatrix * tilemap.GetTransformMatrix(position)).scale;

            var spriteRenderer = tileGameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = tilemap.GetSprite(position);
            spriteRenderer.sortingOrder = CalculateSortingOrder(tilemap, position);
            spriteRenderer.color = tilemap.color;
            spriteRenderer.sortingLayerName = sortingLayerName;

            return tileGameObject;
        }

        static int CalculateSortingOrder(Tilemap tilemap, Vector3Int position)
        {
            switch (tilemap.cellLayout)
            {
                // Caution : Unity 2017.2 does not have Isometric and IsometricZAsY.
                case Grid.CellLayout.Isometric:
                case Grid.CellLayout.IsometricZAsY:
                    return CalculateIsometricSortingOrder(tilemap, position);

                default:
                    return 0;
            }
        }

        static int CalculateIsometricSortingOrder(Tilemap tilemap, Vector3Int position)
        {
            var sortOrder = tilemap.GetComponent<TilemapRenderer>().sortOrder;
            if (sortOrder == TilemapRenderer.SortOrder.TopLeft || sortOrder == TilemapRenderer.SortOrder.BottomRight)
            {
                return 0;
            }

            var bounds = tilemap.cellBounds;
            if (sortOrder == TilemapRenderer.SortOrder.TopRight)
            {
                return -(position.x - bounds.xMin) - (position.y - bounds.yMin);
            }
            else
            { // TilemapRenderer.SortOrder.BottomLeft
                return (position.x - bounds.xMin) + (position.y - bounds.yMin);
            }
        }

        static Vector3 CalculateTilemapAnchor(Tilemap tilemap)
        {
            switch (tilemap.cellLayout)
            {
                case Grid.CellLayout.Rectangle:
                    return CalculateRectangleTilemapAnchor(tilemap);

                // Caution : Unity 2017.2 does not have Isometric and IsometricZAsY.
                case Grid.CellLayout.Isometric:
                case Grid.CellLayout.IsometricZAsY:
                    return CalculateIsometricTilemapAnchor(tilemap);

                default:
                    Debug.LogWarningFormat("Cannot calculate anchor Grid.CellLayout : {0}", tilemap.cellLayout);
                    return Vector3.zero;
            }
        }

        static Vector3 CalculateRectangleTilemapAnchor(Tilemap tilemap)
        {
            return tilemap.orientationMatrix.MultiplyPoint(tilemap.tileAnchor);
        }

        static Vector3 CalculateIsometricTilemapAnchor(Tilemap tilemap)
        {
            var gridCellSize = tilemap.layoutGrid.cellSize;
            var xVector = new Vector3(gridCellSize.x, gridCellSize.y, 0.0F);
            var yVector = new Vector3(-gridCellSize.x, gridCellSize.y, 0.0F);
            var anchor = tilemap.tileAnchor;
            var result = (anchor.x * xVector + anchor.y * yVector) / 2;

            return result;
        }
    }
}