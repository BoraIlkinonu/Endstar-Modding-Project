using System;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x0200038F RID: 911
	public class UICharacterCosmeticsDefinitionSelector : UIGameObject
	{
		// Token: 0x06001730 RID: 5936 RVA: 0x0006C76F File Offset: 0x0006A96F
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.listModel.SelectionChangedUnityEvent.AddListener(new UnityAction<int, bool>(this.HandleOnSelected));
		}

		// Token: 0x06001731 RID: 5937 RVA: 0x0006C7A8 File Offset: 0x0006A9A8
		public void SetSelected(SerializableGuid assetId, bool triggerOnSelected)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} , {3}: {4} )", new object[] { "SetSelected", "assetId", assetId, "triggerOnSelected", triggerOnSelected }), this);
			}
			CharacterCosmeticsDefinition characterCosmeticsDefinition;
			if (!this.characterCosmeticsList.TryGetDefinition(assetId, out characterCosmeticsDefinition))
			{
				DebugUtility.LogWarning(string.Format("could not find {0} of {1} in {2}! Using {3}!", new object[] { "assetId", assetId, "characterCosmeticsList", "fallbackCharacterCosmeticsDefinition" }), this);
				characterCosmeticsDefinition = this.fallbackCharacterCosmeticsDefinition;
			}
			this.SetSelected(characterCosmeticsDefinition, triggerOnSelected);
		}

		// Token: 0x06001732 RID: 5938 RVA: 0x0006C854 File Offset: 0x0006AA54
		public void SetSelected(CharacterCosmeticsDefinition characterCosmeticsDefinition, bool triggerOnSelected)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "SetSelected", "characterCosmeticsDefinition", characterCosmeticsDefinition.DisplayName, "triggerOnSelected", triggerOnSelected }), this);
			}
			if (!this.listModel.Initialized)
			{
				this.listModel.Initialize();
			}
			int num = this.listModel.FilteredList.IndexOf(characterCosmeticsDefinition);
			this.listModel.Select(num, triggerOnSelected);
			if (!triggerOnSelected)
			{
				this.listView.SetDataToAllVisibleCells();
			}
		}

		// Token: 0x06001733 RID: 5939 RVA: 0x0006C8F0 File Offset: 0x0006AAF0
		private void HandleOnSelected(int index, bool selected)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleOnSelected", new object[] { index, selected });
			}
			if (!selected)
			{
				return;
			}
			CharacterCosmeticsDefinition characterCosmeticsDefinition = this.listModel[index];
			this.OnSelectedCharacterVisual.Invoke(characterCosmeticsDefinition);
			this.OnSelectedId.Invoke(characterCosmeticsDefinition.AssetId);
		}

		// Token: 0x0400129F RID: 4767
		public UnityEvent<CharacterCosmeticsDefinition> OnSelectedCharacterVisual = new UnityEvent<CharacterCosmeticsDefinition>();

		// Token: 0x040012A0 RID: 4768
		public UnityEvent<SerializableGuid> OnSelectedId = new UnityEvent<SerializableGuid>();

		// Token: 0x040012A1 RID: 4769
		[SerializeField]
		private CharacterCosmeticsList characterCosmeticsList;

		// Token: 0x040012A2 RID: 4770
		[SerializeField]
		private CharacterCosmeticsDefinition fallbackCharacterCosmeticsDefinition;

		// Token: 0x040012A3 RID: 4771
		[SerializeField]
		private UICharacterCosmeticsDefinitionListModel listModel;

		// Token: 0x040012A4 RID: 4772
		[SerializeField]
		private UICharacterCosmeticsDefinitionListView listView;

		// Token: 0x040012A5 RID: 4773
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
