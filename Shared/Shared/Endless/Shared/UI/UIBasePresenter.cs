using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using Runtime.Shared;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000239 RID: 569
	public abstract class UIBasePresenter<TModel> : UIGameObject, IUIPresentable, IPoolableT, IClearable
	{
		// Token: 0x1400004D RID: 77
		// (add) Token: 0x06000E66 RID: 3686 RVA: 0x0003ED98 File Offset: 0x0003CF98
		// (remove) Token: 0x06000E67 RID: 3687 RVA: 0x0003EDD0 File Offset: 0x0003CFD0
		public event Action<object> OnModelChanged;

		// Token: 0x170002AA RID: 682
		// (get) Token: 0x06000E68 RID: 3688 RVA: 0x0003EE05 File Offset: 0x0003D005
		// (set) Token: 0x06000E69 RID: 3689 RVA: 0x0003EE0D File Offset: 0x0003D00D
		public InterfaceReference<IUITypedViewable<TModel>> View { get; private set; }

		// Token: 0x170002AB RID: 683
		// (get) Token: 0x06000E6A RID: 3690 RVA: 0x0003EE16 File Offset: 0x0003D016
		// (set) Token: 0x06000E6B RID: 3691 RVA: 0x0003EE1E File Offset: 0x0003D01E
		private protected virtual TModel DefaultModel { protected get; private set; }

		// Token: 0x170002AC RID: 684
		// (get) Token: 0x06000E6C RID: 3692 RVA: 0x0003EE27 File Offset: 0x0003D027
		// (set) Token: 0x06000E6D RID: 3693 RVA: 0x0003EE2F File Offset: 0x0003D02F
		protected bool VerboseLogging { get; set; }

		// Token: 0x170002AD RID: 685
		// (get) Token: 0x06000E6E RID: 3694 RVA: 0x0003EE38 File Offset: 0x0003D038
		// (set) Token: 0x06000E6F RID: 3695 RVA: 0x0003EE40 File Offset: 0x0003D040
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x170002AE RID: 686
		// (get) Token: 0x06000E70 RID: 3696 RVA: 0x000050D2 File Offset: 0x000032D2
		public bool IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170002AF RID: 687
		// (get) Token: 0x06000E71 RID: 3697 RVA: 0x0003EE49 File Offset: 0x0003D049
		// (set) Token: 0x06000E72 RID: 3698 RVA: 0x0003EE51 File Offset: 0x0003D051
		public TModel Model { get; private set; }

		// Token: 0x170002B0 RID: 688
		// (get) Token: 0x06000E73 RID: 3699 RVA: 0x0003EE5A File Offset: 0x0003D05A
		public object ModelAsObject
		{
			get
			{
				return this.Model;
			}
		}

		// Token: 0x170002B1 RID: 689
		// (get) Token: 0x06000E74 RID: 3700 RVA: 0x0003EE67 File Offset: 0x0003D067
		public IUIViewable Viewable
		{
			get
			{
				return this.View.Interface;
			}
		}

		// Token: 0x170002B2 RID: 690
		// (get) Token: 0x06000E75 RID: 3701 RVA: 0x0003EE74 File Offset: 0x0003D074
		public Enum Style
		{
			get
			{
				if (this.View == null)
				{
					DebugUtility.LogException(new NullReferenceException("View is null!"), this);
					return null;
				}
				if (this.View.Interface == null)
				{
					DebugUtility.LogException(new NullReferenceException("View.Interface is null!"), this);
					return null;
				}
				if (this.View.Interface.StyleEnum == null)
				{
					DebugUtility.LogException(new NullReferenceException("View.Interface.StyleEnum is null!"), this);
					return null;
				}
				return this.View.Interface.StyleEnum;
			}
		}

		// Token: 0x170002B3 RID: 691
		// (get) Token: 0x06000E76 RID: 3702 RVA: 0x0003EEEE File Offset: 0x0003D0EE
		public Type ModelType
		{
			get
			{
				return typeof(TModel);
			}
		}

		// Token: 0x170002B4 RID: 692
		// (get) Token: 0x06000E77 RID: 3703 RVA: 0x0003EEFA File Offset: 0x0003D0FA
		// (set) Token: 0x06000E78 RID: 3704 RVA: 0x0003EF02 File Offset: 0x0003D102
		private protected bool ModelSet { protected get; private set; }

		// Token: 0x170002B5 RID: 693
		// (get) Token: 0x06000E79 RID: 3705 RVA: 0x000050D2 File Offset: 0x000032D2
		protected virtual bool ViewOnModelChanged
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170002B6 RID: 694
		// (get) Token: 0x06000E7A RID: 3706 RVA: 0x0003EF0B File Offset: 0x0003D10B
		protected virtual bool GuardAgainstNoModelChange
		{
			get
			{
				return typeof(TModel).IsValueType;
			}
		}

		// Token: 0x06000E7B RID: 3707 RVA: 0x0003EF1C File Offset: 0x0003D11C
		protected virtual void Start()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			if (!this.ModelSet && this.setModelOnStartWithDefaultModel)
			{
				this.SetModel(this.DefaultModel, false);
			}
		}

		// Token: 0x06000E7C RID: 3708 RVA: 0x0003EF4E File Offset: 0x0003D14E
		public virtual void OnSpawn()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnSpawn", this);
			}
			this.View.Interface.SetDefaultLayoutElementValueToCurrentValue();
		}

		// Token: 0x06000E7D RID: 3709 RVA: 0x0003EF74 File Offset: 0x0003D174
		public virtual void OnDespawn()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnDespawn", this);
			}
			IUIPresentable presenterWithStyle = this.presenterDictionary.GetPresenterWithStyle(this.ModelType, this.Style);
			base.RectTransform.anchorMin = presenterWithStyle.RectTransform.anchorMin;
			base.RectTransform.anchorMax = presenterWithStyle.RectTransform.anchorMax;
			base.RectTransform.pivot = presenterWithStyle.RectTransform.pivot;
			this.Clear();
			this.OnModelChanged = null;
			this.View.Interface.ApplyDefaultLayoutElementValue();
			IUIInteractable iuiinteractable = this.View.Interface as IUIInteractable;
			if (iuiinteractable != null)
			{
				iuiinteractable.SetInteractable(true);
			}
		}

		// Token: 0x06000E7E RID: 3710 RVA: 0x0003F028 File Offset: 0x0003D228
		public IUIPresentable SpawnPooledInstance(Transform parent = null)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("SpawnPooledInstance ( parent: " + parent.DebugSafeName(true) + " )", this);
			}
			return MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn<UIBasePresenter<TModel>>(this, default(Vector3), default(Quaternion), parent);
		}

		// Token: 0x06000E7F RID: 3711 RVA: 0x0003F079 File Offset: 0x0003D279
		public void ReturnToPool()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("ReturnToPool", this);
			}
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIBasePresenter<TModel>>(this);
		}

		// Token: 0x06000E80 RID: 3712 RVA: 0x0003F099 File Offset: 0x0003D299
		public void PrewarmPool(int count)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "PrewarmPool", "count", count), this);
			}
			MonoBehaviourSingleton<PoolManagerT>.Instance.PrewarmPoolOverTime<UIBasePresenter<TModel>>(this, count);
		}

		// Token: 0x06000E81 RID: 3713 RVA: 0x0003F0D0 File Offset: 0x0003D2D0
		public virtual void Clear()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Clear", this);
			}
			this.Model = default(TModel);
			this.ModelSet = false;
			this.View.Interface.Clear();
		}

		// Token: 0x06000E82 RID: 3714 RVA: 0x0003F118 File Offset: 0x0003D318
		public void SetModelAsObject(object model, bool triggerOnModelChanged)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[]
				{
					"SetModelAsObject",
					"model",
					model.DebugSafeJson(),
					"triggerOnModelChanged",
					triggerOnModelChanged
				}), this);
			}
			if (model is TModel)
			{
				TModel tmodel = (TModel)((object)model);
				this.SetModel(tmodel, triggerOnModelChanged);
				return;
			}
			DebugUtility.LogError(string.Concat(new string[]
			{
				"model's type is ",
				model.GetType().Name,
				" when it needs to be of type ",
				this.ModelType.Name,
				"!"
			}), this);
		}

		// Token: 0x06000E83 RID: 3715 RVA: 0x0003F1CC File Offset: 0x0003D3CC
		public virtual void SetModel(TModel model, bool triggerOnModelChanged)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "SetModel", "model", model, "triggerOnModelChanged", triggerOnModelChanged }), this);
			}
			if (this.GuardAgainstNoModelChange && this.ModelSet && EqualityComparer<TModel>.Default.Equals(this.Model, model))
			{
				if (this.VerboseLogging)
				{
					DebugUtility.LogWarning("Skipping redundant model update as it's identical to the current model.", this);
				}
				return;
			}
			this.Model = model;
			if (this.ViewOnModelChanged)
			{
				this.View.Interface.View(this.Model);
			}
			this.ModelSet = true;
			if (triggerOnModelChanged)
			{
				this.InvokeOnModelChanged();
			}
		}

		// Token: 0x06000E84 RID: 3716 RVA: 0x0003F28D File Offset: 0x0003D48D
		protected void InvokeOnModelChanged()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("InvokeOnModelChanged", this);
			}
			Action<object> onModelChanged = this.OnModelChanged;
			if (onModelChanged == null)
			{
				return;
			}
			onModelChanged(this.Model);
		}

		// Token: 0x06000E85 RID: 3717 RVA: 0x0003F2BD File Offset: 0x0003D4BD
		protected void SetModelAsObjectAndTriggerOnModelChanged(object model)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetModelAsObjectAndTriggerOnModelChanged", "model", model), this);
			}
			this.SetModelAsObject(model, true);
		}

		// Token: 0x06000E86 RID: 3718 RVA: 0x0003F2EA File Offset: 0x0003D4EA
		protected void SetModelAndTriggerOnModelChanged(TModel model)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetModelAndTriggerOnModelChanged", "model", model), this);
			}
			this.SetModel(model, true);
		}

		// Token: 0x0400092D RID: 2349
		[SerializeField]
		protected UIPresenterDictionary presenterDictionary;

		// Token: 0x0400092E RID: 2350
		[SerializeField]
		private bool setModelOnStartWithDefaultModel;
	}
}
