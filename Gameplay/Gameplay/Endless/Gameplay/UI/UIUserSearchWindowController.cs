using System;
using System.Collections.Generic;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x0200041E RID: 1054
	public class UIUserSearchWindowController : UIWindowController
	{
		// Token: 0x17000549 RID: 1353
		// (get) Token: 0x06001A2F RID: 6703 RVA: 0x00078885 File Offset: 0x00076A85
		private UIUserSearchWindowModel Model
		{
			get
			{
				return this.view.Model;
			}
		}

		// Token: 0x06001A30 RID: 6704 RVA: 0x00078894 File Offset: 0x00076A94
		protected override void Start()
		{
			base.Start();
			this.userNameSearchInputField.onValueChanged.AddListener(new UnityAction<string>(this.Search));
			this.confirmSelectionButton.onClick.AddListener(new UnityAction(this.ConfirmSelection));
			this.iEnumerablePresenter.OnModelChanged += this.HandleConfirmSelectionButtonInteractability;
		}

		// Token: 0x06001A31 RID: 6705 RVA: 0x000788F8 File Offset: 0x00076AF8
		private void Search(string userName)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Search", new object[] { userName });
			}
			List<User> list = new List<User>(this.Model.UsersToHide);
			this.userPaginatedGraphQlIEnumerableHandler.SetUserNameToSearchFor(userName, list);
		}

		// Token: 0x06001A32 RID: 6706 RVA: 0x00078940 File Offset: 0x00076B40
		private void ConfirmSelection()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ConfirmSelection", Array.Empty<object>());
			}
			Action<List<object>> onSelectionConfirmed = this.Model.OnSelectionConfirmed;
			if (onSelectionConfirmed != null)
			{
				onSelectionConfirmed(new List<object>(this.iEnumerablePresenter.SelectedItemsList));
			}
			this.Close();
		}

		// Token: 0x06001A33 RID: 6707 RVA: 0x00078991 File Offset: 0x00076B91
		private void HandleConfirmSelectionButtonInteractability(object model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleConfirmSelectionButtonInteractability", new object[] { model });
			}
			this.confirmSelectionButton.interactable = this.iEnumerablePresenter.SelectedItemsList.Count > 0;
		}

		// Token: 0x040014EA RID: 5354
		[Header("UIUserSearchWindowController")]
		[SerializeField]
		private UIUserSearchWindowView view;

		// Token: 0x040014EB RID: 5355
		[SerializeField]
		private UIInputField userNameSearchInputField;

		// Token: 0x040014EC RID: 5356
		[SerializeField]
		private UIUserPaginatedGraphQlIEnumerableHandler userPaginatedGraphQlIEnumerableHandler;

		// Token: 0x040014ED RID: 5357
		[SerializeField]
		private UIIEnumerablePresenter iEnumerablePresenter;

		// Token: 0x040014EE RID: 5358
		[SerializeField]
		private UIButton confirmSelectionButton;
	}
}
