using System;
using Endless.Gameplay;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000241 RID: 577
	public class UIPlayerReferencePresenter : UIBasePresenter<PlayerReference>
	{
		// Token: 0x06000953 RID: 2387 RVA: 0x0002BA23 File Offset: 0x00029C23
		protected override void Start()
		{
			base.Start();
			UIPlayerReferenceView uiplayerReferenceView = base.View.Interface as UIPlayerReferenceView;
			uiplayerReferenceView.OnUseContextChanged += this.SetUseContext;
			uiplayerReferenceView.OnPlayerNumberChanged += this.SetPlayerNumber;
		}

		// Token: 0x06000954 RID: 2388 RVA: 0x0002BA5E File Offset: 0x00029C5E
		private void SetUseContext(bool useContext)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetUseContext", new object[] { useContext });
			}
			PlayerReferenceUtility.SetUseContext(base.Model, useContext);
			this.SetModel(base.Model, true);
		}

		// Token: 0x06000955 RID: 2389 RVA: 0x0002BA9C File Offset: 0x00029C9C
		private void SetPlayerNumber(int playerNumber)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetPlayerNumber", new object[] { playerNumber });
			}
			if (playerNumber < 1)
			{
				playerNumber = 1;
			}
			if (PlayerReferenceUtility.GetPlayerNumber(base.Model) == playerNumber)
			{
				return;
			}
			PlayerReferenceUtility.SetPlayerNumber(base.Model, playerNumber);
			this.SetModel(base.Model, true);
		}
	}
}
