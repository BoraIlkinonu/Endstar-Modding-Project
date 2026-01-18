using System;
using Endless.Gameplay;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000223 RID: 547
	public class UIInstanceReferenceNoneOrContextView : UIBaseInstanceReferenceView<InstanceReference, UIInstanceReferenceView.Styles>
	{
		// Token: 0x17000112 RID: 274
		// (get) Token: 0x060008DF RID: 2271 RVA: 0x0002ABB5 File Offset: 0x00028DB5
		// (set) Token: 0x060008E0 RID: 2272 RVA: 0x0002ABBD File Offset: 0x00028DBD
		public override UIInstanceReferenceView.Styles Style { get; protected set; } = UIInstanceReferenceView.Styles.NoneOrContext;

		// Token: 0x17000113 RID: 275
		// (get) Token: 0x060008E1 RID: 2273 RVA: 0x0002ABC6 File Offset: 0x00028DC6
		protected override ReferenceFilter ReferenceFilter
		{
			get
			{
				return ReferenceFilter.NonStatic;
			}
		}

		// Token: 0x060008E2 RID: 2274 RVA: 0x0002ABC9 File Offset: 0x00028DC9
		protected override void Start()
		{
			base.Start();
			this.noneOrContextRadio.OnValueChanged.AddListener(new UnityAction<NoneOrContext>(this.InvokeNoneOrContextRadioChanged));
		}

		// Token: 0x14000019 RID: 25
		// (add) Token: 0x060008E3 RID: 2275 RVA: 0x0002ABF0 File Offset: 0x00028DF0
		// (remove) Token: 0x060008E4 RID: 2276 RVA: 0x0002AC28 File Offset: 0x00028E28
		public event Action<InstanceReference> NoneOrContextChanged;

		// Token: 0x060008E5 RID: 2277 RVA: 0x0002AC5D File Offset: 0x00028E5D
		public override void View(InstanceReference model)
		{
			base.View(model);
			Debug.Log(model.useContext, this);
			this.noneOrContextRadio.SetValue(model.useContext ? NoneOrContext.Context : NoneOrContext.None, false);
		}

		// Token: 0x060008E6 RID: 2278 RVA: 0x0002AC90 File Offset: 0x00028E90
		private void InvokeNoneOrContextRadioChanged(NoneOrContext noneOrContext)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "InvokeNoneOrContextRadioChanged", "noneOrContext", noneOrContext), this);
			}
			InstanceReference instanceReference = ReferenceFactory.CreateInstanceReference(SerializableGuid.Empty, noneOrContext == NoneOrContext.Context);
			Action<InstanceReference> noneOrContextChanged = this.NoneOrContextChanged;
			if (noneOrContextChanged == null)
			{
				return;
			}
			noneOrContextChanged(instanceReference);
		}

		// Token: 0x04000785 RID: 1925
		[Header("UIInstanceReferenceNoneOrContextView")]
		[SerializeField]
		private UINoneOrContextRadio noneOrContextRadio;
	}
}
