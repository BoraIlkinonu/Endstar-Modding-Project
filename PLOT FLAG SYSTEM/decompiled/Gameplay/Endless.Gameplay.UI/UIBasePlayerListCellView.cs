using System;
using System.Threading;
using System.Threading.Tasks;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Endless.Gameplay.UI;

public class UIBasePlayerListCellView : UIBaseListCellView<PlayerReferenceManager>, IUILoadingSpinnerViewCompatible
{
	[Header("UIBasePlayerListCellView")]
	[SerializeField]
	private TextMeshProUGUI userSlotText;

	[SerializeField]
	private UICharacterCosmeticsDefinitionPortraitView characterCosmeticsDefinitionPortrait;

	[SerializeField]
	private TextMeshProUGUI displayNameText;

	[SerializeField]
	private TweenCollection onInitializeTweens;

	[SerializeField]
	private Color textColorIfLocalClient = Color.white;

	[FormerlySerializedAs("textdColorIfNotLocalClient")]
	[SerializeField]
	private Color textColorIfNotLocalClient = Color.blue;

	[Header("Background")]
	[SerializeField]
	private Image backgroundImage;

	[SerializeField]
	private Color backgroundColorIfLocalClient = Color.blue;

	[SerializeField]
	private Color backgroundColorIfNotLocalClient = Color.black;

	private CancellationTokenSource viewAsyncCancellationTokenSource;

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	protected PlayerReferenceManager PlayerReferenceManager { get; private set; }

	public override void OnDespawn()
	{
		base.OnDespawn();
		CancellationTokenSourceUtility.CancelAndCleanup(ref viewAsyncCancellationTokenSource);
		if ((bool)PlayerReferenceManager)
		{
			PlayerReferenceManager.OnCharacterCosmeticsChanged.RemoveListener(ViewCharacterCosmeticsDefinition);
			PlayerReferenceManager = null;
		}
	}

	public override void View(UIBaseListView<PlayerReferenceManager> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		ViewAsync();
	}

	private async Task ViewAsync()
	{
		if (base.VerboseLogging)
		{
			Debug.Log("ViewAsync", this);
		}
		CancellationTokenSourceUtility.RecreateTokenSource(ref viewAsyncCancellationTokenSource);
		CancellationToken cancellationToken = viewAsyncCancellationTokenSource.Token;
		OnLoadingStarted.Invoke();
		try
		{
			bool isNewData = PlayerReferenceManager != base.Model;
			PlayerReferenceManager = base.Model;
			userSlotText.text = PlayerReferenceManager.UserSlot.ToString();
			bool flag = PlayerReferenceManager.OwnerClientId == NetworkManager.Singleton.LocalClientId;
			userSlotText.color = (flag ? textColorIfLocalClient : textColorIfNotLocalClient);
			displayNameText.color = (flag ? textColorIfLocalClient : textColorIfNotLocalClient);
			backgroundImage.color = (flag ? backgroundColorIfLocalClient : backgroundColorIfNotLocalClient);
			CharacterCosmeticsDefinition characterCosmetics = PlayerReferenceManager.CharacterCosmetics;
			ViewCharacterCosmeticsDefinition(characterCosmetics);
			ulong ownerClientId = PlayerReferenceManager.OwnerClientId;
			displayNameText.text = "Loading...";
			int userId = await NetworkBehaviourSingleton<UserIdManager>.Instance.GetUserIdAsync(ownerClientId, cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();
			string text = await MonoBehaviourSingleton<RuntimeDatabase>.Instance.GetUserNameAsync(userId, cancellationToken);
			cancellationToken.ThrowIfCancellationRequested();
			displayNameText.text = text;
			PlayerReferenceManager.OnCharacterCosmeticsChanged.AddListener(ViewCharacterCosmeticsDefinition);
			if (isNewData)
			{
				onInitializeTweens.Tween();
			}
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

	private void ViewCharacterCosmeticsDefinition(CharacterCosmeticsDefinition characterCosmeticsDefinition)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ViewCharacterCosmeticsDefinition", "characterCosmeticsDefinition", characterCosmeticsDefinition), this);
		}
		characterCosmeticsDefinitionPortrait.Display(characterCosmeticsDefinition.AssetId);
	}
}
