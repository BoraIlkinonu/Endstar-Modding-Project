using System;
using System.Collections.Generic;
using Endless.Gameplay.Serialization;
using Endless.Props.Scripting;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200029C RID: 668
	public class ComponentDefinition : ScriptableObject
	{
		// Token: 0x170002C9 RID: 713
		// (get) Token: 0x06000ECB RID: 3787 RVA: 0x0004E928 File Offset: 0x0004CB28
		public ReferenceFilter Filter
		{
			get
			{
				if (!this.hasGottenFilter)
				{
					this.filter = this.prefab.GetComponent<IComponentBase>().Filter;
				}
				return this.filter;
			}
		}

		// Token: 0x170002CA RID: 714
		// (get) Token: 0x06000ECC RID: 3788 RVA: 0x0004E94E File Offset: 0x0004CB4E
		public GameObject Prefab
		{
			get
			{
				return this.prefab;
			}
		}

		// Token: 0x170002CB RID: 715
		// (get) Token: 0x06000ECD RID: 3789 RVA: 0x0004E956 File Offset: 0x0004CB56
		public SerializableGuid ComponentId
		{
			get
			{
				return this.componentId;
			}
		}

		// Token: 0x170002CC RID: 716
		// (get) Token: 0x06000ECE RID: 3790 RVA: 0x0004E95E File Offset: 0x0004CB5E
		public bool IsNetworked
		{
			get
			{
				return this.isNetworked;
			}
		}

		// Token: 0x170002CD RID: 717
		// (get) Token: 0x06000ECF RID: 3791 RVA: 0x0004E966 File Offset: 0x0004CB66
		public IComponentBase ComponentBase
		{
			get
			{
				if (this.componentBase == null)
				{
					this.componentBase = this.prefab.GetComponent<IComponentBase>();
				}
				return this.componentBase;
			}
		}

		// Token: 0x170002CE RID: 718
		// (get) Token: 0x06000ED0 RID: 3792 RVA: 0x0004E987 File Offset: 0x0004CB87
		public Type ComponentReferenceType
		{
			get
			{
				return this.ComponentBase.ComponentReferenceType;
			}
		}

		// Token: 0x06000ED1 RID: 3793 RVA: 0x0004E994 File Offset: 0x0004CB94
		public bool HasMember(MemberChange memberChange)
		{
			foreach (InspectorExposedVariable inspectorExposedVariable in this.InspectableMembers)
			{
				if (inspectorExposedVariable.MemberName == memberChange.MemberName)
				{
					int typeId = EndlessTypeMapping.Instance.GetTypeId(inspectorExposedVariable.DataType);
					if (typeId == memberChange.DataType)
					{
						return true;
					}
					Type typeFromId = EndlessTypeMapping.Instance.GetTypeFromId(typeId);
					Type typeFromId2 = EndlessTypeMapping.Instance.GetTypeFromId(memberChange.DataType);
					if (typeFromId.IsAssignableFrom(typeFromId2) || typeFromId.BaseType == typeFromId2.BaseType)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x04000D3C RID: 3388
		[SerializeField]
		private GameObject prefab;

		// Token: 0x04000D3D RID: 3389
		[SerializeField]
		private SerializableGuid componentId;

		// Token: 0x04000D3E RID: 3390
		public List<InspectorExposedVariable> InspectableMembers = new List<InspectorExposedVariable>();

		// Token: 0x04000D3F RID: 3391
		public List<EndlessEventInfo> AvailableEvents = new List<EndlessEventInfo>();

		// Token: 0x04000D40 RID: 3392
		public List<EndlessEventInfo> AvailableReceivers = new List<EndlessEventInfo>();

		// Token: 0x04000D41 RID: 3393
		[SerializeField]
		private bool isNetworked;

		// Token: 0x04000D42 RID: 3394
		private bool hasGottenFilter;

		// Token: 0x04000D43 RID: 3395
		private ReferenceFilter filter;

		// Token: 0x04000D44 RID: 3396
		private IComponentBase componentBase;
	}
}
