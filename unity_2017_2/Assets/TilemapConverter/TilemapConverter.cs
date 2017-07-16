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
            var tileAnchor = tilemap.orientationMatrix.MultiplyPoint(tilemap.tileAnchor);
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
            tileGameObject.transform.rotation = tilemap.orientationMatrix.rotation * tilemap.GetTransformMatrix(position).rotation;

            var spriteRenderer = tileGameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = tilemap.GetSprite(position);
            spriteRenderer.color = tilemap.color;
            spriteRenderer.sortingLayerName = sortingLayerName;

            // Caution : Unity 2D Experimental Release 4, need calculating sorting order

            return tileGameObject;
        }
    }
}
