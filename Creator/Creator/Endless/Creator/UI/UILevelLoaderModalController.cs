using System;
using Endless.Gameplay.UI;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020001BB RID: 443
	[RequireComponent(typeof(UILevelLoaderModalView))]
	public class UILevelLoaderModalController : UIGameObject
	{
		// Token: 0x06000696 RID: 1686 RVA: 0x00021E3C File Offset: 0x0002003C
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.loadLevelButton.onClick.AddListener(new UnityAction(this.LoadLevel));
			base.TryGetComponent<UILevelLoaderModalView>(out this.view);
		}

		// Token: 0x06000697 RID: 1687 RVA: 0x00021E8C File Offset: 0x0002008C
		private void LoadLevel()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "LoadLevel", Array.Empty<object>());
			}
			string text = MatchmakingClientController.Instance.ActiveGameId.ToString();
			MonoBehaviourSingleton<UIStartMatchHelper>.Instance.TryToStartMatch(text, null, this.view.LevelId, MainMenuGameContext.Edit);
		}

		// Token: 0x040005E6 RID: 1510
		[SerializeField]
		private UIButton loadLevelButton;

		// Token: 0x040005E7 RID: 1511
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040005E8 RID: 1512
		private UILevelLoaderModalView view;
	}
}
