using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Endless.Gameplay;

internal class SelfCleanup : MonoBehaviour
{
	private void OnDestroy()
	{
		Addressables.ReleaseInstance(base.gameObject);
	}
}
