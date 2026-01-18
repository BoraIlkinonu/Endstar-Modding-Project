using System;
using Endless.Gameplay.UI;
using Endless.Props;
using Endless.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x020000D9 RID: 217
	public class InteractableCollider : MonoBehaviour
	{
		// Token: 0x170000B0 RID: 176
		// (get) Token: 0x06000475 RID: 1141 RVA: 0x00017AB6 File Offset: 0x00015CB6
		// (set) Token: 0x06000476 RID: 1142 RVA: 0x00017ABE File Offset: 0x00015CBE
		public InteractableBase InteractableBase { get; set; }

		// Token: 0x170000B1 RID: 177
		// (get) Token: 0x06000477 RID: 1143 RVA: 0x00017AC7 File Offset: 0x00015CC7
		// (set) Token: 0x06000478 RID: 1144 RVA: 0x00017ACF File Offset: 0x00015CCF
		public ColliderInfo ColliderInfo { get; set; }

		// Token: 0x170000B2 RID: 178
		// (get) Token: 0x06000479 RID: 1145 RVA: 0x00017AD8 File Offset: 0x00015CD8
		// (set) Token: 0x0600047A RID: 1146 RVA: 0x00017AE0 File Offset: 0x00015CE0
		public bool IsInteractable { get; set; } = true;

		// Token: 0x170000B3 RID: 179
		// (get) Token: 0x0600047B RID: 1147 RVA: 0x00017AE9 File Offset: 0x00015CE9
		// (set) Token: 0x0600047C RID: 1148 RVA: 0x00017AF1 File Offset: 0x00015CF1
		public Vector3? OverrideAnchorPosition { get; set; }

		// Token: 0x170000B4 RID: 180
		// (get) Token: 0x0600047D RID: 1149 RVA: 0x00017AFA File Offset: 0x00015CFA
		// (set) Token: 0x0600047E RID: 1150 RVA: 0x00017B02 File Offset: 0x00015D02
		public bool UsePlayerForAnchor { get; set; }

		// Token: 0x170000B5 RID: 181
		// (get) Token: 0x0600047F RID: 1151 RVA: 0x00017B0B File Offset: 0x00015D0B
		// (set) Token: 0x06000480 RID: 1152 RVA: 0x00017B13 File Offset: 0x00015D13
		public UIInteractionPromptAnchor InteractionPromptAnchor { get; private set; }

		// Token: 0x06000481 RID: 1153 RVA: 0x00017B1C File Offset: 0x00015D1C
		private void Start()
		{
			NetworkBehaviourSingleton<GameEndManager>.Instance.OnGameEndScreenTriggered += this.UnselectLocally;
		}

		// Token: 0x06000482 RID: 1154 RVA: 0x00017B34 File Offset: 0x00015D34
		private void OnDestroy()
		{
			NetworkBehaviourSingleton<GameEndManager>.Instance.OnGameEndScreenTriggered -= this.UnselectLocally;
		}

		// Token: 0x06000483 RID: 1155 RVA: 0x00017B4C File Offset: 0x00015D4C
		public void UnselectLocally()
		{
			if (this.InteractionPromptAnchor)
			{
				this.InteractionPromptAnchor.Close();
				this.InteractionPromptAnchor = null;
			}
			this.OnUnselectedLocally.Invoke();
		}

		// Token: 0x06000484 RID: 1156 RVA: 0x00017B78 File Offset: 0x00015D78
		public void SelectLocally()
		{
			if (!this.InteractionPromptAnchor)
			{
				GameplayPlayerReferenceManager gameplayPlayerReferenceManager = MonoBehaviourSingleton<PlayerManager>.Instance.GetLocalPlayerObject() as GameplayPlayerReferenceManager;
				if (this.UsePlayerForAnchor)
				{
					this.InteractionPromptAnchor = MonoBehaviourSingleton<UIInteractionPromptAnchorManager>.Instance.CreateInstance(gameplayPlayerReferenceManager.ApperanceController.AppearanceAnimator.transform, gameplayPlayerReferenceManager, this.InteractableBase.InteractionPrompt, new Vector3?(this.InteractableBase.InteractionPromptOffset * 2f));
				}
				else
				{
					Vector3 vector = ((this.OverrideAnchorPosition != null) ? (this.OverrideAnchorPosition.Value - base.transform.position + this.InteractableBase.InteractionPromptOffset) : this.InteractableBase.InteractionPromptOffset);
					this.InteractionPromptAnchor = MonoBehaviourSingleton<UIInteractionPromptAnchorManager>.Instance.CreateInstance(base.transform, gameplayPlayerReferenceManager, this.InteractableBase.InteractionPrompt, new Vector3?(vector));
				}
			}
			this.OnSelectedLocally.Invoke();
		}

		// Token: 0x06000485 RID: 1157 RVA: 0x00017C74 File Offset: 0x00015E74
		public void SetInteractionResultSprite(Sprite newInteractionResultSprite)
		{
			if (this.InteractionPromptAnchor)
			{
				this.InteractionPromptAnchor.SetInteractionResultSprite(newInteractionResultSprite);
			}
		}

		// Token: 0x06000486 RID: 1158 RVA: 0x00017C90 File Offset: 0x00015E90
		public bool CheckInteractionDistance(InteractorBase interactor)
		{
			float num = interactor.InteractOffset + interactor.InteractRadius * 2f + 2f;
			return this.ColliderInfo.GetDistanceFromPoint(interactor.transform.position) <= num;
		}

		// Token: 0x06000487 RID: 1159 RVA: 0x00017CD3 File Offset: 0x00015ED3
		internal void AttemptInteract(InteractorBase interactorBase)
		{
			this.InteractableBase.AttemptInteract(interactorBase, this);
		}

		// Token: 0x040003C8 RID: 968
		public UnityEvent OnSelectedLocally = new UnityEvent();

		// Token: 0x040003C9 RID: 969
		public UnityEvent OnUnselectedLocally = new UnityEvent();
	}
}
