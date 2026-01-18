using System;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Core.UI
{
	// Token: 0x0200008B RID: 139
	public class UIMatchSectionView : UIGameObject
	{
		// Token: 0x060002BC RID: 700 RVA: 0x0000F009 File Offset: 0x0000D209
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			this.View();
		}

		// Token: 0x060002BD RID: 701 RVA: 0x0000F029 File Offset: 0x0000D229
		private void OnDisable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			this.ClearAll();
		}

		// Token: 0x060002BE RID: 702 RVA: 0x0000F04C File Offset: 0x0000D24C
		private void View()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "View", Array.Empty<object>());
			}
			Game activeGame = MonoBehaviourSingleton<RuntimeDatabase>.Instance.ActiveGame;
			if (activeGame != null)
			{
				this.gameNameText.text = "Game: <b>" + activeGame.Name + "</b>";
				this.gameDescriptionText.text = activeGame.Description;
			}
			else
			{
				DebugUtility.LogWarning("RuntimeDatabase's ActiveGame is null!", this);
				this.ClearGame();
			}
			SerializableGuid activeLevelGuid = MonoBehaviourSingleton<StageManager>.Instance.ActiveLevelGuid;
			if (activeLevelGuid.IsEmpty || !MonoBehaviourSingleton<StageManager>.Instance.LevelIsLoaded(activeLevelGuid))
			{
				DebugUtility.LogWarning("StageManager is not loaded!", this);
				this.ClearLevel();
				return;
			}
			LevelState levelState = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState;
			this.levelNameText.text = "Level: <b>" + levelState.Name + "</b>";
			this.levelDescriptionText.text = levelState.Description;
		}

		// Token: 0x060002BF RID: 703 RVA: 0x0000F137 File Offset: 0x0000D337
		private void ClearAll()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ClearAll", Array.Empty<object>());
			}
			this.ClearGame();
			this.ClearLevel();
		}

		// Token: 0x060002C0 RID: 704 RVA: 0x0000F15D File Offset: 0x0000D35D
		private void ClearGame()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ClearGame", Array.Empty<object>());
			}
			this.gameNameText.text = string.Empty;
			this.gameNameText.text = string.Empty;
		}

		// Token: 0x060002C1 RID: 705 RVA: 0x0000F197 File Offset: 0x0000D397
		private void ClearLevel()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ClearLevel", Array.Empty<object>());
			}
			this.levelNameText.text = string.Empty;
			this.levelDescriptionText.text = string.Empty;
		}

		// Token: 0x04000210 RID: 528
		[Header("UIMatchSectionView")]
		[SerializeField]
		private TextMeshProUGUI gameNameText;

		// Token: 0x04000211 RID: 529
		[SerializeField]
		private TextMeshProUGUI gameDescriptionText;

		// Token: 0x04000212 RID: 530
		[Header("Level")]
		[SerializeField]
		private TextMeshProUGUI levelNameText;

		// Token: 0x04000213 RID: 531
		[SerializeField]
		private TextMeshProUGUI levelDescriptionText;

		// Token: 0x04000214 RID: 532
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
