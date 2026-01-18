using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class EndlessBehaviourSingleton<T> : MonoBehaviourSingleton<T> where T : MonoBehaviour
{
	protected bool IsServer => NetworkManager.Singleton.IsServer;

	protected virtual void Start()
	{
		MonoBehaviourSingleton<EndlessLoop>.Instance.RegisterBehaviour(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)MonoBehaviourSingleton<EndlessLoop>.Instance)
		{
			MonoBehaviourSingleton<EndlessLoop>.Instance.RemoveBehaviour(this);
		}
	}
}
