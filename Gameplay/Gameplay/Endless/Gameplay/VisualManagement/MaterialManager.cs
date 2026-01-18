using System;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay.VisualManagement
{
	// Token: 0x0200037D RID: 893
	[Serializable]
	public class MaterialManager
	{
		// Token: 0x060016E2 RID: 5858 RVA: 0x0006AF56 File Offset: 0x00069156
		private ShaderClusterManager.ShaderCluster GetCluster()
		{
			if (this.clusterId == null)
			{
				return null;
			}
			return MonoBehaviourSingleton<ShaderClusterManager>.Instance.GetShaderCluster(this.clusterId);
		}

		// Token: 0x060016E3 RID: 5859 RVA: 0x0006AF72 File Offset: 0x00069172
		public void SetMaterial(Material material)
		{
			this.material = material;
			ShaderClusterManager.ShaderCluster shaderCluster = MonoBehaviourSingleton<ShaderClusterManager>.Instance.GetShaderCluster(material.shader);
			this.clusterId = ((shaderCluster != null) ? shaderCluster.DisplayId : null);
		}

		// Token: 0x060016E4 RID: 5860 RVA: 0x0006AFA0 File Offset: 0x000691A0
		[ContextMenu("SwapToLowQuality")]
		public void SwapToLowQuality()
		{
			ShaderClusterManager.ShaderCluster cluster = this.GetCluster();
			if (cluster != null)
			{
				if (this.material.shader == cluster.primaryShader)
				{
					this.material.shader = cluster.lowShader;
					return;
				}
				if (this.material.shader == cluster.fadeShader)
				{
					this.material.shader = cluster.fadeLowShader;
				}
			}
		}

		// Token: 0x060016E5 RID: 5861 RVA: 0x0006B00C File Offset: 0x0006920C
		[ContextMenu("SwapToHighQuality")]
		public void SwapToHighQuality()
		{
			ShaderClusterManager.ShaderCluster cluster = this.GetCluster();
			if (cluster != null)
			{
				if (this.material.shader == cluster.lowShader)
				{
					this.material.shader = cluster.primaryShader;
					return;
				}
				if (this.material.shader == cluster.fadeLowShader)
				{
					this.material.shader = cluster.fadeShader;
				}
			}
		}

		// Token: 0x0400125A RID: 4698
		[SerializeField]
		private Material material;

		// Token: 0x0400125B RID: 4699
		[SerializeField]
		private string clusterId;
	}
}
