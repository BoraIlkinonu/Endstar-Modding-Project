using System;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000226 RID: 550
	public static class Int3Extensions
	{
		// Token: 0x06000B69 RID: 2921 RVA: 0x0003EC6C File Offset: 0x0003CE6C
		public static Vector3 ToVector3(this int3 int3)
		{
			return new Vector3((float)int3.x, (float)int3.y, (float)int3.z);
		}

		// Token: 0x06000B6A RID: 2922 RVA: 0x0003EC88 File Offset: 0x0003CE88
		public static Vector3 ToVector3(this float3 float3)
		{
			return new Vector3(float3.x, float3.y, float3.z);
		}

		// Token: 0x06000B6B RID: 2923 RVA: 0x0003ECA1 File Offset: 0x0003CEA1
		public static Vector3Int ToVector3Int(this int3 int3)
		{
			return new Vector3Int(int3.x, int3.y, int3.z);
		}

		// Token: 0x06000B6C RID: 2924 RVA: 0x0003ECBA File Offset: 0x0003CEBA
		public static int3 ToInt3(this Vector3Int vector3Int)
		{
			return new int3(vector3Int.x, vector3Int.y, vector3Int.z);
		}

		// Token: 0x06000B6D RID: 2925 RVA: 0x0003ECD6 File Offset: 0x0003CED6
		public static float3 ToFloat3(this Vector3 vector3)
		{
			return new float3(vector3.x, vector3.y, vector3.z);
		}

		// Token: 0x06000B6E RID: 2926 RVA: 0x0003ECEF File Offset: 0x0003CEEF
		public static int GetManhattanDistance(int3 a, int3 b)
		{
			return math.abs(b.x - a.x) + math.abs(b.y - a.y) + math.abs(b.z - a.z);
		}

		// Token: 0x04000AB8 RID: 2744
		public static readonly int3 Up = new int3(0, 1, 0);

		// Token: 0x04000AB9 RID: 2745
		public static readonly int3 Down = new int3(0, -1, 0);

		// Token: 0x04000ABA RID: 2746
		public static readonly int3 Forward = new int3(0, 0, 1);

		// Token: 0x04000ABB RID: 2747
		public static readonly int3 Back = new int3(0, 0, -1);

		// Token: 0x04000ABC RID: 2748
		public static readonly int3 Right = new int3(1, 0, 0);

		// Token: 0x04000ABD RID: 2749
		public static readonly int3 Left = new int3(-1, 0, 0);

		// Token: 0x04000ABE RID: 2750
		public static readonly int3 Invalid = new int3(-1000, -1000, -1000);

		// Token: 0x04000ABF RID: 2751
		public static readonly int3[] CardinalDirections = new int3[]
		{
			Int3Extensions.Forward,
			Int3Extensions.Right,
			Int3Extensions.Back,
			Int3Extensions.Left
		};

		// Token: 0x04000AC0 RID: 2752
		public static int3 zero = new int3(0, 0, 0);
	}
}
