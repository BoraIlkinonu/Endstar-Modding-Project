using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI;

public class UIIconDefinitionSelector : UIGameObject
{
	[SerializeField]
	private UIIconDefinitionListModel listModel;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	public UnityEvent<IconDefinition> OnSelectedUnityEvent { get; } = new UnityEvent<IconDefinition>();

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		listModel.ItemSelectedUnityEvent.AddListener(OnSelection);
	}

	public void Initialize(IconDefinition selection, bool triggerOnSelected)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Initialize", selection, triggerOnSelected);
		}
		listModel.ClearSelected(triggerEvents: true);
		if (!listModel.Initialized)
		{
			listModel.Initialize();
		}
		if (!(selection == null))
		{
			int num = listModel.ReadOnlyList.IndexOf(selection);
			if (num < 0)
			{
				DebugUtility.LogError($"Could not find {selection} in list!", this);
			}
			else
			{
				listModel.Select(num, triggerOnSelected);
			}
		}
	}

	private void OnSelection(int index)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnSelection", index);
		}
		IconDefinition arg = listModel[index];
		OnSelectedUnityEvent.Invoke(arg);
	}
}
