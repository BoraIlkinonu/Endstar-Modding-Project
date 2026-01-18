using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.VisualManagement;
using Endless.Shared.EndlessQualitySettings;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.Scripting
{
	// Token: 0x020004AD RID: 1197
	public class EndlessVisuals : MonoBehaviour
	{
		// Token: 0x06001D98 RID: 7576 RVA: 0x00081B43 File Offset: 0x0007FD43
		public void FindAndManageChildRenderers(GameObject prefabRoot)
		{
			this.ManageRenderers(prefabRoot.GetComponentsInChildren<Renderer>());
		}

		// Token: 0x06001D99 RID: 7577 RVA: 0x00081B52 File Offset: 0x0007FD52
		public void FindAndManageChildRenderers(Transform prefabRoot)
		{
			this.ManageRenderers(prefabRoot.GetComponentsInChildren<Renderer>());
		}

		// Token: 0x06001D9A RID: 7578 RVA: 0x00081B64 File Offset: 0x0007FD64
		public List<RendererManager> ManageRenderers(IEnumerable<Renderer> renderers)
		{
			List<RendererManager> list = new List<RendererManager>();
			foreach (Renderer renderer in renderers)
			{
				if (!(renderer is ParticleSystemRenderer))
				{
					RendererManager rendererManager = new RendererManager();
					list.Add(rendererManager);
					rendererManager.SetRenderer(renderer);
				}
			}
			if (this.hasSubscribed)
			{
				ShaderQuality.ShaderQualityLevel currentQualityLevel = ShaderQuality.CurrentQualityLevel;
				if (currentQualityLevel != ShaderQuality.ShaderQualityLevel.High)
				{
					if (currentQualityLevel != ShaderQuality.ShaderQualityLevel.Low)
					{
					}
					this.SwapToLowQuality(list);
				}
				else
				{
					this.SwapToHighQuality(list);
				}
			}
			this.managedRenderers.AddRange(list);
			return list;
		}

		// Token: 0x06001D9B RID: 7579 RVA: 0x00081C00 File Offset: 0x0007FE00
		public void UnmanageRenderers(IEnumerable<RendererManager> managers)
		{
			this.managedRenderers = this.managedRenderers.Except(managers).ToList<RendererManager>();
		}

		// Token: 0x06001D9C RID: 7580 RVA: 0x00081C1C File Offset: 0x0007FE1C
		public List<MaterialManager> ManageMaterials(IEnumerable<Material> materials)
		{
			List<MaterialManager> list = new List<MaterialManager>();
			foreach (Material material in materials)
			{
				MaterialManager materialManager = new MaterialManager();
				materialManager.SetMaterial(material);
				list.Add(materialManager);
			}
			if (this.hasSubscribed)
			{
				ShaderQuality.ShaderQualityLevel currentQualityLevel = ShaderQuality.CurrentQualityLevel;
				if (currentQualityLevel != ShaderQuality.ShaderQualityLevel.High)
				{
					if (currentQualityLevel != ShaderQuality.ShaderQualityLevel.Low)
					{
					}
					this.SwapToLowQuality(list);
				}
				else
				{
					this.SwapToHighQuality(list);
				}
			}
			this.managedMaterials.AddRange(list);
			return list;
		}

		// Token: 0x06001D9D RID: 7581 RVA: 0x00081CB0 File Offset: 0x0007FEB0
		public void UnmanageMaterials(IEnumerable<MaterialManager> managers)
		{
			this.managedMaterials = this.managedMaterials.Except(managers).ToList<MaterialManager>();
		}

		// Token: 0x06001D9E RID: 7582 RVA: 0x00081CC9 File Offset: 0x0007FEC9
		private void Start()
		{
			this.hasSubscribed = true;
			this.HandleQualityLevelChanged(ShaderQuality.CurrentQualityLevel);
			ShaderQuality.OnShaderQualityLevelChanged.AddListener(new UnityAction<ShaderQuality.ShaderQualityLevel>(this.HandleQualityLevelChanged));
			if (this.FadeOnStart)
			{
				this.FadeIn();
			}
		}

		// Token: 0x06001D9F RID: 7583 RVA: 0x00081D01 File Offset: 0x0007FF01
		private void OnDestroy()
		{
			ShaderQuality.OnShaderQualityLevelChanged.RemoveListener(new UnityAction<ShaderQuality.ShaderQualityLevel>(this.HandleQualityLevelChanged));
		}

		// Token: 0x06001DA0 RID: 7584 RVA: 0x00081D19 File Offset: 0x0007FF19
		private void HandleQualityLevelChanged(ShaderQuality.ShaderQualityLevel currentQualityLevel)
		{
			if (currentQualityLevel != ShaderQuality.ShaderQualityLevel.High)
			{
				if (currentQualityLevel != ShaderQuality.ShaderQualityLevel.Low)
				{
				}
				this.SwapToLowQuality();
				return;
			}
			this.SwapToHighQuality();
		}

		// Token: 0x06001DA1 RID: 7585 RVA: 0x00081D31 File Offset: 0x0007FF31
		[ContextMenu("SwapToHighQuality")]
		public void SwapToHighQuality()
		{
			this.SwapToHighQuality(this.managedRenderers);
			this.SwapToHighQuality(this.managedMaterials);
		}

		// Token: 0x06001DA2 RID: 7586 RVA: 0x00081D4B File Offset: 0x0007FF4B
		[ContextMenu("SwapToLowQuality")]
		public void SwapToLowQuality()
		{
			this.SwapToLowQuality(this.managedRenderers);
			this.SwapToLowQuality(this.managedMaterials);
		}

		// Token: 0x06001DA3 RID: 7587 RVA: 0x00081D68 File Offset: 0x0007FF68
		public void SwapToHighQuality(List<RendererManager> managers)
		{
			foreach (RendererManager rendererManager in managers)
			{
				rendererManager.SwapToHighQuality();
			}
		}

		// Token: 0x06001DA4 RID: 7588 RVA: 0x00081DB4 File Offset: 0x0007FFB4
		public void SwapToLowQuality(List<RendererManager> managers)
		{
			foreach (RendererManager rendererManager in managers)
			{
				rendererManager.SwapToLowQuality();
			}
		}

		// Token: 0x06001DA5 RID: 7589 RVA: 0x00081E00 File Offset: 0x00080000
		public void SwapToHighQuality(List<MaterialManager> managers)
		{
			foreach (MaterialManager materialManager in managers)
			{
				materialManager.SwapToHighQuality();
			}
		}

		// Token: 0x06001DA6 RID: 7590 RVA: 0x00081E4C File Offset: 0x0008004C
		public void SwapToLowQuality(List<MaterialManager> managers)
		{
			foreach (MaterialManager materialManager in managers)
			{
				materialManager.SwapToLowQuality();
			}
		}

		// Token: 0x06001DA7 RID: 7591 RVA: 0x00081E98 File Offset: 0x00080098
		private IEnumerator ProcessFade(bool fadeIn)
		{
			float start = (fadeIn ? 0f : 4f);
			float end = (fadeIn ? 4f : 0f);
			float num = 0f;
			for (float elapsedTime = num * 1f; elapsedTime <= 1f; elapsedTime += Time.deltaTime)
			{
				while (!base.enabled)
				{
					yield return null;
				}
				float num2 = Mathf.Clamp01(elapsedTime / 1f);
				float num3 = Mathf.Lerp(start, end, num2);
				foreach (RendererManager rendererManager in this.managedRenderers)
				{
					rendererManager.SetFadeProperty(num3);
				}
				yield return null;
			}
			if (!fadeIn)
			{
				using (List<RendererManager>.Enumerator enumerator = this.managedRenderers.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						RendererManager rendererManager2 = enumerator.Current;
						rendererManager2.SetFadeProperty(end);
					}
					yield break;
				}
			}
			using (List<RendererManager>.Enumerator enumerator = this.managedRenderers.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					RendererManager rendererManager3 = enumerator.Current;
					rendererManager3.SwapToNoFade();
				}
				yield break;
			}
			yield break;
		}

		// Token: 0x06001DA8 RID: 7592 RVA: 0x00081EB0 File Offset: 0x000800B0
		[ContextMenu("FadeIn")]
		public void FadeIn()
		{
			foreach (RendererManager rendererManager in this.managedRenderers)
			{
				rendererManager.SwapToFade();
			}
			base.StartCoroutine(this.ProcessFade(true));
		}

		// Token: 0x06001DA9 RID: 7593 RVA: 0x00081F10 File Offset: 0x00080110
		[ContextMenu("FadeOut")]
		public void FadeOut()
		{
			foreach (RendererManager rendererManager in this.managedRenderers)
			{
				rendererManager.SwapToFade();
			}
			base.StartCoroutine(this.ProcessFade(false));
		}

		// Token: 0x06001DAA RID: 7594 RVA: 0x00081F70 File Offset: 0x00080170
		public static void SwapShader(Material material, Shader correctShader, bool checkForKeywordChanges = true)
		{
			int renderQueue = material.renderQueue;
			bool enableInstancing = material.enableInstancing;
			bool doubleSidedGI = material.doubleSidedGI;
			material.EnableKeyword("_LIGHT_COOKIES");
			string[] shaderKeywords = material.shaderKeywords;
			material.shader = correctShader;
			string[] shaderKeywords2 = material.shaderKeywords;
			if (checkForKeywordChanges)
			{
				List<string> list = shaderKeywords.Except(shaderKeywords2).ToList<string>();
				List<string> list2 = shaderKeywords2.Except(shaderKeywords).ToList<string>();
				if (list.Any<string>() || list2.Any<string>())
				{
					if (list.Any<string>())
					{
						Debug.LogWarning("For material: " + material.name + " keywords in original but not in new: " + string.Join(", ", list));
					}
					if (list2.Any<string>())
					{
						Debug.LogWarning("For material: " + material.name + " keywords in new but not in original: " + string.Join(", ", list2));
					}
				}
			}
			if (renderQueue > 0)
			{
				material.renderQueue = renderQueue;
			}
			else if (material.HasProperty("_Surface") && material.GetFloat("_Surface") > 0.5f)
			{
				material.renderQueue = 3000;
			}
			else
			{
				material.renderQueue = 2000;
			}
			material.enableInstancing = enableInstancing;
			material.doubleSidedGI = doubleSidedGI;
		}

		// Token: 0x04001727 RID: 5927
		private const float FADE_DURATION = 1f;

		// Token: 0x04001728 RID: 5928
		public bool FadeOnStart = true;

		// Token: 0x04001729 RID: 5929
		[SerializeField]
		private List<RendererManager> managedRenderers = new List<RendererManager>();

		// Token: 0x0400172A RID: 5930
		[SerializeField]
		private List<MaterialManager> managedMaterials = new List<MaterialManager>();

		// Token: 0x0400172B RID: 5931
		private bool hasSubscribed;
	}
}
