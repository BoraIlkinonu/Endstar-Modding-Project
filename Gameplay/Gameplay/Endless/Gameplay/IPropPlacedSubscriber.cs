using System;
using Endless.Shared.DataTypes;

namespace Endless.Gameplay
{
	// Token: 0x020000EC RID: 236
	public interface IPropPlacedSubscriber
	{
		// Token: 0x06000552 RID: 1362
		void PropPlaced(SerializableGuid instanceId, bool isCopy);
	}
}
