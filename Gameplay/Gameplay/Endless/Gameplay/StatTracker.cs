using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x020000C9 RID: 201
	public class StatTracker : MonoBehaviourSingleton<StatTracker>
	{
		// Token: 0x060003E8 RID: 1000 RVA: 0x00015C0B File Offset: 0x00013E0B
		private void Start()
		{
			MonoBehaviourSingleton<GameplayManager>.Instance.OnGameplayCleanup.AddListener(new UnityAction(this.HandleGameplayCleanup));
		}

		// Token: 0x060003E9 RID: 1001 RVA: 0x00015C28 File Offset: 0x00013E28
		private void HandleGameplayCleanup()
		{
			this.playerDowns = new Dictionary<int, int>();
			this.playerRevives = new Dictionary<int, int>();
		}

		// Token: 0x060003EA RID: 1002 RVA: 0x00015C40 File Offset: 0x00013E40
		public void TrackPlayerDown(int userId)
		{
			if (!this.playerDowns.TryAdd(userId, 1))
			{
				Dictionary<int, int> dictionary = this.playerDowns;
				int num = dictionary[userId];
				dictionary[userId] = num + 1;
			}
		}

		// Token: 0x060003EB RID: 1003 RVA: 0x00015C75 File Offset: 0x00013E75
		public int GetGlobalDowns()
		{
			return this.playerDowns.Values.Sum();
		}

		// Token: 0x060003EC RID: 1004 RVA: 0x00015C88 File Offset: 0x00013E88
		public Dictionary<int, float> GetDownsAsFloat()
		{
			Dictionary<int, float> dictionary = new Dictionary<int, float>();
			foreach (KeyValuePair<int, int> keyValuePair in this.playerDowns)
			{
				dictionary.Add(keyValuePair.Key, (float)keyValuePair.Value);
			}
			return dictionary;
		}

		// Token: 0x060003ED RID: 1005 RVA: 0x00015CF0 File Offset: 0x00013EF0
		public void TrackRevive(int userId)
		{
			if (!this.playerDowns.TryAdd(userId, 1))
			{
				Dictionary<int, int> dictionary = this.playerDowns;
				int num = dictionary[userId];
				dictionary[userId] = num + 1;
			}
		}

		// Token: 0x060003EE RID: 1006 RVA: 0x00015D25 File Offset: 0x00013F25
		public int GetGlobalRevives()
		{
			return this.playerRevives.Values.Sum();
		}

		// Token: 0x060003EF RID: 1007 RVA: 0x00015D38 File Offset: 0x00013F38
		public Dictionary<int, float> GetRevivesAsFloat()
		{
			Dictionary<int, float> dictionary = new Dictionary<int, float>();
			foreach (KeyValuePair<int, int> keyValuePair in this.playerRevives)
			{
				dictionary.Add(keyValuePair.Key, (float)keyValuePair.Value);
			}
			return dictionary;
		}

		// Token: 0x04000386 RID: 902
		private Dictionary<int, int> playerDowns = new Dictionary<int, int>();

		// Token: 0x04000387 RID: 903
		private Dictionary<int, int> playerRevives = new Dictionary<int, int>();
	}
}
