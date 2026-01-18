using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Endless.Shared
{
	// Token: 0x02000047 RID: 71
	public class BackManager : MonoBehaviourSingleton<BackManager>
	{
		// Token: 0x06000280 RID: 640 RVA: 0x0000CF30 File Offset: 0x0000B130
		protected override void Awake()
		{
			base.Awake();
			this.endlessSharedInputActions = new EndlessSharedInputActions();
			this.endlessSharedInputActions.Player.Back.Enable();
			this.endlessSharedInputActions.Player.Back.performed += this.Back;
		}

		// Token: 0x06000281 RID: 641 RVA: 0x0000CF8C File Offset: 0x0000B18C
		protected override void OnDestroy()
		{
			base.OnDestroy();
			this.endlessSharedInputActions.Player.Back.performed -= this.Back;
			this.endlessSharedInputActions.Dispose();
			this.endlessSharedInputActions = null;
		}

		// Token: 0x06000282 RID: 642 RVA: 0x0000CFD5 File Offset: 0x0000B1D5
		public bool HasContext(IBackable context)
		{
			if (this.superVerboseLogging)
			{
				DebugUtility.LogMethod(this, "HasContext", new object[] { context });
			}
			return this.contextList.Contains(context);
		}

		// Token: 0x06000283 RID: 643 RVA: 0x0000D000 File Offset: 0x0000B200
		public void ClaimContext(IBackable context)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ClaimContext", new object[] { context });
			}
			if (context == null)
			{
				DebugUtility.LogError(this, "ClaimContext", "context is null!", new object[] { context });
				return;
			}
			if (this.contextList.Count > 0)
			{
				List<IBackable> list = this.contextList;
				if (list[list.Count - 1] == context)
				{
					DebugUtility.LogWarning(this, "ClaimContext", "Duplicate context claimed!", new object[] { context });
					return;
				}
			}
			this.contextList.Add(context);
		}

		// Token: 0x06000284 RID: 644 RVA: 0x0000D094 File Offset: 0x0000B294
		public void UnclaimContext(IBackable context)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UnclaimContext", new object[] { context });
			}
			if (context == null)
			{
				DebugUtility.LogError(this, "UnclaimContext", "context is null!", new object[] { context });
				this.contextList.RemoveAll((IBackable entry) => entry == null);
				return;
			}
			if (this.contextList.Count == 0)
			{
				DebugUtility.LogWarning(this, "UnclaimContext", "There are no items in contextList to unclaim!", new object[] { context });
				return;
			}
			int count = this.contextList.Count;
			while (count-- > 0)
			{
				if (this.contextList[count] == context)
				{
					this.contextList.RemoveAt(count);
					return;
				}
			}
			DebugUtility.LogWarning(this, "UnclaimContext", "There is no matching context to unclaim!", new object[] { context });
		}

		// Token: 0x06000285 RID: 645 RVA: 0x0000D17C File Offset: 0x0000B37C
		private void Back(InputAction.CallbackContext callbackContext)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "Back", string.Format("{0}.Count: {1}", "contextList", this.contextList.Count), new object[] { callbackContext.action.name });
			}
			this.CleanContextList();
			if (this.contextList.Count == 0)
			{
				this.OnEscapeWithNoContext.Invoke();
				return;
			}
			while (this.contextList.Count > 0)
			{
				List<IBackable> list = this.contextList;
				IBackable backable = list[list.Count - 1];
				if (backable != null)
				{
					backable.OnBack();
					return;
				}
				this.contextList.RemoveAt(this.contextList.Count - 1);
			}
			this.OnEscapeWithNoContext.Invoke();
		}

		// Token: 0x06000286 RID: 646 RVA: 0x0000D240 File Offset: 0x0000B440
		private void CleanContextList()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CleanContextList", Array.Empty<object>());
			}
			int count = this.contextList.Count;
			while (count-- > 0)
			{
				if (this.contextList[count] == null)
				{
					DebugUtility.LogError(this, "CleanContextList", "Null item found and removed in contextList!", new object[] { this });
					this.contextList.RemoveAt(count);
				}
			}
		}

		// Token: 0x04000144 RID: 324
		public UnityEvent OnEscapeWithNoContext = new UnityEvent();

		// Token: 0x04000145 RID: 325
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000146 RID: 326
		[SerializeField]
		private bool superVerboseLogging;

		// Token: 0x04000147 RID: 327
		private readonly List<IBackable> contextList = new List<IBackable>();

		// Token: 0x04000148 RID: 328
		private EndlessSharedInputActions endlessSharedInputActions;
	}
}
