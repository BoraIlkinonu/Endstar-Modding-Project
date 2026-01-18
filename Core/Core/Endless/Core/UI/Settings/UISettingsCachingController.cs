using System;
using System.IO;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.FileManagement;
using Endless.Shared.UI;
using Endless.Shared.Validation;
using SFB;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI.Settings
{
	// Token: 0x020000A9 RID: 169
	public class UISettingsCachingController : UIGameObject, IValidatable
	{
		// Token: 0x17000063 RID: 99
		// (get) Token: 0x060003A4 RID: 932 RVA: 0x00012FD5 File Offset: 0x000111D5
		private LocalFileDatabase LocalFileDatabase
		{
			get
			{
				return MonoBehaviourSingleton<LocalFileDatabase>.Instance;
			}
		}

		// Token: 0x060003A5 RID: 933 RVA: 0x00012FDC File Offset: 0x000111DC
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.clearCacheButton.onClick.AddListener(new UnityAction(this.ClearCache));
			this.verifyCacheButton.onClick.AddListener(new UnityAction(this.VerifyCache));
			this.maxCacheSizeToggleGroup.OnChange.AddListener(new UnityAction(this.SetMaxCacheSize));
			this.clearOldFilesButton.onClick.AddListener(new UnityAction(this.ClearOldFiles));
			this.cachePathInputField.onValueChanged.AddListener(new UnityAction<string>(this.HandleMoveCacheButton));
			this.selectCacheDirectoryButton.onClick.AddListener(new UnityAction(this.SelectDirectory));
			this.moveCacheButton.onClick.AddListener(new UnityAction(this.MoveCache));
			base.TryGetComponent<UISettingsCachingView>(out this.view);
		}

		// Token: 0x060003A6 RID: 934 RVA: 0x000130D2 File Offset: 0x000112D2
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			this.moveCacheButton.interactable = false;
		}

		// Token: 0x060003A7 RID: 935 RVA: 0x000130F8 File Offset: 0x000112F8
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			DebugUtility.DebugHasComponent<UISettingsCachingView>(base.gameObject);
		}

		// Token: 0x060003A8 RID: 936 RVA: 0x0001311E File Offset: 0x0001131E
		private void ClearCache()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ClearCache", Array.Empty<object>());
			}
			this.LocalFileDatabase.ClearDatabase();
			this.view.Display(false);
			MonoBehaviourSingleton<StageManager>.Instance.UnloadAll();
		}

		// Token: 0x060003A9 RID: 937 RVA: 0x00013159 File Offset: 0x00011359
		private void VerifyCache()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "VerifyCache", Array.Empty<object>());
			}
			this.LocalFileDatabase.VerifyDatabase();
			this.view.Display(false);
		}

		// Token: 0x060003AA RID: 938 RVA: 0x0001318C File Offset: 0x0001138C
		private void SetMaxCacheSize()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetMaxCacheSize", Array.Empty<object>());
			}
			long num = 1073741824L;
			int indexOfFirstToggledOnValue = this.maxCacheSizeToggleGroup.IndexOfFirstToggledOnValue;
			long num2 = num * (long)indexOfFirstToggledOnValue;
			this.LocalFileDatabase.SetCacheMaxSize(num2);
			this.view.Display(false);
		}

		// Token: 0x060003AB RID: 939 RVA: 0x000131DF File Offset: 0x000113DF
		private void ClearOldFiles()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ClearOldFiles", Array.Empty<object>());
			}
			this.LocalFileDatabase.ClearOldFiles();
			this.view.Display(false);
		}

		// Token: 0x060003AC RID: 940 RVA: 0x00013210 File Offset: 0x00011410
		private void SelectDirectory()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SelectDirectory", Array.Empty<object>());
			}
			string text = this.LocalFileDatabase.GetCoreFilePath();
			if (!Directory.Exists(text))
			{
				text = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
			}
			StandaloneFileBrowser.OpenFolderPanelAsync("Default Content Installation Path", text, false, new Action<string[]>(this.OnDirectorySelected));
		}

		// Token: 0x060003AD RID: 941 RVA: 0x0001326C File Offset: 0x0001146C
		private void OnDirectorySelected(string[] paths)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDirectorySelected", new object[] { paths.Length });
			}
			if (paths.Length == 0)
			{
				return;
			}
			if (paths.Length > 1)
			{
				DebugUtility.LogWarning(this, "OnDirectorySelected", "An entry in paths is expected.", paths);
			}
			string text = paths[0];
			text = text.Replace('\\', '/');
			if (text.IsNullOrEmptyOrWhiteSpace())
			{
				return;
			}
			this.cachePathInputField.text = text;
		}

		// Token: 0x060003AE RID: 942 RVA: 0x000132DF File Offset: 0x000114DF
		private void HandleMoveCacheButton(string path)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleMoveCacheButton", new object[] { path });
			}
			this.moveCacheButton.interactable = this.CanMoveCache(false);
		}

		// Token: 0x060003AF RID: 943 RVA: 0x00013310 File Offset: 0x00011510
		private void MoveCache()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "MoveCache", Array.Empty<object>());
			}
			if (!this.CanMoveCache(true))
			{
				return;
			}
			string text = this.cachePathInputField.text;
			this.LocalFileDatabase.GetCoreFilePath();
			string text2 = text;
			this.LocalFileDatabase.MoveDatabase(text2);
			this.SetMaxCacheSize();
			this.moveCacheButton.interactable = false;
		}

		// Token: 0x060003B0 RID: 944 RVA: 0x00013378 File Offset: 0x00011578
		private bool CanMoveCache(bool playInvalidInputTweensIfSameAsOldPath)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CanMoveCache", new object[] { playInvalidInputTweensIfSameAsOldPath });
			}
			string text = this.cachePathInputField.text;
			if (text.IsNullOrEmptyOrWhiteSpace() || !Directory.Exists(text))
			{
				this.cachePathInputField.PlayInvalidInputTweens();
				return false;
			}
			if (this.LocalFileDatabase.GetCoreFilePath() == text)
			{
				if (playInvalidInputTweensIfSameAsOldPath)
				{
					this.cachePathInputField.PlayInvalidInputTweens();
				}
				return false;
			}
			return true;
		}

		// Token: 0x040002AF RID: 687
		[SerializeField]
		private UIButton clearCacheButton;

		// Token: 0x040002B0 RID: 688
		[SerializeField]
		private UIButton verifyCacheButton;

		// Token: 0x040002B1 RID: 689
		[SerializeField]
		private UIToggleGroup maxCacheSizeToggleGroup;

		// Token: 0x040002B2 RID: 690
		[SerializeField]
		private UIButton clearOldFilesButton;

		// Token: 0x040002B3 RID: 691
		[SerializeField]
		private UIInputField cachePathInputField;

		// Token: 0x040002B4 RID: 692
		[SerializeField]
		private UIButton selectCacheDirectoryButton;

		// Token: 0x040002B5 RID: 693
		[SerializeField]
		private UIButton moveCacheButton;

		// Token: 0x040002B6 RID: 694
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040002B7 RID: 695
		private UISettingsCachingView view;
	}
}
