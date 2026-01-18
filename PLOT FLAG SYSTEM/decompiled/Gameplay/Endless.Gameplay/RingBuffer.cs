using System;
using UnityEngine;

namespace Endless.Gameplay;

public class RingBuffer<T> where T : IFrameInfo
{
	private int count;

	protected int lastUpdated;

	private T[] ringBufferArray;

	protected T[] RingBufferArray => ringBufferArray;

	public void UpdateValue(ref T value)
	{
		lastUpdated = Mathf.Max(lastUpdated, (int)value.NetFrame);
		ringBufferArray[value.NetFrame % count] = value;
	}

	public int GetUseIndexOfFrame(uint frame)
	{
		return (int)((lastUpdated > frame) ? (frame % count) : (lastUpdated % count));
	}

	public T GetValue(uint frame)
	{
		if (lastUpdated <= frame)
		{
			return ringBufferArray[lastUpdated % count];
		}
		return ringBufferArray[frame % count];
	}

	public ref T GetAtPosition(uint frame)
	{
		return ref ringBufferArray[frame % count];
	}

	public RingBuffer(int ringCount)
	{
		count = Mathf.Max(1, ringCount);
		lastUpdated = 0;
		ringBufferArray = new T[count];
		for (int i = 0; i < ringBufferArray.Length; i++)
		{
			ringBufferArray[i].Initialize();
		}
	}

	public ref T GetReferenceFromBuffer(uint frame)
	{
		return ref ringBufferArray[frame % count];
	}

	public void FrameUpdated(uint frame)
	{
		lastUpdated = Mathf.Max(lastUpdated, (int)frame);
	}

	public void Clear()
	{
		Array.Clear(ringBufferArray, 0, count);
	}
}
