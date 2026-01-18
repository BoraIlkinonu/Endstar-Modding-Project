using System;
using Endless.Gameplay;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x0200020C RID: 524
	public class UICellReferenceView : UIBaseView<CellReference, UICellReferenceView.Styles>, IUIInteractable
	{
		// Token: 0x1400000D RID: 13
		// (add) Token: 0x0600084C RID: 2124 RVA: 0x00028C24 File Offset: 0x00026E24
		// (remove) Token: 0x0600084D RID: 2125 RVA: 0x00028C5C File Offset: 0x00026E5C
		public event Action OnClear;

		// Token: 0x1400000E RID: 14
		// (add) Token: 0x0600084E RID: 2126 RVA: 0x00028C94 File Offset: 0x00026E94
		// (remove) Token: 0x0600084F RID: 2127 RVA: 0x00028CCC File Offset: 0x00026ECC
		public event Action OnEyeDrop;

		// Token: 0x1400000F RID: 15
		// (add) Token: 0x06000850 RID: 2128 RVA: 0x00028D04 File Offset: 0x00026F04
		// (remove) Token: 0x06000851 RID: 2129 RVA: 0x00028D3C File Offset: 0x00026F3C
		public event Action OnHighlight;

		// Token: 0x14000010 RID: 16
		// (add) Token: 0x06000852 RID: 2130 RVA: 0x00028D74 File Offset: 0x00026F74
		// (remove) Token: 0x06000853 RID: 2131 RVA: 0x00028DAC File Offset: 0x00026FAC
		public event Action<Vector3> OnPositionChanged;

		// Token: 0x14000011 RID: 17
		// (add) Token: 0x06000854 RID: 2132 RVA: 0x00028DE4 File Offset: 0x00026FE4
		// (remove) Token: 0x06000855 RID: 2133 RVA: 0x00028E1C File Offset: 0x0002701C
		public event Action<float> OnRotationChanged;

		// Token: 0x14000012 RID: 18
		// (add) Token: 0x06000856 RID: 2134 RVA: 0x00028E54 File Offset: 0x00027054
		// (remove) Token: 0x06000857 RID: 2135 RVA: 0x00028E8C File Offset: 0x0002708C
		public event Action OnRotationClear;

		// Token: 0x170000FD RID: 253
		// (get) Token: 0x06000858 RID: 2136 RVA: 0x00028EC1 File Offset: 0x000270C1
		// (set) Token: 0x06000859 RID: 2137 RVA: 0x00028EC9 File Offset: 0x000270C9
		public override UICellReferenceView.Styles Style { get; protected set; }

		// Token: 0x0600085A RID: 2138 RVA: 0x00028ED4 File Offset: 0x000270D4
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.vector3ClearButton.onClick.AddListener(new UnityAction(this.OnClearButtonPressed));
			this.highlightButton.onClick.AddListener(new UnityAction(this.OnHighlightButtonPressed));
			this.eyeDropButton.onClick.AddListener(new UnityAction(this.OnEyeDropButtonPressed));
			this.vector3Presenter.OnModelChanged += this.InvokeOnPositionChanged;
			this.rotationFloatControl.OnModelChanged += this.OnRotationSet;
			this.rotationClearButton.onClick.AddListener(new UnityAction(this.OnRotationClearButtonPressed));
			this.vector3FieldView.SetRaycastTargetGraphics(false);
		}

		// Token: 0x0600085B RID: 2139 RVA: 0x00028FA4 File Offset: 0x000271A4
		public override void View(CellReference model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
			bool flag = this.Style == UICellReferenceView.Styles.ReadOnly;
			bool flag2 = model != null && model.HasValue;
			bool flag3 = flag2 && model.RotationHasValue;
			this.SetVector3IntActive(flag2 && !flag);
			this.SetRotationActive(flag2 && !flag);
			this.SetNoneVisualActive(!flag2 && !flag);
			bool flag4 = this.canClear && flag2 && !flag;
			this.SetClearButtonActive(flag4);
			this.SetRotationClearButtonActive(flag3 && !flag);
			if (flag2)
			{
				Vector3 cellPosition = model.GetCellPosition();
				this.SetVector3Value(cellPosition);
				if (flag3)
				{
					this.SetRotationValue(model.GetRotation(), true);
				}
				else
				{
					this.SetRotationValue(1E-08f, false);
				}
			}
			else
			{
				this.SetVector3Value(Vector3Int.zero);
				this.SetRotationValue(1E-08f, false);
			}
			this.SetHighlightButtonActive(flag2 && !flag);
			this.SetEyeDropActive(this.canEyeDrop && !flag);
		}

		// Token: 0x0600085C RID: 2140 RVA: 0x000290BF File Offset: 0x000272BF
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
		}

		// Token: 0x0600085D RID: 2141 RVA: 0x000290DC File Offset: 0x000272DC
		public void SetInteractable(bool interactable)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetInteractable", new object[] { interactable });
			}
			IUIInteractable iuiinteractable = this.vector3Presenter.Viewable as IUIInteractable;
			if (iuiinteractable != null)
			{
				iuiinteractable.SetInteractable(interactable);
			}
			this.vector3ClearButton.interactable = interactable;
			this.highlightButton.interactable = interactable;
			this.eyeDropButton.interactable = interactable;
			IUIInteractable iuiinteractable2 = this.rotationFloatControl.View.Interface as IUIInteractable;
			if (iuiinteractable2 != null)
			{
				iuiinteractable2.SetInteractable(interactable);
			}
			this.rotationClearButton.interactable = interactable;
		}

		// Token: 0x0600085E RID: 2142 RVA: 0x00029178 File Offset: 0x00027378
		public void SetVector3IntActive(bool active)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetVector3IntActive", new object[] { active });
			}
			this.vector3Presenter.gameObject.SetActive(active);
			this.vector3ControlFlexibleSpace.SetActive(active);
		}

		// Token: 0x0600085F RID: 2143 RVA: 0x000291C4 File Offset: 0x000273C4
		public void SetRotationActive(bool active)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetRotationActive", new object[] { active });
			}
			this.rotationContainer.SetActive(active);
		}

		// Token: 0x06000860 RID: 2144 RVA: 0x000291F4 File Offset: 0x000273F4
		public void SetNoneVisualActive(bool active)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetNoneVisualActive", new object[] { active });
			}
			this.noneVisual.SetActive(active);
		}

		// Token: 0x06000861 RID: 2145 RVA: 0x00029224 File Offset: 0x00027424
		public void SetClearButtonActive(bool active)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetClearButtonActive", new object[] { active });
			}
			this.vector3ClearButtonSpace.SetActive(active);
			this.vector3ClearButton.gameObject.SetActive(active);
		}

		// Token: 0x06000862 RID: 2146 RVA: 0x00029270 File Offset: 0x00027470
		public void SetHighlightButtonActive(bool active)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetHighlightButtonActive", new object[] { active });
			}
			this.highlightButton.gameObject.SetActive(active);
		}

		// Token: 0x06000863 RID: 2147 RVA: 0x000292A5 File Offset: 0x000274A5
		public void SetRotationClearButtonActive(bool active)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetRotationClearButtonActive", new object[] { active });
			}
			this.rotationClearButton.gameObject.SetActive(active);
		}

		// Token: 0x06000864 RID: 2148 RVA: 0x000292DC File Offset: 0x000274DC
		public void SetEyeDropActive(bool active)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetEyeDropActive", new object[] { active });
			}
			this.eyeDropButton.gameObject.SetActive(active);
			this.eyeDropImage.gameObject.SetActive(active);
		}

		// Token: 0x06000865 RID: 2149 RVA: 0x0002932D File Offset: 0x0002752D
		public void SetVector3Value(Vector3 value)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetVector3Value", new object[] { value });
			}
			this.vector3Presenter.SetModel(value, false);
		}

		// Token: 0x06000866 RID: 2150 RVA: 0x00029360 File Offset: 0x00027560
		public void SetRotationValue(float value, bool hasValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetRotationValue", new object[] { value, hasValue });
			}
			this.rotationFloatControl.SetModel(hasValue ? value : 1E-08f, false);
			if (!hasValue)
			{
				UIFloatView uifloatView = this.rotationFloatControl.View.Interface as UIFloatView;
				if (uifloatView != null)
				{
					uifloatView.OverrideInputField("null");
				}
			}
		}

		// Token: 0x06000867 RID: 2151 RVA: 0x000293D6 File Offset: 0x000275D6
		private void OnClearButtonPressed()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnClearButtonPressed", Array.Empty<object>());
			}
			Action onClear = this.OnClear;
			if (onClear == null)
			{
				return;
			}
			onClear();
		}

		// Token: 0x06000868 RID: 2152 RVA: 0x00029400 File Offset: 0x00027600
		private void OnHighlightButtonPressed()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnHighlightButtonPressed", Array.Empty<object>());
			}
			Action onHighlight = this.OnHighlight;
			if (onHighlight == null)
			{
				return;
			}
			onHighlight();
		}

		// Token: 0x06000869 RID: 2153 RVA: 0x0002942A File Offset: 0x0002762A
		private void OnEyeDropButtonPressed()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEyeDropButtonPressed", Array.Empty<object>());
			}
			Action onEyeDrop = this.OnEyeDrop;
			if (onEyeDrop == null)
			{
				return;
			}
			onEyeDrop();
		}

		// Token: 0x0600086A RID: 2154 RVA: 0x00029454 File Offset: 0x00027654
		private void InvokeOnPositionChanged(object position)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InvokeOnPositionChanged", new object[] { position });
			}
			Vector3 vector = (Vector3)position;
			Action<Vector3> onPositionChanged = this.OnPositionChanged;
			if (onPositionChanged == null)
			{
				return;
			}
			onPositionChanged(vector);
		}

		// Token: 0x0600086B RID: 2155 RVA: 0x00029496 File Offset: 0x00027696
		private void OnRotationSet(object rotationAsFloat)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnRotationSet", new object[] { rotationAsFloat });
			}
			Action<float> onRotationChanged = this.OnRotationChanged;
			if (onRotationChanged == null)
			{
				return;
			}
			onRotationChanged((float)rotationAsFloat);
		}

		// Token: 0x0600086C RID: 2156 RVA: 0x000294CB File Offset: 0x000276CB
		private void OnRotationClearButtonPressed()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnRotationClearButtonPressed", Array.Empty<object>());
			}
			Action onRotationClear = this.OnRotationClear;
			if (onRotationClear == null)
			{
				return;
			}
			onRotationClear();
		}

		// Token: 0x0400073E RID: 1854
		private const float NO_ROTATION_VALUE = 1E-08f;

		// Token: 0x04000746 RID: 1862
		[SerializeField]
		private bool canClear;

		// Token: 0x04000747 RID: 1863
		[Header("UI References")]
		[SerializeField]
		private UIVector3Presenter vector3Presenter;

		// Token: 0x04000748 RID: 1864
		[SerializeField]
		private UIVector3View vector3FieldView;

		// Token: 0x04000749 RID: 1865
		[SerializeField]
		private GameObject vector3ControlFlexibleSpace;

		// Token: 0x0400074A RID: 1866
		[SerializeField]
		private GameObject noneVisual;

		// Token: 0x0400074B RID: 1867
		[SerializeField]
		private GameObject vector3ClearButtonSpace;

		// Token: 0x0400074C RID: 1868
		[SerializeField]
		private UIButton vector3ClearButton;

		// Token: 0x0400074D RID: 1869
		[Header("Highlighting")]
		[SerializeField]
		private UIButton highlightButton;

		// Token: 0x0400074E RID: 1870
		[Header("Eye Drop")]
		[SerializeField]
		private bool canEyeDrop;

		// Token: 0x0400074F RID: 1871
		[SerializeField]
		private UIButton eyeDropButton;

		// Token: 0x04000750 RID: 1872
		[SerializeField]
		private Image eyeDropImage;

		// Token: 0x04000751 RID: 1873
		[Header("Rotation")]
		[SerializeField]
		private GameObject rotationContainer;

		// Token: 0x04000752 RID: 1874
		[SerializeField]
		private UIFloatPresenter rotationFloatControl;

		// Token: 0x04000753 RID: 1875
		[SerializeField]
		private UIButton rotationClearButton;

		// Token: 0x0200020D RID: 525
		public enum Styles
		{
			// Token: 0x04000755 RID: 1877
			Default,
			// Token: 0x04000756 RID: 1878
			ReadOnly
		}
	}
}
