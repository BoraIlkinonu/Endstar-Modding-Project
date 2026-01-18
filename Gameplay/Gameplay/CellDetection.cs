using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using UnityEngine;

// Token: 0x0200000A RID: 10
public class CellDetection : MonoBehaviour
{
	// Token: 0x0600002C RID: 44 RVA: 0x000029B8 File Offset: 0x00000BB8
	public Vector3Int GetCurrentCell()
	{
		return Stage.WorldSpacePointToGridCoordinate(base.transform.position);
	}

	// Token: 0x0600002D RID: 45 RVA: 0x000029CA File Offset: 0x00000BCA
	public void FixedUpdate()
	{
		GridUtilities.DrawDebugCube(this.GetCurrentCell(), 1f, Color.cyan, Time.fixedDeltaTime, false, false);
	}
}
