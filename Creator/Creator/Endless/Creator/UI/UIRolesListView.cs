using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000161 RID: 353
	public class UIRolesListView : UIBaseListView<Roles>
	{
		// Token: 0x06000548 RID: 1352 RVA: 0x0001C980 File Offset: 0x0001AB80
		public override float GetCellViewSize(int index)
		{
			if (base.SuperVerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetCellViewSize", new object[] { index });
			}
			Roles roles = base.Model[index];
			string game = this.rolesDescriptionsDictionary[roles].Game;
			this.dynamicTextSizeReference.text = game;
			this.dynamicTextSizeReference.ForceMeshUpdate(false, false);
			return this.dynamicTextSizeReference.GetRenderedValues().y + 83f;
		}

		// Token: 0x040004C4 RID: 1220
		[Header("UIRolesListView")]
		[SerializeField]
		private UIRolesDescriptionsDictionary rolesDescriptionsDictionary;

		// Token: 0x040004C5 RID: 1221
		[SerializeField]
		private TextMeshProUGUI dynamicTextSizeReference;
	}
}
