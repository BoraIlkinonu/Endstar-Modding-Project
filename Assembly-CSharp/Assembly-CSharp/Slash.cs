using System;
using UnityEngine;
using UnityEngine.VFX;

// Token: 0x02000004 RID: 4
[Serializable]
public class Slash
{
	// Token: 0x0400000A RID: 10
	public VisualEffect slashFX;

	// Token: 0x0400000B RID: 11
	public float newCooldown;

	// Token: 0x0400000C RID: 12
	public float fxPlayFrame;

	// Token: 0x0400000D RID: 13
	public Vector3 slashVelocity;

	// Token: 0x0400000E RID: 14
	public AudioClip[] slashSounds;
}
