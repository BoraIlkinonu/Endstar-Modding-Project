using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000222 RID: 546
	public abstract class UIBaseInstanceReferenceView<TModel, TViewStyle> : UIInspectorPropReferenceView<TModel, TViewStyle>, IInstanceReferenceViewable where TModel : InstanceReference where TViewStyle : Enum
	{
		// Token: 0x14000018 RID: 24
		// (add) Token: 0x060008CE RID: 2254 RVA: 0x0002A704 File Offset: 0x00028904
		// (remove) Token: 0x060008CF RID: 2255 RVA: 0x0002A73C File Offset: 0x0002893C
		public event Action<SerializableGuid> OnInstanceEyeDropped;

		// Token: 0x060008D0 RID: 2256 RVA: 0x0002A774 File Offset: 0x00028974
		protected override void Start()
		{
			base.Start();
			this.inspectorTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<InspectorTool>();
			this.eyeDropperButton.onClick.AddListener(new UnityAction(this.SetStateToEyeDropper));
			this.highlightReferenceButton.onClick.AddListener(new UnityAction(this.HighlightReference));
			this.inspectorTool.OnStateChanged.AddListener(new UnityAction<InspectorTool.States>(this.OnInspectorStateChanged));
		}

		// Token: 0x060008D1 RID: 2257 RVA: 0x0002A7EC File Offset: 0x000289EC
		public override void View(TModel model)
		{
			base.View(model);
			this.isEmpty = this.GetReferenceIsEmpty(model);
			this.eyeDropperButton.gameObject.SetActive(this.isEmpty);
			this.highlightReferenceButton.interactable = !this.isEmpty;
			base.Layout();
			this.ClosePin();
			if (this.isEmpty || model.useContext)
			{
				return;
			}
			GameObject instanceObject = InspectorReferenceUtility.GetInstanceObject(model);
			RectTransform anchorContainer = MonoBehaviourSingleton<UICreatorReferenceManager>.Instance.AnchorContainer;
			SerializableGuid instanceDefinitionId = InspectorReferenceUtility.GetInstanceDefinitionId(model);
			PropLibrary.RuntimePropInfo runtimePropInfo;
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(instanceDefinitionId, out runtimePropInfo))
			{
				DebugUtility.LogException(new NullReferenceException("endlessDefinition is null!"), this);
				return;
			}
			Vector3 pinOffset = this.GetPinOffset(runtimePropInfo);
			UIPinAnchor uipinAnchor = this.pinAnchorSource;
			Transform transform = instanceObject.transform;
			RectTransform rectTransform = anchorContainer;
			Vector3? vector = new Vector3?(pinOffset);
			this.pin = UIPinAnchor.CreateInstance(uipinAnchor, transform, rectTransform, null, vector);
		}

		// Token: 0x060008D2 RID: 2258 RVA: 0x0002A8D7 File Offset: 0x00028AD7
		public override void Clear()
		{
			base.Clear();
			this.ClosePin();
			this.UnsubscribeToOnItemEyeDropped();
		}

		// Token: 0x060008D3 RID: 2259 RVA: 0x0002A8EB File Offset: 0x00028AEB
		public override void SetInteractable(bool interactable)
		{
			base.SetInteractable(interactable);
			this.eyeDropperButton.interactable = interactable;
			this.highlightReferenceButton.interactable = interactable;
		}

		// Token: 0x060008D4 RID: 2260 RVA: 0x0002A90C File Offset: 0x00028B0C
		public override void SetPermission(Permissions permission)
		{
			base.SetPermission(permission);
			this.eyeDropperButton.gameObject.SetActive(this.isEmpty && permission == Permissions.ReadWrite);
		}

		// Token: 0x060008D5 RID: 2261 RVA: 0x0002A934 File Offset: 0x00028B34
		protected override string GetReferenceName(TModel model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetReferenceName", "model", model), this);
			}
			return model.GetReferenceName();
		}

		// Token: 0x060008D6 RID: 2262 RVA: 0x0002A96C File Offset: 0x00028B6C
		protected override bool GetReferenceIsEmpty(TModel model)
		{
			InstanceReference instanceReference = model;
			return base.GetReferenceIsEmpty(model) && !instanceReference.useContext;
		}

		// Token: 0x060008D7 RID: 2263 RVA: 0x0002A994 File Offset: 0x00028B94
		protected virtual Vector3 GetPinOffset(PropLibrary.RuntimePropInfo endlessDefinition)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("GetPinOffset ( endlessDefinition: " + endlessDefinition.PropData.Name + " )", this);
			}
			return Vector3.up * (float)endlessDefinition.PropData.GetBoundingSize().y;
		}

		// Token: 0x060008D8 RID: 2264 RVA: 0x0002A9E8 File Offset: 0x00028BE8
		private void SetStateToEyeDropper()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("SetStateToEyeDropper", this);
			}
			this.inspectorTool.SetStateToEyeDropper(this.ReferenceFilter);
			if (this.subscribedToOnItemEyeDropped)
			{
				return;
			}
			this.inspectorTool.OnItemEyeDropped.AddListener(new UnityAction<SerializableGuid, SerializableGuid, PropEntry>(this.OnItemEyeDropped));
			this.subscribedToOnItemEyeDropped = true;
		}

		// Token: 0x060008D9 RID: 2265 RVA: 0x0002AA48 File Offset: 0x00028C48
		private void OnItemEyeDropped(SerializableGuid assetId, SerializableGuid instanceId, PropEntry propEntry)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", new object[] { "OnItemEyeDropped", "assetId", assetId, "instanceId", instanceId, "propEntry", propEntry }), this);
			}
			if (!this.subscribedToOnItemEyeDropped)
			{
				return;
			}
			Action<SerializableGuid> onInstanceEyeDropped = this.OnInstanceEyeDropped;
			if (onInstanceEyeDropped != null)
			{
				onInstanceEyeDropped(instanceId);
			}
			this.UnsubscribeToOnItemEyeDropped();
		}

		// Token: 0x060008DA RID: 2266 RVA: 0x0002AACA File Offset: 0x00028CCA
		private void OnInspectorStateChanged(InspectorTool.States newState)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "OnInspectorStateChanged", "newState", newState), this);
			}
			if (newState == InspectorTool.States.Inspect && this.subscribedToOnItemEyeDropped)
			{
				this.UnsubscribeToOnItemEyeDropped();
			}
		}

		// Token: 0x060008DB RID: 2267 RVA: 0x0002AB05 File Offset: 0x00028D05
		private void HighlightReference()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("HighlightReference", this);
			}
			UIPinAnchor uipinAnchor = this.pin;
			if (uipinAnchor == null)
			{
				return;
			}
			uipinAnchor.Highlight();
		}

		// Token: 0x060008DC RID: 2268 RVA: 0x0002AB2C File Offset: 0x00028D2C
		private void UnsubscribeToOnItemEyeDropped()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("UnsubscribeToOnItemEyeDropped", this);
			}
			if (!this.subscribedToOnItemEyeDropped)
			{
				return;
			}
			this.inspectorTool.OnItemEyeDropped.RemoveListener(new UnityAction<SerializableGuid, SerializableGuid, PropEntry>(this.OnItemEyeDropped));
			this.subscribedToOnItemEyeDropped = false;
		}

		// Token: 0x060008DD RID: 2269 RVA: 0x0002AB78 File Offset: 0x00028D78
		private void ClosePin()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("ClosePin", this);
			}
			if (!this.pin)
			{
				return;
			}
			this.pin.Close();
			this.pin = null;
		}

		// Token: 0x0400077E RID: 1918
		[Header("UIBaseInstanceReferenceView")]
		[SerializeField]
		private UIButton eyeDropperButton;

		// Token: 0x0400077F RID: 1919
		[SerializeField]
		private UIButton highlightReferenceButton;

		// Token: 0x04000780 RID: 1920
		[SerializeField]
		private UIPinAnchor pinAnchorSource;

		// Token: 0x04000781 RID: 1921
		private InspectorTool inspectorTool;

		// Token: 0x04000782 RID: 1922
		private UIPinAnchor pin;

		// Token: 0x04000783 RID: 1923
		private bool isEmpty;

		// Token: 0x04000784 RID: 1924
		private bool subscribedToOnItemEyeDropped;
	}
}
