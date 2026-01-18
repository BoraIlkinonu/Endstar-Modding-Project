using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002C0 RID: 704
	public class SimpleAmbientEntry : AmbientEntry
	{
		// Token: 0x06001012 RID: 4114 RVA: 0x0005201E File Offset: 0x0005021E
		public override void Activate()
		{
			base.Activate();
			RenderSettings.sun = this.sunSource;
			RenderSettings.ambientLight = this.ambientLight;
			RenderSettings.skybox = this.skyboxMaterial;
			base.gameObject.SetActive(true);
		}

		// Token: 0x06001013 RID: 4115 RVA: 0x00052053 File Offset: 0x00050253
		public override void Deactivate()
		{
			base.Deactivate();
			base.gameObject.SetActive(false);
		}

		// Token: 0x04000DCE RID: 3534
		[SerializeField]
		private Material skyboxMaterial;

		// Token: 0x04000DCF RID: 3535
		[SerializeField]
		private Color ambientLight;

		// Token: 0x04000DD0 RID: 3536
		[SerializeField]
		private Light sunSource;
	}
}
