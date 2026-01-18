using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x02000462 RID: 1122
	public class Sensor
	{
		// Token: 0x06001C0C RID: 7180 RVA: 0x0007D322 File Offset: 0x0007B522
		internal Sensor(SensorComponent sensorComponent)
		{
			this.component = sensorComponent;
		}

		// Token: 0x06001C0D RID: 7181 RVA: 0x0007D331 File Offset: 0x0007B531
		public void SetSenseTeam(Context instigator, int sense)
		{
			this.component.runtimeSensorSettings.TeamSense = (TeamSense)sense;
		}

		// Token: 0x06001C0E RID: 7182 RVA: 0x0007D344 File Offset: 0x0007B544
		public void SetSenseShape(Context instigator, int shape)
		{
			this.component.runtimeSensorSettings.Shape = (SenseShape)shape;
		}

		// Token: 0x06001C0F RID: 7183 RVA: 0x0007D357 File Offset: 0x0007B557
		public void SetVerticalSenseExtents(Context instigator, float height)
		{
			this.component.runtimeSensorSettings.ExtentsVertical = height;
		}

		// Token: 0x06001C10 RID: 7184 RVA: 0x0007D36A File Offset: 0x0007B56A
		public void SetHorizontalSenseExtents(Context instigator, float width)
		{
			this.component.runtimeSensorSettings.ExtentsHorizontal = width;
		}

		// Token: 0x06001C11 RID: 7185 RVA: 0x0007D37D File Offset: 0x0007B57D
		public void SetVerticalSenseAngle(Context instigator, float angle)
		{
			this.component.runtimeSensorSettings.VerticalAngle = angle;
		}

		// Token: 0x06001C12 RID: 7186 RVA: 0x0007D390 File Offset: 0x0007B590
		public void SetHorizontalSenseAngle(Context instigator, float angle)
		{
			this.component.runtimeSensorSettings.HorizontalAngle = angle;
		}

		// Token: 0x06001C13 RID: 7187 RVA: 0x0007D3A3 File Offset: 0x0007B5A3
		public void SetSenseDistance(Context instigator, float distance)
		{
			this.component.runtimeSensorSettings.Distance = distance;
		}

		// Token: 0x06001C14 RID: 7188 RVA: 0x0007D3B6 File Offset: 0x0007B5B6
		public void SetSenseXRay(Context instigator, bool xray)
		{
			this.component.runtimeSensorSettings.XRay = xray;
		}

		// Token: 0x040015C4 RID: 5572
		private readonly SensorComponent component;
	}
}
