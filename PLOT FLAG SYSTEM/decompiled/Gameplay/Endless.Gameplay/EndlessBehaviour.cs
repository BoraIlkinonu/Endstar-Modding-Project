using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class EndlessBehaviour : MonoBehaviour
{
	protected bool IsServer => NetworkManager.Singleton.IsServer;

	protected virtual void Start()
	{
		MonoBehaviourSingleton<EndlessLoop>.Instance.RegisterBehaviour(this);
	}

	protected virtual void OnDestroy()
	{
		if ((bool)MonoBehaviourSingleton<EndlessLoop>.Instance)
		{
			MonoBehaviourSingleton<EndlessLoop>.Instance.RemoveBehaviour(this);
		}
	}
}
