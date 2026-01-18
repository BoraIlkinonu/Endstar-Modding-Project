using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UICharacterCosmeticsDefinitionListModel : UIBaseLocalFilterableListModel<CharacterCosmeticsDefinition>
{
	[Header("UICharacterCosmeticsDefinitionListModel")]
	[SerializeField]
	private CharacterCosmeticsList characterCosmeticsList;

	public bool Initialized { get; private set; }

	protected override Comparison<CharacterCosmeticsDefinition> DefaultSort => (CharacterCosmeticsDefinition x, CharacterCosmeticsDefinition y) => string.Compare(x.DisplayName, y.DisplayName, StringComparison.Ordinal);

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
			List<CharacterCosmeticsDefinition> list = new List<CharacterCosmeticsDefinition>(characterCosmeticsList.Cosmetics);
			list = list.FindAll((CharacterCosmeticsDefinition characterCosmeticsDefinition) => !CharacterCosmeticsList.CharacterCosmeticsDefinitionIsMissingData(characterCosmeticsDefinition));
			Set(list, triggerEvents: true);
			Initialized = true;
		}
	}
}
