using System;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay.VisualManagement
{
	// Token: 0x0200037E RID: 894
	[Serializable]
	public class RendererManager
	{
		// Token: 0x060016E7 RID: 5863 RVA: 0x0006B076 File Offset: 0x00069276
		private ShaderClusterManager.ShaderCluster GetCluster(int index)
		{
			if (this.clusterIds[index] == null)
			{
				return null;
			}
			return MonoBehaviourSingleton<ShaderClusterManager>.Instance.GetShaderCluster(this.clusterIds[index]);
		}

		// Token: 0x060016E8 RID: 5864 RVA: 0x0006B098 File Offset: 0x00069298
		public void SetRenderer(Renderer newRenderer)
		{
			this.renderer = newRenderer;
			this.clusterIds = new string[this.renderer.materials.Length];
			this.alphaFadeSource = new bool[this.renderer.materials.Length];
			for (int i = 0; i < newRenderer.materials.Length; i++)
			{
				ShaderClusterManager.ShaderCluster shaderCluster = MonoBehaviourSingleton<ShaderClusterManager>.Instance.GetShaderCluster(newRenderer.materials[i].shader);
				if (shaderCluster != null)
				{
					this.clusterIds[i] = ((shaderCluster != null) ? shaderCluster.DisplayId : null);
					if (shaderCluster.supportsFade)
					{
						this.alphaFadeSource[i] = this.renderer.materials[i].IsKeywordEnabled("_ALPHATEST_ON");
					}
				}
			}
		}

		// Token: 0x060016E9 RID: 5865 RVA: 0x0006B145 File Offset: 0x00069345
		internal void EnableRenderer()
		{
			this.renderer.enabled = true;
		}

		// Token: 0x060016EA RID: 5866 RVA: 0x0006B153 File Offset: 0x00069353
		internal void DisableRenderer()
		{
			this.renderer.enabled = false;
		}

		// Token: 0x060016EB RID: 5867 RVA: 0x0006B164 File Offset: 0x00069364
		internal void SetFadeProperty(float value)
		{
			for (int i = 0; i < this.renderer.materials.Length; i++)
			{
				this.renderer.materials[i].SetFloat(RendererManager.CutoffHeight, value);
			}
		}

		// Token: 0x060016EC RID: 5868 RVA: 0x0006B1A4 File Offset: 0x000693A4
		internal void SwapToNoFade()
		{
			for (int i = 0; i < this.renderer.materials.Length; i++)
			{
				ShaderClusterManager.ShaderCluster cluster = this.GetCluster(i);
				if (cluster != null)
				{
					if (this.renderer.materials[i].shader == cluster.fadeShader)
					{
						EndlessVisuals.SwapShader(this.renderer.materials[i], cluster.primaryShader, false);
					}
					else if (this.renderer.materials[i].shader == cluster.fadeLowShader)
					{
						EndlessVisuals.SwapShader(this.renderer.materials[i], cluster.lowShader, false);
					}
					if (cluster.supportsFade && !this.alphaFadeSource[i])
					{
						this.renderer.materials[i].DisableKeyword("_ALPHATEST_ON");
					}
				}
			}
		}

		// Token: 0x060016ED RID: 5869 RVA: 0x0006B278 File Offset: 0x00069478
		internal void SwapToFade()
		{
			for (int i = 0; i < this.renderer.materials.Length; i++)
			{
				ShaderClusterManager.ShaderCluster cluster = this.GetCluster(i);
				if (cluster != null)
				{
					if (this.renderer.materials[i].shader == cluster.primaryShader)
					{
						EndlessVisuals.SwapShader(this.renderer.materials[i], cluster.fadeShader, false);
					}
					else if (this.renderer.materials[i].shader == cluster.lowShader)
					{
						EndlessVisuals.SwapShader(this.renderer.materials[i], cluster.fadeLowShader, false);
					}
					if (cluster.supportsFade)
					{
						this.renderer.materials[i].EnableKeyword("_ALPHATEST_ON");
					}
				}
			}
		}

		// Token: 0x060016EE RID: 5870 RVA: 0x0006B344 File Offset: 0x00069544
		[ContextMenu("SwapToLowQuality")]
		public void SwapToLowQuality()
		{
			for (int i = 0; i < this.renderer.materials.Length; i++)
			{
				ShaderClusterManager.ShaderCluster cluster = this.GetCluster(i);
				if (cluster != null)
				{
					if (this.renderer.materials[i].shader == cluster.primaryShader)
					{
						EndlessVisuals.SwapShader(this.renderer.materials[i], cluster.lowShader, false);
					}
					else if (this.renderer.materials[i].shader == cluster.fadeShader)
					{
						EndlessVisuals.SwapShader(this.renderer.materials[i], cluster.fadeLowShader, false);
					}
				}
			}
		}

		// Token: 0x060016EF RID: 5871 RVA: 0x0006B3EC File Offset: 0x000695EC
		[ContextMenu("SwapToHighQuality")]
		public void SwapToHighQuality()
		{
			for (int i = 0; i < this.renderer.materials.Length; i++)
			{
				ShaderClusterManager.ShaderCluster cluster = this.GetCluster(i);
				if (cluster != null)
				{
					if (this.renderer.materials[i].shader == cluster.lowShader)
					{
						EndlessVisuals.SwapShader(this.renderer.materials[i], cluster.primaryShader, false);
					}
					else if (this.renderer.materials[i].shader == cluster.fadeLowShader)
					{
						EndlessVisuals.SwapShader(this.renderer.materials[i], cluster.fadeShader, false);
					}
				}
			}
		}

		// Token: 0x0400125C RID: 4700
		private const string FADE_PROPERTY = "Cutoff_Height";

		// Token: 0x0400125D RID: 4701
		[SerializeField]
		private Renderer renderer;

		// Token: 0x0400125E RID: 4702
		[SerializeField]
		private string[] clusterIds;

		// Token: 0x0400125F RID: 4703
		[SerializeField]
		private bool[] alphaFadeSource;

		// Token: 0x04001260 RID: 4704
		private static readonly int CutoffHeight = Shader.PropertyToID("Cutoff_Height");
	}
}
