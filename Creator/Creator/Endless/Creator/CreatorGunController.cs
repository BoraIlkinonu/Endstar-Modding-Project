using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Creator.UI;
using Endless.Data;
using Endless.Gameplay;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Endless.Creator
{
	// Token: 0x02000092 RID: 146
	public class CreatorGunController : NetworkBehaviour
	{
		// Token: 0x1700001F RID: 31
		// (get) Token: 0x0600022B RID: 555 RVA: 0x0001031E File Offset: 0x0000E51E
		public CreatorGun CreatorGun
		{
			get
			{
				if (this.creatorGun == null)
				{
					this.CreateCreatorGun();
				}
				return this.creatorGun;
			}
		}

		// Token: 0x0600022C RID: 556 RVA: 0x0001033C File Offset: 0x0000E53C
		private void CreateCreatorGun()
		{
			if (this.playerReferenceManager.ApperanceController.AppearanceAnimator)
			{
				Transform bone = this.playerReferenceManager.ApperanceController.GetBone(this.boneName);
				this.creatorGun = global::UnityEngine.Object.Instantiate<CreatorGun>(this.gunPrefab, bone);
				this.playerReferenceManager.ApperanceController.AppearanceAnimator.Animator.SetInteger(this.animatorIntegerName, this.animatorIntegerValue);
				this.creatorGun.SetColor(this.creatorGunColor.Value);
			}
		}

		// Token: 0x0600022D RID: 557 RVA: 0x000103C5 File Offset: 0x0000E5C5
		private void Awake()
		{
			NetworkVariable<Color> networkVariable = this.creatorGunColor;
			networkVariable.OnValueChanged = (NetworkVariable<Color>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<Color>.OnValueChangedDelegate(this.HandleGunColorSet));
			this.playerInputActions = new PlayerInputActions();
		}

		// Token: 0x0600022E RID: 558 RVA: 0x000103FC File Offset: 0x0000E5FC
		private void OnEnable()
		{
			this.playerInputActions.Player.MainToolAction.started += this.OnPrimary;
			this.playerInputActions.Player.MainToolAction.Enable();
		}

		// Token: 0x0600022F RID: 559 RVA: 0x00010448 File Offset: 0x0000E648
		private void OnDisable()
		{
			this.playerInputActions.Player.MainToolAction.started -= this.OnPrimary;
			this.playerInputActions.Player.MainToolAction.Disable();
		}

		// Token: 0x06000230 RID: 560 RVA: 0x00010494 File Offset: 0x0000E694
		private void HandleToolChanged(EndlessTool tool)
		{
			Color color = this.toolTypeColorDictionary[tool.ToolType];
			this.SetColor_ServerRPC(color);
		}

		// Token: 0x06000231 RID: 561 RVA: 0x000104BA File Offset: 0x0000E6BA
		private void HandleGunColorSet(Color previousValue, Color newValue)
		{
			if (this.CreatorGun)
			{
				this.creatorGun.SetColor(newValue);
			}
		}

		// Token: 0x06000232 RID: 562 RVA: 0x000104D8 File Offset: 0x0000E6D8
		[ServerRpc]
		public void SetColor_ServerRPC(Color newValue)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				if (base.OwnerClientId != networkManager.LocalClientId)
				{
					if (networkManager.LogLevel <= LogLevel.Normal)
					{
						Debug.LogError("Only the owner can invoke a ServerRpc that requires ownership!");
					}
					return;
				}
				ServerRpcParams serverRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(3165767815U, serverRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe(in newValue);
				base.__endSendServerRpc(ref fastBufferWriter, 3165767815U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.SetColor(newValue);
		}

		// Token: 0x06000233 RID: 563 RVA: 0x00010605 File Offset: 0x0000E805
		private void Start()
		{
			this.playerReferenceManager.ApperanceController.OnNewAppearence.AddListener(new UnityAction<AppearanceAnimator>(this.HandleAppearenceChanged));
			if (this.creatorGun == null)
			{
				this.CreateCreatorGun();
			}
		}

		// Token: 0x06000234 RID: 564 RVA: 0x0001063C File Offset: 0x0000E83C
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			if (this.playerReferenceManager.PlayerNetworkController.IsOwner)
			{
				MonoBehaviourSingleton<ToolManager>.Instance.OnToolChange.AddListener(new UnityAction<EndlessTool>(this.HandleToolChanged));
			}
			if (this.CreatorGun)
			{
				this.creatorGun.SetColor(this.creatorGunColor.Value);
			}
		}

		// Token: 0x06000235 RID: 565 RVA: 0x000106A0 File Offset: 0x0000E8A0
		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();
			if (this.playerReferenceManager && this.playerReferenceManager.PlayerNetworkController && this.playerReferenceManager.PlayerNetworkController.IsOwner)
			{
				MonoBehaviourSingleton<ToolManager>.Instance.OnToolChange.RemoveListener(new UnityAction<EndlessTool>(this.HandleToolChanged));
			}
		}

		// Token: 0x06000236 RID: 566 RVA: 0x00010700 File Offset: 0x0000E900
		private void HandleAppearenceChanged(AppearanceAnimator newAnimator)
		{
			if (this.creatorGun)
			{
				this.CreatorGun.transform.SetParent(newAnimator.GetBone(this.boneName), false);
				newAnimator.Animator.SetInteger(this.animatorIntegerName, this.animatorIntegerValue);
			}
		}

		// Token: 0x06000237 RID: 567 RVA: 0x0001074E File Offset: 0x0000E94E
		public void SetColor(Color newColor)
		{
			if (this.creatorGunColor.Value != newColor)
			{
				this.creatorGunColor.Value = newColor;
			}
		}

		// Token: 0x06000238 RID: 568 RVA: 0x0001076F File Offset: 0x0000E96F
		private void OnPrimary(InputAction.CallbackContext context)
		{
			if (!base.IsOwner || !this.CreatorGun)
			{
				return;
			}
			CreatorGun creatorGun = this.creatorGun;
			if (creatorGun == null)
			{
				return;
			}
			creatorGun.StartFlash();
		}

		// Token: 0x0600023A RID: 570 RVA: 0x000107D0 File Offset: 0x0000E9D0
		protected override void __initializeVariables()
		{
			bool flag = this.creatorGunColor == null;
			if (flag)
			{
				throw new Exception("CreatorGunController.creatorGunColor cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.creatorGunColor.Initialize(this);
			base.__nameNetworkVariable(this.creatorGunColor, "creatorGunColor");
			this.NetworkVariableFields.Add(this.creatorGunColor);
			base.__initializeVariables();
		}

		// Token: 0x0600023B RID: 571 RVA: 0x00010833 File Offset: 0x0000EA33
		protected override void __initializeRpcs()
		{
			base.__registerRpc(3165767815U, new NetworkBehaviour.RpcReceiveHandler(CreatorGunController.__rpc_handler_3165767815), "SetColor_ServerRPC");
			base.__initializeRpcs();
		}

		// Token: 0x0600023C RID: 572 RVA: 0x0001085C File Offset: 0x0000EA5C
		private static void __rpc_handler_3165767815(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (rpcParams.Server.Receive.SenderClientId != target.OwnerClientId)
			{
				if (networkManager.LogLevel <= LogLevel.Normal)
				{
					Debug.LogError("Only the owner can invoke a ServerRpc that requires ownership!");
				}
				return;
			}
			Color color;
			reader.ReadValueSafe(out color);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((CreatorGunController)target).SetColor_ServerRPC(color);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x0600023D RID: 573 RVA: 0x0001090B File Offset: 0x0000EB0B
		protected internal override string __getTypeName()
		{
			return "CreatorGunController";
		}

		// Token: 0x04000292 RID: 658
		[SerializeField]
		private CreatorGun gunPrefab;

		// Token: 0x04000293 RID: 659
		[SerializeField]
		private string boneName = "";

		// Token: 0x04000294 RID: 660
		[SerializeField]
		private string animatorIntegerName = "EquippedItem";

		// Token: 0x04000295 RID: 661
		[SerializeField]
		private int animatorIntegerValue = 2;

		// Token: 0x04000296 RID: 662
		[SerializeField]
		private CreatorPlayerReferenceManager playerReferenceManager;

		// Token: 0x04000297 RID: 663
		[SerializeField]
		private UIToolTypeColorDictionary toolTypeColorDictionary;

		// Token: 0x04000298 RID: 664
		private CreatorGun creatorGun;

		// Token: 0x04000299 RID: 665
		private PlayerInputActions playerInputActions;

		// Token: 0x0400029A RID: 666
		private NetworkVariable<Color> creatorGunColor = new NetworkVariable<Color>(Color.white, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
	}
}
