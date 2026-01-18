using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x0200021B RID: 539
	public abstract class UIInspectorReferencePresenter<TModel> : UIBasePresenter<TModel> where TModel : InspectorReference
	{
		// Token: 0x17000109 RID: 265
		// (get) Token: 0x06000897 RID: 2199
		protected abstract IEnumerable<object> SelectionOptions { get; }

		// Token: 0x1700010A RID: 266
		// (get) Token: 0x06000898 RID: 2200
		protected abstract string IEnumerableWindowTitle { get; }

		// Token: 0x1700010B RID: 267
		// (get) Token: 0x06000899 RID: 2201 RVA: 0x00029C16 File Offset: 0x00027E16
		protected virtual SelectionType SelectionType { get; }

		// Token: 0x1700010C RID: 268
		// (get) Token: 0x0600089A RID: 2202 RVA: 0x00029C1E File Offset: 0x00027E1E
		protected virtual Dictionary<Type, Enum> TypeStyleOverrideDictionary { get; } = new Dictionary<Type, Enum>();

		// Token: 0x0600089B RID: 2203 RVA: 0x00029C28 File Offset: 0x00027E28
		protected override void Start()
		{
			base.Start();
			IInspectorReferenceViewable inspectorReferenceViewable = base.View.Interface as IInspectorReferenceViewable;
			if (inspectorReferenceViewable != null)
			{
				inspectorReferenceViewable.OnClear += this.ClearReference;
				inspectorReferenceViewable.OnOpenIEnumerableWindow += this.OpenSelectionWindow;
			}
		}

		// Token: 0x0600089C RID: 2204 RVA: 0x00029C73 File Offset: 0x00027E73
		private void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnEnable", this);
			}
			UIBaseWindowView.CloseAction = (Action<UIBaseWindowView>)Delegate.Combine(UIBaseWindowView.CloseAction, new Action<UIBaseWindowView>(this.OnWindowClosed));
		}

		// Token: 0x0600089D RID: 2205 RVA: 0x00029CA8 File Offset: 0x00027EA8
		private void OnDisable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnDisable", this);
			}
			UIBaseWindowView.CloseAction = (Action<UIBaseWindowView>)Delegate.Remove(UIBaseWindowView.CloseAction, new Action<UIBaseWindowView>(this.OnWindowClosed));
			if (this.selectionWindowView)
			{
				this.selectionWindowView.Close();
			}
		}

		// Token: 0x0600089E RID: 2206 RVA: 0x00029D00 File Offset: 0x00027F00
		public override void SetModel(TModel model, bool triggerOnModelChanged)
		{
			if (model == null)
			{
				model = this.CreateDefaultModel();
			}
			base.SetModel(model, triggerOnModelChanged);
			this.originalSelection.Clear();
			this.UpdateOriginalSelection(model);
		}

		// Token: 0x0600089F RID: 2207
		protected abstract TModel CreateDefaultModel();

		// Token: 0x060008A0 RID: 2208 RVA: 0x00029D2C File Offset: 0x00027F2C
		protected virtual void UpdateOriginalSelection(TModel model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "UpdateOriginalSelection", "model", model), this);
			}
			if (model.IsReferenceEmpty())
			{
				return;
			}
			this.originalSelection.Add(model);
		}

		// Token: 0x060008A1 RID: 2209 RVA: 0x00029D80 File Offset: 0x00027F80
		protected virtual void SetSelection(List<object> selection)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("SetSelection ( selection: " + selection.DebugSafeCount<object>() + " )", this);
			}
			if (selection.Count == 0)
			{
				TModel tmodel = this.CreateDefaultModel();
				this.SetModel(tmodel, true);
				return;
			}
			object obj = selection.FirstOrDefault<object>();
			if (obj == null)
			{
				DebugUtility.LogException(new NullReferenceException("firstOrDefaultItem is null!"), this);
				return;
			}
			TModel tmodel2 = obj as TModel;
			if (tmodel2 == null)
			{
				DebugUtility.LogException(new InvalidCastException(string.Concat(new string[]
				{
					"Could not cast ",
					obj.GetType().Name,
					" to type ",
					typeof(TModel).Name,
					"!"
				})), this);
				return;
			}
			if (base.VerboseLogging)
			{
				DebugUtility.Log("firstOrDefaultTypedItem: " + tmodel2.GetType().Name, this);
			}
			this.SetModel(tmodel2, true);
		}

		// Token: 0x060008A2 RID: 2210 RVA: 0x00029E76 File Offset: 0x00028076
		private void ClearReference()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("ClearReference", this);
			}
			this.SetModel(this.CreateDefaultModel(), true);
		}

		// Token: 0x060008A3 RID: 2211 RVA: 0x00029E98 File Offset: 0x00028098
		private void OpenSelectionWindow()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OpenSelectionWindow", this);
			}
			if (this.selectionWindowView)
			{
				this.selectionWindowView.Close();
			}
			UIIEnumerableWindowModel uiienumerableWindowModel = new UIIEnumerableWindowModel(51, this.IEnumerableWindowTitle, UIBaseIEnumerableView.ArrangementStyle.GridVertical, new SelectionType?(this.SelectionType), this.SelectionOptions, this.TypeStyleOverrideDictionary, this.originalSelection, null, new Action<List<object>>(this.SetSelection));
			this.selectionWindowView = UIIEnumerableWindowView.Display(uiienumerableWindowModel, null);
			this.selectionWindowView.CloseUnityEvent.AddListener(new UnityAction(this.ClearSelectionWindowReferences));
		}

		// Token: 0x060008A4 RID: 2212 RVA: 0x00029F33 File Offset: 0x00028133
		private void ClearSelectionWindowReferences()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("ClearSelectionWindowReferences", this);
			}
			this.selectionWindowView.CloseUnityEvent.RemoveListener(new UnityAction(this.ClearSelectionWindowReferences));
			this.selectionWindowView = null;
		}

		// Token: 0x060008A5 RID: 2213 RVA: 0x00029F6B File Offset: 0x0002816B
		private void OnWindowClosed(UIBaseWindowView window)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "OnWindowClosed", "window", window), this);
			}
			if (window == this.selectionWindowView)
			{
				this.ClearSelectionWindowReferences();
			}
		}

		// Token: 0x0400076F RID: 1903
		private const int SELECTION_WINDOW_CANVAS_OVERRIDE_SORTING = 51;

		// Token: 0x04000770 RID: 1904
		private UIBaseWindowView selectionWindowView;

		// Token: 0x04000771 RID: 1905
		protected readonly List<object> originalSelection = new List<object>();
	}
}
