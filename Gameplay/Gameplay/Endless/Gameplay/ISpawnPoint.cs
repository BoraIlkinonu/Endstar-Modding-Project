using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000329 RID: 809
	public interface ISpawnPoint
	{
		// Token: 0x060012CA RID: 4810
		Transform GetSpawnPosition(int index);

		// Token: 0x060012CB RID: 4811
		void ConfigurePlayer(GameplayPlayerReferenceManager playerReferenceManager);

		// Token: 0x060012CC RID: 4812
		void HandlePlayerEnteredLevel(GameplayPlayerReferenceManager playerReferenceManager);
	}
}
