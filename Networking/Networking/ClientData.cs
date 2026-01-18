using System;

namespace Endless.Networking
{
	// Token: 0x02000004 RID: 4
	public readonly struct ClientData
	{
		// Token: 0x0600000B RID: 11 RVA: 0x000021BE File Offset: 0x000003BE
		public ClientData(string platformId, TargetPlatforms platform, string displayName)
		{
			this.CoreData = new CoreClientData(platformId, platform);
			this.DisplayName = displayName;
		}

		// Token: 0x0600000C RID: 12 RVA: 0x000021D5 File Offset: 0x000003D5
		public void Serialize(DataBuffer _buffer)
		{
			_buffer.WriteString(this.CoreData.PlatformId);
			_buffer.WriteInteger((int)this.CoreData.Platform);
			_buffer.WriteString(this.DisplayName);
		}

		// Token: 0x0600000D RID: 13 RVA: 0x0000220C File Offset: 0x0000040C
		public static ClientData Deserialize(DataBuffer _buffer)
		{
			return new ClientData(_buffer.ReadString(true), (TargetPlatforms)_buffer.ReadInteger(true), _buffer.ReadString(true));
		}

		// Token: 0x04000003 RID: 3
		public readonly CoreClientData CoreData;

		// Token: 0x04000004 RID: 4
		public readonly string DisplayName;
	}
}
