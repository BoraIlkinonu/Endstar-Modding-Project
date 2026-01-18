using System;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public class UIConsoleMessageListModel : UIBaseLocalFilterableListModel<UIConsoleMessageModel>
{
	protected override Comparison<UIConsoleMessageModel> DefaultSort { get; } = (UIConsoleMessageModel left, UIConsoleMessageModel right) => left.Message.Timestamp.CompareTo(right.Message.Timestamp);
}
