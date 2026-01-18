using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIEquipmentView : UIMonoBehaviourSingleton<UIEquipmentView>
{
	[SerializeField]
	private UIEquippedSlotView[] equippedSlots;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private readonly List<UIEquippedSlotController> equippedSlotControllers = new List<UIEquippedSlotController>();

	public IReadOnlyList<UIEquippedSlotController> EquippedSlotControllers => equippedSlotControllers;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		if (MobileUtility.IsMobile)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayStarted.AddListener(View);
		}
	}

	public void RegisterEquippedSlotController(UIEquippedSlotController equippedSlotController)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "RegisterEquippedSlotController", equippedSlotController);
		}
		equippedSlotControllers.Add(equippedSlotController);
	}

	private void View()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "View");
		}
		for (int i = 0; i < equippedSlots.Length; i++)
		{
			float delay = (float)(i + 1) * 0.25f;
			equippedSlots[i].Initialize(delay);
		}
	}
}
