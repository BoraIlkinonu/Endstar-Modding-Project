using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x02000373 RID: 883
	public abstract class PlaneCellCollectionMode : CellCollectionMode
	{
		// Token: 0x17000281 RID: 641
		// (get) Token: 0x060010D6 RID: 4310 RVA: 0x0005152F File Offset: 0x0004F72F
		protected Vector3Int StartingPoint
		{
			get
			{
				return this.startingPoint;
			}
		}

		// Token: 0x17000282 RID: 642
		// (get) Token: 0x060010D7 RID: 4311 RVA: 0x00051537 File Offset: 0x0004F737
		protected Vector3 PlanePosition
		{
			get
			{
				return this.planePosition;
			}
		}

		// Token: 0x060010D8 RID: 4312 RVA: 0x0005153F File Offset: 0x0004F73F
		public override void InputDown(Vector3Int point)
		{
			this.startingPoint = point;
			this.planePosition = this.GetPlanePosition();
		}

		// Token: 0x060010D9 RID: 4313 RVA: 0x00051554 File Offset: 0x0004F754
		public override void InputReleased()
		{
			this.isFinished = true;
		}

		// Token: 0x060010DA RID: 4314 RVA: 0x0005155D File Offset: 0x0004F75D
		public override bool IsComplete()
		{
			return this.isFinished;
		}

		// Token: 0x060010DB RID: 4315 RVA: 0x00051565 File Offset: 0x0004F765
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x060010DC RID: 4316 RVA: 0x00051570 File Offset: 0x0004F770
		protected List<Vector3Int> CollectPoints(Plane plane, Ray ray, int maximumPoints)
		{
			List<Vector3Int> list = new List<Vector3Int>();
			float num;
			if (plane.Raycast(ray, out num))
			{
				Vector3Int vector3Int = Stage.WorldSpacePointToGridCoordinate(ray.GetPoint(num) + Vector3.up * 0.1f);
				base.CurrentEndCoordinate = vector3Int;
				Vector3Int vector3Int2 = base.CalculateMinimumCoordinate(this.StartingPoint, vector3Int);
				Vector3Int vector3Int3 = base.CalculateAbsoluteDifference(this.StartingPoint, vector3Int);
				if (vector3Int3.x * vector3Int3.y * vector3Int3.z <= maximumPoints)
				{
					for (int i = 0; i < vector3Int3.x; i++)
					{
						for (int j = 0; j < vector3Int3.y; j++)
						{
							for (int k = 0; k < vector3Int3.z; k++)
							{
								list.Add(new Vector3Int(i + vector3Int2.x, j + vector3Int2.y, k + vector3Int2.z));
							}
						}
					}
				}
				else
				{
					Vector3Int vector3Int4 = vector3Int - this.StartingPoint;
					if (this.StartingPoint.x != vector3Int.x)
					{
						vector3Int4.x = ((vector3Int.x < this.StartingPoint.x) ? (-1) : 1);
					}
					if (this.StartingPoint.y != vector3Int.y)
					{
						vector3Int4.y = ((vector3Int.y < this.StartingPoint.y) ? (-1) : 1);
					}
					if (this.StartingPoint.z != vector3Int.z)
					{
						vector3Int4.z = ((vector3Int.z < this.StartingPoint.z) ? (-1) : 1);
					}
					Vector3Int vector3Int5 = this.StartingPoint;
					int num2 = 0;
					while (vector3Int5 != vector3Int && num2 < 10000)
					{
						Vector3Int vector3Int6 = vector3Int5;
						num2++;
						bool flag = false;
						if (vector3Int5.x != vector3Int.x && vector3Int5.x + vector3Int4.x != vector3Int.x)
						{
							vector3Int6.x += vector3Int4.x;
							flag = true;
						}
						if (vector3Int5.y != vector3Int.y && vector3Int5.y + vector3Int4.y != vector3Int.y)
						{
							vector3Int6.y += vector3Int4.y;
							flag = true;
						}
						if (vector3Int5.z != vector3Int.z && vector3Int5.z + vector3Int4.z != vector3Int.z)
						{
							vector3Int6.z += vector3Int4.z;
							flag = true;
						}
						if (!flag)
						{
							break;
						}
						Vector3Int vector3Int7 = base.CalculateAbsoluteDifference(this.StartingPoint, vector3Int6);
						if (vector3Int7.x * vector3Int7.y * vector3Int7.z >= 100)
						{
							break;
						}
						vector3Int5 = vector3Int6;
						if (num2 >= 10000)
						{
							break;
						}
					}
					vector3Int2 = base.CalculateMinimumCoordinate(this.StartingPoint, vector3Int5);
					vector3Int3 = base.CalculateAbsoluteDifference(this.StartingPoint, vector3Int5);
					for (int l = 0; l < vector3Int3.x; l++)
					{
						for (int m = 0; m < vector3Int3.y; m++)
						{
							for (int n = 0; n < vector3Int3.z; n++)
							{
								list.Add(new Vector3Int(l + vector3Int2.x, m + vector3Int2.y, n + vector3Int2.z));
							}
						}
					}
				}
			}
			return list;
		}

		// Token: 0x060010DD RID: 4317
		protected abstract Vector3 GetPlanePosition();

		// Token: 0x04000DE1 RID: 3553
		private Vector3Int startingPoint;

		// Token: 0x04000DE2 RID: 3554
		private bool isFinished = true;

		// Token: 0x04000DE3 RID: 3555
		private Vector3 planePosition;
	}
}
