using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Creator.LevelEditing
{
	// Token: 0x02000341 RID: 833
	public class BoundaryGrid : MonoBehaviour
	{
		// Token: 0x17000258 RID: 600
		// (get) Token: 0x06000F67 RID: 3943 RVA: 0x000474EC File Offset: 0x000456EC
		public float FadeDistance
		{
			get
			{
				return this.fadeDistance;
			}
		}

		// Token: 0x06000F68 RID: 3944 RVA: 0x000474F4 File Offset: 0x000456F4
		private void Awake()
		{
			this.material = base.GetComponent<MeshRenderer>().material;
			this.material.SetFloat(BoundaryGrid.FadeDistanceId, this.fadeDistance);
			this.material.SetColor(BoundaryGrid.BaseColor, this.baseColor);
			this.material.SetFloat(BoundaryGrid.ScrollSpeed, this.scrollSpeed);
			this.material.SetFloat(BoundaryGrid.LineWidth, this.lineWidth);
			Array.Fill<Vector4>(this.allValues, new Vector4(0f, 0f, 0f, 1f));
		}

		// Token: 0x06000F69 RID: 3945 RVA: 0x00047590 File Offset: 0x00045790
		private void Update()
		{
			List<Transform> list = this.trackedTransforms;
			if (list != null && list.Count > 0)
			{
				for (int i = this.trackedTransforms.Count - 1; i >= 0; i--)
				{
					if (this.trackedTransforms[i] == null)
					{
						this.trackedTransforms.RemoveAt(i);
					}
				}
				int num = 0;
				int num2 = 0;
				while (num2 < this.trackedTransforms.Count && num2 < this.allValues.Length)
				{
					if (this.trackedTransforms[num2].gameObject.activeInHierarchy)
					{
						this.allValues[num2].x = this.trackedTransforms[num2].position.x;
						this.allValues[num2].y = this.trackedTransforms[num2].position.y;
						this.allValues[num2].z = this.trackedTransforms[num2].position.z;
						num++;
					}
					num2++;
				}
				if (num == 0)
				{
					return;
				}
				this.material.SetVectorArray(BoundaryGrid.LocalObjectPositions, this.allValues);
				this.material.SetInteger(BoundaryGrid.LocalObjectPositionsCount, num);
			}
		}

		// Token: 0x06000F6A RID: 3946 RVA: 0x000476D3 File Offset: 0x000458D3
		public void Track(Transform trackedTransform)
		{
			this.trackedTransforms.Add(trackedTransform);
		}

		// Token: 0x06000F6B RID: 3947 RVA: 0x000476E1 File Offset: 0x000458E1
		public void Untrack(Transform untrackedTransform)
		{
			this.trackedTransforms.Remove(untrackedTransform);
		}

		// Token: 0x06000F6C RID: 3948 RVA: 0x000476F0 File Offset: 0x000458F0
		public void SetLineColor(Color color)
		{
			this.material.SetColor(BoundaryGrid.LineColor, color);
		}

		// Token: 0x06000F6D RID: 3949 RVA: 0x00047703 File Offset: 0x00045903
		public void SetFadeDistance(float fadeDistance)
		{
			this.material.SetFloat(BoundaryGrid.FadeDistanceId, fadeDistance);
		}

		// Token: 0x04000CCD RID: 3277
		[SerializeField]
		private float fadeDistance = 10f;

		// Token: 0x04000CCE RID: 3278
		[SerializeField]
		private List<Transform> trackedTransforms;

		// Token: 0x04000CCF RID: 3279
		[SerializeField]
		private Color baseColor = Color.black;

		// Token: 0x04000CD0 RID: 3280
		[SerializeField]
		private float scrollSpeed = 1f;

		// Token: 0x04000CD1 RID: 3281
		[SerializeField]
		private float lineWidth = 0.02f;

		// Token: 0x04000CD2 RID: 3282
		private Material material;

		// Token: 0x04000CD3 RID: 3283
		private static readonly int LocalObjectPositions = Shader.PropertyToID("_LocalObjectPositions");

		// Token: 0x04000CD4 RID: 3284
		private static readonly int LocalObjectPositionsCount = Shader.PropertyToID("_LocalObjectPositionsCount");

		// Token: 0x04000CD5 RID: 3285
		private static readonly int FadeDistanceId = Shader.PropertyToID("_FadeDistance");

		// Token: 0x04000CD6 RID: 3286
		private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

		// Token: 0x04000CD7 RID: 3287
		private readonly Vector4[] allValues = new Vector4[10];

		// Token: 0x04000CD8 RID: 3288
		private static readonly int LineColor = Shader.PropertyToID("_LineColor");

		// Token: 0x04000CD9 RID: 3289
		private static readonly int ScrollSpeed = Shader.PropertyToID("_ScrollSpeed");

		// Token: 0x04000CDA RID: 3290
		private static readonly int LineWidth = Shader.PropertyToID("_LineWidth");
	}
}
