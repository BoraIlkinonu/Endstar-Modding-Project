using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UIUserView : UIBaseSocialView<User>
{
	[Header("UIUserView")]
	[SerializeField]
	private UIText usernameText;

	public override void View(User model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "View", model);
		}
		string value = ((model.Id == EndlessServices.Instance.CloudService.ActiveUserId) ? UITextMeshProUtilities.Bold(model.UserName) : model.UserName);
		usernameText.Value = value;
	}
}
