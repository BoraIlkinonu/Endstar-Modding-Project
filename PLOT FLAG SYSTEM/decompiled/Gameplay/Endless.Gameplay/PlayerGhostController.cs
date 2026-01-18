using UnityEngine;

namespace Endless.Gameplay;

public class PlayerGhostController : MonoBehaviour
{
	[SerializeField]
	private PlayerReferenceManager playerReferenceManager;

	private bool ghostEnabled;

	public void SetGhostMode(bool ghostToggleTo)
	{
		if (ghostToggleTo != ghostEnabled)
		{
			if (ghostToggleTo)
			{
				base.gameObject.layer = LayerMask.NameToLayer("GhostCharacter");
			}
			else
			{
				base.gameObject.layer = LayerMask.NameToLayer("Character");
			}
			ghostEnabled = ghostToggleTo;
		}
	}
}
