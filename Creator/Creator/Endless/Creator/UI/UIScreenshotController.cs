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
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x0200028A RID: 650
	public class UIScreenshotController : UIGameObject, IValidatable
	{
		// Token: 0x06000AC4 RID: 2756 RVA: 0x00032888 File Offset: 0x00030A88
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.saveToDiskButton.onClick.AddListener(new UnityAction(this.SetUpSaveToDisk));
			base.TryGetComponent<UIScreenshotView>(out this.view);
		}

		// Token: 0x06000AC5 RID: 2757 RVA: 0x000328D6 File Offset: 0x00030AD6
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			DebugUtility.DebugHasComponent<UIScreenshotView>(base.gameObject);
		}

		// Token: 0x06000AC6 RID: 2758 RVA: 0x000328FC File Offset: 0x00030AFC
		private void SetUpSaveToDisk()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetUpSaveToDisk", Array.Empty<object>());
			}
			string text = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
			if (PlayerPrefs.HasKey("Last Screenshot Directory Save"))
			{
				string @string = PlayerPrefs.GetString("Last Screenshot Directory Save");
				if (Directory.Exists(@string))
				{
					text = @string;
				}
			}
			StandaloneFileBrowser.OpenFolderPanelAsync("Select A Folder To Save Screenshot To", text, false, new Action<string[]>(this.SaveToDisk));
		}

		// Token: 0x06000AC7 RID: 2759 RVA: 0x00032964 File Offset: 0x00030B64
		private void SaveToDisk(string[] paths)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SaveToDisk", new object[] { paths.Length });
			}
			if (paths.Length == 0)
			{
				return;
			}
			if (paths.Length > 1)
			{
				DebugUtility.LogWarning(this, "SaveToDisk", "Don't know how you selected more than one!", new object[] { paths.Length });
			}
			string text = paths[0];
			PlayerPrefs.SetString("Last Screenshot Directory Save", text);
			byte[] array = this.view.Texture2D.EncodeToPNG();
			string text2 = this.RemoveSymbols(MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.Name);
			string text3 = this.RemoveSymbols(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Name);
			string text4 = string.Format("Endstar {0} {1} {2}-{3}-{4} {5}-{6}-{7}.png", new object[]
			{
				text2,
				text3,
				DateTime.Now.Year,
				DateTime.Now.Month,
				DateTime.Now.Day,
				DateTime.Now.Hour,
				DateTime.Now.Minute,
				DateTime.Now.Second
			});
			string text5 = Path.Combine(text, text4);
			if (this.verboseLogging)
			{
				Debug.Log("fileName: " + text4, this);
				Debug.Log("filePath: " + text5, this);
			}
			File.WriteAllBytes(text5, array);
		}

		// Token: 0x06000AC8 RID: 2760 RVA: 0x00032AF0 File Offset: 0x00030CF0
		private string RemoveSymbols(string inputString)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "RemoveSymbols", new object[] { inputString });
			}
			string text = "[^\\w\\s]";
			return Regex.Replace(inputString, text, string.Empty);
		}

		// Token: 0x0400090D RID: 2317
		[SerializeField]
		private UIButton saveToDiskButton;

		// Token: 0x0400090E RID: 2318
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0400090F RID: 2319
		private UIScreenshotView view;
	}
}
