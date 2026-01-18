using System;
using System.Collections.Generic;
using Endless.Shared;
using UnityEngine;
using UnityEngine.Serialization;

namespace Endless.Gameplay.VisualManagement;

public class ShaderClusterManager : MonoBehaviourSingleton<ShaderClusterManager>
{
	[Serializable]
	public class ShaderCluster
	{
		[FormerlySerializedAs("DisplayName")]
		public string DisplayId = "Unknown";

		public Shader primaryShader;

		public Shader fadeShader;

		public Shader lowShader;

		public Shader fadeLowShader;

		public bool supportsFade;
	}

	[FormerlySerializedAs("shaderCluserList")]
	[SerializeField]
	private List<ShaderCluster> shaderClusterList = new List<ShaderCluster>();

	private Dictionary<Shader, ShaderCluster> shaderMap = new Dictionary<Shader, ShaderCluster>();

	private Dictionary<string, ShaderCluster> shaderNameMap = new Dictionary<string, ShaderCluster>();

	protected override void Awake()
	{
		base.Awake();
		BuildShaderMaps();
	}

	private void BuildShaderMaps()
	{
		foreach (ShaderCluster shaderCluster in shaderClusterList)
		{
			shaderMap.Add(shaderCluster.primaryShader, shaderCluster);
			shaderNameMap.Add(shaderCluster.DisplayId, shaderCluster);
		}
	}

	public ShaderCluster GetShaderCluster(Shader shader)
	{
		return shaderMap.GetValueOrDefault(shader);
	}

	public ShaderCluster GetShaderCluster(string clusterName)
	{
		return shaderNameMap.GetValueOrDefault(clusterName);
	}
}
