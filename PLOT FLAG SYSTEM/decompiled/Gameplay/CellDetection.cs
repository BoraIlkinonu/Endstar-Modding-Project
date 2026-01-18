using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using UnityEngine;

public class CellDetection : MonoBehaviour
{
	public Vector3Int GetCurrentCell()
	{
		return Stage.WorldSpacePointToGridCoordinate(base.transform.position);
	}

	public void FixedUpdate()
	{
		GridUtilities.DrawDebugCube(GetCurrentCell(), 1f, Color.cyan, Time.fixedDeltaTime);
	}
}
