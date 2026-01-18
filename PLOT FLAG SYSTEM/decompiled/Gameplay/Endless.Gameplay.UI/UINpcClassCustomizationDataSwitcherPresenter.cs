using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UINpcClassCustomizationDataSwitcherPresenter : UIBasePresenter<NpcClassCustomizationData>
{
	[Header("UINpcClassCustomizationDataSwitcherPresenter")]
	[SerializeField]
	private UIBlankNpcCustomizationDataPresenter blankNpcCustomizationDataPresenter;

	[SerializeField]
	private UIGruntNpcCustomizationDataPresenter gruntNpcCustomizationDataPresenter;

	[SerializeField]
	private UIRiflemanNpcCustomizationDataPresenter riflemanNpcCustomizationDataPresenter;

	[SerializeField]
	private UIZombieNpcCustomizationDataPresenter zombieNpcCustomizationDataPresenter;

	private new Dictionary<Type, IUINpcClassCustomizationDataPresentable> presenterDictionary;

	private bool initialized;

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

	public override void SetModel(NpcClassCustomizationData model, bool triggerOnModelChanged)
	{
		if (!initialized)
		{
			Initialize();
		}
		Type type = model.GetType();
		if (!presenterDictionary.TryGetValue(type, out var value))
		{
			DebugUtility.LogException(new KeyNotFoundException("Could not get " + type.Name + " out of presenterDictionary!"), this);
			return;
		}
		value.SetModelAsObject(model, triggerOnModelChanged: false);
		base.SetModel(model, triggerOnModelChanged);
	}

	private void Initialize()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Initialize");
		}
		if (initialized)
		{
			return;
		}
		presenterDictionary = new Dictionary<Type, IUINpcClassCustomizationDataPresentable>
		{
			{
				typeof(BlankNpcCustomizationData),
				blankNpcCustomizationDataPresenter
			},
			{
				typeof(GruntNpcCustomizationData),
				gruntNpcCustomizationDataPresenter
			},
			{
				typeof(RiflemanNpcCustomizationData),
				riflemanNpcCustomizationDataPresenter
			},
			{
				typeof(ZombieNpcCustomizationData),
				zombieNpcCustomizationDataPresenter
			}
		};
		foreach (KeyValuePair<Type, IUINpcClassCustomizationDataPresentable> item in presenterDictionary)
		{
			item.Value.OnNpcClassChanged += OnNpcClassChanged;
			item.Value.OnModelChanged += base.SetModelAsObjectAndTriggerOnModelChanged;
		}
		initialized = true;
	}

	private void OnNpcClassChanged(NpcClass npcClass)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnNpcClassChanged", npcClass);
		}
		SetModel(npcClass switch
		{
			NpcClass.Blank => new BlankNpcCustomizationData(), 
			NpcClass.Grunt => new GruntNpcCustomizationData(), 
			NpcClass.Rifleman => new RiflemanNpcCustomizationData(), 
			NpcClass.Zombie => new ZombieNpcCustomizationData(), 
			_ => throw new ArgumentOutOfRangeException("npcClass", npcClass, null), 
		}, triggerOnModelChanged: true);
	}
}
