using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Creator.UI;
using Endless.Data;
using Endless.Gameplay;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Endless.Creator;

public class CreatorGunController : NetworkBehaviour
{
	[SerializeField]
	private CreatorGun gunPrefab;

	[SerializeField]
	private string boneName = "";

	[SerializeField]
	private string animatorIntegerName = "EquippedItem";

	[SerializeField]
	private int animatorIntegerValue = 2;

	[SerializeField]
	private CreatorPlayerReferenceManager playerReferenceManager;

	[SerializeField]
	private UIToolTypeColorDictionary toolTypeColorDictionary;

	private CreatorGun creatorGun;

	private PlayerInputActions playerInputActions;

	private NetworkVariable<Color> creatorGunColor = new NetworkVariable<Color>(Color.white);

	public CreatorGun CreatorGun
	{
		get
		{
			if (creatorGun == null)
			{
				CreateCreatorGun();
			}
			return creatorGun;
		}
	}

	private void CreateCreatorGun()
	{
		if ((bool)playerReferenceManager.ApperanceController.AppearanceAnimator)
		{
			Transform bone = playerReferenceManager.ApperanceController.GetBone(boneName);
			creatorGun = UnityEngine.Object.Instantiate(gunPrefab, bone);
			playerReferenceManager.ApperanceController.AppearanceAnimator.Animator.SetInteger(animatorIntegerName, animatorIntegerValue);
			creatorGun.SetColor(creatorGunColor.Value);
		}
	}

	private void Awake()
	{
		NetworkVariable<Color> networkVariable = creatorGunColor;
		networkVariable.OnValueChanged = (NetworkVariable<Color>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<Color>.OnValueChangedDelegate(HandleGunColorSet));
		playerInputActions = new PlayerInputActions();
	}

	private void OnEnable()
	{
		playerInputActions.Player.MainToolAction.started += OnPrimary;
		playerInputActions.Player.MainToolAction.Enable();
	}

	private void OnDisable()
	{
		playerInputActions.Player.MainToolAction.started -= OnPrimary;
		playerInputActions.Player.MainToolAction.Disable();
	}

	private void HandleToolChanged(EndlessTool tool)
	{
		Color color_ServerRPC = toolTypeColorDictionary[tool.ToolType];
		SetColor_ServerRPC(color_ServerRPC);
	}

	private void HandleGunColorSet(Color previousValue, Color newValue)
	{
		if ((bool)CreatorGun)
		{
			creatorGun.SetColor(newValue);
		}
	}

	[ServerRpc]
	public void SetColor_ServerRPC(Color newValue)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			if (base.OwnerClientId != networkManager.LocalClientId)
			{
				if (networkManager.LogLevel <= LogLevel.Normal)
				{
					Debug.LogError("Only the owner can invoke a ServerRpc that requires ownership!");
				}
				return;
			}
			ServerRpcParams serverRpcParams = default(ServerRpcParams);
			FastBufferWriter bufferWriter = __beginSendServerRpc(3165767815u, serverRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in newValue);
			__endSendServerRpc(ref bufferWriter, 3165767815u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			SetColor(newValue);
		}
	}

	private void Start()
	{
		playerReferenceManager.ApperanceController.OnNewAppearence.AddListener(HandleAppearenceChanged);
		if (creatorGun == null)
		{
			CreateCreatorGun();
		}
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		if (playerReferenceManager.PlayerNetworkController.IsOwner)
		{
			MonoBehaviourSingleton<ToolManager>.Instance.OnToolChange.AddListener(HandleToolChanged);
		}
		if ((bool)CreatorGun)
		{
			creatorGun.SetColor(creatorGunColor.Value);
		}
	}

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();
		if ((bool)playerReferenceManager && (bool)playerReferenceManager.PlayerNetworkController && playerReferenceManager.PlayerNetworkController.IsOwner)
		{
			MonoBehaviourSingleton<ToolManager>.Instance.OnToolChange.RemoveListener(HandleToolChanged);
		}
	}

	private void HandleAppearenceChanged(AppearanceAnimator newAnimator)
	{
		if ((bool)creatorGun)
		{
			CreatorGun.transform.SetParent(newAnimator.GetBone(boneName), worldPositionStays: false);
			newAnimator.Animator.SetInteger(animatorIntegerName, animatorIntegerValue);
		}
	}

	public void SetColor(Color newColor)
	{
		if (creatorGunColor.Value != newColor)
		{
			creatorGunColor.Value = newColor;
		}
	}

	private void OnPrimary(InputAction.CallbackContext context)
	{
		if (base.IsOwner && (bool)CreatorGun)
		{
			creatorGun?.StartFlash();
		}
	}

	protected override void __initializeVariables()
	{
		if (creatorGunColor == null)
		{
			throw new Exception("CreatorGunController.creatorGunColor cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		creatorGunColor.Initialize(this);
		__nameNetworkVariable(creatorGunColor, "creatorGunColor");
		NetworkVariableFields.Add(creatorGunColor);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(3165767815u, __rpc_handler_3165767815, "SetColor_ServerRPC");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_3165767815(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (rpcParams.Server.Receive.SenderClientId != target.OwnerClientId)
		{
			if (networkManager.LogLevel <= LogLevel.Normal)
			{
				Debug.LogError("Only the owner can invoke a ServerRpc that requires ownership!");
			}
		}
		else
		{
			reader.ReadValueSafe(out Color value);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((CreatorGunController)target).SetColor_ServerRPC(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "CreatorGunController";
	}
}
