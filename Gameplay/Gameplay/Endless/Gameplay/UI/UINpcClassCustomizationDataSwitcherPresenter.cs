using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003E7 RID: 999
	public class UINpcClassCustomizationDataSwitcherPresenter : UIBasePresenter<NpcClassCustomizationData>
	{
		// Token: 0x06001912 RID: 6418 RVA: 0x000740A8 File Offset: 0x000722A8
		private void Awake()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Awake", Array.Empty<object>());
			}
			if (!this.initialized)
			{
				this.Initialize();
			}
		}

		// Token: 0x06001913 RID: 6419 RVA: 0x000740D0 File Offset: 0x000722D0
		public override void SetModel(NpcClassCustomizationData model, bool triggerOnModelChanged)
		{
			if (!this.initialized)
			{
				this.Initialize();
			}
			Type type = model.GetType();
			IUINpcClassCustomizationDataPresentable iuinpcClassCustomizationDataPresentable;
			if (!this.presenterDictionary.TryGetValue(type, out iuinpcClassCustomizationDataPresentable))
			{
				DebugUtility.LogException(new KeyNotFoundException("Could not get " + type.Name + " out of presenterDictionary!"), this);
				return;
			}
			iuinpcClassCustomizationDataPresentable.SetModelAsObject(model, false);
			base.SetModel(model, triggerOnModelChanged);
		}

		// Token: 0x06001914 RID: 6420 RVA: 0x00074134 File Offset: 0x00072334
		private void Initialize()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", Array.Empty<object>());
			}
			if (this.initialized)
			{
				return;
			}
			this.presenterDictionary = new Dictionary<Type, IUINpcClassCustomizationDataPresentable>
			{
				{
					typeof(BlankNpcCustomizationData),
					this.blankNpcCustomizationDataPresenter
				},
				{
					typeof(GruntNpcCustomizationData),
					this.gruntNpcCustomizationDataPresenter
				},
				{
					typeof(RiflemanNpcCustomizationData),
					this.riflemanNpcCustomizationDataPresenter
				},
				{
					typeof(ZombieNpcCustomizationData),
					this.zombieNpcCustomizationDataPresenter
				}
			};
			foreach (KeyValuePair<Type, IUINpcClassCustomizationDataPresentable> keyValuePair in this.presenterDictionary)
			{
				keyValuePair.Value.OnNpcClassChanged += this.OnNpcClassChanged;
				keyValuePair.Value.OnModelChanged += base.SetModelAsObjectAndTriggerOnModelChanged;
			}
			this.initialized = true;
		}

		// Token: 0x06001915 RID: 6421 RVA: 0x0007423C File Offset: 0x0007243C
		private void OnNpcClassChanged(NpcClass npcClass)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnNpcClassChanged", new object[] { npcClass });
			}
			NpcClassCustomizationData npcClassCustomizationData;
			switch (npcClass)
			{
			case NpcClass.Blank:
				npcClassCustomizationData = new BlankNpcCustomizationData();
				break;
			case NpcClass.Grunt:
				npcClassCustomizationData = new GruntNpcCustomizationData();
				break;
			case NpcClass.Rifleman:
				npcClassCustomizationData = new RiflemanNpcCustomizationData();
				break;
			case NpcClass.Zombie:
				npcClassCustomizationData = new ZombieNpcCustomizationData();
				break;
			default:
				throw new ArgumentOutOfRangeException("npcClass", npcClass, null);
			}
			NpcClassCustomizationData npcClassCustomizationData2 = npcClassCustomizationData;
			this.SetModel(npcClassCustomizationData2, true);
		}

		// Token: 0x04001417 RID: 5143
		[Header("UINpcClassCustomizationDataSwitcherPresenter")]
		[SerializeField]
		private UIBlankNpcCustomizationDataPresenter blankNpcCustomizationDataPresenter;

		// Token: 0x04001418 RID: 5144
		[SerializeField]
		private UIGruntNpcCustomizationDataPresenter gruntNpcCustomizationDataPresenter;

		// Token: 0x04001419 RID: 5145
		[SerializeField]
		private UIRiflemanNpcCustomizationDataPresenter riflemanNpcCustomizationDataPresenter;

		// Token: 0x0400141A RID: 5146
		[SerializeField]
		private UIZombieNpcCustomizationDataPresenter zombieNpcCustomizationDataPresenter;

		// Token: 0x0400141B RID: 5147
		private new Dictionary<Type, IUINpcClassCustomizationDataPresentable> presenterDictionary;

		// Token: 0x0400141C RID: 5148
		private bool initialized;
	}
}
