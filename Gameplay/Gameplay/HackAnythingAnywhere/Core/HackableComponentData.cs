using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Props.Scripting;
using UnityEngine;

namespace HackAnythingAnywhere.Core
{
	// Token: 0x02000029 RID: 41
	[Serializable]
	public class HackableComponentData
	{
		// Token: 0x1700001F RID: 31
		// (get) Token: 0x06000094 RID: 148 RVA: 0x00003CF6 File Offset: 0x00001EF6
		public List<HackableMember> HackableMembers
		{
			get
			{
				return this.hackableMembers;
			}
		}

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x06000095 RID: 149 RVA: 0x00003CFE File Offset: 0x00001EFE
		public bool HasViewableMember
		{
			get
			{
				return this.HackableMembers.FirstOrDefault((HackableMember member) => member.Hackability > HackableMember.HackabilityOption.Hidden) != null;
			}
		}

		// Token: 0x06000096 RID: 150 RVA: 0x00003D2D File Offset: 0x00001F2D
		public HackableComponentData()
		{
		}

		// Token: 0x06000097 RID: 151 RVA: 0x00003D58 File Offset: 0x00001F58
		public HackableComponentData(HackableComponentData other, bool copyComponentReference = true)
		{
			this.hackableMembers = new List<HackableMember>(other.hackableMembers.Count);
			foreach (HackableMember hackableMember in other.hackableMembers)
			{
				this.hackableMembers.Add(new HackableMember(hackableMember));
			}
			this.AssemblyQualifiedTypeName = other.AssemblyQualifiedTypeName;
		}

		// Token: 0x06000098 RID: 152 RVA: 0x00003E00 File Offset: 0x00002000
		internal List<EndlessEventInfo> CompatableReceiverComponents(EndlessEventInfo selectedEndlessEvent)
		{
			List<EndlessEventInfo> list = new List<EndlessEventInfo>();
			IEnumerable<int> enumerable = selectedEndlessEvent.ParamList.Select((EndlessParameterInfo p) => p.DataType);
			foreach (EndlessEventInfo endlessEventInfo in this.AvailableReceivers)
			{
				if (endlessEventInfo.ParamList.Count == 0)
				{
					list.Add(endlessEventInfo);
				}
				else if (endlessEventInfo.ParamList.Count == 1)
				{
					list.Add(endlessEventInfo);
				}
				else if (enumerable.SequenceEqual(endlessEventInfo.ParamList.Select((EndlessParameterInfo p) => p.DataType)))
				{
					list.Add(endlessEventInfo);
				}
			}
			return list;
		}

		// Token: 0x0400005F RID: 95
		[SerializeField]
		private List<HackableMember> hackableMembers = new List<HackableMember>();

		// Token: 0x04000060 RID: 96
		[SerializeField]
		public string AssemblyQualifiedTypeName;

		// Token: 0x04000061 RID: 97
		[SerializeField]
		public List<EndlessEventInfo> AvailableEvents = new List<EndlessEventInfo>();

		// Token: 0x04000062 RID: 98
		[SerializeField]
		public List<EndlessEventInfo> AvailableReceivers = new List<EndlessEventInfo>();
	}
}
