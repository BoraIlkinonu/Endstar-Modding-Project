using System.Collections.Generic;
using Endless.Props.Scripting;

namespace Endless.Creator.LevelEditing.Runtime;

public class UIEndlessEventList
{
	public string AssemblyQualifiedTypeName;

	public string DisplayName;

	public List<EndlessEventInfo> EndlessEventInfos = new List<EndlessEventInfo>();
}
