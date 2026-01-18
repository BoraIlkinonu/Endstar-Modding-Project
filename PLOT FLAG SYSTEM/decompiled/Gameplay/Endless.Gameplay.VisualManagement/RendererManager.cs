using System;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay.VisualManagement;

[Serializable]
public class RendererManager
{
	private const string FADE_PROPERTY = "Cutoff_Height";

	[SerializeField]
	private Renderer renderer;

	[SerializeField]
	private string[] clusterIds;

	[SerializeField]
	private bool[] alphaFadeSource;

	private static readonly int CutoffHeight = Shader.PropertyToID("Cutoff_Height");

	private ShaderClusterManager.ShaderCluster GetCluster(int index)
	{
		if (clusterIds[index] == null)
		{
			return null;
		}
		return MonoBehaviourSingleton<ShaderClusterManager>.Instance.GetShaderCluster(clusterIds[index]);
	}

	public void SetRenderer(Renderer newRenderer)
	{
		renderer = newRenderer;
		clusterIds = new string[renderer.materials.Length];
		alphaFadeSource = new bool[renderer.materials.Length];
		for (int i = 0; i < newRenderer.materials.Length; i++)
		{
			ShaderClusterManager.ShaderCluster shaderCluster = MonoBehaviourSingleton<ShaderClusterManager>.Instance.GetShaderCluster(newRenderer.materials[i].shader);
			if (shaderCluster != null)
			{
				clusterIds[i] = shaderCluster?.DisplayId;
				if (shaderCluster.supportsFade)
				{
					alphaFadeSource[i] = renderer.materials[i].IsKeywordEnabled("_ALPHATEST_ON");
				}
			}
		}
	}

	internal void EnableRenderer()
	{
		renderer.enabled = true;
	}

	internal void DisableRenderer()
	{
		renderer.enabled = false;
	}

	internal void SetFadeProperty(float value)
	{
		for (int i = 0; i < renderer.materials.Length; i++)
		{
			renderer.materials[i].SetFloat(CutoffHeight, value);
		}
	}

	internal void SwapToNoFade()
	{
		for (int i = 0; i < renderer.materials.Length; i++)
		{
			ShaderClusterManager.ShaderCluster cluster = GetCluster(i);
			if (cluster != null)
			{
				if (renderer.materials[i].shader == cluster.fadeShader)
				{
					EndlessVisuals.SwapShader(renderer.materials[i], cluster.primaryShader, checkForKeywordChanges: false);
				}
				else if (renderer.materials[i].shader == cluster.fadeLowShader)
				{
					EndlessVisuals.SwapShader(renderer.materials[i], cluster.lowShader, checkForKeywordChanges: false);
				}
				if (cluster.supportsFade && !alphaFadeSource[i])
				{
					renderer.materials[i].DisableKeyword("_ALPHATEST_ON");
				}
			}
		}
	}

	internal void SwapToFade()
	{
		for (int i = 0; i < renderer.materials.Length; i++)
		{
			ShaderClusterManager.ShaderCluster cluster = GetCluster(i);
			if (cluster != null)
			{
				if (renderer.materials[i].shader == cluster.primaryShader)
				{
					EndlessVisuals.SwapShader(renderer.materials[i], cluster.fadeShader, checkForKeywordChanges: false);
				}
				else if (renderer.materials[i].shader == cluster.lowShader)
				{
					EndlessVisuals.SwapShader(renderer.materials[i], cluster.fadeLowShader, checkForKeywordChanges: false);
				}
				if (cluster.supportsFade)
				{
					renderer.materials[i].EnableKeyword("_ALPHATEST_ON");
				}
			}
		}
	}

	[ContextMenu("SwapToLowQuality")]
	public void SwapToLowQuality()
	{
		for (int i = 0; i < renderer.materials.Length; i++)
		{
			ShaderClusterManager.ShaderCluster cluster = GetCluster(i);
			if (cluster != null)
			{
				if (renderer.materials[i].shader == cluster.primaryShader)
				{
					EndlessVisuals.SwapShader(renderer.materials[i], cluster.lowShader, checkForKeywordChanges: false);
				}
				else if (renderer.materials[i].shader == cluster.fadeShader)
				{
					EndlessVisuals.SwapShader(renderer.materials[i], cluster.fadeLowShader, checkForKeywordChanges: false);
				}
			}
		}
	}

	[ContextMenu("SwapToHighQuality")]
	public void SwapToHighQuality()
	{
		for (int i = 0; i < renderer.materials.Length; i++)
		{
			ShaderClusterManager.ShaderCluster cluster = GetCluster(i);
			if (cluster != null)
			{
				if (renderer.materials[i].shader == cluster.lowShader)
				{
					EndlessVisuals.SwapShader(renderer.materials[i], cluster.primaryShader, checkForKeywordChanges: false);
				}
				else if (renderer.materials[i].shader == cluster.fadeLowShader)
				{
					EndlessVisuals.SwapShader(renderer.materials[i], cluster.fadeShader, checkForKeywordChanges: false);
				}
			}
		}
	}
}
