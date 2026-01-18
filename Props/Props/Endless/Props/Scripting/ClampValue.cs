using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Props.Scripting
{
	// Token: 0x02000008 RID: 8
	[Serializable]
	public class ClampValue
	{
		// Token: 0x06000018 RID: 24 RVA: 0x00002506 File Offset: 0x00000706
		public ClampValue()
		{
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00002519 File Offset: 0x00000719
		public ClampValue(ClampValue other)
		{
			this.clampUsage = other.clampUsage;
			this.minValue = other.minValue;
			this.maxValue = other.maxValue;
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600001A RID: 26 RVA: 0x00002550 File Offset: 0x00000750
		[JsonIgnore]
		public bool ShouldClampValue
		{
			get
			{
				return this.clampUsage > ClampValue.ClampUsage.None;
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x0600001B RID: 27 RVA: 0x0000255B File Offset: 0x0000075B
		[JsonIgnore]
		public float MinValue
		{
			get
			{
				return this.minValue;
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x0600001C RID: 28 RVA: 0x00002563 File Offset: 0x00000763
		[JsonIgnore]
		public float MaxValue
		{
			get
			{
				return this.maxValue;
			}
		}

		// Token: 0x04000018 RID: 24
		[SerializeField]
		private ClampValue.ClampUsage clampUsage;

		// Token: 0x04000019 RID: 25
		[SerializeField]
		private float minValue;

		// Token: 0x0400001A RID: 26
		[SerializeField]
		private float maxValue = 1f;

		// Token: 0x02000047 RID: 71
		public enum ClampUsage
		{
			// Token: 0x040000E2 RID: 226
			None,
			// Token: 0x040000E3 RID: 227
			Static
		}
	}
}
