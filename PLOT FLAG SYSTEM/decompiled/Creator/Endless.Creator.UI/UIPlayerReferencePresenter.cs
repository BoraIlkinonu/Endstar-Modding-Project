using Endless.Gameplay;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public class UIPlayerReferencePresenter : UIBasePresenter<PlayerReference>
{
	protected override void Start()
	{
		base.Start();
		UIPlayerReferenceView obj = base.View.Interface as UIPlayerReferenceView;
		obj.OnUseContextChanged += SetUseContext;
		obj.OnPlayerNumberChanged += SetPlayerNumber;
	}

	private void SetUseContext(bool useContext)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetUseContext", useContext);
		}
		PlayerReferenceUtility.SetUseContext(base.Model, useContext);
		SetModel(base.Model, triggerOnModelChanged: true);
	}

	private void SetPlayerNumber(int playerNumber)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetPlayerNumber", playerNumber);
		}
		if (playerNumber < 1)
		{
			playerNumber = 1;
		}
		if (PlayerReferenceUtility.GetPlayerNumber(base.Model) != playerNumber)
		{
			PlayerReferenceUtility.SetPlayerNumber(base.Model, playerNumber);
			SetModel(base.Model, triggerOnModelChanged: true);
		}
	}
}
