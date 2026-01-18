using System;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay.VisualManagement;

[Serializable]
public class MaterialManager
{
	[SerializeField]
	private Material material;

	[SerializeField]
	private string clusterId;

	private ShaderClusterManager.ShaderCluster GetCluster()
	{
		if (clusterId == null)
		{
			return null;
		}
		return MonoBehaviourSingleton<ShaderClusterManager>.Instance.GetShaderCluster(clusterId);
	}

	public void SetMaterial(Material material)
	{
		this.material = material;
		clusterId = MonoBehaviourSingleton<ShaderClusterManager>.Instance.GetShaderCluster(material.shader)?.DisplayId;
	}

	[ContextMenu("SwapToLowQuality")]
	public void SwapToLowQuality()
	{
		ShaderClusterManager.ShaderCluster cluster = GetCluster();
		if (cluster != null)
		{
			if (material.shader == cluster.primaryShader)
			{
				material.shader = cluster.lowShader;
			}
			else if (material.shader == cluster.fadeShader)
			{
				material.shader = cluster.fadeLowShader;
			}
		}
	}

	[ContextMenu("SwapToHighQuality")]
	public void SwapToHighQuality()
	{
		ShaderClusterManager.ShaderCluster cluster = GetCluster();
		if (cluster != null)
		{
			if (material.shader == cluster.lowShader)
			{
				material.shader = cluster.primaryShader;
			}
			else if (material.shader == cluster.fadeLowShader)
			{
				material.shader = cluster.fadeShader;
			}
		}
	}
}
