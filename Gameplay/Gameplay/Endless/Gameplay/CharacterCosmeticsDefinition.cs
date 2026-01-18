using System;
using Endless.Shared.DataTypes;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Endless.Gameplay
{
	// Token: 0x02000085 RID: 133
	[CreateAssetMenu(menuName = "ScriptableObject/Character Cosmetics/Character Cosmetics Definition", fileName = "Character Cosmetics")]
	public class CharacterCosmeticsDefinition : ScriptableObject
	{
		// Token: 0x17000071 RID: 113
		// (get) Token: 0x06000260 RID: 608 RVA: 0x0000D6D3 File Offset: 0x0000B8D3
		public string DisplayName
		{
			get
			{
				return this.displayName;
			}
		}

		// Token: 0x17000072 RID: 114
		// (get) Token: 0x06000261 RID: 609 RVA: 0x0000D6DB File Offset: 0x0000B8DB
		public SerializableGuid AssetId
		{
			get
			{
				return this.assetId;
			}
		}

		// Token: 0x17000073 RID: 115
		// (get) Token: 0x06000262 RID: 610 RVA: 0x0000D6E8 File Offset: 0x0000B8E8
		public bool IsMissingAsset
		{
			get
			{
				return !this.assetReference.RuntimeKeyIsValid();
			}
		}

		// Token: 0x17000074 RID: 116
		// (get) Token: 0x06000263 RID: 611 RVA: 0x0000D6F8 File Offset: 0x0000B8F8
		public bool IsLoaded
		{
			get
			{
				return this.assetReference.IsDone;
			}
		}

		// Token: 0x17000075 RID: 117
		// (get) Token: 0x06000264 RID: 612 RVA: 0x0000D705 File Offset: 0x0000B905
		public Sprite PortraitSprite
		{
			get
			{
				return this.portraitSprite;
			}
		}

		// Token: 0x06000265 RID: 613 RVA: 0x0000D710 File Offset: 0x0000B910
		public AsyncOperationHandle<GameObject> Instantiate(Transform parent = null, bool isInstantiatedInWorldSpace = false)
		{
			AsyncOperationHandle<GameObject> asyncOperationHandle = this.assetReference.InstantiateAsync(parent, isInstantiatedInWorldSpace);
			asyncOperationHandle.Completed += CharacterCosmeticsDefinition.HandleObjectInstantiationComplete;
			return asyncOperationHandle;
		}

		// Token: 0x06000266 RID: 614 RVA: 0x0000D740 File Offset: 0x0000B940
		public AsyncOperationHandle<GameObject> Instantiate(Vector3 position, Quaternion rotation, Transform parent = null)
		{
			AsyncOperationHandle<GameObject> asyncOperationHandle = this.assetReference.InstantiateAsync(position, rotation, parent);
			asyncOperationHandle.Completed += CharacterCosmeticsDefinition.HandleObjectInstantiationComplete;
			return asyncOperationHandle;
		}

		// Token: 0x06000267 RID: 615 RVA: 0x0000D770 File Offset: 0x0000B970
		private static void HandleObjectInstantiationComplete(AsyncOperationHandle<GameObject> handle)
		{
			handle.Result.AddComponent<SelfCleanup>();
		}

		// Token: 0x04000254 RID: 596
		[SerializeField]
		private string displayName;

		// Token: 0x04000255 RID: 597
		[SerializeField]
		private string assetId;

		// Token: 0x04000256 RID: 598
		[SerializeField]
		private AssetReferenceGameObject assetReference;

		// Token: 0x04000257 RID: 599
		[SerializeField]
		private Sprite portraitSprite;
	}
}
