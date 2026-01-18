using UnityEngine;

namespace Endless.Gameplay;

public class MovingCollider : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	private LayerMask characterLayer;

	[SerializeField]
	[HideInInspector]
	private Collider myCollider;

	[SerializeField]
	[HideInInspector]
	private Vector3 lastPosition;

	[SerializeField]
	[HideInInspector]
	private Quaternion lastRotation;

	private Collider[] colliderCache = new Collider[20];

	public void Init(Collider targetCollider)
	{
		myCollider = targetCollider;
		characterLayer = 1 << LayerMask.NameToLayer("Character");
		lastPosition = base.transform.position;
		lastRotation = base.transform.rotation;
	}

	public void HandleMovementFrame()
	{
		PushOverlappingCharacters();
		lastPosition = base.transform.position;
		lastRotation = base.transform.rotation;
	}

	private void PushOverlappingCharacters()
	{
		Vector3 movement = base.transform.position - lastPosition;
		int num = Physics.OverlapBoxNonAlloc(myCollider.bounds.center, myCollider.bounds.extents, colliderCache, Quaternion.identity, characterLayer);
		for (int i = 0; i < num; i++)
		{
			PlayerReferenceManager component = colliderCache[i].GetComponent<PlayerReferenceManager>();
			if (component != null && IsOverlapping(component.CharacterController))
			{
				PushPlayer(component, movement);
			}
		}
	}

	private void PushPlayer(PlayerReferenceManager player, Vector3 movement)
	{
		Vector3 vector = CalculateRotationPush(player);
		Vector3 vector2 = movement + vector;
		player.CharacterController.Move(vector2);
		if (Physics.ComputePenetration(player.CharacterController, player.transform.position, player.transform.rotation, myCollider, base.transform.position, base.transform.rotation, out var direction, out var distance))
		{
			Vector3 vector3 = direction * distance;
			player.CharacterController.Move(vector3);
			player.PlayerController.MovingColliderCorrection(vector2 + vector3);
		}
		else
		{
			player.PlayerController.MovingColliderCorrection(vector2);
		}
	}

	private Vector3 CalculateRotationPush(PlayerReferenceManager player)
	{
		Vector3 vector = lastRotation * (player.transform.position - lastPosition);
		return base.transform.position + Quaternion.Inverse(base.transform.rotation) * vector - player.transform.position;
	}

	private bool IsOverlapping(CharacterController character)
	{
		Vector3 direction;
		float distance;
		return Physics.ComputePenetration(myCollider, base.transform.position, base.transform.rotation, character, character.transform.position, character.transform.rotation, out direction, out distance);
	}
}
