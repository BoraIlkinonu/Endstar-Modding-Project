using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x02000372 RID: 882
	public abstract class CellCollectionMode
	{
		// Token: 0x1700027F RID: 639
		// (get) Token: 0x060010CB RID: 4299 RVA: 0x0005144E File Offset: 0x0004F64E
		// (set) Token: 0x060010CC RID: 4300 RVA: 0x00051456 File Offset: 0x0004F656
		public Vector3Int CurrentEndCoordinate { get; protected set; }

		// Token: 0x060010CD RID: 4301
		public abstract void InputDown(Vector3Int point);

		// Token: 0x060010CE RID: 4302
		public abstract bool IsComplete();

		// Token: 0x060010CF RID: 4303
		public abstract void InputReleased();

		// Token: 0x060010D0 RID: 4304
		public abstract void Reset();

		// Token: 0x060010D1 RID: 4305
		public abstract List<Vector3Int> CollectPaintedPositions(Ray ray, int maximumPositions);

		// Token: 0x060010D2 RID: 4306 RVA: 0x00051460 File Offset: 0x0004F660
		protected Vector3Int CalculateMinimumCoordinate(Vector3Int initial, Vector3Int end)
		{
			Vector3Int zero = Vector3Int.zero;
			zero.x = Mathf.Min(initial.x, end.x);
			zero.y = Mathf.Min(initial.y, end.y);
			zero.z = Mathf.Min(initial.z, end.z);
			return zero;
		}

		// Token: 0x060010D3 RID: 4307 RVA: 0x000514C4 File Offset: 0x0004F6C4
		protected Vector3Int CalculateAbsoluteDifference(Vector3Int initial, Vector3Int end)
		{
			Vector3Int zero = Vector3Int.zero;
			zero.x = Mathf.Abs(initial.x - end.x) + 1;
			zero.y = Mathf.Abs(initial.y - end.y) + 1;
			zero.z = Mathf.Abs(initial.z - end.z) + 1;
			return zero;
		}

		// Token: 0x17000280 RID: 640
		// (get) Token: 0x060010D4 RID: 4308
		public abstract string DisplayName { get; }
	}
}
