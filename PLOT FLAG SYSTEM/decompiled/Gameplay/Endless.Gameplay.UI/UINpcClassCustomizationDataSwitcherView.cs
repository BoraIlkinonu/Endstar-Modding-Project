using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UINpcClassCustomizationDataSwitcherView : UIBaseView<NpcClassCustomizationData, UINpcClassCustomizationDataSwitcherView.Styles>, IUIInteractable
{
	public enum Styles
	{
		Default
	}

	[SerializeField]
	private UIBlankNpcCustomizationDataView blankNpcCustomizationDataView;

	[SerializeField]
	private UIGruntNpcCustomizationDataView gruntNpcCustomizationDataView;

	[SerializeField]
	private UIRiflemanNpcCustomizationDataView riflemanNpcCustomizationDataView;

	[SerializeField]
	private UIZombieNpcCustomizationDataView zombieNpcCustomizationDataView;

	private Dictionary<Type, GameObject> typeToViewDictionary;

	private bool initialized;

	[field: Header("UINpcClassCustomizationDataSwitcherView")]
	[field: SerializeField]
	public override Styles Style { get; protected set; }

	private void Awake()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Awake");
		}
		if (!initialized)
		{
			Initialize();
		}
	}

	public override void View(NpcClassCustomizationData model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "View", model);
		}
		if (!initialized)
		{
			Initialize();
		}
		Type type = model.GetType();
		foreach (KeyValuePair<Type, GameObject> item in typeToViewDictionary)
		{
			bool active = type == item.Key;
			item.Value.SetActive(active);
		}
	}

	public override void Clear()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
		if (!initialized)
		{
			Initialize();
		}
		foreach (KeyValuePair<Type, GameObject> item in typeToViewDictionary)
		{
			item.Value.SetActive(value: false);
		}
	}

	public void SetInteractable(bool interactable)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetInteractable", interactable);
		}
		blankNpcCustomizationDataView.SetInteractable(interactable);
		gruntNpcCustomizationDataView.SetInteractable(interactable);
		riflemanNpcCustomizationDataView.SetInteractable(interactable);
		zombieNpcCustomizationDataView.SetInteractable(interactable);
	}

	private void Initialize()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Initialize");
		}
		if (!initialized)
		{
			typeToViewDictionary = new Dictionary<Type, GameObject>
			{
				{
					typeof(BlankNpcCustomizationData),
					blankNpcCustomizationDataView.gameObject
				},
				{
					typeof(GruntNpcCustomizationData),
					gruntNpcCustomizationDataView.gameObject
				},
				{
					typeof(RiflemanNpcCustomizationData),
					riflemanNpcCustomizationDataView.gameObject
				},
				{
					typeof(ZombieNpcCustomizationData),
					zombieNpcCustomizationDataView.gameObject
				}
			};
			initialized = true;
		}
	}
}
