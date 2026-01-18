using System;
using System.Collections;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.PlayerInventory
{
	// Token: 0x0200059C RID: 1436
	public class PlayerEquipment : NetworkBehaviour
	{
		// Token: 0x1700067E RID: 1662
		// (get) Token: 0x0600228C RID: 8844 RVA: 0x0009E97A File Offset: 0x0009CB7A
		public VisualEquipmentSlot EquipmentVisualSlot
		{
			get
			{
				return this.equipmentSlot;
			}
		}

		// Token: 0x1700067F RID: 1663
		// (get) Token: 0x0600228D RID: 8845 RVA: 0x0009E982 File Offset: 0x0009CB82
		public PlayerReferenceManager CarriedPlayerReference
		{
			get
			{
				return this.carriedPlayerReference;
			}
		}

		// Token: 0x0600228E RID: 8846 RVA: 0x0009E98A File Offset: 0x0009CB8A
		private void Awake()
		{
			this.DisableVisuals();
		}

		// Token: 0x0600228F RID: 8847 RVA: 0x0009E992 File Offset: 0x0009CB92
		public override void OnNetworkSpawn()
		{
			if (!base.IsHost && !base.IsServer)
			{
				base.StartCoroutine("SetupProcess");
			}
		}

		// Token: 0x06002290 RID: 8848 RVA: 0x0009E9B0 File Offset: 0x0009CBB0
		private IEnumerator SetupProcess()
		{
			while (!MonoBehaviourSingleton<PlayerManager>.Instance.IsPlayerInitialized(base.OwnerClientId))
			{
				yield return null;
			}
			PlayerEquipmentManager playerEquipmentManager = MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayerObject(base.OwnerClientId).PlayerEquipmentManager;
			yield break;
		}

		// Token: 0x06002291 RID: 8849 RVA: 0x0009E9BF File Offset: 0x0009CBBF
		public void EnableVisuals(PlayerReferenceManager carrier)
		{
			this.carriedPlayerReference = carrier;
			if (carrier && carrier.ApperanceController && carrier.ApperanceController.AppearanceAnimator)
			{
				this.EnableVisuals(true);
				return;
			}
			this.EnableVisuals(false);
		}

		// Token: 0x06002292 RID: 8850 RVA: 0x0009EA00 File Offset: 0x0009CC00
		protected virtual void EnableVisuals(bool objectVisualsReady)
		{
			if (this.visualsEnabled)
			{
				return;
			}
			this.visualsEnabled = true;
			if (this.carriedPlayerReference != null)
			{
				this.carriedPlayerReference.ApperanceController.OnEquipmentInUseChanged.AddListener(new UnityAction<SerializableGuid, bool>(this.HandleInUseChangedCheck));
				this.carriedPlayerReference.ApperanceController.OnEquipmentAvailableChanged.AddListener(new UnityAction<SerializableGuid, bool>(this.HandleAvailableChangedCheck));
				this.carriedPlayerReference.ApperanceController.OnEquipmentCooldownChanged.AddListener(new UnityAction<SerializableGuid, float, float>(this.HandleCooldownChangedCheck));
				this.carriedPlayerReference.ApperanceController.OnEquipmentResourceChanged.AddListener(new UnityAction<SerializableGuid, float>(this.HandleResourceChangedCheck));
				this.carriedPlayerReference.ApperanceController.OnEquipmentUseStateChanged.AddListener(new UnityAction<SerializableGuid, UsableDefinition.UseState>(this.HandleEquipmentUseStateChangedCheck));
			}
			this.visuals.SetActive(true);
		}

		// Token: 0x06002293 RID: 8851 RVA: 0x0009EAE0 File Offset: 0x0009CCE0
		public void DisableVisuals()
		{
			if (this.carriedPlayerReference != null && this.carriedPlayerReference.ApperanceController && this.carriedPlayerReference.ApperanceController.AppearanceAnimator)
			{
				this.DisableVisuals(true);
				return;
			}
			this.DisableVisuals(false);
		}

		// Token: 0x06002294 RID: 8852 RVA: 0x0009EB34 File Offset: 0x0009CD34
		protected virtual void DisableVisuals(bool objectVisualsReady)
		{
			if (!this.visualsEnabled)
			{
				return;
			}
			this.visualsEnabled = false;
			if (this.carriedPlayerReference != null)
			{
				this.carriedPlayerReference.ApperanceController.OnEquipmentInUseChanged.RemoveListener(new UnityAction<SerializableGuid, bool>(this.HandleInUseChangedCheck));
				this.carriedPlayerReference.ApperanceController.OnEquipmentAvailableChanged.RemoveListener(new UnityAction<SerializableGuid, bool>(this.HandleAvailableChangedCheck));
				this.carriedPlayerReference.ApperanceController.OnEquipmentCooldownChanged.RemoveListener(new UnityAction<SerializableGuid, float, float>(this.HandleCooldownChangedCheck));
				this.carriedPlayerReference.ApperanceController.OnEquipmentResourceChanged.RemoveListener(new UnityAction<SerializableGuid, float>(this.HandleResourceChangedCheck));
				this.carriedPlayerReference.ApperanceController.OnEquipmentUseStateChanged.RemoveListener(new UnityAction<SerializableGuid, UsableDefinition.UseState>(this.HandleEquipmentUseStateChangedCheck));
			}
			this.carriedPlayerReference = null;
			this.visuals.SetActive(false);
		}

		// Token: 0x06002295 RID: 8853 RVA: 0x0009EC1A File Offset: 0x0009CE1A
		private void HandleInUseChangedCheck(SerializableGuid guid, bool inUse)
		{
			if (guid.Equals(this.inventoryUsableDefintion.Guid))
			{
				this.HandleInUseChanged(inUse);
			}
		}

		// Token: 0x06002296 RID: 8854 RVA: 0x0009EC37 File Offset: 0x0009CE37
		private void HandleAvailableChangedCheck(SerializableGuid guid, bool available)
		{
			if (guid.Equals(this.inventoryUsableDefintion.Guid))
			{
				this.HandleAvailableChanged(available);
			}
		}

		// Token: 0x06002297 RID: 8855 RVA: 0x0009EC54 File Offset: 0x0009CE54
		private void HandleCooldownChangedCheck(SerializableGuid guid, float currentSeconds, float totalSeconds)
		{
			if (guid.Equals(this.inventoryUsableDefintion.Guid))
			{
				this.HandleCooldownChanged(currentSeconds, totalSeconds);
			}
		}

		// Token: 0x06002298 RID: 8856 RVA: 0x0009EC72 File Offset: 0x0009CE72
		private void HandleResourceChangedCheck(SerializableGuid guid, float percent)
		{
			if (guid.Equals(this.inventoryUsableDefintion.Guid))
			{
				this.HandleResourceChanged(percent);
			}
		}

		// Token: 0x06002299 RID: 8857 RVA: 0x0009EC8F File Offset: 0x0009CE8F
		private void HandleEquipmentUseStateChangedCheck(SerializableGuid guid, UsableDefinition.UseState eus)
		{
			if (guid.Equals(this.inventoryUsableDefintion.Guid))
			{
				this.HandleEquipmentUseStateChanged(eus);
			}
		}

		// Token: 0x0600229A RID: 8858 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void HandleInUseChanged(bool inUse)
		{
		}

		// Token: 0x0600229B RID: 8859 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void HandleAvailableChanged(bool available)
		{
		}

		// Token: 0x0600229C RID: 8860 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void HandleCooldownChanged(float currentSeconds, float totalSeconds)
		{
		}

		// Token: 0x0600229D RID: 8861 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void HandleResourceChanged(float percent)
		{
		}

		// Token: 0x0600229E RID: 8862 RVA: 0x00002DB0 File Offset: 0x00000FB0
		protected virtual void HandleEquipmentUseStateChanged(UsableDefinition.UseState eus)
		{
		}

		// Token: 0x060022A0 RID: 8864 RVA: 0x0009ECBC File Offset: 0x0009CEBC
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x060022A1 RID: 8865 RVA: 0x0000E74E File Offset: 0x0000C94E
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x060022A2 RID: 8866 RVA: 0x0009ECD2 File Offset: 0x0009CED2
		protected internal override string __getTypeName()
		{
			return "PlayerEquipment";
		}

		// Token: 0x04001B7F RID: 7039
		[SerializeField]
		private VisualEquipmentSlot equipmentSlot;

		// Token: 0x04001B80 RID: 7040
		[SerializeField]
		protected InventoryUsableDefinition inventoryUsableDefintion;

		// Token: 0x04001B81 RID: 7041
		[SerializeField]
		protected GameObject visuals;

		// Token: 0x04001B82 RID: 7042
		protected PlayerReferenceManager carriedPlayerReference;

		// Token: 0x04001B83 RID: 7043
		private bool visualsEnabled = true;
	}
}
