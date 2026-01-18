using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;

namespace Endless.Creator.UI
{
	// Token: 0x02000228 RID: 552
	public class UINpcInstanceReferencePresenter : UIBaseInstanceReferencePresenter<NpcInstanceReference>
	{
		// Token: 0x17000119 RID: 281
		// (get) Token: 0x060008FB RID: 2299 RVA: 0x0002AF60 File Offset: 0x00029160
		protected override string IEnumerableWindowTitle
		{
			get
			{
				return "Select an NPC";
			}
		}

		// Token: 0x060008FC RID: 2300 RVA: 0x0002AF68 File Offset: 0x00029168
		protected override void Start()
		{
			base.Start();
			if (object.Equals(base.View.Interface.StyleEnum, UINpcInstanceReferenceView.Styles.NoneOrContext))
			{
				(base.View.Interface as UINpcInstanceReferenceNoneOrContextView).NoneOrContextChanged += base.SetModelAndTriggerOnModelChanged;
			}
		}

		// Token: 0x060008FD RID: 2301 RVA: 0x0002AFBC File Offset: 0x000291BC
		protected override void SetSelection(List<object> selection)
		{
			if (selection.Any<object>())
			{
				List<object> list = new List<object>();
				foreach (object obj in selection)
				{
					PropEntry propEntry = obj as PropEntry;
					if (propEntry == null)
					{
						DebugUtility.LogException(new InvalidCastException("selection's element type must be of type PropEntry"), this);
					}
					else
					{
						bool flag = propEntry.InstanceId == UIPropEntryView.UseContext.InstanceId;
						NpcInstanceReference npcInstanceReference = ReferenceFactory.CreateNpcInstanceReference(propEntry.InstanceId, flag);
						list.Add(npcInstanceReference);
					}
				}
				base.SetSelection(list);
				return;
			}
			base.SetSelection(selection);
		}

		// Token: 0x060008FE RID: 2302 RVA: 0x0002B068 File Offset: 0x00029268
		protected override NpcInstanceReference ConstructNewModelWithInstanceId(SerializableGuid instanceId)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ConstructNewModelWithInstanceId", "instanceId", instanceId), this);
			}
			return ReferenceFactory.CreateNpcInstanceReference(instanceId, false);
		}

		// Token: 0x060008FF RID: 2303 RVA: 0x0002B099 File Offset: 0x00029299
		protected override NpcInstanceReference CreateDefaultModel()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("CreateDefaultModel", this);
			}
			return ReferenceFactory.CreateNpcInstanceReference(SerializableGuid.Empty, false);
		}
	}
}
