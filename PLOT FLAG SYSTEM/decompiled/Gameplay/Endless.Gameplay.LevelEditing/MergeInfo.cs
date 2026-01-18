using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Endless.Gameplay.LevelEditing;

public class MergeInfo
{
	public List<Material> Materials;

	public List<List<CombineInstance>> CombineInstances;

	public GameObject MergedMesh;

	public MergeInfo(ShadowCastingMode shadowCastingMode, string namePrefix)
	{
		Materials = new List<Material>();
		CombineInstances = new List<List<CombineInstance>>();
		MergedMesh = new GameObject(namePrefix + " - ShadowCastingMode." + shadowCastingMode);
	}
}
