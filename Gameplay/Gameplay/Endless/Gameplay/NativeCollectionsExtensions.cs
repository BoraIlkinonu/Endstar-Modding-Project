using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;

namespace Endless.Gameplay
{
	// Token: 0x0200022B RID: 555
	[BurstCompile]
	public static class NativeCollectionsExtensions
	{
		// Token: 0x06000B7A RID: 2938 RVA: 0x0003EF40 File Offset: 0x0003D140
		public static void MergeSort<[IsUnmanaged] T>(this NativeList<T> list) where T : struct, ValueType, IComparable<T>
		{
			int length = list.Length;
			NativeArray<T> nativeArray = new NativeArray<T>(length, Allocator.Temp, NativeArrayOptions.ClearMemory);
			NativeCollectionsExtensions.MergeSort<T>(list, nativeArray, 0, length - 1);
		}

		// Token: 0x06000B7B RID: 2939 RVA: 0x0003EF6C File Offset: 0x0003D16C
		private static void MergeSort<[IsUnmanaged] T>(NativeList<T> list, NativeArray<T> buffer, int lower, int upper) where T : struct, ValueType, IComparable<T>
		{
			if (lower < upper)
			{
				int num = lower + (upper - lower) / 2;
				NativeCollectionsExtensions.MergeSort<T>(list, buffer, lower, num);
				NativeCollectionsExtensions.MergeSort<T>(list, buffer, num + 1, upper);
				NativeCollectionsExtensions.Merge<T>(list, buffer, lower, num, upper);
			}
		}

		// Token: 0x06000B7C RID: 2940 RVA: 0x0003EFA4 File Offset: 0x0003D1A4
		private static void Merge<[IsUnmanaged] T>(NativeList<T> list, NativeArray<T> buffer, int lower, int midpoint, int upper) where T : struct, ValueType, IComparable<T>
		{
			for (int i = lower; i <= upper; i++)
			{
				buffer[i] = list[i];
			}
			int j = lower;
			int k = midpoint + 1;
			int num = lower;
			while (j <= midpoint)
			{
				if (k > upper)
				{
					break;
				}
				T t = buffer[j];
				if (t.CompareTo(buffer[k]) <= 0)
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
			while (j <= midpoint)
			{
				list[num] = buffer[j];
				num++;
				j++;
			}
			while (k <= upper)
			{
				list[num] = buffer[k];
				num++;
				k++;
			}
		}
	}
}
