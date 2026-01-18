using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x0200020B RID: 523
	public class UICellReferencePresenter : UIBasePresenter<CellReference>
	{
		// Token: 0x170000FB RID: 251
		// (get) Token: 0x06000838 RID: 2104 RVA: 0x000284F8 File Offset: 0x000266F8
		private bool HasCellValue
		{
			get
			{
				CellReference model = base.Model;
				return model != null && model.HasValue;
			}
		}

		// Token: 0x170000FC RID: 252
		// (get) Token: 0x06000839 RID: 2105 RVA: 0x00028517 File Offset: 0x00026717
		private bool RotationHasValue
		{
			get
			{
				return this.HasCellValue && base.Model.RotationHasValue;
			}
		}

		// Token: 0x0600083A RID: 2106 RVA: 0x00028530 File Offset: 0x00026730
		protected override void Start()
		{
			base.Start();
			this.inspectorTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<InspectorTool>();
			UICellReferenceView uicellReferenceView = base.View.Interface as UICellReferenceView;
			uicellReferenceView.OnClear += this.Clear;
			uicellReferenceView.OnHighlight += this.Highlight;
			uicellReferenceView.OnEyeDrop += this.SetInspectorToEyeDropperState;
			uicellReferenceView.OnPositionChanged += this.SetPosition;
			uicellReferenceView.OnRotationChanged += this.SetRotation;
			uicellReferenceView.OnRotationClear += this.ClearRotation;
		}

		// Token: 0x0600083B RID: 2107 RVA: 0x000285CF File Offset: 0x000267CF
		private void OnDisable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			if (this.listeningToInspectorEvents)
			{
				this.SetInspectorEventListeners(false);
			}
		}

		// Token: 0x0600083C RID: 2108 RVA: 0x000285F8 File Offset: 0x000267F8
		public override void SetModel(CellReference model, bool triggerOnModelChanged)
		{
			if (base.Model != model && this.HasCellValue && this.isDisplayingHighlightAndPin)
			{
				this.CloseHighlightAndPin();
			}
			base.SetModel(model, triggerOnModelChanged);
			if (this.HasCellValue)
			{
				this.DisplayHighlightAndPin();
				return;
			}
			this.CloseHighlightAndPin();
		}

		// Token: 0x0600083D RID: 2109 RVA: 0x00028638 File Offset: 0x00026838
		public override void Clear()
		{
			base.Clear();
			if (this.isDisplayingHighlightAndPin)
			{
				this.CloseHighlightAndPin();
			}
			CellReference cellReference = ReferenceFactory.CreateCellReference(null, null);
			this.SetModel(cellReference, true);
		}

		// Token: 0x0600083E RID: 2110 RVA: 0x00028679 File Offset: 0x00026879
		private void Highlight()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Highlight", Array.Empty<object>());
			}
			if (this.pin)
			{
				this.pin.Highlight();
			}
		}

		// Token: 0x0600083F RID: 2111 RVA: 0x000286AC File Offset: 0x000268AC
		private void SetRotation(float rotation)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetRotation", new object[] { rotation });
			}
			Vector3 cellPosition = base.Model.GetCellPosition();
			CellReferenceUtility.SetCell(base.Model, new Vector3?(cellPosition), new float?(rotation));
			this.SetModel(base.Model, true);
		}

		// Token: 0x06000840 RID: 2112 RVA: 0x0002870C File Offset: 0x0002690C
		private void ClearRotation()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ClearRotation", Array.Empty<object>());
			}
			Vector3 cellPosition = base.Model.GetCellPosition();
			CellReferenceUtility.SetCell(base.Model, new Vector3?(cellPosition), null);
			this.SetModel(base.Model, true);
		}

		// Token: 0x06000841 RID: 2113 RVA: 0x00028764 File Offset: 0x00026964
		private void SetInspectorToEyeDropperState()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetInspectorToEyeDropperState", Array.Empty<object>());
			}
			InspectorTool inspectorTool = this.inspectorTool;
			if (inspectorTool != null)
			{
				inspectorTool.SetStateToEyeDropper(ReferenceFilter.None);
			}
			this.SetInspectorEventListeners(true);
		}

		// Token: 0x06000842 RID: 2114 RVA: 0x00028798 File Offset: 0x00026998
		private void SetPosition(Vector3 position)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetPosition", new object[] { position });
			}
			if (base.Model.GetCellPosition() == position)
			{
				return;
			}
			CellReferenceUtility.SetCell(base.Model, new Vector3?(position), base.Model.RotationHasValue ? new float?(base.Model.GetRotation()) : null);
			this.SetModel(base.Model, true);
		}

		// Token: 0x06000843 RID: 2115 RVA: 0x00028824 File Offset: 0x00026A24
		private void SetInspectorEventListeners(bool state)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetInspectorEventListeners", new object[] { state });
			}
			if (this.listeningToInspectorEvents == state)
			{
				return;
			}
			if (state)
			{
				this.inspectorTool.OnCellEyedropped.AddListener(new UnityAction<Vector3Int, float?>(this.OnCellEyeDropped));
				this.inspectorTool.OnStateChanged.AddListener(new UnityAction<InspectorTool.States>(this.OnInspectorToolStateChanged));
			}
			else
			{
				this.inspectorTool.OnCellEyedropped.RemoveListener(new UnityAction<Vector3Int, float?>(this.OnCellEyeDropped));
				this.inspectorTool.OnStateChanged.RemoveListener(new UnityAction<InspectorTool.States>(this.OnInspectorToolStateChanged));
			}
			this.listeningToInspectorEvents = state;
		}

		// Token: 0x06000844 RID: 2116 RVA: 0x000288DC File Offset: 0x00026ADC
		private void OnCellEyeDropped(Vector3Int coordinate, float? yawRotation)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnCellEyeDropped", new object[] { coordinate, yawRotation });
			}
			if (this.isDisplayingHighlightAndPin)
			{
				this.CloseHighlightAndPin();
			}
			CellReferenceUtility.SetCell(base.Model, new Vector3?(coordinate), yawRotation);
			this.SetInspectorEventListeners(false);
			this.SetModel(base.Model, true);
		}

		// Token: 0x06000845 RID: 2117 RVA: 0x0002894D File Offset: 0x00026B4D
		private void OnInspectorToolStateChanged(InspectorTool.States state)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnInspectorToolStateChanged", new object[] { state });
			}
			if (state == InspectorTool.States.Inspect && this.listeningToInspectorEvents)
			{
				this.SetInspectorEventListeners(false);
			}
		}

		// Token: 0x06000846 RID: 2118 RVA: 0x00028984 File Offset: 0x00026B84
		private void DisplayHighlightAndPin()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "DisplayHighlightAndPin", string.Format("{0}: {1}", "isDisplayingHighlightAndPin", this.isDisplayingHighlightAndPin), Array.Empty<object>());
			}
			this.ViewMarker();
			if (this.isDisplayingHighlightAndPin)
			{
				this.pinAnchor.position = base.Model.GetCellPosition();
				return;
			}
			if (!this.pinAnchorCreated)
			{
				this.CreatePinAnchor();
			}
			this.pinAnchor.position = base.Model.GetCellPosition();
			RectTransform anchorContainer = MonoBehaviourSingleton<UICreatorReferenceManager>.Instance.AnchorContainer;
			Vector3 up = Vector3.up;
			if (this.pin)
			{
				this.pin.SetTarget(this.pinAnchor);
				this.pin.SetOffset(new Vector3?(up));
			}
			else
			{
				UIPinAnchor uipinAnchor = this.pinAnchorSource;
				Transform transform = this.pinAnchor;
				RectTransform rectTransform = anchorContainer;
				Vector3? vector = new Vector3?(up);
				this.pin = UIPinAnchor.CreateInstance(uipinAnchor, transform, rectTransform, null, vector);
				this.pin.OnClosed += this.OnPinClosed;
			}
			this.isDisplayingHighlightAndPin = true;
		}

		// Token: 0x06000847 RID: 2119 RVA: 0x00028A98 File Offset: 0x00026C98
		private void ViewMarker()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewMarker", Array.Empty<object>());
			}
			MonoBehaviourSingleton<MarkerManager>.Instance.ClearMarkersOfType(MarkerType.CellHighlight);
			Vector3Int cellPositionAsVector3Int = base.Model.GetCellPositionAsVector3Int();
			MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinate(cellPositionAsVector3Int, MarkerType.CellHighlight, this.RotationHasValue ? new float?(base.Model.GetRotation()) : null);
		}

		// Token: 0x06000848 RID: 2120 RVA: 0x00028B04 File Offset: 0x00026D04
		private void CreatePinAnchor()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "CreatePinAnchor", Array.Empty<object>());
			}
			if (this.pinAnchorCreated)
			{
				return;
			}
			GameObject gameObject = new GameObject("UICellReference Pin Anchor");
			this.pinAnchor = gameObject.transform;
			this.pinAnchor.SetParent(MonoBehaviourSingleton<UICreatorReferenceManager>.Instance.AnchorContainer);
			this.pinAnchorCreated = true;
		}

		// Token: 0x06000849 RID: 2121 RVA: 0x00028B68 File Offset: 0x00026D68
		private void CloseHighlightAndPin()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "CloseHighlightAndPin", Array.Empty<object>());
			}
			if (!this.isDisplayingHighlightAndPin)
			{
				return;
			}
			MonoBehaviourSingleton<MarkerManager>.Instance.ReleaseAllMarkers();
			if (this.pin)
			{
				this.pin.Close();
				this.pin = null;
			}
			this.isDisplayingHighlightAndPin = false;
		}

		// Token: 0x0600084A RID: 2122 RVA: 0x00028BC8 File Offset: 0x00026DC8
		private void OnPinClosed()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPinClosed", Array.Empty<object>());
			}
			if (this.pin == null)
			{
				return;
			}
			this.pin.OnClosed -= this.OnPinClosed;
			this.pin = null;
		}

		// Token: 0x04000737 RID: 1847
		[Header("UICellReferencePresenter")]
		[SerializeField]
		private UIPinAnchor pinAnchorSource;

		// Token: 0x04000738 RID: 1848
		private InspectorTool inspectorTool;

		// Token: 0x04000739 RID: 1849
		private bool isDisplayingHighlightAndPin;

		// Token: 0x0400073A RID: 1850
		private bool listeningToInspectorEvents;

		// Token: 0x0400073B RID: 1851
		private UIPinAnchor pin;

		// Token: 0x0400073C RID: 1852
		private Transform pinAnchor;

		// Token: 0x0400073D RID: 1853
		private bool pinAnchorCreated;
	}
}
