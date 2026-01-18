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

namespace Endless.Gameplay.UI;

public class UserPortrait : MonoBehaviour, IPoolableT
{
	[SerializeField]
	private Image profileImage;

	[SerializeField]
	private Image backerImage;

	[SerializeField]
	private Color[] colors;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private UITooltip tooltip;

	[SerializeField]
	private Image hostImage;

	[SerializeField]
	private Image partyLeaderImage;

	[SerializeField]
	private Image border;

	[SerializeField]
	private Color defaultBorderColor;

	[SerializeField]
	private Color partyBorderColor;

	public MonoBehaviour Prefab { get; set; }

	public bool IsUi => true;

	private bool IsInParty
	{
		get
		{
			if (MatchmakingClientController.Instance.LocalGroup != null)
			{
				return MatchmakingClientController.Instance.LocalGroup.Members.Count != 1;
			}
			return false;
		}
	}

	public async void Initialize(int userId, bool showHostAndParty = true)
	{
		tooltip.ShouldShow = false;
		Debug.Log("Initializing portrait");
		string text = await MonoBehaviourSingleton<RuntimeDatabase>.Instance.GetUserName(userId);
		int num = new System.Random(text.GetHashCode()).Next(0, colors.Length);
		backerImage.color = colors[num];
		nameText.SetText(text.Substring(0, 1));
		List<string> list = new List<string> { text };
		bool flag = false;
		if (showHostAndParty)
		{
			flag = NetworkBehaviourSingleton<UserIdManager>.Instance.GetClientId(userId) == 0;
			if (flag)
			{
				list.Add("Host");
			}
		}
		hostImage.enabled = flag;
		bool flag2 = false;
		bool flag3 = false;
		if (showHostAndParty && IsInParty)
		{
			string userIdString = userId.ToString();
			flag2 = MatchmakingClientController.Instance.LocalGroup.Members[0].PlatformId == userIdString;
			flag3 = MatchmakingClientController.Instance.LocalGroup.Members.Any((CoreClientData member) => member.PlatformId == userIdString);
			if (flag2)
			{
				list.Add("Party Leader");
			}
		}
		partyLeaderImage.enabled = flag2;
		Color color = (flag3 ? partyBorderColor : defaultBorderColor);
		border.color = color;
		string text2 = string.Join("\n", list);
		tooltip.SetTooltip(text2);
		tooltip.ShouldShow = true;
	}
}
