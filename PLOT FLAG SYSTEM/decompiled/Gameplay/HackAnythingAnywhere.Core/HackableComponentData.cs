using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Props.Scripting;
using UnityEngine;

namespace HackAnythingAnywhere.Core;

[Serializable]
public class HackableComponentData
{
	[SerializeField]
	private List<HackableMember> hackableMembers = new List<HackableMember>();

	[SerializeField]
	public string AssemblyQualifiedTypeName;

	[SerializeField]
	public List<EndlessEventInfo> AvailableEvents = new List<EndlessEventInfo>();

	[SerializeField]
	public List<EndlessEventInfo> AvailableReceivers = new List<EndlessEventInfo>();

	public List<HackableMember> HackableMembers => hackableMembers;

	public bool HasViewableMember => HackableMembers.FirstOrDefault((HackableMember member) => member.Hackability > HackableMember.HackabilityOption.Hidden) != null;

	public HackableComponentData()
	{
	}

	public HackableComponentData(HackableComponentData other, bool copyComponentReference = true)
	{
		hackableMembers = new List<HackableMember>(other.hackableMembers.Count);
		foreach (HackableMember hackableMember in other.hackableMembers)
		{
			hackableMembers.Add(new HackableMember(hackableMember));
		}
		AssemblyQualifiedTypeName = other.AssemblyQualifiedTypeName;
	}

	internal List<EndlessEventInfo> CompatableReceiverComponents(EndlessEventInfo selectedEndlessEvent)
	{
		List<EndlessEventInfo> list = new List<EndlessEventInfo>();
		IEnumerable<int> first = selectedEndlessEvent.ParamList.Select((EndlessParameterInfo p) => p.DataType);
		foreach (EndlessEventInfo availableReceiver in AvailableReceivers)
		{
			if (availableReceiver.ParamList.Count == 0)
			{
				list.Add(availableReceiver);
			}
			else if (availableReceiver.ParamList.Count == 1)
			{
				list.Add(availableReceiver);
			}
			else if (first.SequenceEqual(availableReceiver.ParamList.Select((EndlessParameterInfo p) => p.DataType)))
			{
				list.Add(availableReceiver);
			}
		}
		return list;
	}
}
