using System;
using UnityEngine;

namespace Endless.Props.ReferenceComponents
{
	// Token: 0x02000016 RID: 22
	public class DoorReferences : ReferenceBase
	{
		// Token: 0x17000020 RID: 32
		// (get) Token: 0x06000059 RID: 89 RVA: 0x00002A6B File Offset: 0x00000C6B
		// (set) Token: 0x0600005A RID: 90 RVA: 0x00002A73 File Offset: 0x00000C73
		public Transform PrimaryDoor { get; set; }

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x0600005B RID: 91 RVA: 0x00002A7C File Offset: 0x00000C7C
		// (set) Token: 0x0600005C RID: 92 RVA: 0x00002A84 File Offset: 0x00000C84
		public Transform SecondaryDoor { get; private set; }

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x0600005D RID: 93 RVA: 0x00002A8D File Offset: 0x00000C8D
		// (set) Token: 0x0600005E RID: 94 RVA: 0x00002A95 File Offset: 0x00000C95
		public Vector3 RotationDegrees { get; private set; } = new Vector3(0f, 90f, 0f);

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x0600005F RID: 95 RVA: 0x00002A9E File Offset: 0x00000C9E
		// (set) Token: 0x06000060 RID: 96 RVA: 0x00002AA6 File Offset: 0x00000CA6
		public uint DoorOpenDelayFrames { get; private set; } = 4U;

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x06000061 RID: 97 RVA: 0x00002AAF File Offset: 0x00000CAF
		// (set) Token: 0x06000062 RID: 98 RVA: 0x00002AB7 File Offset: 0x00000CB7
		public float DoorOpenTime { get; private set; } = 2f;

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x06000063 RID: 99 RVA: 0x00002AC0 File Offset: 0x00000CC0
		// (set) Token: 0x06000064 RID: 100 RVA: 0x00002AC8 File Offset: 0x00000CC8
		public bool OpenBothDirections { get; private set; }

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x06000065 RID: 101 RVA: 0x00002AD1 File Offset: 0x00000CD1
		// (set) Token: 0x06000066 RID: 102 RVA: 0x00002AD9 File Offset: 0x00000CD9
		public Animator DoorAnimator { get; private set; }

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x06000067 RID: 103 RVA: 0x00002AE2 File Offset: 0x00000CE2
		// (set) Token: 0x06000068 RID: 104 RVA: 0x00002AEA File Offset: 0x00000CEA
		public string UnlockAnimName { get; private set; } = "Unlock";

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x06000069 RID: 105 RVA: 0x00002AF3 File Offset: 0x00000CF3
		// (set) Token: 0x0600006A RID: 106 RVA: 0x00002AFB File Offset: 0x00000CFB
		public float UnlockAnimTime { get; private set; } = 1.5f;

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x0600006B RID: 107 RVA: 0x00002B04 File Offset: 0x00000D04
		// (set) Token: 0x0600006C RID: 108 RVA: 0x00002B0C File Offset: 0x00000D0C
		public string LockAnimName { get; private set; } = "Lock";

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x0600006D RID: 109 RVA: 0x00002B15 File Offset: 0x00000D15
		// (set) Token: 0x0600006E RID: 110 RVA: 0x00002B1D File Offset: 0x00000D1D
		public float LockAnimTime { get; private set; } = 1.5f;
	}
}
