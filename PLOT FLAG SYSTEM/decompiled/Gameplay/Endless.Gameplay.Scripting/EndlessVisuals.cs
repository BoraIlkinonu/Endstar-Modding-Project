using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.VisualManagement;
using Endless.Shared.EndlessQualitySettings;
using UnityEngine;

namespace Endless.Gameplay.Scripting;

public class EndlessVisuals : MonoBehaviour
{
	private const float FADE_DURATION = 1f;

	public bool FadeOnStart = true;

	[SerializeField]
	private List<RendererManager> managedRenderers = new List<RendererManager>();

	[SerializeField]
	private List<MaterialManager> managedMaterials = new List<MaterialManager>();

	private bool hasSubscribed;

	public void FindAndManageChildRenderers(GameObject prefabRoot)
	{
		ManageRenderers(prefabRoot.GetComponentsInChildren<Renderer>());
	}

	public void FindAndManageChildRenderers(Transform prefabRoot)
	{
		ManageRenderers(prefabRoot.GetComponentsInChildren<Renderer>());
	}

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
		if (hasSubscribed)
		{
			switch (ShaderQuality.CurrentQualityLevel)
			{
			case ShaderQuality.ShaderQualityLevel.High:
				SwapToHighQuality(list);
				break;
			default:
				SwapToLowQuality(list);
				break;
			}
		}
		managedRenderers.AddRange(list);
		return list;
	}

	public void UnmanageRenderers(IEnumerable<RendererManager> managers)
	{
		managedRenderers = managedRenderers.Except(managers).ToList();
	}

	public List<MaterialManager> ManageMaterials(IEnumerable<Material> materials)
	{
		List<MaterialManager> list = new List<MaterialManager>();
		foreach (Material material in materials)
		{
			MaterialManager materialManager = new MaterialManager();
			materialManager.SetMaterial(material);
			list.Add(materialManager);
		}
		if (hasSubscribed)
		{
			switch (ShaderQuality.CurrentQualityLevel)
			{
			case ShaderQuality.ShaderQualityLevel.High:
				SwapToHighQuality(list);
				break;
			default:
				SwapToLowQuality(list);
				break;
			}
		}
		managedMaterials.AddRange(list);
		return list;
	}

	public void UnmanageMaterials(IEnumerable<MaterialManager> managers)
	{
		managedMaterials = managedMaterials.Except(managers).ToList();
	}

	private void Start()
	{
		hasSubscribed = true;
		HandleQualityLevelChanged(ShaderQuality.CurrentQualityLevel);
		ShaderQuality.OnShaderQualityLevelChanged.AddListener(HandleQualityLevelChanged);
		if (FadeOnStart)
		{
			FadeIn();
		}
	}

	private void OnDestroy()
	{
		ShaderQuality.OnShaderQualityLevelChanged.RemoveListener(HandleQualityLevelChanged);
	}

	private void HandleQualityLevelChanged(ShaderQuality.ShaderQualityLevel currentQualityLevel)
	{
		switch (currentQualityLevel)
		{
		case ShaderQuality.ShaderQualityLevel.High:
			SwapToHighQuality();
			break;
		default:
			SwapToLowQuality();
			break;
		}
	}

	[ContextMenu("SwapToHighQuality")]
	public void SwapToHighQuality()
	{
		SwapToHighQuality(managedRenderers);
		SwapToHighQuality(managedMaterials);
	}

	[ContextMenu("SwapToLowQuality")]
	public void SwapToLowQuality()
	{
		SwapToLowQuality(managedRenderers);
		SwapToLowQuality(managedMaterials);
	}

	public void SwapToHighQuality(List<RendererManager> managers)
	{
		foreach (RendererManager manager in managers)
		{
			manager.SwapToHighQuality();
		}
	}

	public void SwapToLowQuality(List<RendererManager> managers)
	{
		foreach (RendererManager manager in managers)
		{
			manager.SwapToLowQuality();
		}
	}

	public void SwapToHighQuality(List<MaterialManager> managers)
	{
		foreach (MaterialManager manager in managers)
		{
			manager.SwapToHighQuality();
		}
	}

	public void SwapToLowQuality(List<MaterialManager> managers)
	{
		foreach (MaterialManager manager in managers)
		{
			manager.SwapToLowQuality();
		}
	}

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
			float t = Mathf.Clamp01(elapsedTime / 1f);
			float fadeProperty = Mathf.Lerp(start, end, t);
			foreach (RendererManager managedRenderer in managedRenderers)
			{
				managedRenderer.SetFadeProperty(fadeProperty);
			}
			yield return null;
		}
		if (!fadeIn)
		{
			foreach (RendererManager managedRenderer2 in managedRenderers)
			{
				managedRenderer2.SetFadeProperty(end);
			}
			yield break;
		}
		foreach (RendererManager managedRenderer3 in managedRenderers)
		{
			managedRenderer3.SwapToNoFade();
		}
	}

	[ContextMenu("FadeIn")]
	public void FadeIn()
	{
		foreach (RendererManager managedRenderer in managedRenderers)
		{
			managedRenderer.SwapToFade();
		}
		StartCoroutine(ProcessFade(fadeIn: true));
	}

	[ContextMenu("FadeOut")]
	public void FadeOut()
	{
		foreach (RendererManager managedRenderer in managedRenderers)
		{
			managedRenderer.SwapToFade();
		}
		StartCoroutine(ProcessFade(fadeIn: false));
	}

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
			List<string> list = shaderKeywords.Except(shaderKeywords2).ToList();
			List<string> list2 = shaderKeywords2.Except(shaderKeywords).ToList();
			if (list.Any() || list2.Any())
			{
				if (list.Any())
				{
					Debug.LogWarning("For material: " + material.name + " keywords in original but not in new: " + string.Join(", ", list));
				}
				if (list2.Any())
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
}
