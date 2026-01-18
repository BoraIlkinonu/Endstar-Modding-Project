using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000101 RID: 257
	public class MovingCollider : MonoBehaviour
	{
		// Token: 0x060005B9 RID: 1465 RVA: 0x0001C824 File Offset: 0x0001AA24
		public void Init(Collider targetCollider)
		{
			this.myCollider = targetCollider;
			this.characterLayer = 1 << LayerMask.NameToLayer("Character");
			this.lastPosition = base.transform.position;
			this.lastRotation = base.transform.rotation;
		}

		// Token: 0x060005BA RID: 1466 RVA: 0x0001C874 File Offset: 0x0001AA74
		public void HandleMovementFrame()
		{
			this.PushOverlappingCharacters();
			this.lastPosition = base.transform.position;
			this.lastRotation = base.transform.rotation;
		}

		// Token: 0x060005BB RID: 1467 RVA: 0x0001C8A0 File Offset: 0x0001AAA0
		private void PushOverlappingCharacters()
		{
			Vector3 vector = base.transform.position - this.lastPosition;
			int num = Physics.OverlapBoxNonAlloc(this.myCollider.bounds.center, this.myCollider.bounds.extents, this.colliderCache, Quaternion.identity, this.characterLayer);
			for (int i = 0; i < num; i++)
			{
				PlayerReferenceManager component = this.colliderCache[i].GetComponent<PlayerReferenceManager>();
				if (component != null && this.IsOverlapping(component.CharacterController))
				{
					this.PushPlayer(component, vector);
				}
			}
		}

		// Token: 0x060005BC RID: 1468 RVA: 0x0001C944 File Offset: 0x0001AB44
		private void PushPlayer(PlayerReferenceManager player, Vector3 movement)
		{
			Vector3 vector = this.CalculateRotationPush(player);
			Vector3 vector2 = movement + vector;
			player.CharacterController.Move(vector2);
			Vector3 vector3;
			float num;
			if (Physics.ComputePenetration(player.CharacterController, player.transform.position, player.transform.rotation, this.myCollider, base.transform.position, base.transform.rotation, out vector3, out num))
			{
				Vector3 vector4 = vector3 * num;
				player.CharacterController.Move(vector4);
				player.PlayerController.MovingColliderCorrection(vector2 + vector4);
				return;
			}
			player.PlayerController.MovingColliderCorrection(vector2);
		}

		// Token: 0x060005BD RID: 1469 RVA: 0x0001C9E8 File Offset: 0x0001ABE8
		private Vector3 CalculateRotationPush(PlayerReferenceManager player)
		{
			Vector3 vector = this.lastRotation * (player.transform.position - this.lastPosition);
			return base.transform.position + Quaternion.Inverse(base.transform.rotation) * vector - player.transform.position;
		}

		// Token: 0x060005BE RID: 1470 RVA: 0x0001CA50 File Offset: 0x0001AC50
		private bool IsOverlapping(CharacterController character)
		{
			Vector3 vector;
			float num;
			return Physics.ComputePenetration(this.myCollider, base.transform.position, base.transform.rotation, character, character.transform.position, character.transform.rotation, out vector, out num);
		}

		// Token: 0x0400044B RID: 1099
		[SerializeField]
		[HideInInspector]
		private LayerMask characterLayer;

		// Token: 0x0400044C RID: 1100
		[SerializeField]
		[HideInInspector]
		private Collider myCollider;

		// Token: 0x0400044D RID: 1101
		[SerializeField]
		[HideInInspector]
		private Vector3 lastPosition;

		// Token: 0x0400044E RID: 1102
		[SerializeField]
		[HideInInspector]
		private Quaternion lastRotation;

		// Token: 0x0400044F RID: 1103
		private Collider[] colliderCache = new Collider[20];
	}
}
