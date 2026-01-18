using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000241 RID: 577
	[CreateAssetMenu(menuName = "ScriptableObject/NpcSettings", fileName = "NpcSettings")]
	public class NpcSettings : ScriptableObject
	{
		// Token: 0x1700022B RID: 555
		// (get) Token: 0x06000BD5 RID: 3029 RVA: 0x00040DB6 File Offset: 0x0003EFB6
		// (set) Token: 0x06000BD6 RID: 3030 RVA: 0x00040DBE File Offset: 0x0003EFBE
		public float WalkSpeed { get; private set; }

		// Token: 0x1700022C RID: 556
		// (get) Token: 0x06000BD7 RID: 3031 RVA: 0x00040DC7 File Offset: 0x0003EFC7
		// (set) Token: 0x06000BD8 RID: 3032 RVA: 0x00040DCF File Offset: 0x0003EFCF
		public float RunSpeed { get; private set; }

		// Token: 0x1700022D RID: 557
		// (get) Token: 0x06000BD9 RID: 3033 RVA: 0x00040DD8 File Offset: 0x0003EFD8
		// (set) Token: 0x06000BDA RID: 3034 RVA: 0x00040DE0 File Offset: 0x0003EFE0
		public float SprintSpeed { get; private set; }

		// Token: 0x1700022E RID: 558
		// (get) Token: 0x06000BDB RID: 3035 RVA: 0x00040DE9 File Offset: 0x0003EFE9
		// (set) Token: 0x06000BDC RID: 3036 RVA: 0x00040DF1 File Offset: 0x0003EFF1
		public float StrafingSpeed { get; private set; }

		// Token: 0x1700022F RID: 559
		// (get) Token: 0x06000BDD RID: 3037 RVA: 0x00040DFA File Offset: 0x0003EFFA
		// (set) Token: 0x06000BDE RID: 3038 RVA: 0x00040E02 File Offset: 0x0003F002
		public float RotationSpeed { get; private set; }

		// Token: 0x17000230 RID: 560
		// (get) Token: 0x06000BDF RID: 3039 RVA: 0x00040E0B File Offset: 0x0003F00B
		// (set) Token: 0x06000BE0 RID: 3040 RVA: 0x00040E13 File Offset: 0x0003F013
		public uint DownFramesLimit { get; private set; }

		// Token: 0x17000231 RID: 561
		// (get) Token: 0x06000BE1 RID: 3041 RVA: 0x00040E1C File Offset: 0x0003F01C
		// (set) Token: 0x06000BE2 RID: 3042 RVA: 0x00040E24 File Offset: 0x0003F024
		public uint ReplanFrames { get; private set; }

		// Token: 0x17000232 RID: 562
		// (get) Token: 0x06000BE3 RID: 3043 RVA: 0x00040E2D File Offset: 0x0003F02D
		// (set) Token: 0x06000BE4 RID: 3044 RVA: 0x00040E35 File Offset: 0x0003F035
		public LayerMask CharacterCollisionMask { get; private set; }

		// Token: 0x17000233 RID: 563
		// (get) Token: 0x06000BE5 RID: 3045 RVA: 0x00040E3E File Offset: 0x0003F03E
		// (set) Token: 0x06000BE6 RID: 3046 RVA: 0x00040E46 File Offset: 0x0003F046
		public LayerMask GroundCollisionMask { get; private set; }

		// Token: 0x17000234 RID: 564
		// (get) Token: 0x06000BE7 RID: 3047 RVA: 0x00040E4F File Offset: 0x0003F04F
		// (set) Token: 0x06000BE8 RID: 3048 RVA: 0x00040E57 File Offset: 0x0003F057
		public AnimationCurve GravityCurve { get; private set; }

		// Token: 0x17000235 RID: 565
		// (get) Token: 0x06000BE9 RID: 3049 RVA: 0x00040E60 File Offset: 0x0003F060
		// (set) Token: 0x06000BEA RID: 3050 RVA: 0x00040E68 File Offset: 0x0003F068
		public int FramesToTerminalVelocity { get; private set; }

		// Token: 0x17000236 RID: 566
		// (get) Token: 0x06000BEB RID: 3051 RVA: 0x00040E71 File Offset: 0x0003F071
		// (set) Token: 0x06000BEC RID: 3052 RVA: 0x00040E79 File Offset: 0x0003F079
		public float TerminalVelocity { get; private set; }

		// Token: 0x17000237 RID: 567
		// (get) Token: 0x06000BED RID: 3053 RVA: 0x00040E82 File Offset: 0x0003F082
		// (set) Token: 0x06000BEE RID: 3054 RVA: 0x00040E8A File Offset: 0x0003F08A
		public float PredictionTime { get; private set; }
	}
}
