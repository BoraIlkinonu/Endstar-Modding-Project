using System;
using Endless.Gameplay;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000227 RID: 551
	public class UINpcInstanceReferenceNoneOrContextView : UIBaseInstanceReferenceView<NpcInstanceReference, UINpcInstanceReferenceView.Styles>
	{
		// Token: 0x17000117 RID: 279
		// (get) Token: 0x060008F2 RID: 2290 RVA: 0x0002AE35 File Offset: 0x00029035
		// (set) Token: 0x060008F3 RID: 2291 RVA: 0x0002AE3D File Offset: 0x0002903D
		public override UINpcInstanceReferenceView.Styles Style { get; protected set; } = UINpcInstanceReferenceView.Styles.NoneOrContext;

		// Token: 0x17000118 RID: 280
		// (get) Token: 0x060008F4 RID: 2292 RVA: 0x00029A30 File Offset: 0x00027C30
		protected override ReferenceFilter ReferenceFilter
		{
			get
			{
				return ReferenceFilter.Npc;
			}
		}

		// Token: 0x060008F5 RID: 2293 RVA: 0x0002AE46 File Offset: 0x00029046
		protected override void Start()
		{
			base.Start();
			this.noneOrContextRadio.OnValueChanged.AddListener(new UnityAction<NoneOrContext>(this.InvokeNoneOrContextRadioChanged));
		}

		// Token: 0x1400001A RID: 26
		// (add) Token: 0x060008F6 RID: 2294 RVA: 0x0002AE6C File Offset: 0x0002906C
		// (remove) Token: 0x060008F7 RID: 2295 RVA: 0x0002AEA4 File Offset: 0x000290A4
		public event Action<NpcInstanceReference> NoneOrContextChanged;

		// Token: 0x060008F8 RID: 2296 RVA: 0x0002AED9 File Offset: 0x000290D9
		public override void View(NpcInstanceReference model)
		{
			base.View(model);
			this.noneOrContextRadio.SetValue(model.useContext ? NoneOrContext.Context : NoneOrContext.None, false);
		}

		// Token: 0x060008F9 RID: 2297 RVA: 0x0002AEFC File Offset: 0x000290FC
		private void InvokeNoneOrContextRadioChanged(NoneOrContext noneOrContext)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "InvokeNoneOrContextRadioChanged", "noneOrContext", noneOrContext), this);
			}
			NpcInstanceReference npcInstanceReference = ReferenceFactory.CreateNpcInstanceReference(SerializableGuid.Empty, noneOrContext == NoneOrContext.Context);
			Action<NpcInstanceReference> noneOrContextChanged = this.NoneOrContextChanged;
			if (noneOrContextChanged == null)
			{
				return;
			}
			noneOrContextChanged(npcInstanceReference);
		}

		// Token: 0x0400078C RID: 1932
		[Header("UIInstanceReferenceNoneOrContextView")]
		[SerializeField]
		private UINoneOrContextRadio noneOrContextRadio;
	}
}
