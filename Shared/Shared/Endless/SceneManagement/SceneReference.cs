using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Endless.SceneManagement
{
	// Token: 0x02000034 RID: 52
	[Serializable]
	public sealed class SceneReference
	{
		// Token: 0x06000157 RID: 343 RVA: 0x00004452 File Offset: 0x00002652
		public SceneReference()
		{
		}

		// Token: 0x06000158 RID: 344 RVA: 0x00008EE2 File Offset: 0x000070E2
		public SceneReference(string name, bool setActiveOnLoad)
		{
			this.name = name;
			this.setActiveOnLoad = setActiveOnLoad;
		}

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x06000159 RID: 345 RVA: 0x00008EF8 File Offset: 0x000070F8
		public string Name
		{
			get
			{
				return this.name;
			}
		}

		// Token: 0x1700003F RID: 63
		// (get) Token: 0x0600015A RID: 346 RVA: 0x00008F00 File Offset: 0x00007100
		// (set) Token: 0x0600015B RID: 347 RVA: 0x00008F08 File Offset: 0x00007108
		public SceneReference.SceneStates State { get; private set; }

		// Token: 0x17000040 RID: 64
		// (get) Token: 0x0600015C RID: 348 RVA: 0x00008F14 File Offset: 0x00007114
		public float Progress
		{
			get
			{
				SceneReference.SceneStates state = this.State;
				if (state == SceneReference.SceneStates.NotLoaded || state == SceneReference.SceneStates.Loaded)
				{
					return 1f;
				}
				AsyncOperation asyncOperation = this.operation;
				if (asyncOperation == null)
				{
					return 0f;
				}
				return asyncOperation.progress;
			}
		}

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x0600015D RID: 349 RVA: 0x00008F4A File Offset: 0x0000714A
		public Scene? Scene
		{
			get
			{
				return this.scene;
			}
		}

		// Token: 0x14000006 RID: 6
		// (add) Token: 0x0600015E RID: 350 RVA: 0x00008F54 File Offset: 0x00007154
		// (remove) Token: 0x0600015F RID: 351 RVA: 0x00008F8C File Offset: 0x0000718C
		public event Action<SceneReference> OnLoaded;

		// Token: 0x14000007 RID: 7
		// (add) Token: 0x06000160 RID: 352 RVA: 0x00008FC4 File Offset: 0x000071C4
		// (remove) Token: 0x06000161 RID: 353 RVA: 0x00008FFC File Offset: 0x000071FC
		public event Action<SceneReference> OnReady;

		// Token: 0x14000008 RID: 8
		// (add) Token: 0x06000162 RID: 354 RVA: 0x00009034 File Offset: 0x00007234
		// (remove) Token: 0x06000163 RID: 355 RVA: 0x0000906C File Offset: 0x0000726C
		public event Action<SceneReference> OnPreUnload;

		// Token: 0x14000009 RID: 9
		// (add) Token: 0x06000164 RID: 356 RVA: 0x000090A4 File Offset: 0x000072A4
		// (remove) Token: 0x06000165 RID: 357 RVA: 0x000090DC File Offset: 0x000072DC
		public event Action<SceneReference> OnUnloaded;

		// Token: 0x06000166 RID: 358 RVA: 0x00009114 File Offset: 0x00007314
		public bool Load()
		{
			if (this.State != SceneReference.SceneStates.NotLoaded)
			{
				Debug.LogError(string.Format("SceneReference.Load failed. Scene [{0}] is currently [{1}]. Ignoring.", this.Name, this.State));
				return false;
			}
			Debug.Log("Loading scene: [" + this.Name + "]");
			this.State = SceneReference.SceneStates.Loading;
			SceneReference.QueueOperation(this);
			return true;
		}

		// Token: 0x06000167 RID: 359 RVA: 0x00009174 File Offset: 0x00007374
		public void ONLoaded(Scene loadedScene, LoadSceneMode mode)
		{
			if (this.State != SceneReference.SceneStates.Loading)
			{
				Debug.LogError(string.Format("SceneDefinition.onLoaded call failed. Scene [{0}] is currently [{1}]. Ignoring.", this.Name, this.State));
				return;
			}
			SceneManager.sceneLoaded -= this.ONLoaded;
			this.operation = null;
			this.scene = new Scene?(loadedScene);
			if (this.setActiveOnLoad)
			{
				SceneManager.SetActiveScene(this.scene.Value);
			}
			Action<SceneReference> onLoaded = this.OnLoaded;
			if (onLoaded != null)
			{
				onLoaded(this);
			}
			this.State = SceneReference.SceneStates.Loaded;
			SceneReference.ONOperationFinished(this);
		}

		// Token: 0x06000168 RID: 360 RVA: 0x00009208 File Offset: 0x00007408
		public bool Unload()
		{
			if (this.State != SceneReference.SceneStates.Loaded)
			{
				Debug.LogError(string.Format("SceneDefinition.Unload failed. Scene [{0}] is currently [{1}]. Ignoring.", this.Name, this.State));
				return false;
			}
			this.State = SceneReference.SceneStates.Unloading;
			Action<SceneReference> onPreUnload = this.OnPreUnload;
			if (onPreUnload != null)
			{
				onPreUnload(this);
			}
			SceneReference.QueueOperation(this);
			return true;
		}

		// Token: 0x06000169 RID: 361 RVA: 0x00009260 File Offset: 0x00007460
		private void ONUnloaded(Scene unloadedScene)
		{
			if (this.State != SceneReference.SceneStates.Unloading)
			{
				Debug.LogError(string.Format("SceneDefinition onUnloaded failed. Scene [{0}] is currently [{1}]. Ignoring.", this.Name, this.State));
				return;
			}
			SceneManager.sceneUnloaded -= this.ONUnloaded;
			Action<SceneReference> onUnloaded = this.OnUnloaded;
			if (onUnloaded != null)
			{
				onUnloaded(this);
			}
			this.operation = null;
			this.scene = null;
			this.State = SceneReference.SceneStates.NotLoaded;
			SceneReference.ONOperationFinished(this);
		}

		// Token: 0x0600016A RID: 362 RVA: 0x000092DA File Offset: 0x000074DA
		private static void QueueOperation(SceneReference definition)
		{
			if (!SceneReference.operationInProgress)
			{
				SceneReference.StartOperation(definition);
				return;
			}
			SceneReference.operationQueue.Enqueue(definition);
		}

		// Token: 0x0600016B RID: 363 RVA: 0x000092F5 File Offset: 0x000074F5
		private static void StartOperation(SceneReference definition)
		{
			SceneReference.operationInProgress = true;
			if (definition.State == SceneReference.SceneStates.Loading)
			{
				SceneReference.SceneLoading(definition);
				return;
			}
			if (definition.State == SceneReference.SceneStates.Unloading)
			{
				SceneReference.SceneUnloading(definition);
			}
		}

		// Token: 0x0600016C RID: 364 RVA: 0x0000931C File Offset: 0x0000751C
		private static void SceneLoading(SceneReference definition)
		{
			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				Scene sceneAt = SceneManager.GetSceneAt(i);
				if (sceneAt.isLoaded && sceneAt.name == definition.name)
				{
					Debug.LogWarning("SceneReference.sceneLoading failed. Scene [" + definition.name + "] is already loaded.");
					definition.ONLoaded(sceneAt, LoadSceneMode.Additive);
					return;
				}
			}
			definition.operation = SceneManager.LoadSceneAsync(definition.name, LoadSceneMode.Additive);
			SceneManager.sceneLoaded += definition.ONLoaded;
		}

		// Token: 0x0600016D RID: 365 RVA: 0x000093A3 File Offset: 0x000075A3
		private static void SceneUnloading(SceneReference definition)
		{
			definition.operation = SceneManager.UnloadSceneAsync(definition.name, UnloadSceneOptions.None);
			SceneManager.sceneUnloaded += definition.ONUnloaded;
		}

		// Token: 0x0600016E RID: 366 RVA: 0x000093C8 File Offset: 0x000075C8
		private static void ONOperationFinished(SceneReference definition)
		{
			if (SceneReference.operationQueue.Count > 0)
			{
				SceneReference.StartOperation(SceneReference.operationQueue.Dequeue());
				return;
			}
			SceneReference.operationInProgress = false;
		}

		// Token: 0x040000C0 RID: 192
		[SerializeField]
		private string name;

		// Token: 0x040000C1 RID: 193
		[SerializeField]
		private bool setActiveOnLoad;

		// Token: 0x040000C7 RID: 199
		private AsyncOperation operation;

		// Token: 0x040000C8 RID: 200
		private Scene? scene;

		// Token: 0x040000C9 RID: 201
		private static bool operationInProgress = false;

		// Token: 0x040000CA RID: 202
		private static Queue<SceneReference> operationQueue = new Queue<SceneReference>();

		// Token: 0x02000035 RID: 53
		public enum SceneStates
		{
			// Token: 0x040000CC RID: 204
			NotLoaded,
			// Token: 0x040000CD RID: 205
			Loading,
			// Token: 0x040000CE RID: 206
			Loaded,
			// Token: 0x040000CF RID: 207
			Unloading
		}
	}
}
