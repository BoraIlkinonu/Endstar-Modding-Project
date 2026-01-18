using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.RightsManagement;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Gameplay.LevelEditing;
using Runtime.Shared.Matchmaking;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000245 RID: 581
	public class UIRuntimePropInfoDetailView : UIGameObject, IUIViewable<PropLibrary.RuntimePropInfo>, IClearable, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x17000133 RID: 307
		// (get) Token: 0x0600096C RID: 2412 RVA: 0x0002BF5B File Offset: 0x0002A15B
		// (set) Token: 0x0600096D RID: 2413 RVA: 0x0002BF63 File Offset: 0x0002A163
		public UIRuntimePropInfoDetailView.Modes Mode { get; private set; }

		// Token: 0x17000134 RID: 308
		// (get) Token: 0x0600096E RID: 2414 RVA: 0x0002BF6C File Offset: 0x0002A16C
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x17000135 RID: 309
		// (get) Token: 0x0600096F RID: 2415 RVA: 0x0002BF74 File Offset: 0x0002A174
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x17000136 RID: 310
		// (get) Token: 0x06000970 RID: 2416 RVA: 0x0002BF7C File Offset: 0x0002A17C
		// (set) Token: 0x06000971 RID: 2417 RVA: 0x0002BF84 File Offset: 0x0002A184
		public PropLibrary.RuntimePropInfo Model { get; private set; }

		// Token: 0x17000137 RID: 311
		// (get) Token: 0x06000972 RID: 2418 RVA: 0x000240F9 File Offset: 0x000222F9
		private int LocalClientUserId
		{
			get
			{
				return EndlessServices.Instance.CloudService.ActiveUserId;
			}
		}

		// Token: 0x06000973 RID: 2419 RVA: 0x0002BF90 File Offset: 0x0002A190
		public async void View(PropLibrary.RuntimePropInfo model)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
			this.Clear();
			this.Model = model;
			this.nameText.text = model.PropData.Name;
			this.iconImage.sprite = model.Icon;
			GameLibrary gameLibrary = MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.GameLibrary;
			this.isInjected = !gameLibrary.PropReferences.Any((AssetReference propReference) => propReference.AssetID == model.PropData.AssetID);
			this.propIsOpenSource = model.EndlessProp.Prop.OpenSource;
			this.scriptIsOpenSource = model.EndlessProp.ScriptComponent.Script.OpenSource;
			this.descriptionText.text = model.PropData.Description;
			if (this.verboseLogging)
			{
				DebugUtility.Log(JsonUtility.ToJson(model), this);
			}
			this.versionText.text = model.PropData.AssetVersion;
			if (!this.isInjected && model.PropData.HasScript)
			{
				this.propId = model.PropData.AssetID;
				this.scriptId = model.PropData.ScriptAsset.AssetID;
				if (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.IsInjectedProp(model.PropData.AssetID))
				{
					this.isSubscribedToRightsManager = true;
					this.canEditProp = false;
					this.canEditScript = false;
					this.HandleEditScriptButtonInteractabilityAndText();
				}
				else
				{
					this.OnLoadingStarted.Invoke();
					GetAllRolesResult[] array = await Task.WhenAll<GetAllRolesResult>(new List<Task<GetAllRolesResult>>
					{
						MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(this.propId, new Action<IReadOnlyList<UserRole>>(this.OnPropRolesUpdated), false),
						MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(this.scriptId, new Action<IReadOnlyList<UserRole>>(this.OnScriptRolesUpdated), false)
					});
					if (array[0].WasChanged)
					{
						this.OnPropRolesUpdated(array[0].Roles);
					}
					if (array[1].WasChanged)
					{
						this.OnScriptRolesUpdated(array[1].Roles);
					}
					this.OnLoadingEnded.Invoke();
					MonoBehaviourSingleton<RightsManager>.Instance.SubscribeToRoleChangeForAsset(this.propId, new Action<IReadOnlyList<UserRole>>(this.OnPropRolesUpdated));
					MonoBehaviourSingleton<RightsManager>.Instance.SubscribeToRoleChangeForAsset(this.scriptId, new Action<IReadOnlyList<UserRole>>(this.OnScriptRolesUpdated));
					this.isSubscribedToRightsManager = true;
				}
			}
		}

		// Token: 0x06000974 RID: 2420 RVA: 0x0002BFD0 File Offset: 0x0002A1D0
		public void Clear()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			this.iconImage.sprite = null;
			this.canEditProp = false;
			this.canEditScript = false;
			this.editScriptButtonText.text = "Edit Script";
			this.editScriptButton.interactable = false;
			this.propId = SerializableGuid.Empty;
			this.scriptId = SerializableGuid.Empty;
			if (!this.isSubscribedToRightsManager)
			{
				return;
			}
			MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(this.propId, new Action<IReadOnlyList<UserRole>>(this.OnPropRolesUpdated));
			MonoBehaviourSingleton<RightsManager>.Instance.UnsubscribeToRoleChangeForAsset(this.scriptId, new Action<IReadOnlyList<UserRole>>(this.OnScriptRolesUpdated));
			this.isSubscribedToRightsManager = false;
		}

		// Token: 0x06000975 RID: 2421 RVA: 0x0002C08C File Offset: 0x0002A28C
		private void OnPropRolesUpdated(IReadOnlyList<UserRole> userRoles)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPropRolesUpdated", new object[] { userRoles });
			}
			Roles roleForUserId = userRoles.GetRoleForUserId(this.LocalClientUserId);
			this.canEditProp = roleForUserId.IsGreaterThanOrEqualTo(Roles.Editor);
			this.HandleEditScriptButtonInteractabilityAndText();
		}

		// Token: 0x06000976 RID: 2422 RVA: 0x0002C0D8 File Offset: 0x0002A2D8
		private void OnScriptRolesUpdated(IReadOnlyList<UserRole> userRoles)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnScriptRolesUpdated", new object[] { userRoles });
			}
			Roles roleForUserId = userRoles.GetRoleForUserId(this.LocalClientUserId);
			this.canEditScript = roleForUserId.IsGreaterThanOrEqualTo(Roles.Editor);
			this.HandleEditScriptButtonInteractabilityAndText();
		}

		// Token: 0x06000977 RID: 2423 RVA: 0x0002C124 File Offset: 0x0002A324
		private void HandleEditScriptButtonInteractabilityAndText()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleEditScriptButtonInteractabilityAndText", Array.Empty<object>());
			}
			bool flag = this.canEditProp && this.canEditScript;
			if (flag)
			{
				this.Mode = UIRuntimePropInfoDetailView.Modes.Write;
			}
			else
			{
				this.Mode = (this.scriptIsOpenSource ? UIRuntimePropInfoDetailView.Modes.Read : UIRuntimePropInfoDetailView.Modes.Restricted);
			}
			this.editScriptButtonText.text = (flag ? "Edit Script" : "View Script");
			Selectable selectable = this.editScriptButton;
			UIRuntimePropInfoDetailView.Modes mode = this.Mode;
			selectable.interactable = mode == UIRuntimePropInfoDetailView.Modes.Write || mode == UIRuntimePropInfoDetailView.Modes.Read;
			this.cantEditScriptTooltip.SetActive(!flag);
		}

		// Token: 0x040007BE RID: 1982
		private const string EDIT_SCRIPT_LABEL = "Edit Script";

		// Token: 0x040007BF RID: 1983
		private const string VIEW_SCRIPT_LABEL = "View Script";

		// Token: 0x040007C0 RID: 1984
		[Header("UIRuntimePropInfoDetailView")]
		[SerializeField]
		private Image iconImage;

		// Token: 0x040007C1 RID: 1985
		[SerializeField]
		private TextMeshProUGUI nameText;

		// Token: 0x040007C2 RID: 1986
		[SerializeField]
		private TextMeshProUGUI descriptionText;

		// Token: 0x040007C3 RID: 1987
		[SerializeField]
		private UIButton editScriptButton;

		// Token: 0x040007C4 RID: 1988
		[SerializeField]
		private TextMeshProUGUI editScriptButtonText;

		// Token: 0x040007C5 RID: 1989
		[SerializeField]
		private GameObject cantEditScriptTooltip;

		// Token: 0x040007C6 RID: 1990
		[SerializeField]
		private TextMeshProUGUI versionText;

		// Token: 0x040007C7 RID: 1991
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040007C8 RID: 1992
		private bool isInjected;

		// Token: 0x040007C9 RID: 1993
		private bool propIsOpenSource;

		// Token: 0x040007CA RID: 1994
		private bool scriptIsOpenSource;

		// Token: 0x040007CB RID: 1995
		private bool canEditProp;

		// Token: 0x040007CC RID: 1996
		private bool canEditScript;

		// Token: 0x040007CD RID: 1997
		private SerializableGuid propId;

		// Token: 0x040007CE RID: 1998
		private SerializableGuid scriptId;

		// Token: 0x040007CF RID: 1999
		private bool isSubscribedToRightsManager;

		// Token: 0x02000246 RID: 582
		public enum Modes
		{
			// Token: 0x040007D5 RID: 2005
			Write,
			// Token: 0x040007D6 RID: 2006
			Read,
			// Token: 0x040007D7 RID: 2007
			Restricted
		}
	}
}
