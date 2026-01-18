using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Gameplay.UI
{
	// Token: 0x0200040C RID: 1036
	public class UserPortrait : MonoBehaviour, IPoolableT
	{
		// Token: 0x1700053A RID: 1338
		// (get) Token: 0x060019DE RID: 6622 RVA: 0x00076DC0 File Offset: 0x00074FC0
		// (set) Token: 0x060019DF RID: 6623 RVA: 0x00076DC8 File Offset: 0x00074FC8
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x1700053B RID: 1339
		// (get) Token: 0x060019E0 RID: 6624 RVA: 0x00017586 File Offset: 0x00015786
		public bool IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700053C RID: 1340
		// (get) Token: 0x060019E1 RID: 6625 RVA: 0x00076DD1 File Offset: 0x00074FD1
		private bool IsInParty
		{
			get
			{
				return MatchmakingClientController.Instance.LocalGroup != null && MatchmakingClientController.Instance.LocalGroup.Members.Count != 1;
			}
		}

		// Token: 0x060019E2 RID: 6626 RVA: 0x00076DFC File Offset: 0x00074FFC
		public async void Initialize(int userId, bool showHostAndParty = true)
		{
			this.tooltip.ShouldShow = false;
			Debug.Log("Initializing portrait");
			string text = await MonoBehaviourSingleton<RuntimeDatabase>.Instance.GetUserName(userId);
			int num = new global::System.Random(text.GetHashCode()).Next(0, this.colors.Length);
			this.backerImage.color = this.colors[num];
			this.nameText.SetText(text.Substring(0, 1), true);
			List<string> list = new List<string> { text };
			bool flag = false;
			if (showHostAndParty)
			{
				flag = NetworkBehaviourSingleton<UserIdManager>.Instance.GetClientId(userId) == 0UL;
				if (flag)
				{
					list.Add("Host");
				}
			}
			this.hostImage.enabled = flag;
			bool flag2 = false;
			bool flag3 = false;
			if (showHostAndParty && this.IsInParty)
			{
				string userIdString = userId.ToString();
				flag2 = MatchmakingClientController.Instance.LocalGroup.Members[0].PlatformId == userIdString;
				flag3 = MatchmakingClientController.Instance.LocalGroup.Members.Any((CoreClientData member) => member.PlatformId == userIdString);
				if (flag2)
				{
					list.Add("Party Leader");
				}
			}
			this.partyLeaderImage.enabled = flag2;
			Color color = (flag3 ? this.partyBorderColor : this.defaultBorderColor);
			this.border.color = color;
			string text2 = string.Join("\n", list);
			this.tooltip.SetTooltip(text2);
			this.tooltip.ShouldShow = true;
		}

		// Token: 0x04001488 RID: 5256
		[SerializeField]
		private Image profileImage;

		// Token: 0x04001489 RID: 5257
		[SerializeField]
		private Image backerImage;

		// Token: 0x0400148A RID: 5258
		[SerializeField]
		private Color[] colors;

		// Token: 0x0400148B RID: 5259
		[SerializeField]
		private TextMeshProUGUI nameText;

		// Token: 0x0400148C RID: 5260
		[SerializeField]
		private UITooltip tooltip;

		// Token: 0x0400148D RID: 5261
		[SerializeField]
		private Image hostImage;

		// Token: 0x0400148E RID: 5262
		[SerializeField]
		private Image partyLeaderImage;

		// Token: 0x0400148F RID: 5263
		[SerializeField]
		private Image border;

		// Token: 0x04001490 RID: 5264
		[SerializeField]
		private Color defaultBorderColor;

		// Token: 0x04001491 RID: 5265
		[SerializeField]
		private Color partyBorderColor;
	}
}
