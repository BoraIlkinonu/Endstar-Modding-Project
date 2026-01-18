using System;
using Endless.Gameplay;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UILevelDestinationView : UIBaseView<LevelDestination, UILevelDestinationView.Styles>, IUIInteractable
{
	public enum Styles
	{
		Default,
		None
	}

	[SerializeField]
	private TextMeshProUGUI levelNameText;

	[SerializeField]
	private UIButton openLevelSelectionWindowButton;

	[SerializeField]
	private UIButton openSpawnPointSelectionWindowButton;

	[field: Header("UILevelDestinationView")]
	[field: SerializeField]
	public override Styles Style { get; protected set; }

	public event Action OnOpenLevelSelectionWindow;

	public event Action OnOpenSpawnPointSelectionWindow;

	private void Start()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		openLevelSelectionWindowButton.onClick.AddListener(OnOpenLevelSelectionWindowButtonPressed);
		openSpawnPointSelectionWindowButton.onClick.AddListener(OnSpawnPointSelectionWindowButtonPressed);
	}

	public override void View(LevelDestination model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "View", model);
		}
		if (!model.IsValidLevel())
		{
			SetLevelNameText("None");
		}
	}

	public override void Clear()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
		SetLevelNameText(string.Empty);
	}

	public void SetInteractable(bool interactable)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetInteractable", interactable);
		}
		openLevelSelectionWindowButton.interactable = interactable;
		openSpawnPointSelectionWindowButton.interactable = interactable;
	}

	public void SetLevelNameText(string levelName)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetLevelNameText", levelName);
		}
		levelNameText.text = levelName;
	}

	private void OnOpenLevelSelectionWindowButtonPressed()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnOpenLevelSelectionWindowButtonPressed");
		}
		this.OnOpenLevelSelectionWindow?.Invoke();
	}

	private void OnSpawnPointSelectionWindowButtonPressed()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnSpawnPointSelectionWindowButtonPressed");
		}
		this.OnOpenSpawnPointSelectionWindow?.Invoke();
	}
}
