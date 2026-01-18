using System;
using System.Collections.Generic;
using Endless.Creator;
using Endless.Creator.UI;
using Endless.Gameplay;
using Endless.Gameplay.UI;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x02000038 RID: 56
	public class UIPlayerNameAnchorHandler : UIGameObject
	{
		// Token: 0x0600010A RID: 266 RVA: 0x000076E0 File Offset: 0x000058E0
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorStarted.AddListener(new UnityAction(this.Initialize));
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorEnded.AddListener(new UnityAction(this.Deinitialize));
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayStarted.AddListener(new UnityAction(this.Initialize));
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayStopped.AddListener(new UnityAction(this.Deinitialize));
		}

		// Token: 0x0600010B RID: 267 RVA: 0x00007774 File Offset: 0x00005974
		private void OnDestroy()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			if (ExitManager.IsQuitting)
			{
				return;
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorStarted.RemoveListener(new UnityAction(this.Initialize));
			NetworkBehaviourSingleton<CreatorManager>.Instance.OnCreatorEnded.RemoveListener(new UnityAction(this.Deinitialize));
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayStarted.RemoveListener(new UnityAction(this.Initialize));
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayStopped.RemoveListener(new UnityAction(this.Deinitialize));
			if (this.initialized)
			{
				this.Deinitialize();
			}
		}

		// Token: 0x0600010C RID: 268 RVA: 0x0000781C File Offset: 0x00005A1C
		private void Initialize()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", Array.Empty<object>());
			}
			foreach (ulong num in MonoBehaviourSingleton<PlayerManager>.Instance.CurrentPlayerGuids)
			{
				PlayerReferenceManager playerObject = MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayerObject(num);
				this.SpawnPlayerNameAnchor(num, playerObject);
			}
			MonoBehaviourSingleton<PlayerManager>.Instance.OnNewPlayerRegistered.AddListener(new UnityAction<ulong, PlayerReferenceManager>(this.SpawnPlayerNameAnchor));
			MonoBehaviourSingleton<PlayerManager>.Instance.PlayerUnregistered.AddListener(new UnityAction<ulong, PlayerReferenceManager>(this.DespawnPlayerNameAnchor));
			this.initialized = true;
		}

		// Token: 0x0600010D RID: 269 RVA: 0x000078D8 File Offset: 0x00005AD8
		private void Deinitialize()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Deinitialize", Array.Empty<object>());
			}
			MonoBehaviourSingleton<PlayerManager>.Instance.OnNewPlayerRegistered.RemoveListener(new UnityAction<ulong, PlayerReferenceManager>(this.SpawnPlayerNameAnchor));
			MonoBehaviourSingleton<PlayerManager>.Instance.PlayerUnregistered.RemoveListener(new UnityAction<ulong, PlayerReferenceManager>(this.DespawnPlayerNameAnchor));
			foreach (KeyValuePair<ulong, UIPlayerNameAnchor> keyValuePair in this.gameplayPlayerAnchorDictionary)
			{
				keyValuePair.Value.Close();
			}
			this.gameplayPlayerAnchorDictionary.Clear();
			this.initialized = false;
		}

		// Token: 0x0600010E RID: 270 RVA: 0x00007990 File Offset: 0x00005B90
		private void SpawnPlayerNameAnchor(ulong clientId, PlayerReferenceManager playerReferenceManager)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SpawnPlayerNameAnchor", new object[] { clientId, playerReferenceManager });
			}
			if (playerReferenceManager.IsOwner)
			{
				return;
			}
			RectTransform rectTransform = (MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying ? MonoBehaviourSingleton<UIGameplayReferenceManager>.Instance.AnchorContainer : MonoBehaviourSingleton<UICreatorReferenceManager>.Instance.AnchorContainer);
			UIPlayerNameAnchor uiplayerNameAnchor = UIPlayerNameAnchor.CreateInstance(this.playerNameAnchorSource, playerReferenceManager.ApperanceController.transform, rectTransform, playerReferenceManager, new Vector3?(this.offset));
			UIPlayerNameAnchor uiplayerNameAnchor2;
			if (this.gameplayPlayerAnchorDictionary.TryGetValue(clientId, out uiplayerNameAnchor2))
			{
				uiplayerNameAnchor2.Close();
			}
			this.gameplayPlayerAnchorDictionary[clientId] = uiplayerNameAnchor;
		}

		// Token: 0x0600010F RID: 271 RVA: 0x00007A38 File Offset: 0x00005C38
		private void DespawnPlayerNameAnchor(ulong clientId, PlayerReferenceManager _)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "DespawnPlayerNameAnchor", new object[] { clientId, _ });
			}
			UIPlayerNameAnchor uiplayerNameAnchor;
			if (this.gameplayPlayerAnchorDictionary.Remove(clientId, out uiplayerNameAnchor))
			{
				uiplayerNameAnchor.Close();
			}
		}

		// Token: 0x040000AC RID: 172
		[SerializeField]
		private UIPlayerNameAnchor playerNameAnchorSource;

		// Token: 0x040000AD RID: 173
		[SerializeField]
		private Vector3 offset = Vector3.up * 1.5f;

		// Token: 0x040000AE RID: 174
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040000AF RID: 175
		private readonly Dictionary<ulong, UIPlayerNameAnchor> gameplayPlayerAnchorDictionary = new Dictionary<ulong, UIPlayerNameAnchor>();

		// Token: 0x040000B0 RID: 176
		private bool initialized;
	}
}
