using System;
using Endless.Gameplay.Scripting;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x020000DB RID: 219
	public abstract class InteractorBase : NetworkBehaviour
	{
		// Token: 0x170000B6 RID: 182
		// (get) Token: 0x0600048C RID: 1164 RVA: 0x00017D94 File Offset: 0x00015F94
		public float InteractOffset
		{
			get
			{
				return this.interactOffset;
			}
		}

		// Token: 0x170000B7 RID: 183
		// (get) Token: 0x0600048D RID: 1165 RVA: 0x00017D9C File Offset: 0x00015F9C
		public float InteractRadius
		{
			get
			{
				return this.interactRadius;
			}
		}

		// Token: 0x170000B8 RID: 184
		// (get) Token: 0x0600048E RID: 1166 RVA: 0x00017DA4 File Offset: 0x00015FA4
		protected virtual bool IsActive
		{
			get
			{
				return base.IsServer;
			}
		}

		// Token: 0x170000B9 RID: 185
		// (get) Token: 0x0600048F RID: 1167 RVA: 0x00017DAC File Offset: 0x00015FAC
		protected virtual global::UnityEngine.Vector3 Position
		{
			get
			{
				return base.transform.position;
			}
		}

		// Token: 0x170000BA RID: 186
		// (get) Token: 0x06000490 RID: 1168 RVA: 0x00017DB9 File Offset: 0x00015FB9
		protected virtual global::UnityEngine.Vector3 Forward
		{
			get
			{
				return base.transform.forward;
			}
		}

		// Token: 0x170000BB RID: 187
		// (get) Token: 0x06000491 RID: 1169
		public abstract Context ContextObject { get; }

		// Token: 0x170000BC RID: 188
		// (get) Token: 0x06000492 RID: 1170 RVA: 0x00017DC6 File Offset: 0x00015FC6
		// (set) Token: 0x06000493 RID: 1171 RVA: 0x00017DD0 File Offset: 0x00015FD0
		public InteractableBase CurrentInteractable
		{
			get
			{
				return this.currentInteractable;
			}
			protected set
			{
				if (this.currentInteractable != value)
				{
					this.interactionStartTime = Time.time;
					InteractableBase interactableBase = this.currentInteractable;
					this.currentInteractable = value;
					this.OnInteractionChanged.Invoke(interactableBase, value);
				}
			}
		}

		// Token: 0x170000BD RID: 189
		// (get) Token: 0x06000494 RID: 1172 RVA: 0x00017E11 File Offset: 0x00016011
		// (set) Token: 0x06000495 RID: 1173 RVA: 0x00017E1C File Offset: 0x0001601C
		public InteractableCollider CurrentInteractableTarget
		{
			get
			{
				return this.currentInteractableTarget;
			}
			protected set
			{
				if (value != this.currentInteractableTarget)
				{
					if (this.currentInteractableTarget != null)
					{
						this.currentInteractableTarget.UnselectLocally();
					}
					this.currentInteractableTarget = value;
					if (this.currentInteractableTarget != null)
					{
						this.currentInteractableTarget.SelectLocally();
					}
				}
			}
		}

		// Token: 0x170000BE RID: 190
		// (get) Token: 0x06000496 RID: 1174 RVA: 0x00017E70 File Offset: 0x00016070
		public float InteractionTime
		{
			get
			{
				return Time.time - this.interactionStartTime;
			}
		}

		// Token: 0x06000497 RID: 1175 RVA: 0x00017E80 File Offset: 0x00016080
		protected virtual void Update()
		{
			if (this.IsActive)
			{
				global::UnityEngine.Vector3 vector = this.Position + this.Forward * this.interactOffset + global::UnityEngine.Vector3.up * this.interactHeightOffset;
				InteractableCollider interactableCollider = null;
				float num = 0f;
				int num2 = Physics.OverlapSphereNonAlloc(vector, this.interactRadius, this.collisionResults, this.layerMask, QueryTriggerInteraction.Collide);
				int num3 = 0;
				while (num3 < this.collisionResults.Length && num3 < num2)
				{
					if (this.collisionResults[num3])
					{
						InteractableCollider component = this.collisionResults[num3].GetComponent<InteractableCollider>();
						if (component && (component.IsInteractable || component.InteractableBase == this.CurrentInteractable))
						{
							if (!interactableCollider)
							{
								interactableCollider = component;
								num = this.GetRelevancyScore(component);
							}
							else
							{
								float relevancyScore = this.GetRelevancyScore(component);
								if (relevancyScore > num)
								{
									interactableCollider = component;
									num = relevancyScore;
								}
							}
						}
					}
					num3++;
				}
				this.CurrentInteractableTarget = interactableCollider;
				return;
			}
			this.CurrentInteractableTarget = null;
		}

		// Token: 0x06000498 RID: 1176 RVA: 0x00017F87 File Offset: 0x00016187
		public override void OnNetworkDespawn()
		{
			if (base.IsServer)
			{
				this.ServerProcessAbandonInteraction();
			}
		}

		// Token: 0x06000499 RID: 1177 RVA: 0x00017F97 File Offset: 0x00016197
		public virtual void HandleFailedInteraction(InteractableBase interactable)
		{
			this.CurrentInteractable = null;
		}

		// Token: 0x0600049A RID: 1178 RVA: 0x00017FA0 File Offset: 0x000161A0
		public virtual void HandleInteractionSucceeded(InteractableBase interactableBase)
		{
			this.CurrentInteractable = interactableBase;
		}

		// Token: 0x0600049B RID: 1179 RVA: 0x00017FAC File Offset: 0x000161AC
		private float GetRelevancyScore(InteractableCollider interactableCollider)
		{
			if (interactableCollider.InteractableBase == this.CurrentInteractable)
			{
				return 1000f;
			}
			global::UnityEngine.Vector3 position = interactableCollider.transform.position;
			float num = Mathf.Min(1f, global::UnityEngine.Vector3.Angle(position - this.Position, this.Forward));
			float num2 = global::UnityEngine.Vector3.Distance(position, this.Position);
			return 1f - num / 90f + (1f - num2 / (this.interactOffset + this.interactRadius * 2f));
		}

		// Token: 0x0600049C RID: 1180 RVA: 0x00018038 File Offset: 0x00016238
		[ServerRpc(RequireOwnership = false)]
		protected void AbandonInteraction_ServerRPC()
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				ServerRpcParams serverRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(3173210109U, serverRpcParams, RpcDelivery.Reliable);
				base.__endSendServerRpc(ref fastBufferWriter, 3173210109U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.ServerProcessAbandonInteraction();
		}

		// Token: 0x0600049D RID: 1181 RVA: 0x00018111 File Offset: 0x00016311
		private void ServerProcessAbandonInteraction()
		{
			if (this.CurrentInteractable)
			{
				this.CurrentInteractable.InteractionStopped(this);
			}
			this.CurrentInteractable = null;
			this.StopInteracting_ClientRPC();
		}

		// Token: 0x0600049E RID: 1182 RVA: 0x0001813C File Offset: 0x0001633C
		[ClientRpc]
		protected void StopInteracting_ClientRPC()
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1954377598U, clientRpcParams, RpcDelivery.Reliable);
				base.__endSendClientRpc(ref fastBufferWriter, 1954377598U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.CurrentInteractable = null;
		}

		// Token: 0x0600049F RID: 1183 RVA: 0x00018216 File Offset: 0x00016416
		public void HandleHealthZeroed()
		{
			if (this.CurrentInteractable)
			{
				this.AbandonInteraction_ServerRPC();
			}
		}

		// Token: 0x060004A0 RID: 1184 RVA: 0x00017F97 File Offset: 0x00016197
		public void InteractableStoppedInteraction()
		{
			this.CurrentInteractable = null;
		}

		// Token: 0x060004A2 RID: 1186 RVA: 0x0001826C File Offset: 0x0001646C
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x060004A3 RID: 1187 RVA: 0x00018284 File Offset: 0x00016484
		protected override void __initializeRpcs()
		{
			base.__registerRpc(3173210109U, new NetworkBehaviour.RpcReceiveHandler(InteractorBase.__rpc_handler_3173210109), "AbandonInteraction_ServerRPC");
			base.__registerRpc(1954377598U, new NetworkBehaviour.RpcReceiveHandler(InteractorBase.__rpc_handler_1954377598), "StopInteracting_ClientRPC");
			base.__initializeRpcs();
		}

		// Token: 0x060004A4 RID: 1188 RVA: 0x000182D4 File Offset: 0x000164D4
		private static void __rpc_handler_3173210109(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((InteractorBase)target).AbandonInteraction_ServerRPC();
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060004A5 RID: 1189 RVA: 0x00018328 File Offset: 0x00016528
		private static void __rpc_handler_1954377598(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((InteractorBase)target).StopInteracting_ClientRPC();
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060004A6 RID: 1190 RVA: 0x00018379 File Offset: 0x00016579
		protected internal override string __getTypeName()
		{
			return "InteractorBase";
		}

		// Token: 0x040003CB RID: 971
		[SerializeField]
		private float interactOffset = 1f;

		// Token: 0x040003CC RID: 972
		[SerializeField]
		private float interactRadius = 1f;

		// Token: 0x040003CD RID: 973
		[SerializeField]
		private float interactHeightOffset = 0.5f;

		// Token: 0x040003CE RID: 974
		[SerializeField]
		private LayerMask layerMask;

		// Token: 0x040003CF RID: 975
		private readonly Collider[] collisionResults = new Collider[4];

		// Token: 0x040003D0 RID: 976
		private InteractableBase currentInteractable;

		// Token: 0x040003D1 RID: 977
		private float interactionStartTime;

		// Token: 0x040003D2 RID: 978
		private InteractableCollider currentInteractableTarget;

		// Token: 0x040003D3 RID: 979
		public UnityEvent<InteractableBase, InteractableBase> OnInteractionChanged = new UnityEvent<InteractableBase, InteractableBase>();
	}
}
