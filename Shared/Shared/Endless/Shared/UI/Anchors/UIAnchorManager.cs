using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI.Anchors
{
	// Token: 0x020002A5 RID: 677
	public class UIAnchorManager : MonoBehaviourSingleton<UIAnchorManager>
	{
		// Token: 0x17000333 RID: 819
		// (get) Token: 0x060010B1 RID: 4273 RVA: 0x00046F04 File Offset: 0x00045104
		public IReadOnlyList<IUIAnchor> Anchors
		{
			get
			{
				return this.anchors;
			}
		}

		// Token: 0x17000334 RID: 820
		// (get) Token: 0x060010B2 RID: 4274 RVA: 0x00046F0C File Offset: 0x0004510C
		public bool IsDisplayingAnyAnchors
		{
			get
			{
				return this.anchors.Count > 0;
			}
		}

		// Token: 0x060010B3 RID: 4275 RVA: 0x00046F1C File Offset: 0x0004511C
		private void LateUpdate()
		{
			for (int i = this.anchors.Count - 1; i >= 0; i--)
			{
				IUIAnchor iuianchor = this.anchors[i];
				if (iuianchor != null)
				{
					iuianchor.UpdatePosition();
				}
			}
		}

		// Token: 0x060010B4 RID: 4276 RVA: 0x00046F58 File Offset: 0x00045158
		public void Register(IUIAnchor anchor)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Register", new object[] { anchor });
			}
			if (anchor == null)
			{
				Debug.LogException(new NullReferenceException("anchor cannot be null"), this);
				return;
			}
			if (this.anchors.Contains(anchor))
			{
				Debug.LogError(string.Format("{0} called for already registered anchor {1}", "Register", anchor), this);
				return;
			}
			this.anchors.Add(anchor);
			Action<IUIAnchor> onAnchorRegistered = this.OnAnchorRegistered;
			if (onAnchorRegistered == null)
			{
				return;
			}
			onAnchorRegistered(anchor);
		}

		// Token: 0x060010B5 RID: 4277 RVA: 0x00046FD8 File Offset: 0x000451D8
		public void UnregisterAnchor(IUIAnchor anchor)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UnregisterAnchor", new object[] { anchor });
			}
			if (anchor == null)
			{
				Debug.LogException(new NullReferenceException("anchor cannot be null"), this);
				return;
			}
			if (!this.anchors.Contains(anchor))
			{
				Debug.LogError(string.Format("{0} called for an unregistered anchor {1}", "UnregisterAnchor", anchor), this);
				return;
			}
			this.anchors.Remove(anchor);
			Action<IUIAnchor> onAnchorUnregistered = this.OnAnchorUnregistered;
			if (onAnchorUnregistered == null)
			{
				return;
			}
			onAnchorUnregistered(anchor);
		}

		// Token: 0x060010B6 RID: 4278 RVA: 0x0004705C File Offset: 0x0004525C
		public void CloseAllOfType<T>() where T : IUIAnchor
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CloseAllOfType", Array.Empty<object>());
			}
			for (int i = this.anchors.Count - 1; i >= 0; i--)
			{
				if (this.anchors[i] is T)
				{
					this.anchors[i].Close();
				}
			}
		}

		// Token: 0x060010B7 RID: 4279 RVA: 0x000470C0 File Offset: 0x000452C0
		public void CloseAll()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CloseAll", Array.Empty<object>());
			}
			for (int i = this.anchors.Count - 1; i >= 0; i--)
			{
				this.anchors[i].Close();
			}
		}

		// Token: 0x060010B8 RID: 4280 RVA: 0x00047110 File Offset: 0x00045310
		public bool IsAnyOpen<T>() where T : IUIAnchor
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "IsAnyOpen", Array.Empty<object>());
			}
			using (List<IUIAnchor>.Enumerator enumerator = this.anchors.GetEnumerator())
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

		// Token: 0x060010B9 RID: 4281 RVA: 0x00047184 File Offset: 0x00045384
		public IReadOnlyList<T> GetAllOfType<T>() where T : IUIAnchor
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "GetAllOfType", Array.Empty<object>());
			}
			List<T> list = new List<T>();
			foreach (IUIAnchor iuianchor in this.anchors)
			{
				if (iuianchor is T)
				{
					T t = (T)((object)iuianchor);
					list.Add(t);
				}
			}
			return list;
		}

		// Token: 0x04000A87 RID: 2695
		public Action<IUIAnchor> OnAnchorRegistered;

		// Token: 0x04000A88 RID: 2696
		public Action<IUIAnchor> OnAnchorUnregistered;

		// Token: 0x04000A89 RID: 2697
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000A8A RID: 2698
		private readonly List<IUIAnchor> anchors = new List<IUIAnchor>();
	}
}
