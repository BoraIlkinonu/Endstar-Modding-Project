using System;
using System.Collections.Generic;
using Endless.Validation;
using UnityEngine;

namespace Endless.TerrainCosmetics.Validations
{
	// Token: 0x02000012 RID: 18
	public class MeshVertexSizeValidation : Validator
	{
		// Token: 0x0600005F RID: 95 RVA: 0x00002AC0 File Offset: 0x00000CC0
		public MeshVertexSizeValidation(TilesetCosmeticProfile profile)
		{
			this.profile = profile;
		}

		// Token: 0x06000060 RID: 96 RVA: 0x00002AD0 File Offset: 0x00000CD0
		public override List<ValidationResult> PassesValidation()
		{
			List<ValidationResult> list = new List<ValidationResult>();
			TileCosmetic[] tileCosmetics = this.profile.TileCosmetics;
			for (int i = 0; i < tileCosmetics.Length; i++)
			{
				foreach (Transform transform in tileCosmetics[i].Visuals)
				{
					int num = 0;
					list.AddRange(this.ValidateGameObjectVertexCount(transform, ref num, 442, 442));
				}
			}
			if (this.profile.TopDecorationSet)
			{
				foreach (Transform transform2 in this.profile.TopDecorationSet.Values)
				{
					if (transform2)
					{
						int num2 = 0;
						list.AddRange(this.ValidateGameObjectVertexCount(transform2, ref num2, 518, 518));
					}
				}
			}
			if (this.profile.SideDecorationSet)
			{
				foreach (Transform transform3 in this.profile.SideDecorationSet.Values)
				{
					if (transform3)
					{
						int num3 = 0;
						list.AddRange(this.ValidateGameObjectVertexCount(transform3, ref num3, 518, 518));
					}
				}
			}
			if (this.profile.BottomDecorationSet)
			{
				foreach (Transform transform4 in this.profile.BottomDecorationSet.Values)
				{
					if (transform4)
					{
						int num4 = 0;
						list.AddRange(this.ValidateGameObjectVertexCount(transform4, ref num4, 518, 518));
					}
				}
			}
			return ValidationResult.Pass(null);
		}

		// Token: 0x06000061 RID: 97 RVA: 0x00002CDC File Offset: 0x00000EDC
		private List<ValidationResult> ValidateGameObjectVertexCount(Transform transform, ref int currentTotal, int maximumTotalCount, int maxCountPerObject)
		{
			List<ValidationResult> list = new List<ValidationResult>();
			MeshFilter meshFilter;
			if (transform.TryGetComponent<MeshFilter>(out meshFilter) && meshFilter.sharedMesh)
			{
				currentTotal += meshFilter.sharedMesh.vertices.Length;
				if (meshFilter.sharedMesh.vertices.Length > maxCountPerObject)
				{
					list.AddRange(ValidationResult.Fail(string.Format("mesh: {0} has too many vertices on this mesh. Max vertices: {1}", meshFilter.sharedMesh, maxCountPerObject), null));
				}
				if (currentTotal > maximumTotalCount)
				{
					list.AddRange(ValidationResult.Fail("total vertex Count exceeded", null));
				}
			}
			for (int i = 0; i < transform.childCount; i++)
			{
				list.AddRange(this.ValidateGameObjectVertexCount(transform.GetChild(i), ref currentTotal, maximumTotalCount, maxCountPerObject));
			}
			return list;
		}

		// Token: 0x0400004D RID: 77
		private const int MAX_TILE_VERTEX_COUNT = 442;

		// Token: 0x0400004E RID: 78
		private const int MAX_TILE_VERTEX_TOTAL_COUNT = 442;

		// Token: 0x0400004F RID: 79
		private const int MAX_DECORATION_VERTEX_COUNT = 518;

		// Token: 0x04000050 RID: 80
		private const int MAX_DECORATION_VERTEX_TOTAL_COUNT = 518;

		// Token: 0x04000051 RID: 81
		private TilesetCosmeticProfile profile;
	}
}
