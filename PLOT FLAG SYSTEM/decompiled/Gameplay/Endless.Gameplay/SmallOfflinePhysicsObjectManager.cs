using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Unity.Collections;
using UnityEngine;

namespace Endless.Gameplay;

public class SmallOfflinePhysicsObjectManager : MonoBehaviour
{
	private class SmallObjectList
	{
		public int hash;

		public SmallOfflinePhysicsObject prefab;

		public List<SmallOfflinePhysicsObject> instances = new List<SmallOfflinePhysicsObject>(40);

		public List<SmallOfflinePhysicsObject> activeInstances = new List<SmallOfflinePhysicsObject>(40);

		public int numToSkip;

		public int ActiveCount => activeInstances.Count;

		public void Add(SmallOfflinePhysicsObject obj)
		{
			instances.Add(obj);
			activeInstances.Add(obj);
			numToSkip = skipCount;
		}

		public bool Remove(SmallOfflinePhysicsObject obj)
		{
			bool result = instances.RemoveSwapBack(obj);
			if (activeInstances.RemoveSwapBack(obj) && numToSkip > 0)
			{
				numToSkip--;
			}
			return result;
		}

		public bool Remove(int index, SmallOfflinePhysicsObject obj)
		{
			bool result = false;
			if (index >= 0 && index < instances.Count)
			{
				instances.RemoveAtSwapBack(index);
				result = true;
			}
			if (activeInstances.RemoveSwapBack(obj) && numToSkip > 0)
			{
				numToSkip--;
			}
			return result;
		}

		public void Clear()
		{
			if (prefab != null)
			{
				foreach (SmallOfflinePhysicsObject instance in instances)
				{
					MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn(instance);
				}
			}
			else
			{
				foreach (SmallOfflinePhysicsObject instance2 in instances)
				{
					Object.Destroy(instance2.gameObject);
				}
			}
			instances.Clear();
			activeInstances.Clear();
			numToSkip = 0;
		}
	}

	private static SmallOfflinePhysicsObjectManager instance;

	private static List<SmallObjectList> objectsByPrefab;

	private static int skipCount;

	private static int maxCount;

	public static Transform Transform { get; private set; }

	private static int TotalActiveCount
	{
		get
		{
			int num = 0;
			foreach (SmallObjectList item in objectsByPrefab)
			{
				num += item.ActiveCount;
			}
			return num;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	public static void Init()
	{
		instance = new GameObject("SmallOfflinePhysicsObjectManager").AddComponent<SmallOfflinePhysicsObjectManager>();
		Transform = instance.transform;
		objectsByPrefab = new List<SmallObjectList>(20);
		skipCount = 0;
		maxCount = 50;
	}

	public static void SetQuality(int maxObjects, int skip)
	{
		skipCount = skip;
		maxCount = maxObjects;
	}

	private void Start()
	{
		MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayStopped.AddListener(HandleGameplayCleanup);
		MonoBehaviourSingleton<StageManager>.Instance.OnActiveStageChanged.AddListener(HandleActiveStageChanged);
	}

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		foreach (SmallObjectList item in objectsByPrefab)
		{
			for (int num = item.instances.Count - 1; num >= 0; num--)
			{
				SmallOfflinePhysicsObject smallOfflinePhysicsObject = item.instances[num];
				if ((object)smallOfflinePhysicsObject == null || (object)smallOfflinePhysicsObject.gameObject == null)
				{
					item.Remove(num, null);
				}
				else
				{
					smallOfflinePhysicsObject.lifetimeRemaining -= deltaTime;
					if (smallOfflinePhysicsObject.lifetimeRemaining <= 0f)
					{
						if (smallOfflinePhysicsObject.Prefab != null)
						{
							MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn(smallOfflinePhysicsObject);
						}
						else
						{
							Object.Destroy(smallOfflinePhysicsObject.gameObject);
						}
						item.Remove(num, smallOfflinePhysicsObject);
					}
					else if (!smallOfflinePhysicsObject.Stopped)
					{
						smallOfflinePhysicsObject.UpdateStoppedTime(deltaTime);
						if (smallOfflinePhysicsObject.Stopped)
						{
							smallOfflinePhysicsObject.DisablePhysics();
							item.activeInstances.RemoveSwapBack(smallOfflinePhysicsObject);
						}
					}
				}
			}
		}
	}

	private void HandleGameplayCleanup()
	{
		foreach (SmallObjectList item in objectsByPrefab)
		{
			item.Clear();
		}
	}

	private void HandleActiveStageChanged(Stage stage)
	{
		HandleGameplayCleanup();
	}

	public static SmallOfflinePhysicsObject Spawn(int hash, SmallOfflinePhysicsObject prefab, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion), Vector3 scale = default(Vector3), Transform parent = null)
	{
		if (maxCount > 0 && TotalActiveCount >= maxCount)
		{
			return null;
		}
		SmallObjectList smallObjectList = objectsByPrefab.Find((SmallObjectList x) => x.hash == hash);
		if (smallObjectList == null)
		{
			smallObjectList = new SmallObjectList
			{
				hash = hash,
				prefab = prefab
			};
			objectsByPrefab.Add(smallObjectList);
		}
		if (smallObjectList.prefab == null)
		{
			smallObjectList.prefab = prefab;
		}
		if (skipCount > 0 && smallObjectList.numToSkip > 0)
		{
			smallObjectList.numToSkip--;
			return null;
		}
		SmallOfflinePhysicsObject smallOfflinePhysicsObject = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn(smallObjectList.prefab, position, rotation);
		smallOfflinePhysicsObject.transform.localScale = scale;
		smallOfflinePhysicsObject.transform.SetParent(parent, worldPositionStays: true);
		smallObjectList.Add(smallOfflinePhysicsObject);
		return smallOfflinePhysicsObject;
	}

	public static void Remove(SmallOfflinePhysicsObject obj)
	{
		if (obj.Prefab != null)
		{
			objectsByPrefab.Find((SmallObjectList x) => x.prefab == obj.Prefab)?.Remove(obj);
			return;
		}
		using List<SmallObjectList>.Enumerator enumerator = objectsByPrefab.GetEnumerator();
		while (enumerator.MoveNext() && !enumerator.Current.Remove(obj))
		{
		}
	}
}
