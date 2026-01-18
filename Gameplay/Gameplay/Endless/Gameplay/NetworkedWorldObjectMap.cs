using System;
using System.Collections.Generic;
using Endless.Shared;

namespace Endless.Gameplay
{
	// Token: 0x020002BE RID: 702
	public class NetworkedWorldObjectMap : MonoBehaviourSingleton<NetworkedWorldObjectMap>
	{
		// Token: 0x17000322 RID: 802
		// (get) Token: 0x06000FFD RID: 4093 RVA: 0x00051D44 File Offset: 0x0004FF44
		public Dictionary<uint, WorldObject> ObjectMap { get; } = new Dictionary<uint, WorldObject>();
	}
}
