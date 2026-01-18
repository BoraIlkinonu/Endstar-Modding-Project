using System;
using System.Collections.Generic;
using Endless.Shared;
using UnityEngine;
using UnityEngine.Serialization;

namespace Endless.Gameplay.VisualManagement
{
	// Token: 0x0200037F RID: 895
	public class ShaderClusterManager : MonoBehaviourSingleton<ShaderClusterManager>
	{
		// Token: 0x060016F2 RID: 5874 RVA: 0x0006B4A5 File Offset: 0x000696A5
		protected override void Awake()
		{
			base.Awake();
			this.BuildShaderMaps();
		}

		// Token: 0x060016F3 RID: 5875 RVA: 0x0006B4B4 File Offset: 0x000696B4
		private void BuildShaderMaps()
		{
			foreach (ShaderClusterManager.ShaderCluster shaderCluster in this.shaderClusterList)
			{
				this.shaderMap.Add(shaderCluster.primaryShader, shaderCluster);
				this.shaderNameMap.Add(shaderCluster.DisplayId, shaderCluster);
			}
		}

		// Token: 0x060016F4 RID: 5876 RVA: 0x0006B524 File Offset: 0x00069724
		public ShaderClusterManager.ShaderCluster GetShaderCluster(Shader shader)
		{
			return this.shaderMap.GetValueOrDefault(shader);
		}

		// Token: 0x060016F5 RID: 5877 RVA: 0x0006B532 File Offset: 0x00069732
		public ShaderClusterManager.ShaderCluster GetShaderCluster(string clusterName)
		{
			return this.shaderNameMap.GetValueOrDefault(clusterName);
		}

		// Token: 0x04001261 RID: 4705
		[FormerlySerializedAs("shaderCluserList")]
		[SerializeField]
		private List<ShaderClusterManager.ShaderCluster> shaderClusterList = new List<ShaderClusterManager.ShaderCluster>();

		// Token: 0x04001262 RID: 4706
		private Dictionary<Shader, ShaderClusterManager.ShaderCluster> shaderMap = new Dictionary<Shader, ShaderClusterManager.ShaderCluster>();

		// Token: 0x04001263 RID: 4707
		private Dictionary<string, ShaderClusterManager.ShaderCluster> shaderNameMap = new Dictionary<string, ShaderClusterManager.ShaderCluster>();

		// Token: 0x02000380 RID: 896
		[Serializable]
		public class ShaderCluster
		{
			// Token: 0x04001264 RID: 4708
			[FormerlySerializedAs("DisplayName")]
			public string DisplayId = "Unknown";

			// Token: 0x04001265 RID: 4709
			public Shader primaryShader;

			// Token: 0x04001266 RID: 4710
			public Shader fadeShader;

			// Token: 0x04001267 RID: 4711
			public Shader lowShader;

			// Token: 0x04001268 RID: 4712
			public Shader fadeLowShader;

			// Token: 0x04001269 RID: 4713
			public bool supportsFade;
		}
	}
}
