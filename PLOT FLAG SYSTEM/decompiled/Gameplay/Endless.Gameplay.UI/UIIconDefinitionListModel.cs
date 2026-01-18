using System.Collections.Generic;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIIconDefinitionListModel : UIBaseListModel<IconDefinition>
{
	[Header("UIIconDefinitionListModel")]
	[SerializeField]
	private IconList source;

	public bool Initialized { get; private set; }

	private void Start()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		if (!Initialized)
		{
			Initialize();
		}
	}

	public void Initialize()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Initialize");
		}
		if (!Initialized)
		{
			Initialized = true;
			List<IconDefinition> list = new List<IconDefinition>(source.Definitions);
			list = list.FindAll((IconDefinition characterCosmeticsDefinition) => !IconList.IconDefinitionIsMissingData(characterCosmeticsDefinition));
			Set(list, triggerEvents: true);
		}
	}
}
