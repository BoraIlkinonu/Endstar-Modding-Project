using Endless.Gameplay.UI;
using Endless.Shared;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UINewLevelStateModalView : UIEscapableModalView
{
	public enum Contexts
	{
		NewGame,
		Match
	}

	[Header("UINewLevelStateModalView")]
	[SerializeField]
	private UIInputField nameInputField;

	[SerializeField]
	private UIInputField descriptionInputField;

	[SerializeField]
	private UIModalMatchCloseHandler modalMatchCloseHandler;

	private Contexts context;

	public override void OnDisplay(params object[] modalData)
	{
		base.OnDisplay(modalData);
		context = (Contexts)modalData[0];
		modalMatchCloseHandler.enabled = context == Contexts.Match;
		if (MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame != null)
		{
			nameInputField.text = $"Level {MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.levels.Count + 1}";
		}
		else
		{
			nameInputField.text = "Level 1";
		}
		nameInputField.Select();
	}

	public override void Close()
	{
		base.Close();
		modalMatchCloseHandler.enabled = false;
	}

	public override void OnDespawn()
	{
		base.OnDespawn();
		nameInputField.text = string.Empty;
		descriptionInputField.text = string.Empty;
	}
}
