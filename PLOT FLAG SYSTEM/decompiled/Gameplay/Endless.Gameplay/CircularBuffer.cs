using System;

namespace Endless.Gameplay;

public class CircularBuffer<T>
{
	private readonly T[] buffer;

	private readonly int size;

	private int headIndex;

	private int tailIndex;

	private int count;

	public int Count
	{
		get
		{
			return count;
		}
		private set
		{
			count = value;
		}
	}

	public T this[int i]
	{
		get
		{
			int num = (tailIndex + i) % size;
			return buffer[num];
		}
	}

	public CircularBuffer(int size)
	{
		this.size = size;
		buffer = new T[size];
	}

	public void Add(T value)
	{
		Count++;
		buffer[headIndex] = value;
		headIndex = IncrementIndex(headIndex);
		if (Count > size)
		{
			tailIndex = IncrementIndex(tailIndex);
			Count--;
		}
	}

	public void Clear()
	{
		tailIndex = 0;
		headIndex = 0;
		Count = 0;
	}

	public void CopyTo(T[] array, int arrayIndex = 0)
	{
		if (array == null)
		{
			throw new ArgumentNullException($"The array cannot be null {this}");
		}
		if (arrayIndex < 0)
		{
			throw new ArgumentOutOfRangeException($"The starting array index cannot be negative {this}");
		}
		if (Count > array.Length - arrayIndex)
		{
			throw new ArgumentException("The destination array has fewer elements than the collection");
		}
		for (int i = 0; i < Count; i++)
		{
			int num = (tailIndex + i) % size;
			array[i + arrayIndex] = buffer[num];
		}
	}

	public int IncrementIndex(int index)
	{
		index++;
		if (index != size)
		{
			return index;
		}
		return 0;
	}
}
