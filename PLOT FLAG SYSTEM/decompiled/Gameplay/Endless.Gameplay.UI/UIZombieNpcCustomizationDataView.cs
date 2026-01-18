using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIZombieNpcCustomizationDataView : UINpcClassCustomizationDataView<ZombieNpcCustomizationData>
{
	[Header("UIZombieNpcCustomizationDataView")]
	[SerializeField]
	private UIToggle zombifyTargetToggle;

	public event Action<bool> OnZombifyTargetChanged;

	protected override void Start()
	{
		base.Start();
		zombifyTargetToggle.OnChange.AddListener(OnZombifyTargetTogglePressed);
	}

	public override void View(ZombieNpcCustomizationData model)
	{
		base.View(model);
		zombifyTargetToggle.SetIsOn(model.ZombifyTarget, suppressOnChange: false);
	}

	public override void Clear()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
		zombifyTargetToggle.SetIsOn(state: false, suppressOnChange: true);
	}

	private void OnZombifyTargetTogglePressed(bool zombifyTarget)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnZombifyTargetTogglePressed", zombifyTarget);
		}
		this.OnZombifyTargetChanged?.Invoke(zombifyTarget);
	}
}
