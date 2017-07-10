using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

namespace TilemapConverter
{
	public class Executor
	{
		[MenuItem ("Assets/Convert Tilemap to Sprites")]
		public static void Execute ()
		{
			foreach (var grid in GameObject.FindObjectsOfType<Grid>()) {
				var gridGameObject = new GameObject (grid.name);

				foreach (var tilemap in grid.GetComponentsInChildren<Tilemap>()) {
					CreateTilemap (tilemap, gridGameObject);
				}
			}
		}

		static void CreateTilemap (Tilemap tilemap, GameObject gridGameObject)
		{
			var parent = new GameObject (tilemap.name).transform;
			parent.transform.parent = gridGameObject.transform;

			var tileRenderer = tilemap.GetComponent<TilemapRenderer> ();
			var tilemapRotation = tilemap.orientationMatrix.rotation;
			var tileAnchor = CalculateTilemapAnchor (tilemap);

			foreach (var position in tilemap.cellBounds.allPositionsWithin) {
				if (tilemap.HasTile (position)) {
					var matrix = tilemap.orientationMatrix * tilemap.GetTransformMatrix (position);
					var worldPosition = tilemap.CellToWorld (position) + tileAnchor;
					var spriteRenderer = new GameObject (position.ToString (), typeof(SpriteRenderer)).GetComponent<SpriteRenderer> ();

					spriteRenderer.transform.position = worldPosition;
					spriteRenderer.transform.parent = parent;
					spriteRenderer.transform.rotation = matrix.rotation;

					// Caution : Unity 2017.2 change scale -> lossyScale
					spriteRenderer.transform.localScale = matrix.scale;

					spriteRenderer.sprite = tilemap.GetSprite (position);
					spriteRenderer.sortingOrder = CalculateSortingOrder (tilemap, position);
					spriteRenderer.color = tilemap.color;
					spriteRenderer.sortingLayerName = tileRenderer.sortingLayerName;
				}
			}
		}

		static int CalculateSortingOrder (Tilemap tilemap, Vector3Int position)
		{
			switch (tilemap.cellLayout) {
			// Caution : Unity 2017.2 does not have Isometric and IsometricZAsY.
			case Grid.CellLayout.Isometric:
			case Grid.CellLayout.IsometricZAsY:
				return CalculateIsometricSortingOrder(tilemap, position);

			default:
				return 0;
			}
		}

		static int CalculateIsometricSortingOrder (Tilemap tilemap, Vector3Int position)
		{
			var sortOrder = tilemap.GetComponent<TilemapRenderer> ().sortOrder;
			if (sortOrder == TilemapRenderer.SortOrder.TopLeft || sortOrder == TilemapRenderer.SortOrder.BottomRight) {
				return 0;
			}

			var bounds = tilemap.cellBounds;
			if (sortOrder == TilemapRenderer.SortOrder.TopRight) {
				return -(position.x - bounds.xMin) - (position.y - bounds.yMin);
			} else { // TilemapRenderer.SortOrder.BottomLeft
				return (position.x - bounds.xMin) + (position.y - bounds.yMin);
			}
		}

		static Vector3 CalculateTilemapAnchor (Tilemap tilemap)
		{
			switch (tilemap.cellLayout) {
			case Grid.CellLayout.Rectangle:
				return CalculateRectangleTilemapAnchor (tilemap);

			// Caution : Unity 2017.2 does not have Isometric and IsometricZAsY.
			case Grid.CellLayout.Isometric:
			case Grid.CellLayout.IsometricZAsY:
				return CalculateIsometricTilemapAnchor(tilemap);

			default:
				Debug.LogWarningFormat ("Cannot calculate anchor Grid.CellLayout : {0}", tilemap.cellLayout);
				return Vector3.zero;
			}
		}

		static Vector3 CalculateRectangleTilemapAnchor (Tilemap tilemap)
		{
			return tilemap.orientationMatrix.MultiplyPoint (tilemap.tileAnchor);
		}

		static Vector3 CalculateIsometricTilemapAnchor (Tilemap tilemap)
		{			
			var gridCellSize = tilemap.layoutGrid.cellSize;
			var xVector = new Vector3 (gridCellSize.x, gridCellSize.y, 0.0F);
			var yVector = new Vector3 (-gridCellSize.x, gridCellSize.y, 0.0F);
			var anchor = tilemap.tileAnchor;
			var result = (anchor.x * xVector + anchor.y * yVector) / 2;

			return result;
		}
	}
}