using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020002C8 RID: 712
	public class UIGameLibraryModerationWindowView : UIBaseWindowView
	{
		// Token: 0x06000C0A RID: 3082 RVA: 0x00039B2A File Offset: 0x00037D2A
		public static UIGameLibraryModerationWindowView Display(Transform parent = null)
		{
			return (UIGameLibraryModerationWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UIGameLibraryModerationWindowView>(parent, null);
		}

		// Token: 0x06000C0B RID: 3083 RVA: 0x00039B3D File Offset: 0x00037D3D
		public override void Initialize(Dictionary<string, object> supplementalData)
		{
			base.Initialize(supplementalData);
			this.gameLibraryListModel.Synchronize();
		}

		// Token: 0x06000C0C RID: 3084 RVA: 0x00039B51 File Offset: 0x00037D51
		public override void Close()
		{
			base.Close();
			this.gameLibraryListModel.Clear(true);
		}

		// Token: 0x04000A64 RID: 2660
		[Header("UIGameLibraryModerationWindowView")]
		[SerializeField]
		private UIGameLibraryListModel gameLibraryListModel;
	}
}
