using UnityEngine;

public class Utils
{
	/// <summary>
	/// Convert a 3D bounding box represented by a <c>Bounds</c> object into a 2D 
	/// rectangle represented by a <c>Rect</c> object.
	/// </summary>
	/// <returns>The 2D rectangle in Screen coordinates.</returns>
	/// <param name="cam">Camera to use.</param>
	/// <param name="bounds">3D bounding box.</param>
	public static Rect worldBoundsToScreen(Camera cam, Bounds bounds)
	{
		Vector3 center = bounds.center;
		Vector3 extents = bounds.extents;
		Bounds screenBounds = new Bounds(cam.WorldToScreenPoint(center), Vector3.zero);

		screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(+extents.x, +extents.y, +extents.z)));
		screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(+extents.x, +extents.y, -extents.z)));
		screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(+extents.x, -extents.y, +extents.z)));
		screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(+extents.x, -extents.y, -extents.z)));
		screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(-extents.x, +extents.y, +extents.z)));
		screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(-extents.x, +extents.y, -extents.z)));
		screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(-extents.x, -extents.y, +extents.z)));
		screenBounds.Encapsulate(cam.WorldToScreenPoint(center + new Vector3(-extents.x, -extents.y, -extents.z)));
		return Rect.MinMaxRect(screenBounds.min.x, screenBounds.min.y, screenBounds.max.x, screenBounds.max.y);
	}
}

