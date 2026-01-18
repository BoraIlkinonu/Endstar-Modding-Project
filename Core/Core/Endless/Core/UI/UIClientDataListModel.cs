using System;
using System.Collections.Generic;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x02000052 RID: 82
	public class UIClientDataListModel : UIBaseLocalFilterableListModel<ClientData>, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x17000024 RID: 36
		// (get) Token: 0x06000189 RID: 393 RVA: 0x00009EE5 File Offset: 0x000080E5
		// (set) Token: 0x0600018A RID: 394 RVA: 0x00009EED File Offset: 0x000080ED
		public bool CanRemove { get; private set; }

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x0600018B RID: 395 RVA: 0x00009EF6 File Offset: 0x000080F6
		protected override Comparison<ClientData> DefaultSort
		{
			get
			{
				return (ClientData x, ClientData y) => string.Compare(x.DisplayName, y.DisplayName, StringComparison.Ordinal);
			}
		}

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x0600018C RID: 396 RVA: 0x00009F17 File Offset: 0x00008117
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x0600018D RID: 397 RVA: 0x00009F1F File Offset: 0x0000811F
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x0600018E RID: 398 RVA: 0x00009F27 File Offset: 0x00008127
		public void SetCanRemove(bool value)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetCanRemove", new object[] { value });
			}
			this.CanRemove = value;
			base.TriggerModelChanged();
		}

		// Token: 0x0600018F RID: 399 RVA: 0x00009F58 File Offset: 0x00008158
		public void SetClientDataToOmit(HashSet<ClientData> clientDataToOmit)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetClientDataToOmit", new object[] { clientDataToOmit.Count });
			}
			if (clientDataToOmit == null)
			{
				this.clientDataToOmit.Clear();
				return;
			}
			this.clientDataToOmit = clientDataToOmit;
		}

		// Token: 0x06000190 RID: 400 RVA: 0x00009F98 File Offset: 0x00008198
		public void Synchronize()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Synchronize", Array.Empty<object>());
			}
			if (MatchmakingClientController.Instance.LocalClientData != null)
			{
				ClientData value = MatchmakingClientController.Instance.LocalClientData.Value;
				if (this.omitLocalClientData && !this.clientDataToOmit.Contains(value))
				{
					this.clientDataToOmit.Add(value);
				}
				this.OnLoadingStarted.Invoke();
				return;
			}
			DebugUtility.LogException(new Exception("MatchmakingClientController has no LocalClientData!"), this);
		}

		// Token: 0x06000191 RID: 401 RVA: 0x0000A024 File Offset: 0x00008224
		private void UpdateUserList(List<ClientData> list)
		{
			foreach (ClientData clientData in list)
			{
				if (!this.clientDataToOmit.Contains(clientData))
				{
					list.Add(clientData);
				}
			}
			this.Set(list, true);
		}

		// Token: 0x04000118 RID: 280
		[SerializeField]
		public UIClientDataListModel.RemoveTypes RemoveType;

		// Token: 0x04000119 RID: 281
		[SerializeField]
		private bool omitLocalClientData;

		// Token: 0x0400011A RID: 282
		private HashSet<ClientData> clientDataToOmit = new HashSet<ClientData>();

		// Token: 0x02000053 RID: 83
		public enum RemoveTypes
		{
			// Token: 0x0400011E RID: 286
			LocalRemove,
			// Token: 0x0400011F RID: 287
			RemoveFromUserGroup
		}
	}
}
