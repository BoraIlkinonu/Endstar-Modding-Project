using System;

namespace Endless.Props.Scripting
{
	// Token: 0x0200000B RID: 11
	[Serializable]
	public class EndlessParameterInfo : IEquatable<EndlessParameterInfo>
	{
		// Token: 0x06000025 RID: 37 RVA: 0x000026AB File Offset: 0x000008AB
		public EndlessParameterInfo(int dataType, string displayName = null)
		{
			this.DisplayName = displayName;
			this.DataType = dataType;
		}

		// Token: 0x06000026 RID: 38 RVA: 0x000026C4 File Offset: 0x000008C4
		public bool Equals(EndlessParameterInfo other)
		{
			return (other.DisplayName == this.DisplayName || (string.IsNullOrEmpty(other.DisplayName) && string.IsNullOrEmpty(this.DisplayName))) && other.DataType == this.DataType;
		}

		// Token: 0x04000020 RID: 32
		public string DisplayName;

		// Token: 0x04000021 RID: 33
		public int DataType;

		// Token: 0x04000022 RID: 34
		public string Description;

		// Token: 0x04000023 RID: 35
		public string EnumName;
	}
}
