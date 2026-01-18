using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x02000115 RID: 277
	public class SmallOfflinePhysicsObjectManager : MonoBehaviour
	{
		// Token: 0x1700011B RID: 283
		// (get) Token: 0x0600063C RID: 1596 RVA: 0x0001EC16 File Offset: 0x0001CE16
		// (set) Token: 0x0600063D RID: 1597 RVA: 0x0001EC1D File Offset: 0x0001CE1D
		public static Transform Transform { get; private set; }

		// Token: 0x1700011C RID: 284
		// (get) Token: 0x0600063E RID: 1598 RVA: 0x0001EC28 File Offset: 0x0001CE28
		private static int TotalActiveCount
		{
			get
			{
				int num = 0;
				foreach (SmallOfflinePhysicsObjectManager.SmallObjectList smallObjectList in SmallOfflinePhysicsObjectManager.objectsByPrefab)
				{
					num += smallObjectList.ActiveCount;
				}
				return num;
			}
		}

		// Token: 0x0600063F RID: 1599 RVA: 0x0001EC80 File Offset: 0x0001CE80
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		public static void Init()
		{
			SmallOfflinePhysicsObjectManager.instance = new GameObject("SmallOfflinePhysicsObjectManager").AddComponent<SmallOfflinePhysicsObjectManager>();
			SmallOfflinePhysicsObjectManager.Transform = SmallOfflinePhysicsObjectManager.instance.transform;
			SmallOfflinePhysicsObjectManager.objectsByPrefab = new List<SmallOfflinePhysicsObjectManager.SmallObjectList>(20);
			SmallOfflinePhysicsObjectManager.skipCount = 0;
			SmallOfflinePhysicsObjectManager.maxCount = 50;
		}

		// Token: 0x06000640 RID: 1600 RVA: 0x0001ECBE File Offset: 0x0001CEBE
		public static void SetQuality(int maxObjects, int skip)
		{
			SmallOfflinePhysicsObjectManager.skipCount = skip;
			SmallOfflinePhysicsObjectManager.maxCount = maxObjects;
		}

		// Token: 0x06000641 RID: 1601 RVA: 0x0001ECCC File Offset: 0x0001CECC
		private void Start()
		{
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayStopped.AddListener(new UnityAction(this.HandleGameplayCleanup));
			MonoBehaviourSingleton<StageManager>.Instance.OnActiveStageChanged.AddListener(new UnityAction<Stage>(this.HandleActiveStageChanged));
		}

		// Token: 0x06000642 RID: 1602 RVA: 0x0001ED04 File Offset: 0x0001CF04
		private void Update()
		{
			float deltaTime = Time.deltaTime;
			foreach (SmallOfflinePhysicsObjectManager.SmallObjectList smallObjectList in SmallOfflinePhysicsObjectManager.objectsByPrefab)
			{
				for (int i = smallObjectList.instances.Count - 1; i >= 0; i--)
				{
					SmallOfflinePhysicsObject smallOfflinePhysicsObject = smallObjectList.instances[i];
					if (smallOfflinePhysicsObject == null || smallOfflinePhysicsObject.gameObject == null)
					{
						smallObjectList.Remove(i, null);
					}
					else
					{
						smallOfflinePhysicsObject.lifetimeRemaining -= deltaTime;
						if (smallOfflinePhysicsObject.lifetimeRemaining <= 0f)
						{
							if (smallOfflinePhysicsObject.Prefab != null)
							{
								MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<SmallOfflinePhysicsObject>(smallOfflinePhysicsObject);
							}
							else
							{
								global::UnityEngine.Object.Destroy(smallOfflinePhysicsObject.gameObject);
							}
							smallObjectList.Remove(i, smallOfflinePhysicsObject);
						}
						else if (!smallOfflinePhysicsObject.Stopped)
						{
							smallOfflinePhysicsObject.UpdateStoppedTime(deltaTime);
							if (smallOfflinePhysicsObject.Stopped)
							{
								smallOfflinePhysicsObject.DisablePhysics();
								smallObjectList.activeInstances.RemoveSwapBack(smallOfflinePhysicsObject);
							}
						}
					}
				}
			}
		}

		// Token: 0x06000643 RID: 1603 RVA: 0x0001EE24 File Offset: 0x0001D024
		private void HandleGameplayCleanup()
		{
			foreach (SmallOfflinePhysicsObjectManager.SmallObjectList smallObjectList in SmallOfflinePhysicsObjectManager.objectsByPrefab)
			{
				smallObjectList.Clear();
			}
		}

		// Token: 0x06000644 RID: 1604 RVA: 0x0001EE74 File Offset: 0x0001D074
		private void HandleActiveStageChanged(Stage stage)
		{
			this.HandleGameplayCleanup();
		}

		// Token: 0x06000645 RID: 1605 RVA: 0x0001EE7C File Offset: 0x0001D07C
		public static SmallOfflinePhysicsObject Spawn(int hash, SmallOfflinePhysicsObject prefab, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion), Vector3 scale = default(Vector3), Transform parent = null)
		{
			if (SmallOfflinePhysicsObjectManager.maxCount > 0 && SmallOfflinePhysicsObjectManager.TotalActiveCount >= SmallOfflinePhysicsObjectManager.maxCount)
			{
				return null;
			}
			SmallOfflinePhysicsObjectManager.SmallObjectList smallObjectList = SmallOfflinePhysicsObjectManager.objectsByPrefab.Find((SmallOfflinePhysicsObjectManager.SmallObjectList x) => x.hash == hash);
			if (smallObjectList == null)
			{
				smallObjectList = new SmallOfflinePhysicsObjectManager.SmallObjectList
				{
					hash = hash,
					prefab = prefab
				};
				SmallOfflinePhysicsObjectManager.objectsByPrefab.Add(smallObjectList);
			}
			if (smallObjectList.prefab == null)
			{
				smallObjectList.prefab = prefab;
			}
			if (SmallOfflinePhysicsObjectManager.skipCount > 0 && smallObjectList.numToSkip > 0)
			{
				smallObjectList.numToSkip--;
				return null;
			}
			SmallOfflinePhysicsObject smallOfflinePhysicsObject = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn<SmallOfflinePhysicsObject>(smallObjectList.prefab, position, rotation, null);
			smallOfflinePhysicsObject.transform.localScale = scale;
			smallOfflinePhysicsObject.transform.SetParent(parent, true);
			smallObjectList.Add(smallOfflinePhysicsObject);
			return smallOfflinePhysicsObject;
		}

		// Token: 0x06000646 RID: 1606 RVA: 0x0001EF58 File Offset: 0x0001D158
		public static void Remove(SmallOfflinePhysicsObject obj)
		{
			if (obj.Prefab != null)
			{
				SmallOfflinePhysicsObjectManager.SmallObjectList smallObjectList = SmallOfflinePhysicsObjectManager.objectsByPrefab.Find((SmallOfflinePhysicsObjectManager.SmallObjectList x) => x.prefab == obj.Prefab);
				if (smallObjectList != null)
				{
					smallObjectList.Remove(obj);
				}
				return;
			}
			using (List<SmallOfflinePhysicsObjectManager.SmallObjectList>.Enumerator enumerator = SmallOfflinePhysicsObjectManager.objectsByPrefab.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.Remove(obj))
					{
						break;
					}
				}
			}
		}

		// Token: 0x040004BA RID: 1210
		private static SmallOfflinePhysicsObjectManager instance;

		// Token: 0x040004BB RID: 1211
		private static List<SmallOfflinePhysicsObjectManager.SmallObjectList> objectsByPrefab;

		// Token: 0x040004BC RID: 1212
		private static int skipCount;

		// Token: 0x040004BD RID: 1213
		private static int maxCount;

		// Token: 0x02000116 RID: 278
		private class SmallObjectList
		{
			// Token: 0x1700011D RID: 285
			// (get) Token: 0x06000648 RID: 1608 RVA: 0x0001EFFC File Offset: 0x0001D1FC
			public int ActiveCount
			{
				get
				{
					return this.activeInstances.Count;
				}
			}

			// Token: 0x06000649 RID: 1609 RVA: 0x0001F009 File Offset: 0x0001D209
			public void Add(SmallOfflinePhysicsObject obj)
			{
				this.instances.Add(obj);
				this.activeInstances.Add(obj);
				this.numToSkip = SmallOfflinePhysicsObjectManager.skipCount;
			}

			// Token: 0x0600064A RID: 1610 RVA: 0x0001F02E File Offset: 0x0001D22E
			public bool Remove(SmallOfflinePhysicsObject obj)
			{
				bool flag = this.instances.RemoveSwapBack(obj);
				if (this.activeInstances.RemoveSwapBack(obj) && this.numToSkip > 0)
				{
					this.numToSkip--;
				}
				return flag;
			}

			// Token: 0x0600064B RID: 1611 RVA: 0x0001F064 File Offset: 0x0001D264
			public bool Remove(int index, SmallOfflinePhysicsObject obj)
			{
				bool flag = false;
				if (index >= 0 && index < this.instances.Count)
				{
					this.instances.RemoveAtSwapBack(index);
					flag = true;
				}
				if (this.activeInstances.RemoveSwapBack(obj) && this.numToSkip > 0)
				{
					this.numToSkip--;
				}
				return flag;
			}

			// Token: 0x0600064C RID: 1612 RVA: 0x0001F0BC File Offset: 0x0001D2BC
			public void Clear()
			{
				if (this.prefab != null)
				{
					using (List<SmallOfflinePhysicsObject>.Enumerator enumerator = this.instances.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							SmallOfflinePhysicsObject smallOfflinePhysicsObject = enumerator.Current;
							MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<SmallOfflinePhysicsObject>(smallOfflinePhysicsObject);
						}
						goto IL_0080;
					}
				}
				foreach (SmallOfflinePhysicsObject smallOfflinePhysicsObject2 in this.instances)
				{
					global::UnityEngine.Object.Destroy(smallOfflinePhysicsObject2.gameObject);
				}
				IL_0080:
				this.instances.Clear();
				this.activeInstances.Clear();
				this.numToSkip = 0;
			}

			// Token: 0x040004BE RID: 1214
			public int hash;

			// Token: 0x040004BF RID: 1215
			public SmallOfflinePhysicsObject prefab;

			// Token: 0x040004C0 RID: 1216
			public List<SmallOfflinePhysicsObject> instances = new List<SmallOfflinePhysicsObject>(40);

			// Token: 0x040004C1 RID: 1217
			public List<SmallOfflinePhysicsObject> activeInstances = new List<SmallOfflinePhysicsObject>(40);

			// Token: 0x040004C2 RID: 1218
			public int numToSkip;
		}
	}
}
