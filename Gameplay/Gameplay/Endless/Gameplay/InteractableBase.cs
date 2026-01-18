using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using Endless.Gameplay.SoVariables;
using Endless.Props;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020000D8 RID: 216
	public abstract class InteractableBase : EndlessNetworkBehaviour
	{
		// Token: 0x170000AA RID: 170
		// (get) Token: 0x06000453 RID: 1107 RVA: 0x00016E38 File Offset: 0x00015038
		// (set) Token: 0x06000454 RID: 1108 RVA: 0x00016E40 File Offset: 0x00015040
		public UIInteractionPromptVariable InteractionPrompt { get; protected set; }

		// Token: 0x170000AB RID: 171
		// (get) Token: 0x06000455 RID: 1109 RVA: 0x00016E49 File Offset: 0x00015049
		public global::UnityEngine.Vector3 InteractionPromptOffset
		{
			get
			{
				return this.interactionPromptOffset;
			}
		}

		// Token: 0x170000AC RID: 172
		// (get) Token: 0x06000456 RID: 1110 RVA: 0x00016E51 File Offset: 0x00015051
		// (set) Token: 0x06000457 RID: 1111 RVA: 0x00016E5E File Offset: 0x0001505E
		public bool IsHeldInteraction
		{
			get
			{
				return this.isHeldInteraction.Value;
			}
			set
			{
				if (base.IsServer)
				{
					this.isHeldInteraction.Value = value;
				}
			}
		}

		// Token: 0x170000AD RID: 173
		// (get) Token: 0x06000458 RID: 1112 RVA: 0x00016E74 File Offset: 0x00015074
		// (set) Token: 0x06000459 RID: 1113 RVA: 0x00016E81 File Offset: 0x00015081
		public bool HidePromptDuringInteraction
		{
			get
			{
				return this.hidePromptDuringInteraction.Value;
			}
			set
			{
				if (base.IsServer)
				{
					this.hidePromptDuringInteraction.Value = value;
				}
			}
		}

		// Token: 0x170000AE RID: 174
		// (get) Token: 0x0600045A RID: 1114 RVA: 0x00016E97 File Offset: 0x00015097
		// (set) Token: 0x0600045B RID: 1115 RVA: 0x00016EB2 File Offset: 0x000150B2
		public float InteractionDuration
		{
			get
			{
				if (!this.IsHeldInteraction)
				{
					return 0f;
				}
				return this.interactionDuration.Value;
			}
			set
			{
				if (base.IsServer)
				{
					this.interactionDuration.Value = value;
				}
			}
		}

		// Token: 0x170000AF RID: 175
		// (get) Token: 0x0600045C RID: 1116 RVA: 0x00016EC8 File Offset: 0x000150C8
		// (set) Token: 0x0600045D RID: 1117 RVA: 0x00016ED5 File Offset: 0x000150D5
		public InteractionAnimation InteractionAnimation
		{
			get
			{
				return this.interactionAnimation.Value;
			}
			set
			{
				if (base.IsServer)
				{
					this.interactionAnimation.Value = value;
				}
			}
		}

		// Token: 0x0600045E RID: 1118 RVA: 0x00016EEB File Offset: 0x000150EB
		private void OnValidate()
		{
			if (this.InteractionPrompt == null)
			{
				Debug.LogError("An InteractionPrompt is required!", this);
			}
		}

		// Token: 0x0600045F RID: 1119 RVA: 0x00016F08 File Offset: 0x00015108
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			this.overridePositions.OnListChanged += this.OnOverridePositionsListChanged;
			this.usePlayerAnchor.OnListChanged += this.OnUsePlayerAnchorChanged;
			this.isInteractable.OnListChanged += this.OnIsInteractableListChanged;
		}

		// Token: 0x06000460 RID: 1120 RVA: 0x00016F60 File Offset: 0x00015160
		private void OnIsInteractableListChanged(NetworkListEvent<bool> changeEvent)
		{
			switch (changeEvent.Type)
			{
			case NetworkListEvent<bool>.EventType.Add:
			case NetworkListEvent<bool>.EventType.Insert:
			case NetworkListEvent<bool>.EventType.Value:
				this.interactableColliders[changeEvent.Index].IsInteractable = changeEvent.Value;
				return;
			case NetworkListEvent<bool>.EventType.Full:
			{
				for (int i = 0; i < this.isInteractable.Count; i++)
				{
					this.interactableColliders[i].IsInteractable = this.isInteractable[i];
				}
				return;
			}
			}
			throw new ArgumentOutOfRangeException();
		}

		// Token: 0x06000461 RID: 1121 RVA: 0x00016FF0 File Offset: 0x000151F0
		private void OnUsePlayerAnchorChanged(NetworkListEvent<bool> changeEvent)
		{
			switch (changeEvent.Type)
			{
			case NetworkListEvent<bool>.EventType.Add:
			case NetworkListEvent<bool>.EventType.Insert:
			case NetworkListEvent<bool>.EventType.Value:
				this.interactableColliders[changeEvent.Index].UsePlayerForAnchor = changeEvent.Value;
				return;
			case NetworkListEvent<bool>.EventType.Full:
			{
				for (int i = 0; i < this.usePlayerAnchor.Count; i++)
				{
					this.interactableColliders[i].UsePlayerForAnchor = this.usePlayerAnchor[i];
				}
				return;
			}
			}
			throw new ArgumentOutOfRangeException();
		}

		// Token: 0x06000462 RID: 1122 RVA: 0x00017080 File Offset: 0x00015280
		private void OnOverridePositionsListChanged(NetworkListEvent<NetworkedNullableVector3> changeEvent)
		{
			switch (changeEvent.Type)
			{
			case NetworkListEvent<NetworkedNullableVector3>.EventType.Add:
			case NetworkListEvent<NetworkedNullableVector3>.EventType.Insert:
			case NetworkListEvent<NetworkedNullableVector3>.EventType.Value:
				this.interactableColliders[changeEvent.Index].OverrideAnchorPosition = changeEvent.Value;
				return;
			case NetworkListEvent<NetworkedNullableVector3>.EventType.Full:
			{
				for (int i = 0; i < this.overridePositions.Count; i++)
				{
					this.interactableColliders[i].OverrideAnchorPosition = this.overridePositions[i];
				}
				return;
			}
			}
			throw new ArgumentOutOfRangeException();
		}

		// Token: 0x06000463 RID: 1123 RVA: 0x00017118 File Offset: 0x00015318
		protected void SetupColliders(IReadOnlyList<ColliderInfo> colliderInfos)
		{
			this.interactableColliders = new List<InteractableCollider>();
			foreach (ColliderInfo colliderInfo in colliderInfos)
			{
				Collider[] cachedColliders = colliderInfo.CachedColliders;
				for (int i = 0; i < cachedColliders.Length; i++)
				{
					InteractableCollider interactableCollider = cachedColliders[i].gameObject.AddComponent<InteractableCollider>();
					interactableCollider.InteractableBase = this;
					interactableCollider.ColliderInfo = colliderInfo;
					this.interactableColliders.Add(interactableCollider);
				}
			}
		}

		// Token: 0x06000464 RID: 1124 RVA: 0x000171A8 File Offset: 0x000153A8
		internal void SetInteractableEnabled(Context _, int index, bool newValue)
		{
			if (index >= this.interactableColliders.Count)
			{
				throw new ArgumentOutOfRangeException("index", "Provided index to Anchor Override is greater than the number of available colliders");
			}
			while (this.isInteractable.Count <= index)
			{
				this.isInteractable.Add(true);
			}
			this.isInteractable[index] = newValue;
		}

		// Token: 0x06000465 RID: 1125 RVA: 0x000171FC File Offset: 0x000153FC
		public virtual void SetAllInteractablesEnabled(bool interactable)
		{
			while (this.isInteractable.Count < this.interactableColliders.Count)
			{
				this.isInteractable.Add(true);
			}
			for (int i = 0; i < this.isInteractable.Count; i++)
			{
				this.isInteractable[i] = interactable;
			}
		}

		// Token: 0x06000466 RID: 1126 RVA: 0x00017254 File Offset: 0x00015454
		[ClientRpc]
		protected void AttemptInteractResult_ClientRPC(NetworkObjectReference interactorNetworkObject, bool result, ClientRpcParams rpcParams)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(3862296847U, rpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<NetworkObjectReference>(in interactorNetworkObject, default(FastBufferWriter.ForNetworkSerializable));
				fastBufferWriter.WriteValueSafe<bool>(in result, default(FastBufferWriter.ForPrimitives));
				base.__endSendClientRpc(ref fastBufferWriter, 3862296847U, rpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			NetworkObject networkObject;
			if (interactorNetworkObject.TryGet(out networkObject, null))
			{
				InteractorBase component = networkObject.GetComponent<InteractorBase>();
				if (component)
				{
					if (result)
					{
						component.HandleInteractionSucceeded(this);
						return;
					}
					component.HandleFailedInteraction(this);
				}
			}
		}

		// Token: 0x06000467 RID: 1127 RVA: 0x0001738C File Offset: 0x0001558C
		public void SetInteractionResultSprite(Sprite newInteractionResultSprite)
		{
			foreach (InteractableCollider interactableCollider in this.interactableColliders)
			{
				interactableCollider.SetInteractionResultSprite(newInteractionResultSprite);
			}
		}

		// Token: 0x06000468 RID: 1128 RVA: 0x000173E0 File Offset: 0x000155E0
		[ServerRpc(RequireOwnership = false)]
		protected virtual void AttemptInteract_ServerRPC(NetworkObjectReference interactorNetworkObject, int colliderIndex, ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(3318549560U, serverRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<NetworkObjectReference>(in interactorNetworkObject, default(FastBufferWriter.ForNetworkSerializable));
				BytePacker.WriteValueBitPacked(fastBufferWriter, colliderIndex);
				base.__endSendServerRpc(ref fastBufferWriter, 3318549560U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			NetworkObject networkObject;
			if (colliderIndex >= 0 && this.interactableColliders.Count > colliderIndex && interactorNetworkObject.TryGet(out networkObject, null))
			{
				InteractorBase component = networkObject.GetComponent<InteractorBase>();
				if (component != null)
				{
					bool flag = this.interactableColliders[colliderIndex].CheckInteractionDistance(component) && this.AttemptInteract_ServerLogic(component, colliderIndex);
					ClientRpcParams clientRpcParams = new ClientRpcParams
					{
						Send = new ClientRpcSendParams
						{
							TargetClientIds = new ulong[]
							{
								base.OwnerClientId,
								serverRpcParams.Receive.SenderClientId
							}
						}
					};
					this.AttemptInteractResult_ClientRPC(interactorNetworkObject, flag, clientRpcParams);
					if (flag)
					{
						this.InteractServerLogic(component, colliderIndex);
					}
				}
			}
		}

		// Token: 0x06000469 RID: 1129 RVA: 0x00017586 File Offset: 0x00015786
		protected virtual bool AttemptInteract_ServerLogic(InteractorBase interactor, int colliderIndex)
		{
			return true;
		}

		// Token: 0x0600046A RID: 1130 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void InteractServerLogic(InteractorBase interactor, int colliderIndex)
		{
		}

		// Token: 0x0600046B RID: 1131 RVA: 0x0001758C File Offset: 0x0001578C
		public void AttemptInteract(InteractorBase interactor, InteractableCollider interactableCollider)
		{
			int num = this.interactableColliders.IndexOf(interactableCollider);
			if (num >= 0)
			{
				this.AttemptInteract_ServerRPC(interactor.NetworkObject, num, default(ServerRpcParams));
			}
		}

		// Token: 0x0600046C RID: 1132 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public virtual void InteractionStopped(InteractorBase interactor)
		{
		}

		// Token: 0x0600046D RID: 1133 RVA: 0x000175C8 File Offset: 0x000157C8
		public void SetAnchorPosition(global::UnityEngine.Vector3? anchorPosition, int index)
		{
			if (index >= this.interactableColliders.Count)
			{
				throw new ArgumentOutOfRangeException("index", "Provided index to Anchor Override is greater than the number of available colliders");
			}
			while (this.overridePositions.Count <= index)
			{
				this.overridePositions.Add(null);
			}
			this.overridePositions[index] = anchorPosition;
		}

		// Token: 0x0600046E RID: 1134 RVA: 0x0001762C File Offset: 0x0001582C
		public void SetUsePlayerAnchor(bool newValue, int index)
		{
			if (index >= this.interactableColliders.Count)
			{
				throw new ArgumentOutOfRangeException("index", "Provided index to Anchor Override is greater than the number of available colliders");
			}
			while (this.usePlayerAnchor.Count <= index)
			{
				this.usePlayerAnchor.Add(false);
			}
			this.usePlayerAnchor[index] = newValue;
		}

		// Token: 0x06000470 RID: 1136 RVA: 0x000176FC File Offset: 0x000158FC
		protected override void __initializeVariables()
		{
			bool flag = this.interactionAnimation == null;
			if (flag)
			{
				throw new Exception("InteractableBase.interactionAnimation cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.interactionAnimation.Initialize(this);
			base.__nameNetworkVariable(this.interactionAnimation, "interactionAnimation");
			this.NetworkVariableFields.Add(this.interactionAnimation);
			flag = this.interactionDuration == null;
			if (flag)
			{
				throw new Exception("InteractableBase.interactionDuration cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.interactionDuration.Initialize(this);
			base.__nameNetworkVariable(this.interactionDuration, "interactionDuration");
			this.NetworkVariableFields.Add(this.interactionDuration);
			flag = this.isHeldInteraction == null;
			if (flag)
			{
				throw new Exception("InteractableBase.isHeldInteraction cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.isHeldInteraction.Initialize(this);
			base.__nameNetworkVariable(this.isHeldInteraction, "isHeldInteraction");
			this.NetworkVariableFields.Add(this.isHeldInteraction);
			flag = this.hidePromptDuringInteraction == null;
			if (flag)
			{
				throw new Exception("InteractableBase.hidePromptDuringInteraction cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.hidePromptDuringInteraction.Initialize(this);
			base.__nameNetworkVariable(this.hidePromptDuringInteraction, "hidePromptDuringInteraction");
			this.NetworkVariableFields.Add(this.hidePromptDuringInteraction);
			flag = this.overridePositions == null;
			if (flag)
			{
				throw new Exception("InteractableBase.overridePositions cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.overridePositions.Initialize(this);
			base.__nameNetworkVariable(this.overridePositions, "overridePositions");
			this.NetworkVariableFields.Add(this.overridePositions);
			flag = this.usePlayerAnchor == null;
			if (flag)
			{
				throw new Exception("InteractableBase.usePlayerAnchor cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.usePlayerAnchor.Initialize(this);
			base.__nameNetworkVariable(this.usePlayerAnchor, "usePlayerAnchor");
			this.NetworkVariableFields.Add(this.usePlayerAnchor);
			flag = this.isInteractable == null;
			if (flag)
			{
				throw new Exception("InteractableBase.isInteractable cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.isInteractable.Initialize(this);
			base.__nameNetworkVariable(this.isInteractable, "isInteractable");
			this.NetworkVariableFields.Add(this.isInteractable);
			base.__initializeVariables();
		}

		// Token: 0x06000471 RID: 1137 RVA: 0x00017930 File Offset: 0x00015B30
		protected override void __initializeRpcs()
		{
			base.__registerRpc(3862296847U, new NetworkBehaviour.RpcReceiveHandler(InteractableBase.__rpc_handler_3862296847), "AttemptInteractResult_ClientRPC");
			base.__registerRpc(3318549560U, new NetworkBehaviour.RpcReceiveHandler(InteractableBase.__rpc_handler_3318549560), "AttemptInteract_ServerRPC");
			base.__initializeRpcs();
		}

		// Token: 0x06000472 RID: 1138 RVA: 0x00017980 File Offset: 0x00015B80
		private static void __rpc_handler_3862296847(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			NetworkObjectReference networkObjectReference;
			reader.ReadValueSafe<NetworkObjectReference>(out networkObjectReference, default(FastBufferWriter.ForNetworkSerializable));
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((InteractableBase)target).AttemptInteractResult_ClientRPC(networkObjectReference, flag, client);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000473 RID: 1139 RVA: 0x00017A20 File Offset: 0x00015C20
		private static void __rpc_handler_3318549560(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			NetworkObjectReference networkObjectReference;
			reader.ReadValueSafe<NetworkObjectReference>(out networkObjectReference, default(FastBufferWriter.ForNetworkSerializable));
			int num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((InteractableBase)target).AttemptInteract_ServerRPC(networkObjectReference, num, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000474 RID: 1140 RVA: 0x00017AAF File Offset: 0x00015CAF
		protected internal override string __getTypeName()
		{
			return "InteractableBase";
		}

		// Token: 0x040003BA RID: 954
		[SerializeField]
		private global::UnityEngine.Vector3 interactionPromptOffset = global::UnityEngine.Vector3.up;

		// Token: 0x040003BB RID: 955
		protected readonly NetworkVariable<InteractionAnimation> interactionAnimation = new NetworkVariable<InteractionAnimation>(InteractionAnimation.Default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x040003BC RID: 956
		protected readonly NetworkVariable<float> interactionDuration = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x040003BD RID: 957
		protected readonly NetworkVariable<bool> isHeldInteraction = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x040003BE RID: 958
		protected readonly NetworkVariable<bool> hidePromptDuringInteraction = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x040003BF RID: 959
		private readonly NetworkList<NetworkedNullableVector3> overridePositions = new NetworkList<NetworkedNullableVector3>();

		// Token: 0x040003C0 RID: 960
		private readonly NetworkList<bool> usePlayerAnchor = new NetworkList<bool>();

		// Token: 0x040003C1 RID: 961
		private readonly NetworkList<bool> isInteractable = new NetworkList<bool>();

		// Token: 0x040003C2 RID: 962
		[SerializeField]
		protected List<InteractableCollider> interactableColliders;
	}
}
