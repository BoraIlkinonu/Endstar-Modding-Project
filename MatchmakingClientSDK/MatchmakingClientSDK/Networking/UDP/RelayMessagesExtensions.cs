using System;

namespace Networking.UDP
{
	// Token: 0x02000032 RID: 50
	public static class RelayMessagesExtensions
	{
		// Token: 0x06000165 RID: 357 RVA: 0x00008937 File Offset: 0x00006B37
		public static bool TryGetRelayMessage(this byte message, out RelayMessages relayMessage)
		{
			if ((int)message >= RelayMessagesExtensions.RelayMessagesArray.Length)
			{
				relayMessage = RelayMessages.ClientConnected;
				return false;
			}
			relayMessage = RelayMessagesExtensions.RelayMessagesArray[(int)message];
			return true;
		}

		// Token: 0x04000140 RID: 320
		public static RelayMessages[] RelayMessagesArray = Enum.GetValues(typeof(RelayMessages)) as RelayMessages[];
	}
}
