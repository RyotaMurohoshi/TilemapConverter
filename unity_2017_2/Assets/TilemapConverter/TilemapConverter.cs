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
                    CreateTilemap(tilemap, gridGameObject);
                }
            }
        }

        static void CreateTilemap(Tilemap tilemap, GameObject gridGameObject)
        {
            var parent = new GameObject(tilemap.name).transform;
            parent.transform.parent = gridGameObject.transform;

            var tileRenderer = tilemap.GetComponent<TilemapRenderer>();
            var tilemapRotation = tilemap.orientationMatrix.rotation;
            var tileAnchor = CalculateTilemapAnchor(tilemap);

            foreach (var position in tilemap.cellBounds.allPositionsWithin)
            {
                if (tilemap.HasTile(position))
                {
                    var matrix = tilemap.orientationMatrix * tilemap.GetTransformMatrix(position);
                    var worldPosition = tilemap.CellToWorld(position) + tileAnchor;
                    var spriteRenderer = new GameObject(position.ToString(), typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();

                    spriteRenderer.transform.position = worldPosition;
                    spriteRenderer.transform.parent = parent;
                    spriteRenderer.transform.rotation = matrix.rotation;

                    // Caution : Unity 2D Experimental Release 4 change lossyScale -> scale
                    spriteRenderer.transform.localScale = matrix.lossyScale;

                    spriteRenderer.sprite = tilemap.GetSprite(position);
                    // Caution : Unity 2D Experimental Release 4, need calculating sorting order
                    spriteRenderer.color = tilemap.color;
                    spriteRenderer.sortingLayerName = tileRenderer.sortingLayerName;
                }
            }
        }

        static Vector3 CalculateTilemapAnchor(Tilemap tilemap)
        {
            return tilemap.orientationMatrix.MultiplyPoint(tilemap.tileAnchor);
        }
    }
}