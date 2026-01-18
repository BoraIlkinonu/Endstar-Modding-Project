using System;
using Endless.Gameplay.LuaEnums;
using Endless.Shared.Debugging;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003EC RID: 1004
	public class UIZombieNpcCustomizationDataPresenter : UINpcClassCustomizationDataPresenter<ZombieNpcCustomizationData>
	{
		// Token: 0x17000521 RID: 1313
		// (get) Token: 0x06001923 RID: 6435 RVA: 0x0003FE71 File Offset: 0x0003E071
		public override NpcClass NpcClass
		{
			get
			{
				return NpcClass.Zombie;
			}
		}

		// Token: 0x06001924 RID: 6436 RVA: 0x0007455A File Offset: 0x0007275A
		protected override void Start()
		{
			base.Start();
			(base.Viewable as UIZombieNpcCustomizationDataView).OnZombifyTargetChanged += this.SetZombifyTarget;
		}

		// Token: 0x06001925 RID: 6437 RVA: 0x0007457E File Offset: 0x0007277E
		private void SetZombifyTarget(bool zombifyTarget)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetZombifyTarget", new object[] { zombifyTarget });
			}
			base.Model.ZombifyTarget = zombifyTarget;
			this.SetModel(base.Model, true);
		}
	}
}
