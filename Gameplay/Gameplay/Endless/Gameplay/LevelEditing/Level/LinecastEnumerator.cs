using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x0200055A RID: 1370
	public class LinecastEnumerator : IEnumerator<Vector3Int>, IEnumerator, IDisposable
	{
		// Token: 0x1700064A RID: 1610
		// (get) Token: 0x060020F8 RID: 8440 RVA: 0x000949B1 File Offset: 0x00092BB1
		// (set) Token: 0x060020F9 RID: 8441 RVA: 0x000949B9 File Offset: 0x00092BB9
		public Vector3Int Current { get; private set; }

		// Token: 0x1700064B RID: 1611
		// (get) Token: 0x060020FA RID: 8442 RVA: 0x000949C2 File Offset: 0x00092BC2
		// (set) Token: 0x060020FB RID: 8443 RVA: 0x000949CA File Offset: 0x00092BCA
		public Vector3 PositionAtStep { get; private set; }

		// Token: 0x1700064C RID: 1612
		// (get) Token: 0x060020FC RID: 8444 RVA: 0x000949D3 File Offset: 0x00092BD3
		object IEnumerator.Current
		{
			get
			{
				return this.Current;
			}
		}

		// Token: 0x060020FD RID: 8445 RVA: 0x000949E0 File Offset: 0x00092BE0
		public LinecastEnumerator(Ray ray, float length, float scalar)
		{
			this.ray = ray;
			this.endPosition = ray.origin + ray.direction * length;
			this.steps = Mathf.CeilToInt(length) + 1;
			this.steps = (int)((float)this.steps * scalar);
			this.Current = Stage.WorldSpacePointToGridCoordinate(ray.origin);
			this.currentStepCount = 0;
		}

		// Token: 0x060020FE RID: 8446 RVA: 0x00094A50 File Offset: 0x00092C50
		public LinecastEnumerator(int steps)
		{
			this.steps = steps;
		}

		// Token: 0x060020FF RID: 8447 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public void Dispose()
		{
		}

		// Token: 0x06002100 RID: 8448 RVA: 0x00094A60 File Offset: 0x00092C60
		public bool MoveNext()
		{
			Vector3Int vector3Int;
			for (;;)
			{
				this.currentStepCount++;
				this.PositionAtStep = Vector3.Lerp(this.ray.origin, this.endPosition, (float)this.currentStepCount / (float)this.steps);
				vector3Int = Stage.WorldSpacePointToGridCoordinate(this.PositionAtStep);
				if (vector3Int != this.Current)
				{
					break;
				}
				if (this.currentStepCount >= this.steps)
				{
					return false;
				}
			}
			this.Current = vector3Int;
			return true;
		}

		// Token: 0x06002101 RID: 8449 RVA: 0x00094ADB File Offset: 0x00092CDB
		public void Reset()
		{
			this.currentStepCount = 0;
		}

		// Token: 0x04001A44 RID: 6724
		private readonly Vector3 endPosition;

		// Token: 0x04001A45 RID: 6725
		private readonly int steps;

		// Token: 0x04001A46 RID: 6726
		private readonly Ray ray;

		// Token: 0x04001A47 RID: 6727
		private int currentStepCount;
	}
}
