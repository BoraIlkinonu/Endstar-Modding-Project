using System;
using Endless.Props.Scripting;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIEndlessEventInfoListModel : UIBaseLocalFilterableListModel<EndlessEventInfo>
{
	public enum Types
	{
		Emitter,
		Receiver
	}

	[field: SerializeField]
	public Types Type { get; private set; }

	protected override Comparison<EndlessEventInfo> DefaultSort => (EndlessEventInfo x, EndlessEventInfo y) => string.Compare(x.MemberName, y.MemberName, StringComparison.Ordinal);
}
