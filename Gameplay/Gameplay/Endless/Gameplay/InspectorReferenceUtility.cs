using System;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020000D5 RID: 213
	public static class InspectorReferenceUtility
	{
		// Token: 0x06000435 RID: 1077 RVA: 0x00016B42 File Offset: 0x00014D42
		public static SerializableGuid GetId(InspectorReference reference)
		{
			return reference.Id;
		}

		// Token: 0x06000436 RID: 1078 RVA: 0x00016B4A File Offset: 0x00014D4A
		public static GameObject GetInstanceObject(InstanceReference reference)
		{
			return reference.GetInstanceObject();
		}

		// Token: 0x06000437 RID: 1079 RVA: 0x00016B52 File Offset: 0x00014D52
		public static SerializableGuid GetInstanceDefinitionId(InstanceReference reference)
		{
			return reference.GetInstanceDefintionId();
		}

		// Token: 0x06000438 RID: 1080 RVA: 0x00016B5A File Offset: 0x00014D5A
		public static ReferenceFilter GetReferenceFilter(InspectorPropReference reference)
		{
			return reference.Filter;
		}

		// Token: 0x06000439 RID: 1081 RVA: 0x00016B62 File Offset: 0x00014D62
		public static void SetId(InspectorReference reference, SerializableGuid newAssetId)
		{
			reference.Id = newAssetId;
		}
	}
}
