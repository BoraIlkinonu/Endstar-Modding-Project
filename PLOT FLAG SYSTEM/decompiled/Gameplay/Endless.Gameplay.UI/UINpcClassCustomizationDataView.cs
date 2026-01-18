using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Matchmaking;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared;
using UnityEngine;

namespace Endless.Gameplay.UI;

public abstract class UINpcClassCustomizationDataView<TModel> : UIBaseView<TModel, UINpcClassCustomizationDataView<TModel>.Styles>, IUIInteractable, IUINpcClassCustomizationDataViewable where TModel : NpcClassCustomizationData
{
	public enum Styles
	{
		Default
	}

	[SerializeField]
	private UIDropdownEnum npcClassDropdown;

	public override Styles Style { get; protected set; }

	public event Action<NpcClass> OnNpcClassChanged;

	protected virtual void Start()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("Start", this);
		}
		List<int> list = new List<int>();
		if (!EndlessCloudService.CanHaveGrunt())
		{
			list.Add(1);
		}
		if (!EndlessCloudService.CanHaveRiflemen())
		{
			list.Add(2);
		}
		if (!EndlessCloudService.CanHaveZombies())
		{
			list.Add(3);
		}
		npcClassDropdown.SetOptionIndexesToHide(list.ToArray());
		npcClassDropdown.OnEnumValueChanged.AddListener(OnNpcClassDropdownChanged);
	}

	public override void View(TModel model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "View", "model", model), this);
		}
		npcClassDropdown.SetEnumValue(model.NpcClass, triggerValueChanged: false);
	}

	public void SetInteractable(bool interactable)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetInteractable", interactable);
		}
		npcClassDropdown.SetIsInteractable(interactable);
	}

	private void OnNpcClassDropdownChanged(Enum npcClassAsEnum)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "OnNpcClassDropdownChanged", "npcClassAsEnum", npcClassAsEnum), this);
		}
		NpcClass obj = (NpcClass)(object)npcClassAsEnum;
		this.OnNpcClassChanged?.Invoke(obj);
	}
}
