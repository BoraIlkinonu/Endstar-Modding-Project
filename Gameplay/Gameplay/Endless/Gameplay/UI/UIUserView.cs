using System;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Endless.Gameplay.UI
{
	// Token: 0x02000402 RID: 1026
	public class UIUserView : UIBaseSocialView<User>
	{
		// Token: 0x060019A4 RID: 6564 RVA: 0x00076078 File Offset: 0x00074278
		public override void View(User model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
			string text = ((model.Id == EndlessServices.Instance.CloudService.ActiveUserId) ? UITextMeshProUtilities.Bold(model.UserName) : model.UserName);
			this.usernameText.Value = text;
		}

		// Token: 0x04001460 RID: 5216
		[Header("UIUserView")]
		[SerializeField]
		private UIText usernameText;
	}
}
