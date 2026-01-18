using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public abstract class UIInspectorReferencePresenter<TModel> : UIBasePresenter<TModel> where TModel : InspectorReference
{
	private const int SELECTION_WINDOW_CANVAS_OVERRIDE_SORTING = 51;

	private UIBaseWindowView selectionWindowView;

	protected readonly List<object> originalSelection = new List<object>();

	protected abstract IEnumerable<object> SelectionOptions { get; }

	protected abstract string IEnumerableWindowTitle { get; }

	protected virtual SelectionType SelectionType { get; }

	protected virtual Dictionary<Type, Enum> TypeStyleOverrideDictionary { get; } = new Dictionary<Type, Enum>();

	protected override void Start()
	{
		base.Start();
		if (base.View.Interface is IInspectorReferenceViewable inspectorReferenceViewable)
		{
			inspectorReferenceViewable.OnClear += ClearReference;
			inspectorReferenceViewable.OnOpenIEnumerableWindow += OpenSelectionWindow;
		}
	}

	private void OnEnable()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("OnEnable", this);
		}
		UIBaseWindowView.CloseAction = (Action<UIBaseWindowView>)Delegate.Combine(UIBaseWindowView.CloseAction, new Action<UIBaseWindowView>(OnWindowClosed));
	}

	private void OnDisable()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("OnDisable", this);
		}
		UIBaseWindowView.CloseAction = (Action<UIBaseWindowView>)Delegate.Remove(UIBaseWindowView.CloseAction, new Action<UIBaseWindowView>(OnWindowClosed));
		if ((bool)selectionWindowView)
		{
			selectionWindowView.Close();
		}
	}

	public override void SetModel(TModel model, bool triggerOnModelChanged)
	{
		if (model == null)
		{
			model = CreateDefaultModel();
		}
		base.SetModel(model, triggerOnModelChanged);
		originalSelection.Clear();
		UpdateOriginalSelection(model);
	}

	protected abstract TModel CreateDefaultModel();

	protected virtual void UpdateOriginalSelection(TModel model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "UpdateOriginalSelection", "model", model), this);
		}
		if (!model.IsReferenceEmpty())
		{
			originalSelection.Add(model);
		}
	}

	protected virtual void SetSelection(List<object> selection)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("SetSelection ( selection: " + selection.DebugSafeCount() + " )", this);
		}
		if (selection.Count == 0)
		{
			TModel model = CreateDefaultModel();
			SetModel(model, triggerOnModelChanged: true);
			return;
		}
		object obj = selection.FirstOrDefault();
		if (obj == null)
		{
			DebugUtility.LogException(new NullReferenceException("firstOrDefaultItem is null!"), this);
		}
		else if (!(obj is TModel val))
		{
			DebugUtility.LogException(new InvalidCastException("Could not cast " + obj.GetType().Name + " to type " + typeof(TModel).Name + "!"), this);
		}
		else
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("firstOrDefaultTypedItem: " + val.GetType().Name, this);
			}
			SetModel(val, triggerOnModelChanged: true);
		}
	}

	private void ClearReference()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("ClearReference", this);
		}
		SetModel(CreateDefaultModel(), triggerOnModelChanged: true);
	}

	private void OpenSelectionWindow()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("OpenSelectionWindow", this);
		}
		if ((bool)selectionWindowView)
		{
			selectionWindowView.Close();
		}
		UIIEnumerableWindowModel model = new UIIEnumerableWindowModel(51, IEnumerableWindowTitle, UIBaseIEnumerableView.ArrangementStyle.GridVertical, SelectionType, SelectionOptions, TypeStyleOverrideDictionary, originalSelection, null, SetSelection);
		selectionWindowView = UIIEnumerableWindowView.Display(model);
		selectionWindowView.CloseUnityEvent.AddListener(ClearSelectionWindowReferences);
	}

	private void ClearSelectionWindowReferences()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("ClearSelectionWindowReferences", this);
		}
		selectionWindowView.CloseUnityEvent.RemoveListener(ClearSelectionWindowReferences);
		selectionWindowView = null;
	}

	private void OnWindowClosed(UIBaseWindowView window)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "OnWindowClosed", "window", window), this);
		}
		if (window == selectionWindowView)
		{
			ClearSelectionWindowReferences();
		}
	}
}
