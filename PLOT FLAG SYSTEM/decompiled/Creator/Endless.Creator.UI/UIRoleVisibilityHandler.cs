using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIRoleVisibilityHandler : UIGameObject, IRoleInteractable
{
	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	public void SetLocalUserCanInteract(bool localUserCanInteract)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetLocalUserCanInteract", localUserCanInteract);
		}
		base.gameObject.SetActive(localUserCanInteract);
	}
}
