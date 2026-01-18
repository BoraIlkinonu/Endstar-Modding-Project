using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using UnityEngine;
using UnityEngine.Rendering;

namespace Endless.Gameplay.LevelEditing;

public class MeshCombiner : MonoBehaviour
{
	public static List<GameObject> MergeStage()
	{
		_ = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage;
		IReadOnlyCollection<Cell> cells = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.Cells;
		Dictionary<Vector3Int, Cell> dictionary = new Dictionary<Vector3Int, Cell>();
		Vector3Int vector3Int = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
		Vector3Int vector3Int2 = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
		for (int i = 0; i < cells.Count; i++)
		{
			Cell cell = cells.ElementAt(i);
			Vector3Int coordinate = cell.Coordinate;
			dictionary.Add(coordinate, cell);
			vector3Int.x = Mathf.Min(vector3Int.x, coordinate.x);
			vector3Int.y = Mathf.Min(vector3Int.y, coordinate.y);
			vector3Int.z = Mathf.Min(vector3Int.z, coordinate.z);
			vector3Int2.x = Mathf.Max(vector3Int2.x, coordinate.x);
			vector3Int2.y = Mathf.Max(vector3Int2.y, coordinate.y);
			vector3Int2.z = Mathf.Max(vector3Int2.z, coordinate.z);
		}
		Vector3Int vector3Int3 = (vector3Int2 - vector3Int) / 16 + Vector3Int.one;
		List<GameObject>[,,] array = new List<GameObject>[vector3Int3.x, vector3Int3.y, vector3Int3.z];
		for (int j = 0; j < vector3Int3.x; j++)
		{
			for (int k = 0; k < vector3Int3.y; k++)
			{
				for (int l = 0; l < vector3Int3.z; l++)
				{
					array[j, k, l] = new List<GameObject>();
					Vector3Int vector3Int4 = vector3Int + new Vector3Int(j, k, l) * 16;
					for (int m = vector3Int4.x; m < vector3Int4.x + 16; m++)
					{
						for (int n = vector3Int4.y; n < vector3Int4.y + 16; n++)
						{
							for (int num = vector3Int4.z; num < vector3Int4.z + 16; num++)
							{
								dictionary.TryGetValue(new Vector3Int(m, n, num), out var value);
								if (value != null && (bool)value.CellBase)
								{
									array[j, k, l].Add(value.CellBase.gameObject);
								}
							}
						}
					}
				}
			}
		}
		List<GameObject> list = new List<GameObject>();
		for (int num2 = 0; num2 < vector3Int3.x; num2++)
		{
			for (int num3 = 0; num3 < vector3Int3.y; num3++)
			{
				for (int num4 = 0; num4 < vector3Int3.z; num4++)
				{
					list.AddRange(MergeGameObjects(array[num2, num3, num4].ToArray(), $"Chunk ({num2}, {num3}, {num4})"));
				}
			}
		}
		return list;
	}

	public static List<GameObject> MergeGameObjects(GameObject[] groupToMerge, string namePrefix = "Merged Mesh")
	{
		if (groupToMerge.Length == 0)
		{
			return new List<GameObject>();
		}
		Dictionary<ShadowCastingMode, MergeInfo> dictionary = new Dictionary<ShadowCastingMode, MergeInfo>();
		for (int i = 0; i < groupToMerge.Length; i++)
		{
			Transform transform = groupToMerge[i].transform;
			GetMergeInfo(namePrefix, dictionary, transform);
		}
		List<GameObject> list = new List<GameObject>();
		foreach (ShadowCastingMode key in dictionary.Keys)
		{
			MergeInfo mergeInfo = dictionary[key];
			CombineInstance[] array = new CombineInstance[mergeInfo.Materials.Count];
			for (int j = 0; j < mergeInfo.Materials.Count; j++)
			{
				array[j].mesh = new Mesh();
				CombineInstance[] array2 = mergeInfo.CombineInstances[j].ToArray();
				int num = 0;
				CombineInstance[] array3 = array2;
				foreach (CombineInstance combineInstance in array3)
				{
					num += combineInstance.mesh.vertices.Length;
				}
				if (num > 65535)
				{
					array[j].mesh.indexFormat = IndexFormat.UInt32;
				}
				array[j].mesh.CombineMeshes(array2, mergeSubMeshes: true, useMatrices: true);
				array[j].subMeshIndex = 0;
			}
			MeshFilter meshFilter = mergeInfo.MergedMesh.AddComponent<MeshFilter>();
			meshFilter.sharedMesh = new Mesh();
			meshFilter.sharedMesh.CombineMeshes(array, mergeSubMeshes: false, useMatrices: false);
			MeshRenderer meshRenderer = mergeInfo.MergedMesh.AddComponent<MeshRenderer>();
			meshRenderer.shadowCastingMode = key;
			meshRenderer.sharedMaterials = mergeInfo.Materials.ToArray();
			list.Add(mergeInfo.MergedMesh);
		}
		return list;
	}

	private static void GetMergeInfo(string namePrefix, Dictionary<ShadowCastingMode, MergeInfo> mergeMap, Transform transform)
	{
		ProcessRenderer(namePrefix, mergeMap, transform);
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			GetMergeInfo(namePrefix, mergeMap, transform.GetChild(i));
		}
	}

	private static void ProcessRenderer(string namePrefix, Dictionary<ShadowCastingMode, MergeInfo> mergeMap, Transform currentTransform)
	{
		MeshFilter component = currentTransform.GetComponent<MeshFilter>();
		if (component == null)
		{
			return;
		}
		Renderer component2 = currentTransform.GetComponent<Renderer>();
		if (component2 == null)
		{
			return;
		}
		ShadowCastingMode shadowCastingMode = component2.shadowCastingMode;
		if (!mergeMap.ContainsKey(shadowCastingMode))
		{
			mergeMap.Add(shadowCastingMode, new MergeInfo(shadowCastingMode, namePrefix));
		}
		MergeInfo mergeInfo = mergeMap[shadowCastingMode];
		for (int i = 0; i < component2.sharedMaterials.Length; i++)
		{
			int num = mergeInfo.Materials.IndexOf(component2.sharedMaterials[i]);
			if (num == -1)
			{
				mergeInfo.Materials.Add(component2.sharedMaterials[i]);
				mergeInfo.CombineInstances.Add(new List<CombineInstance>());
				num = mergeInfo.Materials.Count - 1;
			}
			CombineInstance item = new CombineInstance
			{
				transform = currentTransform.localToWorldMatrix,
				mesh = component.sharedMesh,
				subMeshIndex = i
			};
			mergeInfo.CombineInstances[num].Add(item);
		}
		component2.enabled = false;
	}
}
