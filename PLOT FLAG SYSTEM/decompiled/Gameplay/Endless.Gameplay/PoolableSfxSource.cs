using System;
using Endless.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay;

public class PoolableSfxSource : MonoBehaviour, IPoolableT
{
	private int audioId = -1;

	private bool isActive;

	private Transform followTarget;

	[NonSerialized]
	public readonly UnityEvent<int> OnSelfDisabled = new UnityEvent<int>();

	[field: SerializeField]
	public AudioSource AudioSource { get; private set; }

	public MonoBehaviour Prefab { get; set; }

	private void LateUpdate()
	{
		if ((bool)followTarget)
		{
			base.transform.position = followTarget.position;
		}
		else if (audioId != -1)
		{
			OnSelfDisabled.Invoke(audioId);
		}
	}

	public void OnSpawn()
	{
		isActive = true;
	}

	public void OnDespawn()
	{
		isActive = false;
		audioId = -1;
		followTarget = null;
		OnSelfDisabled.RemoveAllListeners();
	}

	public void SetAudioIdAndFollowTarget(int newAudioId, Transform transformToAttachTo)
	{
		audioId = newAudioId;
		followTarget = transformToAttachTo;
	}
}
