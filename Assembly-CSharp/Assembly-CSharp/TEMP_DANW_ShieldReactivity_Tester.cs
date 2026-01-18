using System;
using UnityEngine;
using UnityEngine.VFX;

// Token: 0x02000002 RID: 2
public class TEMP_DANW_ShieldReactivity_Tester : MonoBehaviour
{
	// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
	private void Start()
	{
		this.shieldRipples = base.GetComponent<VisualEffect>();
	}

	// Token: 0x06000002 RID: 2 RVA: 0x0000205E File Offset: 0x0000025E
	private void CollisionExp(Vector3 impactPoint)
	{
		this.shieldRipples.SetVector3("Impact Point", impactPoint);
		this.shieldRipples.Play();
	}

	// Token: 0x06000003 RID: 3 RVA: 0x0000207C File Offset: 0x0000027C
	private void OnCollisionEnter(Collision collision)
	{
		Vector3 point = collision.GetContact(0).point;
		this.CollisionExp(point);
		this.cubeSpot = point;
	}

	// Token: 0x06000004 RID: 4 RVA: 0x000020A7 File Offset: 0x000002A7
	private void OnDrawGizmos()
	{
		Gizmos.DrawCube(this.cubeSpot, Vector3.one * 0.2f);
	}

	// Token: 0x04000001 RID: 1
	public VisualEffect shieldRipples;

	// Token: 0x04000002 RID: 2
	public LayerMask layerMask;

	// Token: 0x04000003 RID: 3
	private Vector3 cubeSpot;
}
