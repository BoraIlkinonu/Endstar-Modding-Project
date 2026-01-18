using System;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x020000A1 RID: 161
	public class UIDebugWindowController : UIWindowController
	{
		// Token: 0x0600036E RID: 878 RVA: 0x000121D0 File Offset: 0x000103D0
		protected override void Start()
		{
			base.Start();
			this.extractLevelButton.onClick.AddListener(new UnityAction(this.ExtractLevel));
			this.deleteAllPlayerPrefsAndQuitButton.onClick.AddListener(new UnityAction(this.DeleteAllPlayerPrefsAndQuit));
			this.developerConsoleToggle.OnChange.AddListener(new UnityAction<bool>(this.SetDeveloperConsole));
			this.runFpsTestButton.onClick.AddListener(new UnityAction(this.RunFpsTests));
		}

		// Token: 0x0600036F RID: 879 RVA: 0x00012253 File Offset: 0x00010453
		private void RunFpsTests()
		{
			global::UnityEngine.Object.Instantiate<GameObject>(this.fpsTestObject);
			this.Close();
		}

		// Token: 0x06000370 RID: 880 RVA: 0x00012267 File Offset: 0x00010467
		private void ExtractLevel()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ExtractLevel", Array.Empty<object>());
			}
		}

		// Token: 0x06000371 RID: 881 RVA: 0x00012281 File Offset: 0x00010481
		private void DeleteAllPlayerPrefsAndQuit()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "DeleteAllPlayerPrefsAndQuit", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
			EndlessCloudService.ClearCachedToken();
			MatchmakingClientController.Instance.Disconnect();
			Application.Quit();
		}

		// Token: 0x06000372 RID: 882 RVA: 0x000122B9 File Offset: 0x000104B9
		private void SetDeveloperConsole(bool developerConsoleEnabled)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetDeveloperConsole", new object[] { developerConsoleEnabled });
			}
			Debug.developerConsoleEnabled = developerConsoleEnabled;
			Debug.developerConsoleVisible = developerConsoleEnabled;
		}

		// Token: 0x0400027E RID: 638
		[Header("UIDebugWindowController")]
		[SerializeField]
		private UIButton saveLevelToDiskButton;

		// Token: 0x0400027F RID: 639
		[SerializeField]
		private UIButton toggleChunkVisualizationButton;

		// Token: 0x04000280 RID: 640
		[SerializeField]
		private UIButton extractLevelButton;

		// Token: 0x04000281 RID: 641
		[SerializeField]
		private UIButton deleteAllPlayerPrefsAndQuitButton;

		// Token: 0x04000282 RID: 642
		[SerializeField]
		private UIToggle developerConsoleToggle;

		// Token: 0x04000283 RID: 643
		[SerializeField]
		private UIButton runFpsTestButton;

		// Token: 0x04000284 RID: 644
		[SerializeField]
		private GameObject fpsTestObject;
	}
}
