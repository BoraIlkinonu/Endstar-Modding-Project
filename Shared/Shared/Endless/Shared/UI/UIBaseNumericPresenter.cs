using System;
using Endless.Shared.Debugging;
using Runtime.Shared;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x0200020D RID: 525
	public abstract class UIBaseNumericPresenter<TModel> : UIBasePresenter<TModel>, IFieldCountable, IUIInteractable
	{
		// Token: 0x1700027C RID: 636
		// (get) Token: 0x06000D9C RID: 3484
		public abstract int FieldCount { get; }

		// Token: 0x06000D9D RID: 3485 RVA: 0x0003BCD4 File Offset: 0x00039ED4
		public override void OnDespawn()
		{
			base.OnDespawn();
			this.Unclamp();
		}

		// Token: 0x06000D9E RID: 3486 RVA: 0x0003BCE4 File Offset: 0x00039EE4
		public override void SetModel(TModel model, bool triggerOnModelChanged)
		{
			TModel tmodel = model;
			model = this.Clamp(model);
			base.SetModel(model, triggerOnModelChanged);
			if (!tmodel.Equals(model))
			{
				base.View.Interface.View(model);
			}
		}

		// Token: 0x06000D9F RID: 3487
		public abstract void Unclamp();

		// Token: 0x06000DA0 RID: 3488 RVA: 0x0003BD30 File Offset: 0x00039F30
		public void SetInteractable(bool interactable)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetInteractable", "interactable", interactable), this);
			}
			InterfaceReference<IUIInteractable>[] array = this.interactables;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Interface.SetInteractable(interactable);
			}
		}

		// Token: 0x06000DA1 RID: 3489
		protected abstract TModel Clamp(TModel value);

		// Token: 0x040008C8 RID: 2248
		[Header("UIBaseNumericPresenter")]
		[SerializeField]
		private InterfaceReference<IUIInteractable>[] interactables = Array.Empty<InterfaceReference<IUIInteractable>>();
	}
}
