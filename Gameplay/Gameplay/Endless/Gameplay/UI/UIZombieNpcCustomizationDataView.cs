using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003ED RID: 1005
	public class UIZombieNpcCustomizationDataView : UINpcClassCustomizationDataView<ZombieNpcCustomizationData>
	{
		// Token: 0x06001927 RID: 6439 RVA: 0x000745C3 File Offset: 0x000727C3
		protected override void Start()
		{
			base.Start();
			this.zombifyTargetToggle.OnChange.AddListener(new UnityAction<bool>(this.OnZombifyTargetTogglePressed));
		}

		// Token: 0x14000035 RID: 53
		// (add) Token: 0x06001928 RID: 6440 RVA: 0x000745E8 File Offset: 0x000727E8
		// (remove) Token: 0x06001929 RID: 6441 RVA: 0x00074620 File Offset: 0x00072820
		public event Action<bool> OnZombifyTargetChanged;

		// Token: 0x0600192A RID: 6442 RVA: 0x00074655 File Offset: 0x00072855
		public override void View(ZombieNpcCustomizationData model)
		{
			base.View(model);
			this.zombifyTargetToggle.SetIsOn(model.ZombifyTarget, false, true);
		}

		// Token: 0x0600192B RID: 6443 RVA: 0x00074671 File Offset: 0x00072871
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			this.zombifyTargetToggle.SetIsOn(false, true, true);
		}

		// Token: 0x0600192C RID: 6444 RVA: 0x00074699 File Offset: 0x00072899
		private void OnZombifyTargetTogglePressed(bool zombifyTarget)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnZombifyTargetTogglePressed", new object[] { zombifyTarget });
			}
			Action<bool> onZombifyTargetChanged = this.OnZombifyTargetChanged;
			if (onZombifyTargetChanged == null)
			{
				return;
			}
			onZombifyTargetChanged(zombifyTarget);
		}

		// Token: 0x04001426 RID: 5158
		[Header("UIZombieNpcCustomizationDataView")]
		[SerializeField]
		private UIToggle zombifyTargetToggle;
	}
}
