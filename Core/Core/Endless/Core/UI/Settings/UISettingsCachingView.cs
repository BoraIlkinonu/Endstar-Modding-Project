using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.FileManagement;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Core.UI.Settings
{
	// Token: 0x020000AA RID: 170
	public class UISettingsCachingView : UIGameObject
	{
		// Token: 0x060003B2 RID: 946 RVA: 0x000133F4 File Offset: 0x000115F4
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			this.Display(true);
		}

		// Token: 0x060003B3 RID: 947 RVA: 0x00013418 File Offset: 0x00011618
		public void Display(bool replaceCachePath)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Display", Array.Empty<object>());
			}
			LocalFileDatabase instance = MonoBehaviourSingleton<LocalFileDatabase>.Instance;
			long cacheSize = instance.GetCacheSize();
			long cacheMaxSize = instance.GetCacheMaxSize();
			string text = this.FormatBytes(cacheSize);
			string text2 = this.FormatBytes(cacheMaxSize);
			this.spaceOccupiedText.text = "Cache Size: " + text + " / " + text2;
			if (!this.maxCacheSizeToggleGroup.Initialized)
			{
				this.maxCacheSizeToggleGroup.Initialize();
			}
			long num = 1073741824L;
			long num2 = cacheMaxSize / num;
			if (num2 >= -2147483648L && num2 <= 2147483647L)
			{
				int num3 = (int)num2;
				this.maxCacheSizeToggleGroup.SetToggledOnValue(num3, true, true);
			}
			else
			{
				DebugUtility.LogException(this, "Display", string.Format("Could not find index of current {0} for ui!", cacheMaxSize), Array.Empty<object>());
			}
			if (replaceCachePath)
			{
				this.cachePathInputField.text = instance.GetCoreFilePath();
			}
		}

		// Token: 0x060003B4 RID: 948 RVA: 0x00013504 File Offset: 0x00011704
		public string FormatBytes(long bytes)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "FormatBytes", new object[] { bytes });
			}
			string[] array = new string[] { "B", "KB", "MB", "GB", "TB" };
			int num = 0;
			double num2 = (double)bytes;
			while (num2 >= 1024.0 && num < array.Length - 1)
			{
				num2 /= 1024.0;
				num++;
			}
			return string.Format("{0:0.##} {1}", num2, array[num]);
		}

		// Token: 0x040002B8 RID: 696
		[SerializeField]
		private TextMeshProUGUI spaceOccupiedText;

		// Token: 0x040002B9 RID: 697
		[SerializeField]
		private UIToggleGroup maxCacheSizeToggleGroup;

		// Token: 0x040002BA RID: 698
		[SerializeField]
		private UIInputField cachePathInputField;

		// Token: 0x040002BB RID: 699
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
