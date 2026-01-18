using System;
using Endless.Shared.DataTypes;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003F1 RID: 1009
	public abstract class UIBaseSocialPresenter<T> : UIBasePresenter<T>
	{
		// Token: 0x17000526 RID: 1318
		// (get) Token: 0x06001944 RID: 6468
		protected abstract User UserModel { get; }

		// Token: 0x06001945 RID: 6469 RVA: 0x00074B16 File Offset: 0x00072D16
		public override void SetModel(T model, bool triggerOnModelChanged)
		{
			base.SetModel(model, triggerOnModelChanged);
			this.userPresenter.SetModel(this.UserModel, triggerOnModelChanged);
		}

		// Token: 0x04001431 RID: 5169
		[Header("UIBaseSocialPresenter")]
		[SerializeField]
		private UIUserPresenter userPresenter;
	}
}
