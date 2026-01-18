using UnityEngine;

namespace Endless.Gameplay;

public class Vector3CircularBuffer : CircularBuffer<Vector3>
{
	public Vector3CircularBuffer(int size)
		: base(size)
	{
	}
}
