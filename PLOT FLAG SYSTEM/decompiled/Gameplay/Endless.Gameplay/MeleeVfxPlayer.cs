using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace Endless.Gameplay;

public class MeleeVfxPlayer : MonoBehaviour
{
	protected static Dictionary<GameObject, List<MeleeVfxPlayer>> pool = new Dictionary<GameObject, List<MeleeVfxPlayer>>();

	[SerializeField]
	private VisualEffect visualEffect;

	[SerializeField]
	private GameObject effectPrefab;

	private float killTime;

	private Transform trackTargetPosition;

	private Transform trackTargetRotation;

	private void OnEnable()
	{
		visualEffect.Stop();
	}

	private void OnValidate()
	{
		if (effectPrefab != base.gameObject)
		{
			effectPrefab = base.gameObject;
		}
	}

	public void PlayEffect(Transform vfxTrackTargetPosition, Transform vfxTrackTargetRotation)
	{
		visualEffect.Play();
		killTime = Time.realtimeSinceStartup + 2f;
		trackTargetPosition = vfxTrackTargetPosition;
		trackTargetRotation = vfxTrackTargetRotation;
	}

	private void Update()
	{
		if (Time.realtimeSinceStartup > killTime)
		{
			visualEffect.Stop();
			RetireToPool(this);
		}
		if (trackTargetPosition != null)
		{
			base.transform.position = trackTargetPosition.position;
		}
		if (trackTargetRotation != null)
		{
			base.transform.rotation = trackTargetRotation.rotation;
		}
	}

	public static MeleeVfxPlayer GetFromPool(GameObject prefab)
	{
		if (pool.ContainsKey(prefab))
		{
			if (pool[prefab] == null)
			{
				pool[prefab] = new List<MeleeVfxPlayer>();
			}
			if (pool[prefab].Count > 0)
			{
				MeleeVfxPlayer result = pool[prefab][0];
				pool[prefab].RemoveAt(0);
				return result;
			}
		}
		return null;
	}

	public static void RetireToPool(MeleeVfxPlayer item)
	{
		if (!pool.ContainsKey(item.effectPrefab))
		{
			pool.Add(item.effectPrefab, new List<MeleeVfxPlayer>());
		}
		item.gameObject.SetActive(value: false);
		pool[item.effectPrefab].Add(item);
	}
}
