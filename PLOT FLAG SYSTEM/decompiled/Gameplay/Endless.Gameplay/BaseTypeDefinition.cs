using UnityEngine;

namespace Endless.Gameplay;

public class BaseTypeDefinition : ComponentDefinition
{
	[SerializeField]
	private bool isUserExposed = true;

	[SerializeField]
	private bool isSpawnPoint;

	public bool IsUserExposed => isUserExposed;

	public bool IsSpawnPoint => isSpawnPoint;
}
