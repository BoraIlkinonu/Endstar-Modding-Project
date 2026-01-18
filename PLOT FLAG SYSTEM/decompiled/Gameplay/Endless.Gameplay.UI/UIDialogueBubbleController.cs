using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

[RequireComponent(typeof(UIDialogueBubbleAnchor))]
public class UIDialogueBubbleController : UIGameObject
{
	[SerializeField]
	private UIButton displayNextTextButton;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private UIDialogueBubbleAnchor view;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		TryGetComponent<UIDialogueBubbleAnchor>(out view);
		displayNextTextButton.onClick.AddListener(DisplayNextText);
	}

	private void DisplayNextText()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "DisplayNextText");
		}
		throw new NotImplementedException();
	}
}
