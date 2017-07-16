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
                    CreateTilemap(grid, tilemap, gridGameObject);
                }
            }
        }

        static void CreateTilemap(Grid grid, Tilemap tilemap, GameObject gridGameObject)
        {
            var parent = new GameObject(tilemap.name).transform;
            parent.transform.parent = gridGameObject.transform;

            var sortingLayerName = tilemap.GetComponent<TilemapRenderer>().sortingLayerName;
            var tileAnchor = tilemap.orientationMatrix.MultiplyPoint(tilemap.tileAnchor);
            tileAnchor.Scale(grid.cellSize);

            foreach (var position in tilemap.cellBounds.allPositionsWithin)
            {
                if (tilemap.HasTile(position))
                {
                    var spriteRenderer = new GameObject(position.ToString(), typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();

                    spriteRenderer.transform.position = tilemap.CellToWorld(position) + tileAnchor;
                    spriteRenderer.transform.parent = parent;
                    spriteRenderer.transform.rotation = tilemap.orientationMatrix.rotation * tilemap.GetTransformMatrix(position).rotation;
                    spriteRenderer.sprite = tilemap.GetSprite(position);
                    spriteRenderer.color = tilemap.color;
                    spriteRenderer.sortingLayerName = sortingLayerName;

                    // Caution : Unity 2D Experimental Release 4, need calculating sorting order
                }
            }
        }
    }
}
