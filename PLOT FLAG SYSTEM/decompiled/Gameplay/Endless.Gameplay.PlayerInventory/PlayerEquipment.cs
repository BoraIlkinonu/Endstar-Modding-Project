using System.Collections;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay.PlayerInventory;

public class PlayerEquipment : NetworkBehaviour
{
	[SerializeField]
	private VisualEquipmentSlot equipmentSlot;

	[SerializeField]
	protected InventoryUsableDefinition inventoryUsableDefintion;

	[SerializeField]
	protected GameObject visuals;

	protected PlayerReferenceManager carriedPlayerReference;

	private bool visualsEnabled = true;

	public VisualEquipmentSlot EquipmentVisualSlot => equipmentSlot;

	public PlayerReferenceManager CarriedPlayerReference => carriedPlayerReference;

	private void Awake()
	{
		DisableVisuals();
	}

	public override void OnNetworkSpawn()
	{
		if (!base.IsHost && !base.IsServer)
		{
			StartCoroutine("SetupProcess");
		}
	}

	private IEnumerator SetupProcess()
	{
		while (!MonoBehaviourSingleton<PlayerManager>.Instance.IsPlayerInitialized(base.OwnerClientId))
		{
			yield return null;
		}
		_ = MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayerObject(base.OwnerClientId).PlayerEquipmentManager;
	}

	public void EnableVisuals(PlayerReferenceManager carrier)
	{
		carriedPlayerReference = carrier;
		if ((bool)carrier && (bool)carrier.ApperanceController && (bool)carrier.ApperanceController.AppearanceAnimator)
		{
			EnableVisuals(objectVisualsReady: true);
		}
		else
		{
			EnableVisuals(objectVisualsReady: false);
		}
	}

	protected virtual void EnableVisuals(bool objectVisualsReady)
	{
		if (!visualsEnabled)
		{
			visualsEnabled = true;
			if (carriedPlayerReference != null)
			{
				carriedPlayerReference.ApperanceController.OnEquipmentInUseChanged.AddListener(HandleInUseChangedCheck);
				carriedPlayerReference.ApperanceController.OnEquipmentAvailableChanged.AddListener(HandleAvailableChangedCheck);
				carriedPlayerReference.ApperanceController.OnEquipmentCooldownChanged.AddListener(HandleCooldownChangedCheck);
				carriedPlayerReference.ApperanceController.OnEquipmentResourceChanged.AddListener(HandleResourceChangedCheck);
				carriedPlayerReference.ApperanceController.OnEquipmentUseStateChanged.AddListener(HandleEquipmentUseStateChangedCheck);
			}
			visuals.SetActive(value: true);
		}
	}

	public void DisableVisuals()
	{
		if (carriedPlayerReference != null && (bool)carriedPlayerReference.ApperanceController && (bool)carriedPlayerReference.ApperanceController.AppearanceAnimator)
		{
			DisableVisuals(objectVisualsReady: true);
		}
		else
		{
			DisableVisuals(objectVisualsReady: false);
		}
	}

	protected virtual void DisableVisuals(bool objectVisualsReady)
	{
		if (visualsEnabled)
		{
			visualsEnabled = false;
			if (carriedPlayerReference != null)
			{
				carriedPlayerReference.ApperanceController.OnEquipmentInUseChanged.RemoveListener(HandleInUseChangedCheck);
				carriedPlayerReference.ApperanceController.OnEquipmentAvailableChanged.RemoveListener(HandleAvailableChangedCheck);
				carriedPlayerReference.ApperanceController.OnEquipmentCooldownChanged.RemoveListener(HandleCooldownChangedCheck);
				carriedPlayerReference.ApperanceController.OnEquipmentResourceChanged.RemoveListener(HandleResourceChangedCheck);
				carriedPlayerReference.ApperanceController.OnEquipmentUseStateChanged.RemoveListener(HandleEquipmentUseStateChangedCheck);
			}
			carriedPlayerReference = null;
			visuals.SetActive(value: false);
		}
	}

	private void HandleInUseChangedCheck(SerializableGuid guid, bool inUse)
	{
		if (guid.Equals(inventoryUsableDefintion.Guid))
		{
			HandleInUseChanged(inUse);
		}
	}

	private void HandleAvailableChangedCheck(SerializableGuid guid, bool available)
	{
		if (guid.Equals(inventoryUsableDefintion.Guid))
		{
			HandleAvailableChanged(available);
		}
	}

	private void HandleCooldownChangedCheck(SerializableGuid guid, float currentSeconds, float totalSeconds)
	{
		if (guid.Equals(inventoryUsableDefintion.Guid))
		{
			HandleCooldownChanged(currentSeconds, totalSeconds);
		}
	}

	private void HandleResourceChangedCheck(SerializableGuid guid, float percent)
	{
		if (guid.Equals(inventoryUsableDefintion.Guid))
		{
			HandleResourceChanged(percent);
		}
	}

	private void HandleEquipmentUseStateChangedCheck(SerializableGuid guid, UsableDefinition.UseState eus)
	{
		if (guid.Equals(inventoryUsableDefintion.Guid))
		{
			HandleEquipmentUseStateChanged(eus);
		}
	}

	protected virtual void HandleInUseChanged(bool inUse)
	{
	}

	protected virtual void HandleAvailableChanged(bool available)
	{
	}

	protected virtual void HandleCooldownChanged(float currentSeconds, float totalSeconds)
	{
	}

	protected virtual void HandleResourceChanged(float percent)
	{
	}

	protected virtual void HandleEquipmentUseStateChanged(UsableDefinition.UseState eus)
	{
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
		return "PlayerEquipment";
	}
}
