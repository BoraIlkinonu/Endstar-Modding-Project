using UnityEngine;

namespace Endless.Gameplay;

public interface ISpawnPoint
{
	Transform GetSpawnPosition(int index);

	void ConfigurePlayer(GameplayPlayerReferenceManager playerReferenceManager);

	void HandlePlayerEnteredLevel(GameplayPlayerReferenceManager playerReferenceManager);
}
