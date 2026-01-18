using System;
using System.IO;
using System.Text.RegularExpressions;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Endless.Shared.Validation;
using SFB;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIScreenshotController : UIGameObject, IValidatable
{
	[SerializeField]
	private UIButton saveToDiskButton;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private UIScreenshotView view;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		saveToDiskButton.onClick.AddListener(SetUpSaveToDisk);
		TryGetComponent<UIScreenshotView>(out view);
	}

	[ContextMenu("Validate")]
	public void Validate()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Validate");
		}
		DebugUtility.DebugHasComponent<UIScreenshotView>(base.gameObject);
	}

	private void SetUpSaveToDisk()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetUpSaveToDisk");
		}
		string directory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
		if (PlayerPrefs.HasKey("Last Screenshot Directory Save"))
		{
			string text = PlayerPrefs.GetString("Last Screenshot Directory Save");
			if (Directory.Exists(text))
			{
				directory = text;
			}
		}
		StandaloneFileBrowser.OpenFolderPanelAsync("Select A Folder To Save Screenshot To", directory, multiselect: false, SaveToDisk);
	}

	private void SaveToDisk(string[] paths)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SaveToDisk", paths.Length);
		}
		if (paths.Length != 0)
		{
			if (paths.Length > 1)
			{
				DebugUtility.LogWarning(this, "SaveToDisk", "Don't know how you selected more than one!", paths.Length);
			}
			string text = paths[0];
			PlayerPrefs.SetString("Last Screenshot Directory Save", text);
			byte[] bytes = view.Texture2D.EncodeToPNG();
			string text2 = RemoveSymbols(MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.Name);
			string text3 = RemoveSymbols(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Name);
			string text4 = $"Endstar {text2} {text3} {DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day} {DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}.png";
			string text5 = Path.Combine(text, text4);
			if (verboseLogging)
			{
				Debug.Log("fileName: " + text4, this);
				Debug.Log("filePath: " + text5, this);
			}
			File.WriteAllBytes(text5, bytes);
		}
	}

	private string RemoveSymbols(string inputString)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "RemoveSymbols", inputString);
		}
		string pattern = "[^\\w\\s]";
		return Regex.Replace(inputString, pattern, string.Empty);
	}
}
