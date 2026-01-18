using System;
using UnityEngine;

namespace Endless.Gameplay.Scripting;

public class Vector3
{
	private static Vector3 instance;

	internal static Vector3 Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new Vector3();
			}
			return instance;
		}
	}

	public UnityEngine.Vector3 Create(float x, float y, float z)
	{
		return new UnityEngine.Vector3(x, y, z);
	}

	public UnityEngine.Vector3 Add(UnityEngine.Vector3 lhs, UnityEngine.Vector3 rhs)
	{
		return lhs + rhs;
	}

	public UnityEngine.Vector3 Scale(UnityEngine.Vector3 vector, float scalar)
	{
		return vector * scalar;
	}

	public UnityEngine.Vector3 Lerp(UnityEngine.Vector3 a, UnityEngine.Vector3 b, float t)
	{
		return UnityEngine.Vector3.Lerp(a, b, t);
	}

	public UnityEngine.Vector3 Cross(UnityEngine.Vector3 lhs, UnityEngine.Vector3 rhs)
	{
		return UnityEngine.Vector3.Cross(lhs, rhs);
	}

	public float Dot(UnityEngine.Vector3 lhs, UnityEngine.Vector3 rhs)
	{
		return UnityEngine.Vector3.Dot(lhs, rhs);
	}

	public UnityEngine.Vector3 Normalize(UnityEngine.Vector3 vec)
	{
		return UnityEngine.Vector3.Normalize(vec);
	}

	public UnityEngine.Vector3 Reflect(UnityEngine.Vector3 inDirection, UnityEngine.Vector3 inNormal)
	{
		return UnityEngine.Vector3.Reflect(inDirection, inNormal);
	}

	public UnityEngine.Vector3 Scale(UnityEngine.Vector3 a, UnityEngine.Vector3 b)
	{
		return UnityEngine.Vector3.Scale(a, b);
	}

	public UnityEngine.Vector3 MoveTowards(UnityEngine.Vector3 current, UnityEngine.Vector3 target, float maxDistanceDelta)
	{
		return UnityEngine.Vector3.MoveTowards(current, target, maxDistanceDelta);
	}

	public UnityEngine.Vector3 Project(UnityEngine.Vector3 vector, UnityEngine.Vector3 onNormal)
	{
		return UnityEngine.Vector3.Project(vector, onNormal);
	}

	public UnityEngine.Vector3 RotateTowardsDegrees(UnityEngine.Vector3 current, UnityEngine.Vector3 target, float maxDegreesDelta, float maxMagnitudeDelta)
	{
		float maxRadiansDelta = MathF.PI / 180f * maxDegreesDelta;
		return RotateTowardsRadians(current, target, maxRadiansDelta, maxMagnitudeDelta);
	}

	public UnityEngine.Vector3 RotateTowardsRadians(UnityEngine.Vector3 current, UnityEngine.Vector3 target, float maxRadiansDelta, float maxMagnitudeDelta)
	{
		return UnityEngine.Vector3.RotateTowards(current, target, maxRadiansDelta, maxMagnitudeDelta);
	}

	public float Angle(UnityEngine.Vector3 from, UnityEngine.Vector3 to)
	{
		return UnityEngine.Vector3.Angle(from, to);
	}

	public float SignedAngle(UnityEngine.Vector3 from, UnityEngine.Vector3 to, UnityEngine.Vector3 axis)
	{
		return UnityEngine.Vector3.SignedAngle(from, to, axis);
	}

	public float Distance(UnityEngine.Vector3 a, UnityEngine.Vector3 b)
	{
		return UnityEngine.Vector3.Distance(a, b);
	}

	public float Magnitude(UnityEngine.Vector3 vector)
	{
		return UnityEngine.Vector3.Magnitude(vector);
	}

	public float SqrMagnitude(UnityEngine.Vector3 vector)
	{
		return UnityEngine.Vector3.SqrMagnitude(vector);
	}

	public UnityEngine.Vector3 Min(UnityEngine.Vector3 lhs, UnityEngine.Vector3 rhs)
	{
		return UnityEngine.Vector3.Min(lhs, rhs);
	}

	public UnityEngine.Vector3 Max(UnityEngine.Vector3 lhs, UnityEngine.Vector3 rhs)
	{
		return UnityEngine.Vector3.Max(lhs, rhs);
	}
}
