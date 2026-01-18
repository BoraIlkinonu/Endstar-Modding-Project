using System;

namespace Endless.Gameplay.Fsm
{
	// Token: 0x02000431 RID: 1073
	public class FsmParameters
	{
		// Token: 0x06001ABD RID: 6845 RVA: 0x0007A818 File Offset: 0x00078A18
		public FsmParameters(IndividualStateUpdater stateUpdater)
		{
			stateUpdater.OnCleanupTriggers += this.HandleCleanupTriggers;
		}

		// Token: 0x17000559 RID: 1369
		// (get) Token: 0x06001ABE RID: 6846 RVA: 0x0007A832 File Offset: 0x00078A32
		// (set) Token: 0x06001ABF RID: 6847 RVA: 0x0007A83A File Offset: 0x00078A3A
		public bool InteractionStartedTrigger { get; set; }

		// Token: 0x1700055A RID: 1370
		// (get) Token: 0x06001AC0 RID: 6848 RVA: 0x0007A843 File Offset: 0x00078A43
		// (set) Token: 0x06001AC1 RID: 6849 RVA: 0x0007A84B File Offset: 0x00078A4B
		public bool PhysicsTrigger { get; set; }

		// Token: 0x1700055B RID: 1371
		// (get) Token: 0x06001AC2 RID: 6850 RVA: 0x0007A854 File Offset: 0x00078A54
		// (set) Token: 0x06001AC3 RID: 6851 RVA: 0x0007A85C File Offset: 0x00078A5C
		public bool FlinchTrigger { get; set; }

		// Token: 0x1700055C RID: 1372
		// (get) Token: 0x06001AC4 RID: 6852 RVA: 0x0007A865 File Offset: 0x00078A65
		// (set) Token: 0x06001AC5 RID: 6853 RVA: 0x0007A86D File Offset: 0x00078A6D
		public bool FlinchFinishedTrigger { get; set; }

		// Token: 0x1700055D RID: 1373
		// (get) Token: 0x06001AC6 RID: 6854 RVA: 0x0007A876 File Offset: 0x00078A76
		// (set) Token: 0x06001AC7 RID: 6855 RVA: 0x0007A87E File Offset: 0x00078A7E
		public bool InteractionFinishedTrigger { get; set; }

		// Token: 0x1700055E RID: 1374
		// (get) Token: 0x06001AC8 RID: 6856 RVA: 0x0007A887 File Offset: 0x00078A87
		// (set) Token: 0x06001AC9 RID: 6857 RVA: 0x0007A88F File Offset: 0x00078A8F
		public bool WillRoll { get; set; }

		// Token: 0x1700055F RID: 1375
		// (get) Token: 0x06001ACA RID: 6858 RVA: 0x0007A898 File Offset: 0x00078A98
		// (set) Token: 0x06001ACB RID: 6859 RVA: 0x0007A8A0 File Offset: 0x00078AA0
		public bool WillSplat { get; set; }

		// Token: 0x17000560 RID: 1376
		// (get) Token: 0x06001ACC RID: 6860 RVA: 0x0007A8A9 File Offset: 0x00078AA9
		// (set) Token: 0x06001ACD RID: 6861 RVA: 0x0007A8B1 File Offset: 0x00078AB1
		public bool WarpCompleteTrigger { get; set; }

		// Token: 0x17000561 RID: 1377
		// (get) Token: 0x06001ACE RID: 6862 RVA: 0x0007A8BA File Offset: 0x00078ABA
		// (set) Token: 0x06001ACF RID: 6863 RVA: 0x0007A8C2 File Offset: 0x00078AC2
		public bool JumpCompleteTrigger { get; set; }

		// Token: 0x17000562 RID: 1378
		// (get) Token: 0x06001AD0 RID: 6864 RVA: 0x0007A8CB File Offset: 0x00078ACB
		// (set) Token: 0x06001AD1 RID: 6865 RVA: 0x0007A8D3 File Offset: 0x00078AD3
		public bool ReviveTrigger { get; set; }

		// Token: 0x17000563 RID: 1379
		// (get) Token: 0x06001AD2 RID: 6866 RVA: 0x0007A8DC File Offset: 0x00078ADC
		// (set) Token: 0x06001AD3 RID: 6867 RVA: 0x0007A8E4 File Offset: 0x00078AE4
		public bool WarpTrigger { get; set; }

		// Token: 0x17000564 RID: 1380
		// (get) Token: 0x06001AD4 RID: 6868 RVA: 0x0007A8ED File Offset: 0x00078AED
		// (set) Token: 0x06001AD5 RID: 6869 RVA: 0x0007A8F5 File Offset: 0x00078AF5
		public bool JumpTrigger { get; set; }

		// Token: 0x17000565 RID: 1381
		// (get) Token: 0x06001AD6 RID: 6870 RVA: 0x0007A8FE File Offset: 0x00078AFE
		// (set) Token: 0x06001AD7 RID: 6871 RVA: 0x0007A906 File Offset: 0x00078B06
		public bool LandingCompleteTrigger { get; set; }

		// Token: 0x17000566 RID: 1382
		// (get) Token: 0x06001AD8 RID: 6872 RVA: 0x0007A90F File Offset: 0x00078B0F
		// (set) Token: 0x06001AD9 RID: 6873 RVA: 0x0007A917 File Offset: 0x00078B17
		public bool StandUpCompleteTrigger { get; set; }

		// Token: 0x17000567 RID: 1383
		// (get) Token: 0x06001ADA RID: 6874 RVA: 0x0007A920 File Offset: 0x00078B20
		// (set) Token: 0x06001ADB RID: 6875 RVA: 0x0007A928 File Offset: 0x00078B28
		public bool LocalPhysicsTrigger { get; set; }

		// Token: 0x17000568 RID: 1384
		// (get) Token: 0x06001ADC RID: 6876 RVA: 0x0007A931 File Offset: 0x00078B31
		// (set) Token: 0x06001ADD RID: 6877 RVA: 0x0007A939 File Offset: 0x00078B39
		public bool NetworkSimulationTrigger { get; set; }

		// Token: 0x17000569 RID: 1385
		// (get) Token: 0x06001ADE RID: 6878 RVA: 0x0007A942 File Offset: 0x00078B42
		// (set) Token: 0x06001ADF RID: 6879 RVA: 0x0007A94A File Offset: 0x00078B4A
		public bool TeleportCompleteTrigger { get; set; }

		// Token: 0x1700056A RID: 1386
		// (get) Token: 0x06001AE0 RID: 6880 RVA: 0x0007A953 File Offset: 0x00078B53
		// (set) Token: 0x06001AE1 RID: 6881 RVA: 0x0007A95B File Offset: 0x00078B5B
		public bool TeleportTrigger { get; set; }

		// Token: 0x1700056B RID: 1387
		// (get) Token: 0x06001AE2 RID: 6882 RVA: 0x0007A964 File Offset: 0x00078B64
		// (set) Token: 0x06001AE3 RID: 6883 RVA: 0x0007A96C File Offset: 0x00078B6C
		public bool IsSpawnAnimationComplete { get; set; }

		// Token: 0x06001AE4 RID: 6884 RVA: 0x0007A978 File Offset: 0x00078B78
		private void HandleCleanupTriggers()
		{
			this.InteractionStartedTrigger = false;
			this.PhysicsTrigger = false;
			this.FlinchTrigger = false;
			this.FlinchFinishedTrigger = false;
			this.InteractionFinishedTrigger = false;
			this.WarpCompleteTrigger = false;
			this.JumpCompleteTrigger = false;
			this.ReviveTrigger = false;
			this.WarpTrigger = false;
			this.JumpTrigger = false;
			this.LandingCompleteTrigger = false;
			this.StandUpCompleteTrigger = false;
			this.LocalPhysicsTrigger = false;
			this.NetworkSimulationTrigger = false;
			this.TeleportCompleteTrigger = false;
			this.TeleportTrigger = false;
		}
	}
}
