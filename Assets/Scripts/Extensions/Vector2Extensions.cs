using UnityEngine;

public static class Vector2Extensions
{
	public static Vector3 ToFlatVector3(this Vector2 vector)
	{
		return new Vector3(vector.x, 0, vector.y);
	}
}