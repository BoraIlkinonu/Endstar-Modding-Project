using System;
using System.Threading;
using System.Threading.Tasks;
using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIUserRoleListCellView : UIBaseListCellView<UserRole>, IUILoadingSpinnerViewCompatible
{
	[Header("UIUserRoleListCellView")]
	[SerializeField]
	private TextMeshProUGUI userNameText;

	[SerializeField]
	private GameObject inheritedVisual;

	[SerializeField]
	private TextMeshProUGUI roleText;

	[SerializeField]
	private UIButton changeRoleButton;

	private CancellationTokenSource initializeCancellationTokenSource;

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	public string UserName { get; private set; }

	public override void OnDespawn()
	{
		base.OnDespawn();
		UserName = string.Empty;
		userNameText.enabled = false;
		CancellationTokenSourceUtility.CancelAndCleanup(ref initializeCancellationTokenSource);
	}

	public override void View(UIBaseListView<UserRole> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		if (!IsAddButton)
		{
			ViewAsync();
		}
	}

	private async Task ViewAsync()
	{
		if (base.VerboseLogging)
		{
			Debug.Log("ViewAsync", this);
		}
		CancellationTokenSourceUtility.RecreateTokenSource(ref initializeCancellationTokenSource);
		CancellationToken cancellationToken = initializeCancellationTokenSource.Token;
		OnLoadingStarted.Invoke();
		try
		{
			bool isLocalClient = EndlessServices.Instance.CloudService.ActiveUserId == base.Model.UserId;
			UIUserRoleListModel typedListModel = (UIUserRoleListModel)base.ListModel;
			Roles localClientRole = typedListModel.UserRolesModel.LocalClientRole;
			bool localClientRoleIsGreater = localClientRole.IsGreaterThan(base.Model.Role);
			UIUserRolesModel userRolesModel = typedListModel.UserRolesModel;
			UIUserRoleWizard.AssetTypes assetType = userRolesModel.AssetType;
			bool isForProp = assetType == UIUserRoleWizard.AssetTypes.Prop;
			string text = await MonoBehaviourSingleton<RuntimeDatabase>.Instance.GetUserNameAsync(base.Model.UserId, cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();
			UserName = text;
			userNameText.text = text;
			userNameText.enabled = true;
			inheritedVisual.gameObject.SetActive(base.Model.InheritedFromParent);
			roleText.text = base.Model.Role.ToString();
			bool flag = isForProp || ((userRolesModel.AssetContext != AssetContexts.NewGame) ? (userRolesModel.AssetContext.CanEditRoles() && GetCanLocalClientChangeRole(isLocalClient, localClientRole, typedListModel, localClientRoleIsGreater)) : (!isLocalClient));
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0}: {1}, ", "localClientRole", localClientRole) + string.Format("{0}: {1}, ", "isLocalClient", isLocalClient) + string.Format("{0}: {1}, ", "localClientRoleIsGreater", localClientRoleIsGreater) + string.Format("{0}: {1}", "localClientCanChangeRoleOfThisUser", flag), this);
			}
			changeRoleButton.interactable = flag && typedListModel.UserRolesModel.AssetContext.CanEditRoles();
		}
		catch (OperationCanceledException)
		{
			if (base.VerboseLogging)
			{
				Debug.Log("ViewAsync was cancelled.", this);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, this);
		}
		finally
		{
			OnLoadingEnded.Invoke();
		}
	}

	private bool GetCanLocalClientChangeRole(bool isLocalClient, Roles localClientRole, UIUserRoleListModel userRoleListModel, bool localClientRoleIsGreater)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "GetCanLocalClientChangeRole", isLocalClient, localClientRole, userRoleListModel, localClientRoleIsGreater);
		}
		if (isLocalClient)
		{
			if (localClientRole == Roles.Owner)
			{
				return userRoleListModel.OwnerCount > 1;
			}
			return true;
		}
		return localClientRole.IsGreaterThan(Roles.Viewer) && localClientRoleIsGreater;
	}
}
