using System;
using Endless.Data.UI;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x0200003E RID: 62
	public class UIEndMatchButton : UIGameObject
	{
		// Token: 0x17000017 RID: 23
		// (get) Token: 0x06000129 RID: 297 RVA: 0x0000813B File Offset: 0x0000633B
		public UnityEvent OnEndMatchUnityEvent { get; } = new UnityEvent();

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x0600012A RID: 298 RVA: 0x00008144 File Offset: 0x00006344
		private static bool LeaveGroup
		{
			get
			{
				if (MatchmakingClientController.Instance.LocalGroup == null)
				{
					return false;
				}
				if (MatchmakingClientController.Instance.LocalClientData == null)
				{
					return false;
				}
				ClientData value = MatchmakingClientController.Instance.LocalClientData.Value;
				CoreClientData host = MatchmakingClientController.Instance.LocalGroup.Host;
				return !value.CoreDataEquals(host);
			}
		}

		// Token: 0x0600012B RID: 299 RVA: 0x000081A0 File Offset: 0x000063A0
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.button.onClick.AddListener(new UnityAction(this.EndMatch));
		}

		// Token: 0x0600012C RID: 300 RVA: 0x000081D8 File Offset: 0x000063D8
		private void EndMatch()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "EndMatch", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
			MonoBehaviourSingleton<UIScreenCoverTokenHandler>.Instance.Display(UIScreenCoverTokens.EndingMatch, null, true);
			MonoBehaviourSingleton<ConnectionActions>.Instance.EndMatch(new Action<int, string>(MonoBehaviourSingleton<UIScreenCoverHandler>.Instance.OnEndMatchError));
			this.OnEndMatchUnityEvent.Invoke();
		}

		// Token: 0x040000C4 RID: 196
		[SerializeField]
		private UIButton button;

		// Token: 0x040000C5 RID: 197
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
