using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x02000465 RID: 1125
	public class Targeter
	{
		// Token: 0x06001C2E RID: 7214 RVA: 0x0007D614 File Offset: 0x0007B814
		internal Targeter(TargeterComponent targeterComponent)
		{
			this.targeter = targeterComponent;
		}

		// Token: 0x06001C2F RID: 7215 RVA: 0x0007D623 File Offset: 0x0007B823
		public void SetMaxLookDistance(Context instigator, float distance)
		{
			this.targeter.MaxLookDistance = distance;
		}

		// Token: 0x06001C30 RID: 7216 RVA: 0x0007D631 File Offset: 0x0007B831
		public void SetVerticalViewAngle(Context instigator, float angle)
		{
			this.targeter.VerticalViewAngle = angle;
		}

		// Token: 0x06001C31 RID: 7217 RVA: 0x0007D63F File Offset: 0x0007B83F
		public void SetHorizontalViewWidth(Context instigator, float angle)
		{
			this.targeter.HorizontalViewAngle = angle;
		}

		// Token: 0x06001C32 RID: 7218 RVA: 0x0007D64D File Offset: 0x0007B84D
		public void SetTargetSelectionMode(Context instigator, int targetSelectionMode)
		{
			this.targeter.TargetSelectionMode = (TargetSelectionMode)targetSelectionMode;
		}

		// Token: 0x06001C33 RID: 7219 RVA: 0x0007D65B File Offset: 0x0007B85B
		public void SetTargetPrioritizationMode(Context instigator, int targetPrioritizationMode)
		{
			this.targeter.TargetPrioritizationMode = (TargetPrioritizationMode)targetPrioritizationMode;
		}

		// Token: 0x06001C34 RID: 7220 RVA: 0x0007D669 File Offset: 0x0007B869
		public void SetCurrentTargetHandlingMode(Context instigator, int currentTargetHandlingMode)
		{
			this.targeter.CurrentTargetHandlingMode = (CurrentTargetHandlingMode)currentTargetHandlingMode;
		}

		// Token: 0x06001C35 RID: 7221 RVA: 0x0007D677 File Offset: 0x0007B877
		public void SetTargetHostilityMode(Context instigator, int targetHostilityMode)
		{
			this.targeter.TargetHostilityMode = (TargetHostilityMode)targetHostilityMode;
		}

		// Token: 0x06001C36 RID: 7222 RVA: 0x0007D685 File Offset: 0x0007B885
		public void SetZeroHealthTargetMode(Context instigator, int zeroHealthTargetMode)
		{
			this.targeter.ZeroHealthTargetMode = (ZeroHealthTargetMode)zeroHealthTargetMode;
		}

		// Token: 0x06001C37 RID: 7223 RVA: 0x0007D693 File Offset: 0x0007B893
		public void UseXRayLos(Context instigator, bool useXray)
		{
			this.targeter.useXRayLos = useXray;
		}

		// Token: 0x06001C38 RID: 7224 RVA: 0x0007D6A1 File Offset: 0x0007B8A1
		public void IsNavigationDependent(bool isNavigationDependent)
		{
			this.targeter.isNavigationDependent = isNavigationDependent;
		}

		// Token: 0x06001C39 RID: 7225 RVA: 0x0007D6AF File Offset: 0x0007B8AF
		public void SetAwarenessLossRate(Context instigator, float newLossRate)
		{
			this.targeter.awarenessLossRate = newLossRate;
		}

		// Token: 0x06001C3A RID: 7226 RVA: 0x0007D6BD File Offset: 0x0007B8BD
		public float GetAwarenessLossRate()
		{
			return this.targeter.awarenessLossRate;
		}

		// Token: 0x040015C7 RID: 5575
		private readonly TargeterComponent targeter;
	}
}
