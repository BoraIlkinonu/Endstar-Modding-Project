using System;
using Endless.Networking;

namespace Endless.Matchmaking
{
	// Token: 0x0200003A RID: 58
	// (Invoke) Token: 0x0600018A RID: 394
	public delegate void MessageReceivedEvent<in T>(T msg) where T : Message;
}
