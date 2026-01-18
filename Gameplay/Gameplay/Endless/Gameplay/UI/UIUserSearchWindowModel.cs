using System;
using System.Collections.Generic;
using Endless.Shared.DataTypes;
using Endless.Shared.UI;

namespace Endless.Gameplay.UI
{
	// Token: 0x0200041F RID: 1055
	public class UIUserSearchWindowModel
	{
		// Token: 0x06001A35 RID: 6709 RVA: 0x000789CE File Offset: 0x00076BCE
		public UIUserSearchWindowModel(string windowTitle, List<User> usersToHide, SelectionType selectionType, Action<List<object>> onSelectionConfirmed)
		{
			this.WindowTitle = windowTitle;
			this.usersToHide = usersToHide;
			this.SelectionType = selectionType;
			this.OnSelectionConfirmed = onSelectionConfirmed;
		}

		// Token: 0x1700054A RID: 1354
		// (get) Token: 0x06001A36 RID: 6710 RVA: 0x000789F3 File Offset: 0x00076BF3
		// (set) Token: 0x06001A37 RID: 6711 RVA: 0x000789FB File Offset: 0x00076BFB
		public string WindowTitle { get; private set; }

		// Token: 0x1700054B RID: 1355
		// (get) Token: 0x06001A38 RID: 6712 RVA: 0x00078A04 File Offset: 0x00076C04
		public IReadOnlyList<User> UsersToHide
		{
			get
			{
				return this.usersToHide;
			}
		}

		// Token: 0x1700054C RID: 1356
		// (get) Token: 0x06001A39 RID: 6713 RVA: 0x00078A0C File Offset: 0x00076C0C
		// (set) Token: 0x06001A3A RID: 6714 RVA: 0x00078A14 File Offset: 0x00076C14
		public SelectionType SelectionType { get; private set; }

		// Token: 0x1700054D RID: 1357
		// (get) Token: 0x06001A3B RID: 6715 RVA: 0x00078A1D File Offset: 0x00076C1D
		// (set) Token: 0x06001A3C RID: 6716 RVA: 0x00078A25 File Offset: 0x00076C25
		public Action<List<object>> OnSelectionConfirmed { get; private set; }

		// Token: 0x06001A3D RID: 6717 RVA: 0x00078A30 File Offset: 0x00076C30
		public override string ToString()
		{
			return string.Format("{{ {0}: {1}, {2}: {3}, {4}: {5}, {6}: {7} }}", new object[]
			{
				"WindowTitle",
				this.WindowTitle,
				"UsersToHide",
				this.UsersToHide.Count,
				"SelectionType",
				this.SelectionType,
				"OnSelectionConfirmed",
				(this.OnSelectionConfirmed == null) ? "null" : "not null"
			});
		}

		// Token: 0x040014EF RID: 5359
		private readonly List<User> usersToHide;
	}
}
