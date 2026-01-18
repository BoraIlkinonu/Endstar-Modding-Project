using System;

namespace Networking.UDP.Utils
{
	// Token: 0x02000039 RID: 57
	public interface INetSerializable
	{
		// Token: 0x06000191 RID: 401
		void Serialize(NetDataWriter writer);

		// Token: 0x06000192 RID: 402
		void Deserialize(NetDataReader reader);
	}
}
