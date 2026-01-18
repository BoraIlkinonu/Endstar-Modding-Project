using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x0200053A RID: 1338
	public class ChunkManager : MonoBehaviour
	{
		// Token: 0x1700062B RID: 1579
		// (get) Token: 0x0600202F RID: 8239 RVA: 0x0009103E File Offset: 0x0008F23E
		public bool HasDirtyChunks
		{
			get
			{
				return this.chunkMap.Values.Any((ChunkManager.Chunk chunk) => chunk.IsDirty);
			}
		}

		// Token: 0x1700062C RID: 1580
		// (get) Token: 0x06002030 RID: 8240 RVA: 0x0009106F File Offset: 0x0008F26F
		public bool HasChunksAwaitingBuild
		{
			get
			{
				return this.chunkMap.Values.Any((ChunkManager.Chunk chunk) => chunk.IsDirty && Time.time >= chunk.LastModifiedTime + this.mergeDelay);
			}
		}

		// Token: 0x1700062D RID: 1581
		// (get) Token: 0x06002031 RID: 8241 RVA: 0x0009108D File Offset: 0x0008F28D
		public int AwaitingChunkCount
		{
			get
			{
				return this.chunkMap.Values.Count((ChunkManager.Chunk chunk) => chunk.IsDirty && Time.time >= chunk.LastModifiedTime + this.mergeDelay);
			}
		}

		// Token: 0x06002032 RID: 8242 RVA: 0x000910AC File Offset: 0x0008F2AC
		private void HandleDebugChucksUpdated()
		{
			if (this.visualizationEnabled)
			{
				using (Dictionary<Vector3Int, ChunkManager.Chunk>.KeyCollection.Enumerator enumerator = this.chunkMap.Keys.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Vector3Int vector3Int = enumerator.Current;
						if (!this.visualsMap.ContainsKey(vector3Int))
						{
							this.AddChunkVisualizer(vector3Int);
						}
						if (this.currentMergingChunk != null && this.currentMergingChunk.ChunkPosition == vector3Int)
						{
							this.visualsMap[vector3Int].material = this.mergingVisualizationMaterial;
						}
						else
						{
							this.visualsMap[vector3Int].material = (this.chunkMap[vector3Int].IsDirty ? this.dirtyVisualizationMaterial : this.mergedVisualizationMaterial);
						}
						this.visualsMap[vector3Int].enabled = true;
					}
					return;
				}
			}
			foreach (Renderer renderer in this.visualsMap.Values)
			{
				renderer.enabled = false;
			}
		}

		// Token: 0x06002033 RID: 8243 RVA: 0x000911E0 File Offset: 0x0008F3E0
		private void AddChunkVisualizer(Vector3Int chunkPosition)
		{
			Renderer renderer = global::UnityEngine.Object.Instantiate<Renderer>(this.chunkVisualizerPrefab, chunkPosition * 16f + Vector3.one * 16f / 2f - Vector3.one / 2f, Quaternion.identity);
			renderer.transform.localScale = Vector3.one * 16f;
			renderer.gameObject.name = string.Format("Chunk visualizer ({0}, {1}, {2})", chunkPosition.x, chunkPosition.y, chunkPosition.z);
			this.visualsMap.Add(chunkPosition, renderer);
		}

		// Token: 0x06002034 RID: 8244 RVA: 0x000912A0 File Offset: 0x0008F4A0
		private ChunkManager.Chunk GetChunkForModification(Vector3Int position)
		{
			Vector3Int chunkPosition = ChunkManager.GetChunkPosition(position);
			if (!this.chunkMap.ContainsKey(chunkPosition))
			{
				this.chunkMap.Add(chunkPosition, new ChunkManager.Chunk(chunkPosition));
				if (this.visualizationEnabled)
				{
					this.AddChunkVisualizer(chunkPosition);
				}
			}
			if (this.currentMergeRoutine != null && this.currentMergingChunk == this.chunkMap[chunkPosition])
			{
				this.AbandonCurrentMerge();
			}
			return this.chunkMap[chunkPosition];
		}

		// Token: 0x06002035 RID: 8245 RVA: 0x00091311 File Offset: 0x0008F511
		public static Vector3Int GetChunkPosition(Vector3Int position)
		{
			return new Vector3Int(Mathf.FloorToInt((float)position.x / 16f), Mathf.FloorToInt((float)position.y / 16f), Mathf.FloorToInt((float)position.z / 16f));
		}

		// Token: 0x06002036 RID: 8246 RVA: 0x00091354 File Offset: 0x0008F554
		private void AbandonCurrentMerge()
		{
			if (this.debug)
			{
				global::UnityEngine.Debug.Log("Abandonded merging for chunk: " + this.currentMergingChunk.ChunkName);
			}
			this.currentMergingChunk.RemoveMergedVisuals();
			base.StopCoroutine(this.currentMergeRoutine);
			this.currentMergingChunk = null;
			this.currentMergeRoutine = null;
		}

		// Token: 0x06002037 RID: 8247 RVA: 0x000913A8 File Offset: 0x0008F5A8
		public void CellUpdated(Vector3Int position)
		{
			ChunkManager.Chunk chunkForModification = this.GetChunkForModification(position);
			if (this.visualizationEnabled && this.visualsMap.ContainsKey(chunkForModification.ChunkPosition))
			{
				this.visualsMap[chunkForModification.ChunkPosition].material = this.dirtyVisualizationMaterial;
			}
			chunkForModification.ForceMarkDirty();
		}

		// Token: 0x06002038 RID: 8248 RVA: 0x000913FC File Offset: 0x0008F5FC
		public void CellAdded(Vector3Int position, GameObject cellRoot)
		{
			ChunkManager.Chunk chunkForModification = this.GetChunkForModification(position);
			if (this.visualizationEnabled && this.visualsMap.ContainsKey(chunkForModification.ChunkPosition))
			{
				this.visualsMap[chunkForModification.ChunkPosition].material = this.dirtyVisualizationMaterial;
			}
			chunkForModification.AddCell(cellRoot);
		}

		// Token: 0x06002039 RID: 8249 RVA: 0x00091450 File Offset: 0x0008F650
		public void CellRemoved(Vector3Int position, GameObject cellRoot)
		{
			ChunkManager.Chunk chunkForModification = this.GetChunkForModification(position);
			if (this.visualizationEnabled && this.visualsMap.ContainsKey(chunkForModification.ChunkPosition))
			{
				this.visualsMap[chunkForModification.ChunkPosition].material = this.dirtyVisualizationMaterial;
			}
			chunkForModification.RemoveCell(cellRoot);
		}

		// Token: 0x0600203A RID: 8250 RVA: 0x000914A4 File Offset: 0x0008F6A4
		private void Update()
		{
			if (this.currentMergeRoutine == null)
			{
				float num = float.PositiveInfinity;
				ChunkManager.Chunk chunk = null;
				foreach (ChunkManager.Chunk chunk2 in this.chunkMap.Values)
				{
					if (chunk2.IsDirty && chunk2.LastModifiedTime < num)
					{
						chunk = chunk2;
						num = chunk2.LastModifiedTime;
					}
				}
				if (chunk != null && Time.time >= chunk.LastModifiedTime + this.mergeDelay)
				{
					this.StartMerge(chunk);
				}
			}
		}

		// Token: 0x0600203B RID: 8251 RVA: 0x00091540 File Offset: 0x0008F740
		private void StartMerge(ChunkManager.Chunk oldestChunk)
		{
			this.currentMergingChunk = oldestChunk;
			this.currentMergeRoutine = base.StartCoroutine(this.MergeChunk(oldestChunk));
		}

		// Token: 0x0600203C RID: 8252 RVA: 0x0009155C File Offset: 0x0008F75C
		private void OnDisable()
		{
			if (this.currentMergeRoutine != null)
			{
				this.AbandonCurrentMerge();
			}
		}

		// Token: 0x0600203D RID: 8253 RVA: 0x0009156C File Offset: 0x0008F76C
		private IEnumerator MergeChunk(ChunkManager.Chunk chunk)
		{
			if (this.debug)
			{
				global::UnityEngine.Debug.Log("Started merging for chunk: " + chunk.ChunkName);
			}
			if (chunk.UnmergedGameObjects.Count == 0)
			{
				this.CompleteMerge(chunk);
			}
			else
			{
				Stopwatch totalMergeStopwatch = new Stopwatch();
				totalMergeStopwatch.Start();
				if (this.visualizationEnabled && this.visualsMap.ContainsKey(chunk.ChunkPosition))
				{
					this.visualsMap[chunk.ChunkPosition].material = this.mergingVisualizationMaterial;
				}
				Dictionary<ShadowCastingMode, ChunkManager.MergeInfo> mergeMap = new Dictionary<ShadowCastingMode, ChunkManager.MergeInfo>();
				Stopwatch frameStopwatch = new Stopwatch();
				frameStopwatch.Start();
				int i;
				for (int index = 0; index < chunk.UnmergedGameObjects.Count; index = i + 1)
				{
					Transform transform = chunk.UnmergedGameObjects[index].transform;
					if (ChunkManager.GetMergeInfo(chunk.ChunkName, mergeMap, transform, this.mergeRoot) && frameStopwatch.ElapsedMilliseconds > (long)this.currentMillisecondLimit)
					{
						yield return null;
						frameStopwatch.Restart();
					}
					i = index;
				}
				foreach (ShadowCastingMode castingMode in mergeMap.Keys)
				{
					ChunkManager.MergeInfo mergeInfo = mergeMap[castingMode];
					CombineInstance[] finalCombine = new CombineInstance[mergeInfo.Materials.Count];
					for (int index = 0; index < mergeInfo.Materials.Count; index = i + 1)
					{
						finalCombine[index].mesh = new Mesh();
						CombineInstance[] array = mergeInfo.CombineInstances[index].ToArray();
						int num = 0;
						foreach (CombineInstance combineInstance in array)
						{
							num += combineInstance.mesh.vertices.Length;
						}
						if (num > 65535)
						{
							finalCombine[index].mesh.indexFormat = IndexFormat.UInt32;
						}
						finalCombine[index].mesh.CombineMeshes(array, true, true);
						finalCombine[index].subMeshIndex = 0;
						if (frameStopwatch.ElapsedMilliseconds > (long)this.currentMillisecondLimit)
						{
							yield return null;
							frameStopwatch.Restart();
						}
						i = index;
					}
					MeshFilter meshFilter = mergeInfo.MergedMesh.AddComponent<MeshFilter>();
					meshFilter.sharedMesh = new Mesh();
					int num2 = 0;
					foreach (CombineInstance combineInstance2 in finalCombine)
					{
						num2 += combineInstance2.mesh.vertices.Length;
					}
					if (num2 > 65535)
					{
						meshFilter.sharedMesh.indexFormat = IndexFormat.UInt32;
					}
					meshFilter.sharedMesh.CombineMeshes(finalCombine, false, false);
					Renderer renderer = mergeInfo.MergedMesh.AddComponent<MeshRenderer>();
					renderer.shadowCastingMode = castingMode;
					renderer.sharedMaterials = mergeInfo.Materials.ToArray();
					chunk.MergedGameObjects.Add(renderer);
					if (frameStopwatch.ElapsedMilliseconds > (long)this.currentMillisecondLimit)
					{
						yield return null;
						frameStopwatch.Restart();
					}
					mergeInfo = null;
					finalCombine = null;
				}
				Dictionary<ShadowCastingMode, ChunkManager.MergeInfo>.KeyCollection.Enumerator enumerator = default(Dictionary<ShadowCastingMode, ChunkManager.MergeInfo>.KeyCollection.Enumerator);
				totalMergeStopwatch.Stop();
				yield return null;
				this.CompleteMerge(chunk);
				totalMergeStopwatch = null;
				mergeMap = null;
				frameStopwatch = null;
			}
			yield break;
			yield break;
		}

		// Token: 0x0600203E RID: 8254 RVA: 0x00091584 File Offset: 0x0008F784
		private void CompleteMerge(ChunkManager.Chunk chunk)
		{
			if (this.debug)
			{
				global::UnityEngine.Debug.Log("Finished merging for chunk: " + chunk.ChunkName);
			}
			chunk.CompleteMerge();
			if (this.visualizationEnabled && this.visualsMap.ContainsKey(chunk.ChunkPosition))
			{
				this.visualsMap[chunk.ChunkPosition].material = this.mergedVisualizationMaterial;
			}
			this.currentMergeRoutine = null;
			this.currentMergingChunk = null;
			if (!this.HasDirtyChunks)
			{
				if (this.debug)
				{
					global::UnityEngine.Debug.Log("Finished merging chunks!");
				}
				this.OnMergingComplete.Invoke();
			}
		}

		// Token: 0x0600203F RID: 8255 RVA: 0x00091620 File Offset: 0x0008F820
		private static bool GetMergeInfo(string namePrefix, Dictionary<ShadowCastingMode, ChunkManager.MergeInfo> mergeMap, Transform transform, Transform chunkParent = null)
		{
			bool flag = ChunkManager.ProcessRenderer(namePrefix, mergeMap, transform, chunkParent);
			int childCount = transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				bool mergeInfo = ChunkManager.GetMergeInfo(namePrefix, mergeMap, transform.GetChild(i), chunkParent);
				flag = flag || mergeInfo;
			}
			return flag;
		}

		// Token: 0x06002040 RID: 8256 RVA: 0x00091660 File Offset: 0x0008F860
		private static bool ProcessRenderer(string namePrefix, Dictionary<ShadowCastingMode, ChunkManager.MergeInfo> mergeMap, Transform currentTransform, Transform chunkParent = null)
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
				mergeMap.Add(shadowCastingMode, new ChunkManager.MergeInfo(shadowCastingMode, namePrefix, chunkParent));
			}
			ChunkManager.MergeInfo mergeInfo = mergeMap[shadowCastingMode];
			for (int i = 0; i < component2.sharedMaterials.Length; i++)
			{
				int num = mergeInfo.Materials.IndexOf(component2.sharedMaterials[i]);
				if (num == -1)
				{
					mergeInfo.Materials.Add(component2.sharedMaterials[i]);
					mergeInfo.CombineInstances.Add(new List<CombineInstance>());
					num = mergeInfo.Materials.Count - 1;
				}
				CombineInstance combineInstance = default(CombineInstance);
				combineInstance.transform = currentTransform.localToWorldMatrix;
				combineInstance.mesh = component.sharedMesh;
				combineInstance.subMeshIndex = i;
				mergeInfo.CombineInstances[num].Add(combineInstance);
			}
			return true;
		}

		// Token: 0x06002041 RID: 8257 RVA: 0x00091764 File Offset: 0x0008F964
		internal void MarkChunkAsReadyToMerge(Vector3Int chunkCoords)
		{
			if (this.chunkMap.ContainsKey(chunkCoords))
			{
				ChunkManager.Chunk chunk = this.chunkMap[chunkCoords];
				if (this.currentMergingChunk == null)
				{
					this.StartMerge(chunk);
					return;
				}
				chunk.ForceModifiedTime(Time.time - this.mergeDelay * 2f);
			}
		}

		// Token: 0x06002042 RID: 8258 RVA: 0x000917B4 File Offset: 0x0008F9B4
		public void SetCollectionModeToRuntime()
		{
			this.currentMillisecondLimit = 4;
		}

		// Token: 0x06002043 RID: 8259 RVA: 0x000917BD File Offset: 0x0008F9BD
		public void SetCollectionModeToLoadTime()
		{
			this.currentMillisecondLimit = 32;
		}

		// Token: 0x040019C0 RID: 6592
		private const int LOAD_LIMIT_MS = 32;

		// Token: 0x040019C1 RID: 6593
		private const int RUNTIME_LIMIT_MS = 4;

		// Token: 0x040019C2 RID: 6594
		private int currentMillisecondLimit = 4;

		// Token: 0x040019C3 RID: 6595
		private const float CHUNK_SIZE = 16f;

		// Token: 0x040019C4 RID: 6596
		[SerializeField]
		private Transform mergeRoot;

		// Token: 0x040019C5 RID: 6597
		[SerializeField]
		private Renderer chunkVisualizerPrefab;

		// Token: 0x040019C6 RID: 6598
		[SerializeField]
		private Material mergedVisualizationMaterial;

		// Token: 0x040019C7 RID: 6599
		[SerializeField]
		private Material mergingVisualizationMaterial;

		// Token: 0x040019C8 RID: 6600
		[SerializeField]
		private Material dirtyVisualizationMaterial;

		// Token: 0x040019C9 RID: 6601
		[SerializeField]
		private bool debug;

		// Token: 0x040019CA RID: 6602
		[HideInInspector]
		public UnityEvent OnMergingComplete = new UnityEvent();

		// Token: 0x040019CB RID: 6603
		[SerializeField]
		private bool visualizationEnabled;

		// Token: 0x040019CC RID: 6604
		private Dictionary<Vector3Int, ChunkManager.Chunk> chunkMap = new Dictionary<Vector3Int, ChunkManager.Chunk>();

		// Token: 0x040019CD RID: 6605
		private Dictionary<Vector3Int, Renderer> visualsMap = new Dictionary<Vector3Int, Renderer>();

		// Token: 0x040019CE RID: 6606
		private float mergeDelay = 3f;

		// Token: 0x040019CF RID: 6607
		private Coroutine currentMergeRoutine;

		// Token: 0x040019D0 RID: 6608
		private ChunkManager.Chunk currentMergingChunk;

		// Token: 0x040019D1 RID: 6609
		private ProfilerMarker mergeMarker1 = new ProfilerMarker("Chunk Merge Marker 1");

		// Token: 0x040019D2 RID: 6610
		private ProfilerMarker mergeMarker2 = new ProfilerMarker("Chunk Merge Marker 2");

		// Token: 0x040019D3 RID: 6611
		private ProfilerMarker mergeMarker3 = new ProfilerMarker("Chunk Merge Marker 3");

		// Token: 0x0200053B RID: 1339
		public class MergeInfo
		{
			// Token: 0x06002047 RID: 8263 RVA: 0x00091864 File Offset: 0x0008FA64
			public MergeInfo(ShadowCastingMode shadowCastingMode, string namePrefix, Transform parent = null)
			{
				this.Materials = new List<Material>();
				this.CombineInstances = new List<List<CombineInstance>>();
				this.MergedMesh = new GameObject(namePrefix + " - ShadowCastingMode." + shadowCastingMode.ToString());
				if (parent != null)
				{
					this.MergedMesh.transform.SetParent(parent);
				}
			}

			// Token: 0x040019D4 RID: 6612
			public List<Material> Materials;

			// Token: 0x040019D5 RID: 6613
			public List<List<CombineInstance>> CombineInstances;

			// Token: 0x040019D6 RID: 6614
			public GameObject MergedMesh;
		}

		// Token: 0x0200053C RID: 1340
		public class Chunk
		{
			// Token: 0x1700062E RID: 1582
			// (get) Token: 0x06002048 RID: 8264 RVA: 0x000918CA File Offset: 0x0008FACA
			// (set) Token: 0x06002049 RID: 8265 RVA: 0x000918D2 File Offset: 0x0008FAD2
			public bool IsDirty { get; private set; }

			// Token: 0x1700062F RID: 1583
			// (get) Token: 0x0600204A RID: 8266 RVA: 0x000918DB File Offset: 0x0008FADB
			// (set) Token: 0x0600204B RID: 8267 RVA: 0x000918E3 File Offset: 0x0008FAE3
			public float LastModifiedTime { get; private set; }

			// Token: 0x17000630 RID: 1584
			// (get) Token: 0x0600204C RID: 8268 RVA: 0x000918EC File Offset: 0x0008FAEC
			// (set) Token: 0x0600204D RID: 8269 RVA: 0x000918F4 File Offset: 0x0008FAF4
			public List<Renderer> MergedGameObjects { get; private set; } = new List<Renderer>();

			// Token: 0x17000631 RID: 1585
			// (get) Token: 0x0600204E RID: 8270 RVA: 0x000918FD File Offset: 0x0008FAFD
			// (set) Token: 0x0600204F RID: 8271 RVA: 0x00091905 File Offset: 0x0008FB05
			public List<GameObject> UnmergedGameObjects { get; private set; } = new List<GameObject>();

			// Token: 0x17000632 RID: 1586
			// (get) Token: 0x06002050 RID: 8272 RVA: 0x0009190E File Offset: 0x0008FB0E
			// (set) Token: 0x06002051 RID: 8273 RVA: 0x00091916 File Offset: 0x0008FB16
			public string ChunkName { get; private set; } = "Chunk";

			// Token: 0x17000633 RID: 1587
			// (get) Token: 0x06002052 RID: 8274 RVA: 0x0009191F File Offset: 0x0008FB1F
			// (set) Token: 0x06002053 RID: 8275 RVA: 0x00091927 File Offset: 0x0008FB27
			public Vector3Int ChunkPosition { get; private set; }

			// Token: 0x06002054 RID: 8276 RVA: 0x00091930 File Offset: 0x0008FB30
			public Chunk(Vector3Int chunkPosition)
			{
				this.ChunkName = string.Format("Chunk ({0}, {1}, {2})", chunkPosition.x, chunkPosition.y, chunkPosition.z);
				this.ChunkPosition = chunkPosition;
			}

			// Token: 0x06002055 RID: 8277 RVA: 0x0009199F File Offset: 0x0008FB9F
			internal void AddCell(GameObject cellRoot)
			{
				this.UnmergedGameObjects.Add(cellRoot);
				this.MarkAsDirty();
			}

			// Token: 0x06002056 RID: 8278 RVA: 0x000919B3 File Offset: 0x0008FBB3
			internal void RemoveCell(GameObject cellRoot)
			{
				if (this.UnmergedGameObjects.Contains(cellRoot))
				{
					this.UnmergedGameObjects.Remove(cellRoot);
					this.MarkAsDirty();
				}
			}

			// Token: 0x06002057 RID: 8279 RVA: 0x000919D6 File Offset: 0x0008FBD6
			private void MarkAsDirty()
			{
				this.LastModifiedTime = Time.time;
				if (!this.IsDirty)
				{
					this.RemoveMergedVisuals();
					this.ToggleRenderers(true);
					this.IsDirty = true;
				}
			}

			// Token: 0x06002058 RID: 8280 RVA: 0x000919FF File Offset: 0x0008FBFF
			internal void ForceModifiedTime(float time)
			{
				this.LastModifiedTime = time;
			}

			// Token: 0x06002059 RID: 8281 RVA: 0x00091A08 File Offset: 0x0008FC08
			internal void ToggleRenderers(bool enabled)
			{
				new ProfilerMarker("Chunk Toggle Visuals Marker");
				foreach (GameObject gameObject in this.UnmergedGameObjects)
				{
					Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].enabled = enabled;
					}
				}
			}

			// Token: 0x0600205A RID: 8282 RVA: 0x00091A7C File Offset: 0x0008FC7C
			internal void CompleteMerge()
			{
				this.IsDirty = false;
				this.ToggleRenderers(false);
			}

			// Token: 0x0600205B RID: 8283 RVA: 0x00091A8C File Offset: 0x0008FC8C
			internal void ForceMarkDirty()
			{
				this.MarkAsDirty();
			}

			// Token: 0x0600205C RID: 8284 RVA: 0x00091A94 File Offset: 0x0008FC94
			internal void RemoveMergedVisuals()
			{
				foreach (Renderer renderer in this.MergedGameObjects)
				{
					global::UnityEngine.Object.Destroy(renderer.gameObject);
				}
				this.MergedGameObjects.Clear();
			}
		}
	}
}
