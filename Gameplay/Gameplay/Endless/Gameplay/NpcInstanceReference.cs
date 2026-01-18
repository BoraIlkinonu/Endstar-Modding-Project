using System;
using Endless.Gameplay.Scripting;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020000F8 RID: 248
	[Serializable]
	public class NpcInstanceReference : InstanceReference
	{
		// Token: 0x170000E9 RID: 233
		// (get) Token: 0x06000580 RID: 1408 RVA: 0x0001BD04 File Offset: 0x00019F04
		internal override ReferenceFilter Filter
		{
			get
			{
				return ReferenceFilter.Npc;
			}
		}

		// Token: 0x06000581 RID: 1409 RVA: 0x0001BD08 File Offset: 0x00019F08
		internal NpcEntity GetNpcEntity(Context context)
		{
			if (this.useContext)
			{
				if (!context.IsNpc())
				{
					return null;
				}
				return context.WorldObject.GetUserComponent<NpcEntity>();
			}
			else
			{
				GameObject instanceObject = base.GetInstanceObject();
				if (!instanceObject)
				{
					return null;
				}
				return instanceObject.GetComponentInChildren<NpcEntity>();
			}
		}

		// Token: 0x06000582 RID: 1410 RVA: 0x0001BD4C File Offset: 0x00019F4C
		public Context GetNpc()
		{
			if (this.useContext)
			{
				if (!Context.StaticLastContext.IsNpc())
				{
					return null;
				}
				return Context.StaticLastContext;
			}
			else
			{
				GameObject instanceObject = base.GetInstanceObject();
				if (!instanceObject)
				{
					return null;
				}
				return instanceObject.GetComponent<WorldObject>().Context;
			}
		}

		// Token: 0x06000583 RID: 1411 RVA: 0x0001BD91 File Offset: 0x00019F91
		internal NpcInstanceReference()
		{
		}

		// Token: 0x06000584 RID: 1412 RVA: 0x0001BD99 File Offset: 0x00019F99
		internal NpcInstanceReference(SerializableGuid instanceId, bool useContext)
			: base(instanceId, useContext)
		{
		}
	}
}
