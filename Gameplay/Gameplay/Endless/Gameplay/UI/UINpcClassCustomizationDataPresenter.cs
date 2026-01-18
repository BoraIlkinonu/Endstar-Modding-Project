using System;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003E0 RID: 992
	public abstract class UINpcClassCustomizationDataPresenter<TModel> : UIBasePresenter<TModel>, IUINpcClassCustomizationDataPresentable, IUIPresentable, IPoolableT, IClearable where TModel : NpcClassCustomizationData
	{
		// Token: 0x1700051B RID: 1307
		// (get) Token: 0x060018FB RID: 6395
		public abstract NpcClass NpcClass { get; }

		// Token: 0x060018FC RID: 6396 RVA: 0x00073D67 File Offset: 0x00071F67
		protected override void Start()
		{
			base.Start();
			(base.View.Interface as IUINpcClassCustomizationDataViewable).OnNpcClassChanged += this.SetNpcClass;
		}

		// Token: 0x14000033 RID: 51
		// (add) Token: 0x060018FD RID: 6397 RVA: 0x00073D90 File Offset: 0x00071F90
		// (remove) Token: 0x060018FE RID: 6398 RVA: 0x00073DC8 File Offset: 0x00071FC8
		public event Action<NpcClass> OnNpcClassChanged;

		// Token: 0x060018FF RID: 6399 RVA: 0x00073E00 File Offset: 0x00072000
		private void SetNpcClass(NpcClass npcClass)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} ) | {3}: {4}", new object[] { "SetNpcClass", "npcClass", npcClass, "NpcClass", this.NpcClass }), this);
			}
			if (this.NpcClass == npcClass)
			{
				return;
			}
			Action<NpcClass> onNpcClassChanged = this.OnNpcClassChanged;
			if (onNpcClassChanged == null)
			{
				return;
			}
			onNpcClassChanged(npcClass);
		}
	}
}
