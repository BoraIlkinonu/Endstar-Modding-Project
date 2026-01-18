using UnityEngine;

namespace Endless.Gameplay;

public abstract class InventoryUsableDefinition : UsableDefinition
{
	public enum InventoryTypes
	{
		Major,
		Minor
	}

	public enum EquipmentShowPriority
	{
		NotShown,
		MinorOutOfUse,
		Major,
		MinorInUse
	}

	public enum MobileUI_MajorLayoutType
	{
		Generic,
		Melee,
		Ranged
	}

	public enum MobileUI_MinorLayoutType
	{
		Button,
		ButtonWithJoystick
	}

	public struct EventData
	{
		public bool InUse;

		public bool Available;

		public float CooldownSecondsLeft;

		public float CooldownSecondsTotal;

		public float ResourcePercent;

		public uint UseFrame;

		public void Reset()
		{
			InUse = false;
			Available = true;
			CooldownSecondsLeft = 0f;
			CooldownSecondsTotal = 0f;
			ResourcePercent = 1f;
			UseFrame = 0u;
		}

		public void CopyTo(ref EventData target)
		{
			target.InUse = InUse;
			target.Available = Available;
			target.CooldownSecondsLeft = CooldownSecondsLeft;
			target.CooldownSecondsTotal = CooldownSecondsTotal;
			target.ResourcePercent = ResourcePercent;
			target.UseFrame = UseFrame;
		}
	}

	[SerializeField]
	private InventoryTypes inventoryType = InventoryTypes.Minor;

	[SerializeField]
	private bool isStackable;

	[SerializeField]
	[Header("--")]
	private MobileUI_MajorLayoutType mobileMajorLayout;

	[SerializeField]
	private MobileUI_MinorLayoutType mobileMinorLayout;

	[SerializeField]
	[Header("--")]
	private string animationTrigger;

	public InventoryTypes InventoryType => inventoryType;

	public bool IsStackable => isStackable;

	public MobileUI_MajorLayoutType MobileUIMajorLayout => mobileMajorLayout;

	public MobileUI_MinorLayoutType MobileUIMinorLayout => mobileMinorLayout;

	public string AnimationTrigger => animationTrigger;

	public virtual string GetAnimationTrigger(UseState eus, uint currentVisualFrame)
	{
		return AnimationTrigger;
	}

	public virtual EquipmentShowPriority GetShowPriority(UseState eus)
	{
		return EquipmentShowPriority.Major;
	}

	public virtual void GetEventData(NetState state, UseState useState, double appearanceTime, ref EventData data)
	{
		data.Reset();
	}
}
