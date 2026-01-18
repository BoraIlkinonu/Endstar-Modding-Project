using System;
using Unity.Burst;
using Unity.Collections;

namespace Endless.Gameplay;

[BurstCompile]
public static class NativeCollectionsExtensions
{
	public static void MergeSort<T>(this NativeList<T> list) where T : unmanaged, IComparable<T>
	{
		int length = list.Length;
		MergeSort(buffer: new NativeArray<T>(length, Allocator.Temp), list: list, lower: 0, upper: length - 1);
	}

	private static void MergeSort<T>(NativeList<T> list, NativeArray<T> buffer, int lower, int upper) where T : unmanaged, IComparable<T>
	{
		if (lower < upper)
		{
			int num = lower + (upper - lower) / 2;
			MergeSort(list, buffer, lower, num);
			MergeSort(list, buffer, num + 1, upper);
			Merge(list, buffer, lower, num, upper);
		}
	}

	private static void Merge<T>(NativeList<T> list, NativeArray<T> buffer, int lower, int midpoint, int upper) where T : unmanaged, IComparable<T>
	{
		for (int i = lower; i <= upper; i++)
		{
			buffer[i] = list[i];
		}
		int j = lower;
		int k = midpoint + 1;
		int num = lower;
		while (j <= midpoint && k <= upper)
		{
			if (buffer[j].CompareTo(buffer[k]) <= 0)
			{
				list[num] = buffer[j];
				num++;
				j++;
			}
			else
			{
				list[num] = buffer[k];
				num++;
				k++;
			}
		}
		for (; j <= midpoint; j++)
		{
			list[num] = buffer[j];
			num++;
		}
		for (; k <= upper; k++)
		{
			list[num] = buffer[k];
			num++;
		}
	}
}
