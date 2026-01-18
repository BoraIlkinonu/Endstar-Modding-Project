using System;
using Endless.Shared.Debugging;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000199 RID: 409
	public abstract class UIBaseRearrangeableListModel<T> : UIBaseListModel<T>
	{
		// Token: 0x170001F2 RID: 498
		// (get) Token: 0x06000A6C RID: 2668 RVA: 0x0002C901 File Offset: 0x0002AB01
		public UnityEvent<int, int> OnItemMovedUnityEvent { get; } = new UnityEvent<int, int>();

		// Token: 0x06000A6D RID: 2669 RVA: 0x0002C90C File Offset: 0x0002AB0C
		public void Move(int oldIndex, int newIndex, bool triggerEvents)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", new object[] { "Move", "oldIndex", oldIndex, "newIndex", newIndex, "triggerEvents", triggerEvents }), this);
			}
			if (oldIndex < 0 || oldIndex >= this.Count)
			{
				DebugUtility.LogException(new ArgumentOutOfRangeException("oldIndex", string.Format("{0} is out of range ( {1}: {2} ).", "oldIndex", "Count", this.Count)), this);
			}
			if (newIndex < 0 || newIndex >= this.Count)
			{
				DebugUtility.LogException(new ArgumentOutOfRangeException("newIndex", string.Format("{0} is out of range ( {1}: {2} ).", "newIndex", "Count", this.Count)), this);
			}
			T t = this.List[oldIndex];
			this.List.RemoveAt(oldIndex);
			this.List.Insert(newIndex, t);
			if (!triggerEvents)
			{
				return;
			}
			Action<UIBaseListModel<T>, int, int> itemMovedAction = UIBaseRearrangeableListModel<T>.ItemMovedAction;
			if (itemMovedAction != null)
			{
				itemMovedAction(this, oldIndex, newIndex);
			}
			this.OnItemMovedUnityEvent.Invoke(oldIndex, newIndex);
			base.TriggerModelChanged();
		}

		// Token: 0x0400068B RID: 1675
		public static readonly Action<UIBaseListModel<T>, int, int> ItemMovedAction;
	}
}
