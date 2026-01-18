using System;
using System.Collections.Generic;
using UnityEngine;

namespace SQLiter
{
	// Token: 0x02000005 RID: 5
	public class LoomManager : MonoBehaviour
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000015 RID: 21 RVA: 0x000026D3 File Offset: 0x000008D3
		public static LoomManager.ILoom Loom
		{
			get
			{
				if (LoomManager._loom != null)
				{
					return LoomManager._loom;
				}
				return LoomManager._nullLoom;
			}
		}

		// Token: 0x06000016 RID: 22 RVA: 0x000026E7 File Offset: 0x000008E7
		private void Awake()
		{
			LoomManager._loom = new LoomManager.LoomDispatcher();
		}

		// Token: 0x06000017 RID: 23 RVA: 0x000026F3 File Offset: 0x000008F3
		private void OnDestroy()
		{
			LoomManager._loom = null;
		}

		// Token: 0x06000018 RID: 24 RVA: 0x000026FB File Offset: 0x000008FB
		private void Update()
		{
			if (Application.isPlaying)
			{
				LoomManager._loom.Update();
			}
		}

		// Token: 0x04000012 RID: 18
		private static LoomManager.NullLoom _nullLoom = new LoomManager.NullLoom();

		// Token: 0x04000013 RID: 19
		private static LoomManager.LoomDispatcher _loom;

		// Token: 0x02000006 RID: 6
		public interface ILoom
		{
			// Token: 0x0600001B RID: 27
			void QueueOnMainThread(Action action);
		}

		// Token: 0x02000007 RID: 7
		private class NullLoom : LoomManager.ILoom
		{
			// Token: 0x0600001C RID: 28 RVA: 0x0000271A File Offset: 0x0000091A
			public void QueueOnMainThread(Action action)
			{
			}
		}

		// Token: 0x02000008 RID: 8
		private class LoomDispatcher : LoomManager.ILoom
		{
			// Token: 0x0600001E RID: 30 RVA: 0x0000271C File Offset: 0x0000091C
			public void QueueOnMainThread(Action action)
			{
				List<Action> list = this.actions;
				lock (list)
				{
					this.actions.Add(action);
				}
			}

			// Token: 0x0600001F RID: 31 RVA: 0x00002764 File Offset: 0x00000964
			public void Update()
			{
				Action[] array = null;
				List<Action> list = this.actions;
				lock (list)
				{
					array = this.actions.ToArray();
					this.actions.Clear();
				}
				Action[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i]();
				}
			}

			// Token: 0x04000014 RID: 20
			private readonly List<Action> actions = new List<Action>();
		}
	}
}
