using System;
using System.Collections.Generic;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;

namespace Endless.Creator.UI
{
	// Token: 0x02000224 RID: 548
	public class UIInstanceReferencePresenter : UIBaseInstanceReferencePresenter<InstanceReference>
	{
		// Token: 0x17000114 RID: 276
		// (get) Token: 0x060008E8 RID: 2280 RVA: 0x0002ACF4 File Offset: 0x00028EF4
		protected override string IEnumerableWindowTitle
		{
			get
			{
				return "Select an Instance";
			}
		}

		// Token: 0x060008E9 RID: 2281 RVA: 0x0002ACFC File Offset: 0x00028EFC
		protected override void Start()
		{
			base.Start();
			if (object.Equals(base.View.Interface.StyleEnum, UIInstanceReferenceView.Styles.NoneOrContext))
			{
				(base.View.Interface as UIInstanceReferenceNoneOrContextView).NoneOrContextChanged += base.SetModelAndTriggerOnModelChanged;
			}
		}

		// Token: 0x060008EA RID: 2282 RVA: 0x0002AD4D File Offset: 0x00028F4D
		protected override InstanceReference ConstructNewModelWithInstanceId(SerializableGuid instanceId)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ConstructNewModelWithInstanceId", "instanceId", instanceId), this);
			}
			return ReferenceFactory.CreateInstanceReference(instanceId, false);
		}

		// Token: 0x060008EB RID: 2283 RVA: 0x0002AD80 File Offset: 0x00028F80
		protected override void SetSelection(List<object> selection)
		{
			List<object> list = new List<object>();
			foreach (object obj in selection)
			{
				PropEntry propEntry = obj as PropEntry;
				bool flag = propEntry == UIPropEntryView.UseContext;
				InstanceReference instanceReference = ReferenceFactory.CreateInstanceReference(propEntry.InstanceId, flag);
				list.Add(instanceReference);
			}
			base.SetSelection(list);
		}

		// Token: 0x060008EC RID: 2284 RVA: 0x0002ADF4 File Offset: 0x00028FF4
		protected override InstanceReference CreateDefaultModel()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("CreateDefaultModel", this);
			}
			return ReferenceFactory.CreateInstanceReference(SerializableGuid.Empty, false);
		}
	}
}
