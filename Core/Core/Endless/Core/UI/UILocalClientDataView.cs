using System;
using Endless.Matchmaking;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Core.UI
{
	// Token: 0x02000042 RID: 66
	[RequireComponent(typeof(UIClientDataView))]
	public class UILocalClientDataView : UIGameObject
	{
		// Token: 0x06000145 RID: 325 RVA: 0x000088A4 File Offset: 0x00006AA4
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			UIClientDataView uiclientDataView;
			base.TryGetComponent<UIClientDataView>(out uiclientDataView);
			uiclientDataView.Display(MatchmakingClientController.Instance.LocalClientData.Value.CoreData);
		}

		// Token: 0x040000D9 RID: 217
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
