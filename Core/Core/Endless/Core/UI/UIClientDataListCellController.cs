using System;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x0200004F RID: 79
	public class UIClientDataListCellController : UIBaseListCellController<ClientData>
	{
		// Token: 0x06000180 RID: 384 RVA: 0x00009C9F File Offset: 0x00007E9F
		protected override void Start()
		{
			base.Start();
			this.removeButton.onClick.AddListener(new UnityAction(this.Remove));
		}

		// Token: 0x06000181 RID: 385 RVA: 0x00009CC4 File Offset: 0x00007EC4
		protected override void OnAddButton()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			throw new NotImplementedException();
		}

		// Token: 0x06000182 RID: 386 RVA: 0x00009CE4 File Offset: 0x00007EE4
		protected override void Remove()
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log("Remove", this);
			}
			UIClientDataListModel uiclientDataListModel = (UIClientDataListModel)base.ListModel;
			UIClientDataListModel.RemoveTypes removeType = uiclientDataListModel.RemoveType;
			if (removeType == UIClientDataListModel.RemoveTypes.LocalRemove)
			{
				base.Remove();
				return;
			}
			if (removeType != UIClientDataListModel.RemoveTypes.RemoveFromUserGroup)
			{
				DebugUtility.LogNoEnumSupportError<UIClientDataListModel.RemoveTypes>(this, uiclientDataListModel.RemoveType);
				return;
			}
			if (!MatchmakingClientController.Instance)
			{
				DebugUtility.LogError("There is no MatchmakingClientController!", this);
				return;
			}
			if (MatchmakingClientController.Instance.LocalGroup == null)
			{
				DebugUtility.LogError("There is no LocalGroup!", this);
				return;
			}
			Debug.Log(string.Format("DataIndex: {0}", base.DataIndex), this);
			Debug.Log(string.Format("ListModel.Count: {0}", base.ListModel.Count), this);
			Debug.Log(string.Format("Model: {0}", base.Model), this);
			MatchmakingClientController.Instance.RemoveFromGroup(base.Model.CoreData.PlatformId, null);
			base.Remove();
		}

		// Token: 0x04000113 RID: 275
		[Header("UIClientDataListCellController")]
		[SerializeField]
		private UIButton removeButton;

		// Token: 0x04000114 RID: 276
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
