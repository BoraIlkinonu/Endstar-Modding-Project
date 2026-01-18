using System;
using Endless.Gameplay;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UICellReferenceView : UIBaseView<CellReference, UICellReferenceView.Styles>, IUIInteractable
{
	public enum Styles
	{
		Default,
		ReadOnly
	}

	private const float NO_ROTATION_VALUE = 1E-08f;

	[SerializeField]
	private bool canClear;

	[Header("UI References")]
	[SerializeField]
	private UIVector3Presenter vector3Presenter;

	[SerializeField]
	private UIVector3View vector3FieldView;

	[SerializeField]
	private GameObject vector3ControlFlexibleSpace;

	[SerializeField]
	private GameObject noneVisual;

	[SerializeField]
	private GameObject vector3ClearButtonSpace;

	[SerializeField]
	private UIButton vector3ClearButton;

	[Header("Highlighting")]
	[SerializeField]
	private UIButton highlightButton;

	[Header("Eye Drop")]
	[SerializeField]
	private bool canEyeDrop;

	[SerializeField]
	private UIButton eyeDropButton;

	[SerializeField]
	private Image eyeDropImage;

	[Header("Rotation")]
	[SerializeField]
	private GameObject rotationContainer;

	[SerializeField]
	private UIFloatPresenter rotationFloatControl;

	[SerializeField]
	private UIButton rotationClearButton;

	[field: Header("UICellReferenceView")]
	[field: SerializeField]
	public override Styles Style { get; protected set; }

	public event Action OnClear;

	public event Action OnEyeDrop;

	public event Action OnHighlight;

	public event Action<Vector3> OnPositionChanged;

	public event Action<float> OnRotationChanged;

	public event Action OnRotationClear;

	private void Start()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		vector3ClearButton.onClick.AddListener(OnClearButtonPressed);
		highlightButton.onClick.AddListener(OnHighlightButtonPressed);
		eyeDropButton.onClick.AddListener(OnEyeDropButtonPressed);
		vector3Presenter.OnModelChanged += InvokeOnPositionChanged;
		rotationFloatControl.OnModelChanged += OnRotationSet;
		rotationClearButton.onClick.AddListener(OnRotationClearButtonPressed);
		vector3FieldView.SetRaycastTargetGraphics(state: false);
	}

	public override void View(CellReference model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "View", model);
		}
		bool flag = Style == Styles.ReadOnly;
		bool flag2 = model?.HasValue ?? false;
		bool flag3 = flag2 && model.RotationHasValue;
		SetVector3IntActive(flag2 && !flag);
		SetRotationActive(flag2 && !flag);
		SetNoneVisualActive(!flag2 && !flag);
		bool clearButtonActive = canClear && flag2 && !flag;
		SetClearButtonActive(clearButtonActive);
		SetRotationClearButtonActive(flag3 && !flag);
		if (flag2)
		{
			Vector3 cellPosition = model.GetCellPosition();
			SetVector3Value(cellPosition);
			if (flag3)
			{
				SetRotationValue(model.GetRotation(), hasValue: true);
			}
			else
			{
				SetRotationValue(1E-08f, hasValue: false);
			}
		}
		else
		{
			SetVector3Value(Vector3Int.zero);
			SetRotationValue(1E-08f, hasValue: false);
		}
		SetHighlightButtonActive(flag2 && !flag);
		SetEyeDropActive(canEyeDrop && !flag);
	}

	public override void Clear()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
	}

	public void SetInteractable(bool interactable)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetInteractable", interactable);
		}
		if (vector3Presenter.Viewable is IUIInteractable iUIInteractable)
		{
			iUIInteractable.SetInteractable(interactable);
		}
		vector3ClearButton.interactable = interactable;
		highlightButton.interactable = interactable;
		eyeDropButton.interactable = interactable;
		if (rotationFloatControl.View.Interface is IUIInteractable iUIInteractable2)
		{
			iUIInteractable2.SetInteractable(interactable);
		}
		rotationClearButton.interactable = interactable;
	}

	public void SetVector3IntActive(bool active)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetVector3IntActive", active);
		}
		vector3Presenter.gameObject.SetActive(active);
		vector3ControlFlexibleSpace.SetActive(active);
	}

	public void SetRotationActive(bool active)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetRotationActive", active);
		}
		rotationContainer.SetActive(active);
	}

	public void SetNoneVisualActive(bool active)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetNoneVisualActive", active);
		}
		noneVisual.SetActive(active);
	}

	public void SetClearButtonActive(bool active)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetClearButtonActive", active);
		}
		vector3ClearButtonSpace.SetActive(active);
		vector3ClearButton.gameObject.SetActive(active);
	}

	public void SetHighlightButtonActive(bool active)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetHighlightButtonActive", active);
		}
		highlightButton.gameObject.SetActive(active);
	}

	public void SetRotationClearButtonActive(bool active)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetRotationClearButtonActive", active);
		}
		rotationClearButton.gameObject.SetActive(active);
	}

	public void SetEyeDropActive(bool active)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetEyeDropActive", active);
		}
		eyeDropButton.gameObject.SetActive(active);
		eyeDropImage.gameObject.SetActive(active);
	}

	public void SetVector3Value(Vector3 value)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetVector3Value", value);
		}
		vector3Presenter.SetModel(value, triggerOnModelChanged: false);
	}

	public void SetRotationValue(float value, bool hasValue)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetRotationValue", value, hasValue);
		}
		rotationFloatControl.SetModel(hasValue ? value : 1E-08f, triggerOnModelChanged: false);
		if (!hasValue && rotationFloatControl.View.Interface is UIFloatView uIFloatView)
		{
			uIFloatView.OverrideInputField("null");
		}
	}

	private void OnClearButtonPressed()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnClearButtonPressed");
		}
		this.OnClear?.Invoke();
	}

	private void OnHighlightButtonPressed()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnHighlightButtonPressed");
		}
		this.OnHighlight?.Invoke();
	}

	private void OnEyeDropButtonPressed()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEyeDropButtonPressed");
		}
		this.OnEyeDrop?.Invoke();
	}

	private void InvokeOnPositionChanged(object position)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "InvokeOnPositionChanged", position);
		}
		Vector3 obj = (Vector3)position;
		this.OnPositionChanged?.Invoke(obj);
	}

	private void OnRotationSet(object rotationAsFloat)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnRotationSet", rotationAsFloat);
		}
		this.OnRotationChanged?.Invoke((float)rotationAsFloat);
	}

	private void OnRotationClearButtonPressed()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnRotationClearButtonPressed");
		}
		this.OnRotationClear?.Invoke();
	}
}
