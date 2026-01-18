using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level;

public class QuadPlane
{
	private Vector3 v1;

	private Vector3 v2;

	private Vector3 v3;

	private Vector3 v4;

	public Vector3 Normal { get; }

	public QuadPlane(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
	{
		this.v1 = v1;
		this.v2 = v2;
		this.v3 = v3;
		this.v4 = v4;
		Normal = Vector3.Cross(v2 - v1, v4 - v1).normalized;
	}

	public bool QuadIntersection(Ray ray, Vector3 worldPosition, out Vector3 intersection)
	{
		intersection = Vector3.zero;
		float num = Vector3.Dot(Normal, ray.direction);
		if (Mathf.Abs(num) < float.Epsilon)
		{
			return false;
		}
		float num2 = Vector3.Dot(worldPosition + v1 - ray.origin, Normal) / num;
		if (num2 < 0f)
		{
			return false;
		}
		intersection = ray.origin + ray.direction * num2;
		return true;
	}

	public bool PointIsOnQuad(Vector3 point, Vector3 worldPosition)
	{
		Vector3 vector = v1 + worldPosition;
		Vector3 vector2 = v2 + worldPosition;
		Vector3 vector3 = v3 + worldPosition;
		Vector3 vector4 = v4 + worldPosition;
		Vector3 normalized = Vector3.Cross(vector2 - vector, vector4 - vector3).normalized;
		if (Mathf.Abs(Vector3.Dot(point - vector, normalized)) > float.Epsilon)
		{
			return false;
		}
		if (!PointInTriangle(point, vector, vector2, vector3))
		{
			return PointInTriangle(point, vector3, vector4, vector);
		}
		return true;
	}

	private bool PointInTriangle(Vector3 point, Vector3 a, Vector3 b, Vector3 c)
	{
		Vector3 vector = c - a;
		Vector3 vector2 = b - a;
		Vector3 rhs = point - a;
		float num = Vector3.Dot(vector, vector);
		float num2 = Vector3.Dot(vector, vector2);
		float num3 = Vector3.Dot(vector, rhs);
		float num4 = Vector3.Dot(vector2, vector2);
		float num5 = Vector3.Dot(vector2, rhs);
		float num6 = num * num4 - num2 * num2;
		if (Mathf.Approximately(num6, 0f))
		{
			return false;
		}
		float num7 = 1f / num6;
		float num8 = (num4 * num3 - num2 * num5) * num7;
		float num9 = (num * num5 - num2 * num3) * num7;
		if (num8 >= 0f && num9 >= 0f)
		{
			return num8 + num9 <= 1f;
		}
		return false;
	}

	public void DebugDraw(Vector3 worldPosition, Color color, float duration)
	{
		Debug.DrawLine(worldPosition + v1, worldPosition + v2, color, duration);
		Debug.DrawLine(worldPosition + v2, worldPosition + v3, color, duration);
		Debug.DrawLine(worldPosition + v3, worldPosition + v4, color, duration);
		Debug.DrawLine(worldPosition + v4, worldPosition + v1, color, duration);
	}
}
