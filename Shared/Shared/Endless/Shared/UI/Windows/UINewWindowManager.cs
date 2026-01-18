using System;
using System.Collections.Generic;

namespace Endless.Shared.UI.Windows
{
	// Token: 0x0200028C RID: 652
	public class UINewWindowManager : UIMonoBehaviourSingleton<UINewWindowManager>
	{
		// Token: 0x14000054 RID: 84
		// (add) Token: 0x06001056 RID: 4182 RVA: 0x0004588C File Offset: 0x00043A8C
		// (remove) Token: 0x06001057 RID: 4183 RVA: 0x000458C4 File Offset: 0x00043AC4
		public event Action OnWindowOpened;

		// Token: 0x14000055 RID: 85
		// (add) Token: 0x06001058 RID: 4184 RVA: 0x000458FC File Offset: 0x00043AFC
		// (remove) Token: 0x06001059 RID: 4185 RVA: 0x00045934 File Offset: 0x00043B34
		public event Action OnAllWindowsClosed;

		// Token: 0x1700031E RID: 798
		// (get) Token: 0x0600105A RID: 4186 RVA: 0x00045969 File Offset: 0x00043B69
		public IReadOnlyList<IUIWindow> OpenWindows
		{
			get
			{
				return this.openWindows;
			}
		}

		// Token: 0x1700031F RID: 799
		// (get) Token: 0x0600105B RID: 4187 RVA: 0x00045971 File Offset: 0x00043B71
		public bool IsDisplayingAnyWindows
		{
			get
			{
				return this.openWindows.Count > 0;
			}
		}

		// Token: 0x0600105C RID: 4188 RVA: 0x00045981 File Offset: 0x00043B81
		public void RegisterWindow(IUIWindow window)
		{
			if (window == null || this.openWindows.Contains(window))
			{
				return;
			}
			this.openWindows.Add(window);
			Action onWindowOpened = this.OnWindowOpened;
			if (onWindowOpened == null)
			{
				return;
			}
			onWindowOpened();
		}

		// Token: 0x0600105D RID: 4189 RVA: 0x000459B1 File Offset: 0x00043BB1
		public void UnregisterWindow(IUIWindow window)
		{
			if (window == null || !this.openWindows.Contains(window))
			{
				return;
			}
			this.openWindows.Remove(window);
			if (this.openWindows.Count == 0)
			{
				Action onAllWindowsClosed = this.OnAllWindowsClosed;
				if (onAllWindowsClosed == null)
				{
					return;
				}
				onAllWindowsClosed();
			}
		}

		// Token: 0x0600105E RID: 4190 RVA: 0x000459EF File Offset: 0x00043BEF
		public int GetSuggestedSiblingIndex()
		{
			return -1;
		}

		// Token: 0x0600105F RID: 4191 RVA: 0x000459F2 File Offset: 0x00043BF2
		public void Close(IUIWindow window)
		{
			if (window == null)
			{
				return;
			}
			window.Close();
		}

		// Token: 0x06001060 RID: 4192 RVA: 0x00045A00 File Offset: 0x00043C00
		public void CloseAllOfType<T>() where T : IUIWindow
		{
			for (int i = this.openWindows.Count - 1; i >= 0; i--)
			{
				if (this.openWindows[i] is T)
				{
					this.Close(this.openWindows[i]);
				}
			}
		}

		// Token: 0x06001061 RID: 4193 RVA: 0x00045A4C File Offset: 0x00043C4C
		public void CloseAll()
		{
			for (int i = this.openWindows.Count - 1; i >= 0; i--)
			{
				this.Close(this.openWindows[i]);
			}
		}

		// Token: 0x06001062 RID: 4194 RVA: 0x00045A83 File Offset: 0x00043C83
		public IUIWindow GetTopmostWindow()
		{
			if (this.openWindows.Count <= 0)
			{
				return null;
			}
			List<IUIWindow> list = this.openWindows;
			return list[list.Count - 1];
		}

		// Token: 0x06001063 RID: 4195 RVA: 0x00045AA8 File Offset: 0x00043CA8
		public bool IsAnyOpen<T>() where T : IUIWindow
		{
			using (List<IUIWindow>.Enumerator enumerator = this.openWindows.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current is T)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06001064 RID: 4196 RVA: 0x00045B04 File Offset: 0x00043D04
		public IReadOnlyList<T> GetAllOfType<T>(List<T> result = null) where T : IUIWindow
		{
			if (result == null)
			{
				result = new List<T>();
			}
			result.Clear();
			foreach (IUIWindow iuiwindow in this.openWindows)
			{
				if (iuiwindow is T)
				{
					T t = (T)((object)iuiwindow);
					result.Add(t);
				}
			}
			return result;
		}

		// Token: 0x06001065 RID: 4197 RVA: 0x00045B78 File Offset: 0x00043D78
		public void BringToFront(IUIWindow window)
		{
			if (window == null || !this.openWindows.Contains(window))
			{
				return;
			}
			window.Prefab.transform.SetAsLastSibling();
			this.openWindows.Remove(window);
			this.openWindows.Add(window);
		}

		// Token: 0x04000A5E RID: 2654
		private readonly List<IUIWindow> openWindows = new List<IUIWindow>();
	}
}
