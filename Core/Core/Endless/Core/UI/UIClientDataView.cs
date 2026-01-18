using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Endless.Shared.Validation;
using Runtime.Shared.Matchmaking;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Core.UI
{
	// Token: 0x02000040 RID: 64
	public class UIClientDataView : UIGameObject, IValidatable, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x17000019 RID: 25
		// (get) Token: 0x06000131 RID: 305 RVA: 0x000082AF File Offset: 0x000064AF
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x06000132 RID: 306 RVA: 0x000082B7 File Offset: 0x000064B7
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x06000133 RID: 307 RVA: 0x000082BF File Offset: 0x000064BF
		// (set) Token: 0x06000134 RID: 308 RVA: 0x000082C7 File Offset: 0x000064C7
		public int UserId { get; private set; }

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x06000135 RID: 309 RVA: 0x000082D0 File Offset: 0x000064D0
		private static GroupInfo UserGroup
		{
			get
			{
				return MatchmakingClientController.Instance.LocalGroup;
			}
		}

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x06000136 RID: 310 RVA: 0x000082DC File Offset: 0x000064DC
		private static bool UserGroupIsActive
		{
			get
			{
				return UIClientDataView.UserGroup != null;
			}
		}

		// Token: 0x06000137 RID: 311 RVA: 0x000082E8 File Offset: 0x000064E8
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			MatchmakingClientController.GroupJoined += this.OnUserGroupJoined;
			MatchmakingClientController.GroupJoin += this.OnUserGroupJoin;
			MatchmakingClientController.GroupLeave += this.OnUserGroupLeave;
			MatchmakingClientController.GroupLeft += this.OnUserGroupLeft;
		}

		// Token: 0x06000138 RID: 312 RVA: 0x00008354 File Offset: 0x00006554
		private void OnDisable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			MatchmakingClientController.GroupJoined -= this.OnUserGroupJoined;
			MatchmakingClientController.GroupJoin -= this.OnUserGroupJoin;
			MatchmakingClientController.GroupLeave -= this.OnUserGroupLeave;
			MatchmakingClientController.GroupLeft -= this.OnUserGroupLeft;
			CancellationTokenSourceUtility.CancelAndCleanup(ref this.displayAsyncCancellationTokenSource);
		}

		// Token: 0x06000139 RID: 313 RVA: 0x000083C8 File Offset: 0x000065C8
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			if (!this.playerIconBackgroundImageColorDictionary.Contains("IsFriend"))
			{
				DebugUtility.LogError("UIClientDataView requires a key of 'IsFriend' in its playerIconBackgroundImageColorDictionary", this.playerIconBackgroundImageColorDictionary);
			}
			if (!this.playerIconBackgroundImageColorDictionary.Contains("Default"))
			{
				DebugUtility.LogError("UIClientDataView requires a key of 'Default' in its playerIconBackgroundImageColorDictionary", this.playerIconBackgroundImageColorDictionary);
			}
		}

		// Token: 0x0600013A RID: 314 RVA: 0x00008434 File Offset: 0x00006634
		public void Display(CoreClientData clientData)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("Display ( clientData: " + clientData.ToPrettyString() + " )", this);
			}
			int num = clientData.PlatformIdToEndlessUserId();
			this.Display(num);
		}

		// Token: 0x0600013B RID: 315 RVA: 0x00008472 File Offset: 0x00006672
		public void Display(int userId)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "Display", "userId", userId), this);
			}
			this.DisplayAsync(userId);
		}

		// Token: 0x0600013C RID: 316 RVA: 0x000084A4 File Offset: 0x000066A4
		private Task DisplayAsync(int userId)
		{
			UIClientDataView.<DisplayAsync>d__27 <DisplayAsync>d__;
			<DisplayAsync>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<DisplayAsync>d__.<>4__this = this;
			<DisplayAsync>d__.userId = userId;
			<DisplayAsync>d__.<>1__state = -1;
			<DisplayAsync>d__.<>t__builder.Start<UIClientDataView.<DisplayAsync>d__27>(ref <DisplayAsync>d__);
			return <DisplayAsync>d__.<>t__builder.Task;
		}

		// Token: 0x0600013D RID: 317 RVA: 0x000084F0 File Offset: 0x000066F0
		private void UpdatePlayerIconBackgroundImageColor()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdatePlayerIconBackgroundImageColor", Array.Empty<object>());
			}
			bool flag = false;
			bool flag2 = false;
			if (UIClientDataView.UserGroupIsActive)
			{
				using (List<CoreClientData>.Enumerator enumerator = MatchmakingClientController.Instance.LocalGroup.Members.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.PlatformIdToEndlessUserId() == this.UserId)
						{
							flag2 = true;
							break;
						}
					}
				}
			}
			Color color;
			if (flag2)
			{
				color = this.playerIconBackgroundImageColorDictionary["InUserGroup"];
			}
			else
			{
				color = this.playerIconBackgroundImageColorDictionary[flag ? "IsFriend" : "Default"];
			}
			Image[] array = this.playerIconBackgroundImages;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].color = color;
			}
		}

		// Token: 0x0600013E RID: 318 RVA: 0x000085D0 File Offset: 0x000067D0
		private void OnUserGroupJoined()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnUserGroupJoined", Array.Empty<object>());
			}
			this.UpdatePlayerIconBackgroundImageColor();
		}

		// Token: 0x0600013F RID: 319 RVA: 0x000085F0 File Offset: 0x000067F0
		private void OnUserGroupJoin(string joinedUserId)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnUserGroupJoin", new object[] { new CoreClientData(joinedUserId, TargetPlatforms.Endless).ToPrettyString() });
			}
			this.UpdatePlayerIconBackgroundImageColor();
		}

		// Token: 0x06000140 RID: 320 RVA: 0x00008620 File Offset: 0x00006820
		private void OnUserGroupLeave(string leaverUserId)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnUserGroupLeave", new object[] { new CoreClientData(leaverUserId, TargetPlatforms.Endless).ToPrettyString() });
			}
			this.UpdatePlayerIconBackgroundImageColor();
		}

		// Token: 0x06000141 RID: 321 RVA: 0x00008650 File Offset: 0x00006850
		private void OnUserGroupLeft()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnUserGroupLeft", Array.Empty<object>());
			}
			this.UpdatePlayerIconBackgroundImageColor();
		}

		// Token: 0x040000C8 RID: 200
		private const string IN_USER_GROUP_COLOR_KEY = "InUserGroup";

		// Token: 0x040000C9 RID: 201
		private const string IN_FRIEND_COLOR_KEY = "IsFriend";

		// Token: 0x040000CA RID: 202
		private const string DEFAULT_COLOR_KEY = "Default";

		// Token: 0x040000CB RID: 203
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040000CC RID: 204
		[SerializeField]
		private TextMeshProUGUI displayNameText;

		// Token: 0x040000CD RID: 205
		[SerializeField]
		private Image[] playerIconBackgroundImages = Array.Empty<Image>();

		// Token: 0x040000CE RID: 206
		[SerializeField]
		private StringColorDictionary playerIconBackgroundImageColorDictionary;

		// Token: 0x040000CF RID: 207
		private CancellationTokenSource displayAsyncCancellationTokenSource;
	}
}
