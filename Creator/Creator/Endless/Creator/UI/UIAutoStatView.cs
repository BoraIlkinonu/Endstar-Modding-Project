using System;
using Endless.Gameplay;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020001E5 RID: 485
	public class UIAutoStatView : UIBaseView<GameEndBlock.AutoStat, UIAutoStatView.Styles>
	{
		// Token: 0x14000004 RID: 4
		// (add) Token: 0x06000783 RID: 1923 RVA: 0x000255D8 File Offset: 0x000237D8
		// (remove) Token: 0x06000784 RID: 1924 RVA: 0x00025610 File Offset: 0x00023810
		public event Action<GameEndBlock.Stats> StatChanged;

		// Token: 0x14000005 RID: 5
		// (add) Token: 0x06000785 RID: 1925 RVA: 0x00025648 File Offset: 0x00023848
		// (remove) Token: 0x06000786 RID: 1926 RVA: 0x00025680 File Offset: 0x00023880
		public event Action<int> PriorityChanged;

		// Token: 0x14000006 RID: 6
		// (add) Token: 0x06000787 RID: 1927 RVA: 0x000256B8 File Offset: 0x000238B8
		// (remove) Token: 0x06000788 RID: 1928 RVA: 0x000256F0 File Offset: 0x000238F0
		public event Action<GameEndBlock.StatType> StatTypeChanged;

		// Token: 0x170000E2 RID: 226
		// (get) Token: 0x06000789 RID: 1929 RVA: 0x00025725 File Offset: 0x00023925
		// (set) Token: 0x0600078A RID: 1930 RVA: 0x0002572D File Offset: 0x0002392D
		public override UIAutoStatView.Styles Style { get; protected set; }

		// Token: 0x0600078B RID: 1931 RVA: 0x00025738 File Offset: 0x00023938
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.statDropdown.SetLabel("Stat");
			this.statTypeDropdown.SetLabel("Stat Type");
			this.statDropdown.OnEnumValueChanged.AddListener(new UnityAction<Enum>(this.InvokeStatChanged));
			this.priorityIntPresenter.OnModelChanged += this.InvokePriorityChanged;
			this.statTypeDropdown.OnEnumValueChanged.AddListener(new UnityAction<Enum>(this.InvokeStatTypeChanged));
		}

		// Token: 0x0600078C RID: 1932 RVA: 0x000257CC File Offset: 0x000239CC
		private void OnDestroy()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			this.priorityIntPresenter.OnModelChanged -= this.InvokePriorityChanged;
		}

		// Token: 0x0600078D RID: 1933 RVA: 0x00025800 File Offset: 0x00023A00
		public override void View(GameEndBlock.AutoStat model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
			if (!this.controlsSetUp)
			{
				this.SetUpControls();
			}
			this.statDropdown.SetEnumValue(model.Stat, false);
			this.priorityIntPresenter.SetModel(model.Order, false);
			this.statTypeDropdown.SetEnumValue(model.StatType, false);
		}

		// Token: 0x0600078E RID: 1934 RVA: 0x00025878 File Offset: 0x00023A78
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			this.priorityIntPresenter.Clear();
		}

		// Token: 0x0600078F RID: 1935 RVA: 0x000258A0 File Offset: 0x00023AA0
		private void InvokeStatChanged(Enum stat)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InvokeStatChanged", new object[] { stat });
			}
			GameEndBlock.Stats stats = (GameEndBlock.Stats)stat;
			Action<GameEndBlock.Stats> statChanged = this.StatChanged;
			if (statChanged == null)
			{
				return;
			}
			statChanged(stats);
		}

		// Token: 0x06000790 RID: 1936 RVA: 0x000258E4 File Offset: 0x00023AE4
		private void InvokePriorityChanged(object priority)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InvokePriorityChanged", new object[] { priority });
			}
			int num = (int)priority;
			Action<int> priorityChanged = this.PriorityChanged;
			if (priorityChanged == null)
			{
				return;
			}
			priorityChanged(num);
		}

		// Token: 0x06000791 RID: 1937 RVA: 0x00025928 File Offset: 0x00023B28
		private void InvokeStatTypeChanged(Enum statType)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InvokeStatTypeChanged", new object[] { statType });
			}
			GameEndBlock.StatType statType2 = (GameEndBlock.StatType)statType;
			Action<GameEndBlock.StatType> statTypeChanged = this.StatTypeChanged;
			if (statTypeChanged == null)
			{
				return;
			}
			statTypeChanged(statType2);
		}

		// Token: 0x06000792 RID: 1938 RVA: 0x0002596C File Offset: 0x00023B6C
		private void SetUpControls()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetUpControls", Array.Empty<object>());
			}
			if (this.controlsSetUp)
			{
				return;
			}
			this.statDropdown.InitializeDropdownWithEnum(typeof(GameEndBlock.Stats));
			this.statTypeDropdown.InitializeDropdownWithEnum(typeof(GameEndBlock.StatType));
			this.controlsSetUp = true;
		}

		// Token: 0x040006C1 RID: 1729
		[SerializeField]
		private UIDropdownEnum statDropdown;

		// Token: 0x040006C2 RID: 1730
		[SerializeField]
		private UIIntPresenter priorityIntPresenter;

		// Token: 0x040006C3 RID: 1731
		[SerializeField]
		private UIDropdownEnum statTypeDropdown;

		// Token: 0x040006C4 RID: 1732
		private bool controlsSetUp;

		// Token: 0x020001E6 RID: 486
		public enum Styles
		{
			// Token: 0x040006CA RID: 1738
			Default
		}
	}
}
