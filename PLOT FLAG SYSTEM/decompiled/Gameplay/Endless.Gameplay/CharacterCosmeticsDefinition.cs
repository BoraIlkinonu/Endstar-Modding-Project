using Endless.Shared.DataTypes;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Endless.Gameplay;

[CreateAssetMenu(menuName = "ScriptableObject/Character Cosmetics/Character Cosmetics Definition", fileName = "Character Cosmetics")]
public class CharacterCosmeticsDefinition : ScriptableObject
{
	[SerializeField]
	private string displayName;

	[SerializeField]
	private string assetId;

	[SerializeField]
	private AssetReferenceGameObject assetReference;

	[SerializeField]
	private Sprite portraitSprite;

	public string DisplayName => displayName;

	public SerializableGuid AssetId => assetId;

	public bool IsMissingAsset => !assetReference.RuntimeKeyIsValid();

	public bool IsLoaded => assetReference.IsDone;

	public Sprite PortraitSprite => portraitSprite;

	public AsyncOperationHandle<GameObject> Instantiate(Transform parent = null, bool isInstantiatedInWorldSpace = false)
	{
		AsyncOperationHandle<GameObject> result = assetReference.InstantiateAsync(parent, isInstantiatedInWorldSpace);
		result.Completed += HandleObjectInstantiationComplete;
		return result;
	}

	public AsyncOperationHandle<GameObject> Instantiate(Vector3 position, Quaternion rotation, Transform parent = null)
	{
		AsyncOperationHandle<GameObject> result = assetReference.InstantiateAsync(position, rotation, parent);
		result.Completed += HandleObjectInstantiationComplete;
		return result;
	}

	private static void HandleObjectInstantiationComplete(AsyncOperationHandle<GameObject> handle)
	{
		handle.Result.AddComponent<SelfCleanup>();
	}
}
