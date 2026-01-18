using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

[RequireComponent(typeof(NetworkObject))]
public class EndlessNetworkObject : NetworkBehaviour
{
	[SerializeField]
	private bool scopeManaged = true;

	private void Awake()
	{
		if (scopeManaged)
		{
			base.NetworkObject.CheckObjectVisibility = (ulong clientId) => NetworkScopeManager.CheckIfInScope(clientId, base.NetworkObject);
		}
	}

	private void OnEnable()
	{
		if (scopeManaged)
		{
			NetworkScopeManager.AddNetworkObject(base.NetworkObject);
		}
	}

	private void OnDisable()
	{
		if (scopeManaged)
		{
			NetworkScopeManager.RemoveNetworkObject(base.NetworkObject);
		}
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		if (!base.IsServer)
		{
			Stage activeStage = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage;
			if (activeStage != null)
			{
				activeStage.TrackNetworkObject(base.NetworkObject.NetworkObjectId, base.gameObject);
			}
			else
			{
				MonoBehaviourSingleton<StageManager>.Instance.OnActiveStageChanged.AddListener(HandleActiveStageChanged);
			}
		}
	}

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();
		if ((bool)MonoBehaviourSingleton<StageManager>.Instance)
		{
			MonoBehaviourSingleton<StageManager>.Instance.OnActiveStageChanged.RemoveListener(HandleActiveStageChanged);
			if ((bool)MonoBehaviourSingleton<StageManager>.Instance.ActiveStage)
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.UntrackNetworkObject(base.gameObject, base.NetworkObject.NetworkObjectId);
			}
		}
	}

	private void HandleActiveStageChanged(Stage stage)
	{
		stage.TrackNetworkObject(base.NetworkObject.NetworkObjectId, base.gameObject);
		MonoBehaviourSingleton<StageManager>.Instance.OnActiveStageChanged.RemoveListener(HandleActiveStageChanged);
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
		return "EndlessNetworkObject";
	}
}
