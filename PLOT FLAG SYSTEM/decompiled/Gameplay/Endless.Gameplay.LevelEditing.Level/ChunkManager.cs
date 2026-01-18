using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace Endless.Gameplay.LevelEditing.Level;

public class ChunkManager : MonoBehaviour
{
	public class MergeInfo
	{
		public List<Material> Materials;

		public List<List<CombineInstance>> CombineInstances;

		public GameObject MergedMesh;

		public MergeInfo(ShadowCastingMode shadowCastingMode, string namePrefix, Transform parent = null)
		{
			Materials = new List<Material>();
			CombineInstances = new List<List<CombineInstance>>();
			MergedMesh = new GameObject(namePrefix + " - ShadowCastingMode." + shadowCastingMode);
			if (parent != null)
			{
				MergedMesh.transform.SetParent(parent);
			}
		}
	}

	public class Chunk
	{
		public bool IsDirty { get; private set; }

		public float LastModifiedTime { get; private set; }

		public List<Renderer> MergedGameObjects { get; private set; } = new List<Renderer>();

		public List<GameObject> UnmergedGameObjects { get; private set; } = new List<GameObject>();

		public string ChunkName { get; private set; } = "Chunk";

		public Vector3Int ChunkPosition { get; private set; }

		public Chunk(Vector3Int chunkPosition)
		{
			ChunkName = $"Chunk ({chunkPosition.x}, {chunkPosition.y}, {chunkPosition.z})";
			ChunkPosition = chunkPosition;
		}

		internal void AddCell(GameObject cellRoot)
		{
			UnmergedGameObjects.Add(cellRoot);
			MarkAsDirty();
		}

		internal void RemoveCell(GameObject cellRoot)
		{
			if (UnmergedGameObjects.Contains(cellRoot))
			{
				UnmergedGameObjects.Remove(cellRoot);
				MarkAsDirty();
			}
		}

		private void MarkAsDirty()
		{
			LastModifiedTime = Time.time;
			if (!IsDirty)
			{
				RemoveMergedVisuals();
				ToggleRenderers(enabled: true);
				IsDirty = true;
			}
		}

		internal void ForceModifiedTime(float time)
		{
			LastModifiedTime = time;
		}

		internal void ToggleRenderers(bool enabled)
		{
			new ProfilerMarker("Chunk Toggle Visuals Marker");
			foreach (GameObject unmergedGameObject in UnmergedGameObjects)
			{
				Renderer[] componentsInChildren = unmergedGameObject.GetComponentsInChildren<Renderer>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].enabled = enabled;
				}
			}
		}

		internal void CompleteMerge()
		{
			IsDirty = false;
			ToggleRenderers(enabled: false);
		}

		internal void ForceMarkDirty()
		{
			MarkAsDirty();
		}

		internal void RemoveMergedVisuals()
		{
			foreach (Renderer mergedGameObject in MergedGameObjects)
			{
				Object.Destroy(mergedGameObject.gameObject);
			}
			MergedGameObjects.Clear();
		}
	}

	private const int LOAD_LIMIT_MS = 32;

	private const int RUNTIME_LIMIT_MS = 4;

	private int currentMillisecondLimit = 4;

	private const float CHUNK_SIZE = 16f;

	[SerializeField]
	private Transform mergeRoot;

	[SerializeField]
	private Renderer chunkVisualizerPrefab;

	[SerializeField]
	private Material mergedVisualizationMaterial;

	[SerializeField]
	private Material mergingVisualizationMaterial;

	[SerializeField]
	private Material dirtyVisualizationMaterial;

	[SerializeField]
	private bool debug;

	[HideInInspector]
	public UnityEvent OnMergingComplete = new UnityEvent();

	[SerializeField]
	private bool visualizationEnabled;

	private Dictionary<Vector3Int, Chunk> chunkMap = new Dictionary<Vector3Int, Chunk>();

	private Dictionary<Vector3Int, Renderer> visualsMap = new Dictionary<Vector3Int, Renderer>();

	private float mergeDelay = 3f;

	private Coroutine currentMergeRoutine;

	private Chunk currentMergingChunk;

	private ProfilerMarker mergeMarker1 = new ProfilerMarker("Chunk Merge Marker 1");

	private ProfilerMarker mergeMarker2 = new ProfilerMarker("Chunk Merge Marker 2");

	private ProfilerMarker mergeMarker3 = new ProfilerMarker("Chunk Merge Marker 3");

	public bool HasDirtyChunks => chunkMap.Values.Any((Chunk chunk) => chunk.IsDirty);

	public bool HasChunksAwaitingBuild => chunkMap.Values.Any((Chunk chunk) => chunk.IsDirty && Time.time >= chunk.LastModifiedTime + mergeDelay);

	public int AwaitingChunkCount => chunkMap.Values.Count((Chunk chunk) => chunk.IsDirty && Time.time >= chunk.LastModifiedTime + mergeDelay);

	private void HandleDebugChucksUpdated()
	{
		if (visualizationEnabled)
		{
			foreach (Vector3Int key in chunkMap.Keys)
			{
				if (!visualsMap.ContainsKey(key))
				{
					AddChunkVisualizer(key);
				}
				if (currentMergingChunk != null && currentMergingChunk.ChunkPosition == key)
				{
					visualsMap[key].material = mergingVisualizationMaterial;
				}
				else
				{
					visualsMap[key].material = (chunkMap[key].IsDirty ? dirtyVisualizationMaterial : mergedVisualizationMaterial);
				}
				visualsMap[key].enabled = true;
			}
			return;
		}
		foreach (Renderer value in visualsMap.Values)
		{
			value.enabled = false;
		}
	}

	private void AddChunkVisualizer(Vector3Int chunkPosition)
	{
		Renderer renderer = Object.Instantiate(chunkVisualizerPrefab, (Vector3)chunkPosition * 16f + Vector3.one * 16f / 2f - Vector3.one / 2f, Quaternion.identity);
		renderer.transform.localScale = Vector3.one * 16f;
		renderer.gameObject.name = $"Chunk visualizer ({chunkPosition.x}, {chunkPosition.y}, {chunkPosition.z})";
		visualsMap.Add(chunkPosition, renderer);
	}

	private Chunk GetChunkForModification(Vector3Int position)
	{
		Vector3Int chunkPosition = GetChunkPosition(position);
		if (!chunkMap.ContainsKey(chunkPosition))
		{
			chunkMap.Add(chunkPosition, new Chunk(chunkPosition));
			if (visualizationEnabled)
			{
				AddChunkVisualizer(chunkPosition);
			}
		}
		if (currentMergeRoutine != null && currentMergingChunk == chunkMap[chunkPosition])
		{
			AbandonCurrentMerge();
		}
		return chunkMap[chunkPosition];
	}

	public static Vector3Int GetChunkPosition(Vector3Int position)
	{
		return new Vector3Int(Mathf.FloorToInt((float)position.x / 16f), Mathf.FloorToInt((float)position.y / 16f), Mathf.FloorToInt((float)position.z / 16f));
	}

	private void AbandonCurrentMerge()
	{
		if (debug)
		{
			UnityEngine.Debug.Log("Abandonded merging for chunk: " + currentMergingChunk.ChunkName);
		}
		currentMergingChunk.RemoveMergedVisuals();
		StopCoroutine(currentMergeRoutine);
		currentMergingChunk = null;
		currentMergeRoutine = null;
	}

	public void CellUpdated(Vector3Int position)
	{
		Chunk chunkForModification = GetChunkForModification(position);
		if (visualizationEnabled && visualsMap.ContainsKey(chunkForModification.ChunkPosition))
		{
			visualsMap[chunkForModification.ChunkPosition].material = dirtyVisualizationMaterial;
		}
		chunkForModification.ForceMarkDirty();
	}

	public void CellAdded(Vector3Int position, GameObject cellRoot)
	{
		Chunk chunkForModification = GetChunkForModification(position);
		if (visualizationEnabled && visualsMap.ContainsKey(chunkForModification.ChunkPosition))
		{
			visualsMap[chunkForModification.ChunkPosition].material = dirtyVisualizationMaterial;
		}
		chunkForModification.AddCell(cellRoot);
	}

	public void CellRemoved(Vector3Int position, GameObject cellRoot)
	{
		Chunk chunkForModification = GetChunkForModification(position);
		if (visualizationEnabled && visualsMap.ContainsKey(chunkForModification.ChunkPosition))
		{
			visualsMap[chunkForModification.ChunkPosition].material = dirtyVisualizationMaterial;
		}
		chunkForModification.RemoveCell(cellRoot);
	}

	private void Update()
	{
		if (currentMergeRoutine != null)
		{
			return;
		}
		float num = float.PositiveInfinity;
		Chunk chunk = null;
		foreach (Chunk value in chunkMap.Values)
		{
			if (value.IsDirty && value.LastModifiedTime < num)
			{
				chunk = value;
				num = value.LastModifiedTime;
			}
		}
		if (chunk != null && Time.time >= chunk.LastModifiedTime + mergeDelay)
		{
			StartMerge(chunk);
		}
	}

	private void StartMerge(Chunk oldestChunk)
	{
		currentMergingChunk = oldestChunk;
		currentMergeRoutine = StartCoroutine(MergeChunk(oldestChunk));
	}

	private void OnDisable()
	{
		if (currentMergeRoutine != null)
		{
			AbandonCurrentMerge();
		}
	}

	private IEnumerator MergeChunk(Chunk chunk)
	{
		if (debug)
		{
			UnityEngine.Debug.Log("Started merging for chunk: " + chunk.ChunkName);
		}
		if (chunk.UnmergedGameObjects.Count == 0)
		{
			CompleteMerge(chunk);
			yield break;
		}
		Stopwatch totalMergeStopwatch = new Stopwatch();
		totalMergeStopwatch.Start();
		if (visualizationEnabled && visualsMap.ContainsKey(chunk.ChunkPosition))
		{
			visualsMap[chunk.ChunkPosition].material = mergingVisualizationMaterial;
		}
		Dictionary<ShadowCastingMode, MergeInfo> mergeMap = new Dictionary<ShadowCastingMode, MergeInfo>();
		Stopwatch frameStopwatch = new Stopwatch();
		frameStopwatch.Start();
		for (int index = 0; index < chunk.UnmergedGameObjects.Count; index++)
		{
			Transform transform = chunk.UnmergedGameObjects[index].transform;
			if (GetMergeInfo(chunk.ChunkName, mergeMap, transform, mergeRoot) && frameStopwatch.ElapsedMilliseconds > currentMillisecondLimit)
			{
				yield return null;
				frameStopwatch.Restart();
			}
		}
		foreach (ShadowCastingMode castingMode in mergeMap.Keys)
		{
			MergeInfo mergeInfo = mergeMap[castingMode];
			CombineInstance[] finalCombine = new CombineInstance[mergeInfo.Materials.Count];
			CombineInstance[] array2;
			for (int index = 0; index < mergeInfo.Materials.Count; index++)
			{
				finalCombine[index].mesh = new Mesh();
				CombineInstance[] array = mergeInfo.CombineInstances[index].ToArray();
				int num = 0;
				array2 = array;
				foreach (CombineInstance combineInstance in array2)
				{
					num += combineInstance.mesh.vertices.Length;
				}
				if (num > 65535)
				{
					finalCombine[index].mesh.indexFormat = IndexFormat.UInt32;
				}
				finalCombine[index].mesh.CombineMeshes(array, mergeSubMeshes: true, useMatrices: true);
				finalCombine[index].subMeshIndex = 0;
				if (frameStopwatch.ElapsedMilliseconds > currentMillisecondLimit)
				{
					yield return null;
					frameStopwatch.Restart();
				}
			}
			MeshFilter meshFilter = mergeInfo.MergedMesh.AddComponent<MeshFilter>();
			meshFilter.sharedMesh = new Mesh();
			int num2 = 0;
			array2 = finalCombine;
			foreach (CombineInstance combineInstance2 in array2)
			{
				num2 += combineInstance2.mesh.vertices.Length;
			}
			if (num2 > 65535)
			{
				meshFilter.sharedMesh.indexFormat = IndexFormat.UInt32;
			}
			meshFilter.sharedMesh.CombineMeshes(finalCombine, mergeSubMeshes: false, useMatrices: false);
			Renderer renderer = mergeInfo.MergedMesh.AddComponent<MeshRenderer>();
			renderer.shadowCastingMode = castingMode;
			renderer.sharedMaterials = mergeInfo.Materials.ToArray();
			chunk.MergedGameObjects.Add(renderer);
			if (frameStopwatch.ElapsedMilliseconds > currentMillisecondLimit)
			{
				yield return null;
				frameStopwatch.Restart();
			}
		}
		totalMergeStopwatch.Stop();
		yield return null;
		CompleteMerge(chunk);
	}

	private void CompleteMerge(Chunk chunk)
	{
		if (debug)
		{
			UnityEngine.Debug.Log("Finished merging for chunk: " + chunk.ChunkName);
		}
		chunk.CompleteMerge();
		if (visualizationEnabled && visualsMap.ContainsKey(chunk.ChunkPosition))
		{
			visualsMap[chunk.ChunkPosition].material = mergedVisualizationMaterial;
		}
		currentMergeRoutine = null;
		currentMergingChunk = null;
		if (!HasDirtyChunks)
		{
			if (debug)
			{
				UnityEngine.Debug.Log("Finished merging chunks!");
			}
			OnMergingComplete.Invoke();
		}
	}

	private static bool GetMergeInfo(string namePrefix, Dictionary<ShadowCastingMode, MergeInfo> mergeMap, Transform transform, Transform chunkParent = null)
	{
		bool flag = ProcessRenderer(namePrefix, mergeMap, transform, chunkParent);
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			bool mergeInfo = GetMergeInfo(namePrefix, mergeMap, transform.GetChild(i), chunkParent);
			flag = flag || mergeInfo;
		}
		return flag;
	}

	private static bool ProcessRenderer(string namePrefix, Dictionary<ShadowCastingMode, MergeInfo> mergeMap, Transform currentTransform, Transform chunkParent = null)
	{
		MeshFilter component = currentTransform.GetComponent<MeshFilter>();
		if (component == null)
		{
			return false;
		}
		Renderer component2 = currentTransform.GetComponent<Renderer>();
		if (component2 == null)
		{
			return false;
		}
		ShadowCastingMode shadowCastingMode = component2.shadowCastingMode;
		if (!mergeMap.ContainsKey(shadowCastingMode))
		{
			mergeMap.Add(shadowCastingMode, new MergeInfo(shadowCastingMode, namePrefix, chunkParent));
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
		return true;
	}

	internal void MarkChunkAsReadyToMerge(Vector3Int chunkCoords)
	{
		if (chunkMap.ContainsKey(chunkCoords))
		{
			Chunk chunk = chunkMap[chunkCoords];
			if (currentMergingChunk == null)
			{
				StartMerge(chunk);
			}
			else
			{
				chunk.ForceModifiedTime(Time.time - mergeDelay * 2f);
			}
		}
	}

	public void SetCollectionModeToRuntime()
	{
		currentMillisecondLimit = 4;
	}

	public void SetCollectionModeToLoadTime()
	{
		currentMillisecondLimit = 32;
	}
}
