using System;
using Endless.Gameplay.UI;
using Endless.Shared;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020001C1 RID: 449
	public class UINewLevelStateModalView : UIEscapableModalView
	{
		// Token: 0x060006AB RID: 1707 RVA: 0x000223E4 File Offset: 0x000205E4
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			this.context = (UINewLevelStateModalView.Contexts)modalData[0];
			this.modalMatchCloseHandler.enabled = this.context == UINewLevelStateModalView.Contexts.Match;
			if (MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame != null)
			{
				this.nameInputField.text = string.Format("Level {0}", MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.levels.Count + 1);
			}
			else
			{
				this.nameInputField.text = "Level 1";
			}
			this.nameInputField.Select();
		}

		// Token: 0x060006AC RID: 1708 RVA: 0x00022473 File Offset: 0x00020673
		public override void Close()
		{
			base.Close();
			this.modalMatchCloseHandler.enabled = false;
		}

		// Token: 0x060006AD RID: 1709 RVA: 0x00022487 File Offset: 0x00020687
		public override void OnDespawn()
		{
			base.OnDespawn();
			this.nameInputField.text = string.Empty;
			this.descriptionInputField.text = string.Empty;
		}

		// Token: 0x040005FD RID: 1533
		[Header("UINewLevelStateModalView")]
		[SerializeField]
		private UIInputField nameInputField;

		// Token: 0x040005FE RID: 1534
		[SerializeField]
		private UIInputField descriptionInputField;

		// Token: 0x040005FF RID: 1535
		[SerializeField]
		private UIModalMatchCloseHandler modalMatchCloseHandler;

		// Token: 0x04000600 RID: 1536
		private UINewLevelStateModalView.Contexts context;

		// Token: 0x020001C2 RID: 450
		public enum Contexts
		{
			// Token: 0x04000602 RID: 1538
			NewGame,
			// Token: 0x04000603 RID: 1539
			Match
		}
	}
}
