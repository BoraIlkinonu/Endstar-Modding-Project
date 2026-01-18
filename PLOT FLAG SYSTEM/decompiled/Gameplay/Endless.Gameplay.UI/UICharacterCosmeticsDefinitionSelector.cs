using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI;

public class UICharacterCosmeticsDefinitionSelector : UIGameObject
{
	public UnityEvent<CharacterCosmeticsDefinition> OnSelectedCharacterVisual = new UnityEvent<CharacterCosmeticsDefinition>();

	public UnityEvent<SerializableGuid> OnSelectedId = new UnityEvent<SerializableGuid>();

	[SerializeField]
	private CharacterCosmeticsList characterCosmeticsList;

	[SerializeField]
	private CharacterCosmeticsDefinition fallbackCharacterCosmeticsDefinition;

	[SerializeField]
	private UICharacterCosmeticsDefinitionListModel listModel;

	[SerializeField]
	private UICharacterCosmeticsDefinitionListView listView;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		listModel.SelectionChangedUnityEvent.AddListener(HandleOnSelected);
	}

	public void SetSelected(SerializableGuid assetId, bool triggerOnSelected)
	{
		if (verboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} , {3}: {4} )", "SetSelected", "assetId", assetId, "triggerOnSelected", triggerOnSelected), this);
		}
		if (!characterCosmeticsList.TryGetDefinition(assetId, out var definition))
		{
			DebugUtility.LogWarning(string.Format("could not find {0} of {1} in {2}! Using {3}!", "assetId", assetId, "characterCosmeticsList", "fallbackCharacterCosmeticsDefinition"), this);
			definition = fallbackCharacterCosmeticsDefinition;
		}
		SetSelected(definition, triggerOnSelected);
	}

	public void SetSelected(CharacterCosmeticsDefinition characterCosmeticsDefinition, bool triggerOnSelected)
	{
		if (verboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", "SetSelected", "characterCosmeticsDefinition", characterCosmeticsDefinition.DisplayName, "triggerOnSelected", triggerOnSelected), this);
		}
		if (!listModel.Initialized)
		{
			listModel.Initialize();
		}
		int index = listModel.FilteredList.IndexOf(characterCosmeticsDefinition);
		listModel.Select(index, triggerOnSelected);
		if (!triggerOnSelected)
		{
			listView.SetDataToAllVisibleCells();
		}
	}

	private void HandleOnSelected(int index, bool selected)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "HandleOnSelected", index, selected);
		}
		if (selected)
		{
			CharacterCosmeticsDefinition characterCosmeticsDefinition = listModel[index];
			OnSelectedCharacterVisual.Invoke(characterCosmeticsDefinition);
			OnSelectedId.Invoke(characterCosmeticsDefinition.AssetId);
		}
	}
}
