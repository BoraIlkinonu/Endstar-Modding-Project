using System;
using Endless.Shared.Debugging;

namespace Endless.Shared.UI
{
	// Token: 0x02000186 RID: 390
	public abstract class UIBaseLocalFilterableListController<T> : UIBaseFilterableListController<T>
	{
		// Token: 0x060009AF RID: 2479 RVA: 0x0002962C File Offset: 0x0002782C
		protected override void Start()
		{
			base.Start();
			base.Model.TryGetComponent<UIBaseLocalFilterableListModel<T>>(out this.LocalFilterableListModel);
		}

		// Token: 0x060009B0 RID: 2480 RVA: 0x00029648 File Offset: 0x00027848
		public override void Validate()
		{
			base.Validate();
			if (!base.Model)
			{
				return;
			}
			UIBaseLocalFilterableListModel<T> uibaseLocalFilterableListModel;
			if (!base.Model.TryGetComponent<UIBaseLocalFilterableListModel<T>>(out uibaseLocalFilterableListModel))
			{
				DebugUtility.LogError("Model must be a " + this.LocalFilterableListModel.GetType().Name + "!", this);
			}
		}

		// Token: 0x060009B1 RID: 2481
		protected abstract bool IncludeInFilteredResults(T item);

		// Token: 0x060009B2 RID: 2482 RVA: 0x000296A0 File Offset: 0x000278A0
		protected override void SetStringFilter(string toFilterBy)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("SetStringFilter ( toFilterBy: " + toFilterBy + " )", this);
			}
			if (ExitManager.IsQuitting)
			{
				return;
			}
			this.LocalFilterableListModel.Filter(new Func<T, bool>(this.IncludeInFilteredResults), true);
		}

		// Token: 0x0400061A RID: 1562
		protected UIBaseLocalFilterableListModel<T> LocalFilterableListModel;
	}
}
