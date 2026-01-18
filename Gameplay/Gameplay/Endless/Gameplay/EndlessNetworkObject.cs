using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x0200009A RID: 154
	[RequireComponent(typeof(NetworkObject))]
	public class EndlessNetworkObject : NetworkBehaviour
	{
		// Token: 0x060002B6 RID: 694 RVA: 0x0000E7AF File Offset: 0x0000C9AF
		private void Awake()
		{
			if (this.scopeManaged)
			{
				base.NetworkObject.CheckObjectVisibility = (ulong clientId) => NetworkScopeManager.CheckIfInScope(clientId, base.NetworkObject);
			}
		}

		// Token: 0x060002B7 RID: 695 RVA: 0x0000E7D0 File Offset: 0x0000C9D0
		private void OnEnable()
		{
			if (this.scopeManaged)
			{
				NetworkScopeManager.AddNetworkObject(base.NetworkObject);
			}
		}

		// Token: 0x060002B8 RID: 696 RVA: 0x0000E7E5 File Offset: 0x0000C9E5
		private void OnDisable()
		{
			if (this.scopeManaged)
			{
				NetworkScopeManager.RemoveNetworkObject(base.NetworkObject);
			}
		}

		// Token: 0x060002B9 RID: 697 RVA: 0x0000E7FC File Offset: 0x0000C9FC
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			if (!base.IsServer)
			{
				Stage activeStage = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage;
				if (activeStage != null)
				{
					activeStage.TrackNetworkObject(base.NetworkObject.NetworkObjectId, base.gameObject, null);
					return;
				}
				MonoBehaviourSingleton<StageManager>.Instance.OnActiveStageChanged.AddListener(new UnityAction<Stage>(this.HandleActiveStageChanged));
			}
		}

		// Token: 0x060002BA RID: 698 RVA: 0x0000E860 File Offset: 0x0000CA60
		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();
			if (!MonoBehaviourSingleton<StageManager>.Instance)
			{
				return;
			}
			MonoBehaviourSingleton<StageManager>.Instance.OnActiveStageChanged.RemoveListener(new UnityAction<Stage>(this.HandleActiveStageChanged));
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage)
			{
				MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.UntrackNetworkObject(base.gameObject, base.NetworkObject.NetworkObjectId);
			}
		}

		// Token: 0x060002BB RID: 699 RVA: 0x0000E8CC File Offset: 0x0000CACC
		private void HandleActiveStageChanged(Stage stage)
		{
			stage.TrackNetworkObject(base.NetworkObject.NetworkObjectId, base.gameObject, null);
			MonoBehaviourSingleton<StageManager>.Instance.OnActiveStageChanged.RemoveListener(new UnityAction<Stage>(this.HandleActiveStageChanged));
		}

		// Token: 0x060002BE RID: 702 RVA: 0x0000E920 File Offset: 0x0000CB20
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x060002BF RID: 703 RVA: 0x0000E74E File Offset: 0x0000C94E
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x060002C0 RID: 704 RVA: 0x0000E936 File Offset: 0x0000CB36
		protected internal override string __getTypeName()
		{
			return "EndlessNetworkObject";
		}

		// Token: 0x04000289 RID: 649
		[SerializeField]
		private bool scopeManaged = true;
	}
}
