using Endless.Shared;
using Unity.Netcode;

namespace Endless.Gameplay;

public class EndlessNetworkBehaviourSingleton<T> : NetworkBehaviourSingleton<T> where T : NetworkBehaviour
{
	protected virtual void Start()
	{
		MonoBehaviourSingleton<EndlessLoop>.Instance.RegisterBehaviour(this);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)MonoBehaviourSingleton<EndlessLoop>.Instance)
		{
			MonoBehaviourSingleton<EndlessLoop>.Instance.RemoveBehaviour(this);
		}
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected internal override string __getTypeName()
	{
		return "EndlessNetworkBehaviourSingleton`1";
	}
}
