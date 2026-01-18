using System;
using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Endless.Core.UI
{
	// Token: 0x02000095 RID: 149
	public class UIUserGroupScreenCharacter : MonoBehaviour, IPoolableT
	{
		// Token: 0x1700004D RID: 77
		// (get) Token: 0x06000312 RID: 786 RVA: 0x000107C8 File Offset: 0x0000E9C8
		// (set) Token: 0x06000313 RID: 787 RVA: 0x000107D0 File Offset: 0x0000E9D0
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x1700004E RID: 78
		// (get) Token: 0x06000314 RID: 788 RVA: 0x000027B9 File Offset: 0x000009B9
		public bool IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000315 RID: 789 RVA: 0x000107D9 File Offset: 0x0000E9D9
		public void OnSpawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSpawn", Array.Empty<object>());
			}
		}

		// Token: 0x06000316 RID: 790 RVA: 0x000107F4 File Offset: 0x0000E9F4
		public void OnDespawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDespawn", Array.Empty<object>());
			}
			if (!this.monitorClientCharacter)
			{
				return;
			}
			CharacterCosmeticsDefinitionUtility.ClientCharacterCosmeticsDefinitionAssetSetAction = (Action<SerializableGuid>)Delegate.Remove(CharacterCosmeticsDefinitionUtility.ClientCharacterCosmeticsDefinitionAssetSetAction, new Action<SerializableGuid>(this.Display));
			this.monitorClientCharacter = false;
		}

		// Token: 0x06000317 RID: 791 RVA: 0x0001084C File Offset: 0x0000EA4C
		public void Display(SerializableGuid assetId, bool monitorClientCharacter)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "Display", "assetId", assetId, "monitorClientCharacter", monitorClientCharacter }), this);
			}
			this.monitorClientCharacter = monitorClientCharacter;
			if (monitorClientCharacter)
			{
				CharacterCosmeticsDefinitionUtility.ClientCharacterCosmeticsDefinitionAssetSetAction = (Action<SerializableGuid>)Delegate.Combine(CharacterCosmeticsDefinitionUtility.ClientCharacterCosmeticsDefinitionAssetSetAction, new Action<SerializableGuid>(this.Display));
			}
			this.Display(assetId);
		}

		// Token: 0x06000318 RID: 792 RVA: 0x000108D4 File Offset: 0x0000EAD4
		private void Display(SerializableGuid assetId)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "Display", "assetId", assetId), this);
			}
			if (assetId.IsEmpty && this.characterCosmeticsList.Cosmetics.Count > 0)
			{
				assetId = this.characterCosmeticsList.Cosmetics[0].AssetId;
			}
			CharacterCosmeticsDefinition characterCosmeticsDefinition;
			if (!this.characterCosmeticsList.TryGetDefinition(assetId, out characterCosmeticsDefinition))
			{
				DebugUtility.LogWarning(string.Format("could not find {0} of {1} in {2}! Using {3}!", new object[] { "assetId", assetId, "characterCosmeticsList", "fallbackCharacterCosmeticsDefinition" }), this);
				characterCosmeticsDefinition = this.fallbackCharacterCosmeticsDefinition;
			}
			if (this.instance)
			{
				global::UnityEngine.Object.Destroy(this.instance);
			}
			characterCosmeticsDefinition.Instantiate(base.transform, false).Completed += this.HandleCosmeticInstantiated;
		}

		// Token: 0x06000319 RID: 793 RVA: 0x000109C4 File Offset: 0x0000EBC4
		private void HandleCosmeticInstantiated(AsyncOperationHandle<GameObject> handle)
		{
			this.instance = handle.Result;
			UIUserGroupScreenCharacter.SetGameLayerRecursive(this.instance.gameObject, LayerMask.NameToLayer("UI Background"));
			this.instance.transform.localPosition = Vector3.zero;
			this.instance.transform.localEulerAngles = Vector3.zero;
		}

		// Token: 0x0600031A RID: 794 RVA: 0x00010A24 File Offset: 0x0000EC24
		private static void SetGameLayerRecursive(GameObject target, int layer)
		{
			Transform[] componentsInChildren = target.GetComponentsInChildren<Transform>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.layer = layer;
			}
		}

		// Token: 0x04000243 RID: 579
		[SerializeField]
		private CharacterCosmeticsList characterCosmeticsList;

		// Token: 0x04000244 RID: 580
		[SerializeField]
		private CharacterCosmeticsDefinition fallbackCharacterCosmeticsDefinition;

		// Token: 0x04000245 RID: 581
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000246 RID: 582
		private GameObject instance;

		// Token: 0x04000247 RID: 583
		private bool monitorClientCharacter;
	}
}
