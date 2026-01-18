using System;
using Endless.Gameplay;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020001F0 RID: 496
	public class UILevelDestinationView : UIBaseView<LevelDestination, UILevelDestinationView.Styles>, IUIInteractable
	{
		// Token: 0x14000008 RID: 8
		// (add) Token: 0x060007BD RID: 1981 RVA: 0x000263A4 File Offset: 0x000245A4
		// (remove) Token: 0x060007BE RID: 1982 RVA: 0x000263DC File Offset: 0x000245DC
		public event Action OnOpenLevelSelectionWindow;

		// Token: 0x14000009 RID: 9
		// (add) Token: 0x060007BF RID: 1983 RVA: 0x00026414 File Offset: 0x00024614
		// (remove) Token: 0x060007C0 RID: 1984 RVA: 0x0002644C File Offset: 0x0002464C
		public event Action OnOpenSpawnPointSelectionWindow;

		// Token: 0x170000EA RID: 234
		// (get) Token: 0x060007C1 RID: 1985 RVA: 0x00026481 File Offset: 0x00024681
		// (set) Token: 0x060007C2 RID: 1986 RVA: 0x00026489 File Offset: 0x00024689
		public override UILevelDestinationView.Styles Style { get; protected set; }

		// Token: 0x060007C3 RID: 1987 RVA: 0x00026494 File Offset: 0x00024694
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.openLevelSelectionWindowButton.onClick.AddListener(new UnityAction(this.OnOpenLevelSelectionWindowButtonPressed));
			this.openSpawnPointSelectionWindowButton.onClick.AddListener(new UnityAction(this.OnSpawnPointSelectionWindowButtonPressed));
		}

		// Token: 0x060007C4 RID: 1988 RVA: 0x000264F1 File Offset: 0x000246F1
		public override void View(LevelDestination model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
			if (!model.IsValidLevel())
			{
				this.SetLevelNameText("None");
			}
		}

		// Token: 0x060007C5 RID: 1989 RVA: 0x00026523 File Offset: 0x00024723
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			this.SetLevelNameText(string.Empty);
		}

		// Token: 0x060007C6 RID: 1990 RVA: 0x00026548 File Offset: 0x00024748
		public void SetInteractable(bool interactable)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetInteractable", new object[] { interactable });
			}
			this.openLevelSelectionWindowButton.interactable = interactable;
			this.openSpawnPointSelectionWindowButton.interactable = interactable;
		}

		// Token: 0x060007C7 RID: 1991 RVA: 0x00026584 File Offset: 0x00024784
		public void SetLevelNameText(string levelName)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetLevelNameText", new object[] { levelName });
			}
			this.levelNameText.text = levelName;
		}

		// Token: 0x060007C8 RID: 1992 RVA: 0x000265AF File Offset: 0x000247AF
		private void OnOpenLevelSelectionWindowButtonPressed()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnOpenLevelSelectionWindowButtonPressed", Array.Empty<object>());
			}
			Action onOpenLevelSelectionWindow = this.OnOpenLevelSelectionWindow;
			if (onOpenLevelSelectionWindow == null)
			{
				return;
			}
			onOpenLevelSelectionWindow();
		}

		// Token: 0x060007C9 RID: 1993 RVA: 0x000265D9 File Offset: 0x000247D9
		private void OnSpawnPointSelectionWindowButtonPressed()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSpawnPointSelectionWindowButtonPressed", Array.Empty<object>());
			}
			Action onOpenSpawnPointSelectionWindow = this.OnOpenSpawnPointSelectionWindow;
			if (onOpenSpawnPointSelectionWindow == null)
			{
				return;
			}
			onOpenSpawnPointSelectionWindow();
		}

		// Token: 0x040006E8 RID: 1768
		[SerializeField]
		private TextMeshProUGUI levelNameText;

		// Token: 0x040006E9 RID: 1769
		[SerializeField]
		private UIButton openLevelSelectionWindowButton;

		// Token: 0x040006EA RID: 1770
		[SerializeField]
		private UIButton openSpawnPointSelectionWindowButton;

		// Token: 0x020001F1 RID: 497
		public enum Styles
		{
			// Token: 0x040006EC RID: 1772
			Default,
			// Token: 0x040006ED RID: 1773
			None
		}
	}
}
