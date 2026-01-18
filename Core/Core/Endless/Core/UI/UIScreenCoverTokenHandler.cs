using System;
using System.Collections.Generic;
using Endless.Data.UI;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.CrashReportHandler;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x02000077 RID: 119
	[DefaultExecutionOrder(-1)]
	public class UIScreenCoverTokenHandler : UIMonoBehaviourSingleton<UIScreenCoverTokenHandler>
	{
		// Token: 0x06000252 RID: 594 RVA: 0x0000CDB4 File Offset: 0x0000AFB4
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.endMatchButtonDisplayAndHideHandler.SetToHideEnd(true);
			MatchmakingClientController.MatchAllocated += this.DisplayEndMatchButton;
			this.endMatchButton.OnEndMatchUnityEvent.AddListener(new UnityAction(this.HideEndMatchButton));
			MatchmakingClientController.MatchLeft += this.HideEndMatchButton;
		}

		// Token: 0x06000253 RID: 595 RVA: 0x0000CE23 File Offset: 0x0000B023
		protected override void OnDestroy()
		{
			base.OnDestroy();
			MatchmakingClientController.MatchAllocated -= this.DisplayEndMatchButton;
			MatchmakingClientController.MatchLeft -= this.HideEndMatchButton;
		}

		// Token: 0x06000254 RID: 596 RVA: 0x0000CE50 File Offset: 0x0000B050
		public void Display(UIScreenCoverTokens token, Action onScreenCovered = null, bool tweenIn = true)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Display", new object[]
				{
					token,
					onScreenCovered.DebugIsNull(),
					tweenIn
				});
			}
			if (this.tokens.Contains(token))
			{
				DebugUtility.LogWarning(this, "Display", "This token is already accounted for!", new object[]
				{
					token,
					onScreenCovered.DebugIsNull(),
					tweenIn
				});
				return;
			}
			UserFacingTextAttribute attributeOfType = token.GetAttributeOfType<UserFacingTextAttribute>();
			string text = ((attributeOfType != null) ? attributeOfType.UserFacingText : null) ?? "Loading...";
			this.tokens.Add(token);
			this.displayedToken = token;
			this.screenCover.Display(text, onScreenCovered, tweenIn);
			this.SetUserMetadata();
			if (this.isInMatch)
			{
				if (this.endMatchButtonDisplayAndHideHandler.IsTweeningHide)
				{
					this.endMatchButtonDisplayAndHideHandler.CancelHideTweens();
					this.endMatchButtonDisplayAndHideHandler.SetToDisplayEnd(true);
					return;
				}
				this.endMatchButtonDisplayAndHideHandler.Display();
			}
		}

		// Token: 0x06000255 RID: 597 RVA: 0x0000CF54 File Offset: 0x0000B154
		public void ReplaceToken(UIScreenCoverTokens toReplace, UIScreenCoverTokens replaceWith)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ReplaceToken", new object[] { toReplace, replaceWith });
			}
			if (this.tokens.Contains(replaceWith))
			{
				DebugUtility.LogWarning(this, "ReplaceToken", "This replaceWith is already accounted for!", new object[] { toReplace, replaceWith });
				return;
			}
			if (!this.tokens.Contains(toReplace))
			{
				DebugUtility.LogWarning(this, "ReplaceToken", "This toReplace is NOT accounted for!", new object[] { toReplace, replaceWith });
				return;
			}
			this.tokens.Remove(toReplace);
			this.Display(replaceWith, null, true);
		}

		// Token: 0x06000256 RID: 598 RVA: 0x0000D010 File Offset: 0x0000B210
		public void UpdateText(UIScreenCoverTokens token, string textToDisplay)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateText", new object[] { token, this.displayedToken, textToDisplay });
			}
			if (!this.tokens.Contains(token))
			{
				DebugUtility.LogWarning(this, "UpdateText", "This token is NOT accounted for!", new object[] { token, textToDisplay });
				return;
			}
			if (this.displayedToken == token)
			{
				this.screenCover.UpdateText(textToDisplay);
			}
		}

		// Token: 0x06000257 RID: 599 RVA: 0x0000D098 File Offset: 0x0000B298
		public void ReleaseToken(UIScreenCoverTokens token)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ReleaseToken", new object[] { token });
			}
			if (!this.tokens.Contains(token))
			{
				DebugUtility.LogWarning(this, "ReleaseToken", "This token is NOT accounted for!", new object[] { token });
				return;
			}
			this.tokens.Remove(token);
			if (this.tokens.Count == 0)
			{
				this.screenCover.Close();
				this.endMatchButtonDisplayAndHideHandler.Hide();
			}
			else
			{
				UserFacingTextAttribute attributeOfType = this.tokens[0].GetAttributeOfType<UserFacingTextAttribute>();
				string text = ((attributeOfType != null) ? attributeOfType.UserFacingText : null) ?? "Loading...";
				this.screenCover.UpdateText(text);
			}
			this.SetUserMetadata();
		}

		// Token: 0x06000258 RID: 600 RVA: 0x0000D165 File Offset: 0x0000B365
		public bool Contains(UIScreenCoverTokens token)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Contains", new object[] { token });
			}
			return this.tokens.Contains(token);
		}

		// Token: 0x06000259 RID: 601 RVA: 0x0000D198 File Offset: 0x0000B398
		public void ForceClose()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ForceClose", Array.Empty<object>());
			}
			for (int i = this.tokens.Count - 1; i >= 0; i--)
			{
				UIScreenCoverTokens uiscreenCoverTokens = this.tokens[i];
				this.ReleaseToken(uiscreenCoverTokens);
			}
		}

		// Token: 0x0600025A RID: 602 RVA: 0x0000D1E9 File Offset: 0x0000B3E9
		private void DisplayEndMatchButton()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayEndMatchButton", Array.Empty<object>());
			}
			this.isInMatch = true;
			this.endMatchButtonDisplayAndHideHandler.Display();
		}

		// Token: 0x0600025B RID: 603 RVA: 0x0000D218 File Offset: 0x0000B418
		private void HideEndMatchButton()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HideEndMatchButton", Array.Empty<object>());
			}
			this.isInMatch = false;
			if (this.tokens.Count > 0)
			{
				this.endMatchButtonDisplayAndHideHandler.Hide();
				return;
			}
			this.endMatchButtonDisplayAndHideHandler.SetToHideEnd(true);
		}

		// Token: 0x0600025C RID: 604 RVA: 0x0000D26C File Offset: 0x0000B46C
		private void SetUserMetadata()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetUserMetadata", Array.Empty<object>());
			}
			string text = "UIScreenCoverTokenHandler-tokens";
			string text2 = ((this.tokens.Count == 0) ? "None" : string.Empty);
			for (int i = 0; i < this.tokens.Count; i++)
			{
				text2 += this.tokens[i].ToString();
				if (i < this.tokens.Count - 1)
				{
					text2 += ", ";
				}
			}
			CrashReportHandler.SetUserMetadata(text, text2);
		}

		// Token: 0x040001A2 RID: 418
		[SerializeField]
		private UIScreenCover screenCover;

		// Token: 0x040001A3 RID: 419
		[SerializeField]
		private UIEndMatchButton endMatchButton;

		// Token: 0x040001A4 RID: 420
		[SerializeField]
		private UIDisplayAndHideHandler endMatchButtonDisplayAndHideHandler;

		// Token: 0x040001A5 RID: 421
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040001A6 RID: 422
		private List<UIScreenCoverTokens> tokens = new List<UIScreenCoverTokens>();

		// Token: 0x040001A7 RID: 423
		private UIScreenCoverTokens displayedToken;

		// Token: 0x040001A8 RID: 424
		private bool isInMatch;
	}
}
