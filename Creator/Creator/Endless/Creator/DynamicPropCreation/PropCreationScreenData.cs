using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Endless.Assets;
using Endless.Gameplay;
using Endless.Gameplay.RightsManagement;
using Endless.GraphQl;
using Endless.Props.Assets;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Endless.Creator.DynamicPropCreation
{
	// Token: 0x020003B6 RID: 950
	public abstract class PropCreationScreenData : PropCreationData
	{
		// Token: 0x170002AB RID: 683
		// (get) Token: 0x06001288 RID: 4744 RVA: 0x0001BF89 File Offset: 0x0001A189
		public override bool IsSubMenu
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170002AC RID: 684
		// (get) Token: 0x06001289 RID: 4745 RVA: 0x0005F5F5 File Offset: 0x0005D7F5
		public string DefaultName
		{
			get
			{
				return this.prop.Name;
			}
		}

		// Token: 0x170002AD RID: 685
		// (get) Token: 0x0600128A RID: 4746 RVA: 0x0005F602 File Offset: 0x0005D802
		public string DefaultDescription
		{
			get
			{
				return this.prop.Description;
			}
		}

		// Token: 0x0600128B RID: 4747 RVA: 0x0005F610 File Offset: 0x0005D810
		protected async Task<Prop> UploadProp(Prop newProp, Script newScript, Texture2D capturedIconTexture, bool shareWithGameOwners)
		{
			FileUploadResult fileUploadResult = await this.UploadIcon(base.name + " Icon", capturedIconTexture);
			if (fileUploadResult.HasErrors)
			{
				throw fileUploadResult.Exception;
			}
			newProp.IconFileInstanceId = fileUploadResult.FileInstanceId;
			GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.CreateAssetAsync(newScript, false, 10);
			if (graphQlResult.HasErrors)
			{
				throw graphQlResult.GetErrorMessage(0);
			}
			GraphQlResult graphQlResult2 = await EndlessServices.Instance.CloudService.CreateAssetAsync(newProp, false, 10);
			if (graphQlResult2.HasErrors)
			{
				throw graphQlResult2.GetErrorMessage(0);
			}
			Prop finalProp = JsonConvert.DeserializeObject<Prop>(graphQlResult2.GetDataMember().ToString());
			if (shareWithGameOwners)
			{
				ref GetAllRolesResult ptr = await MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.AssetID, null, false);
				List<Task<SetUserRoleForAssetResult>> list = new List<Task<SetUserRoleForAssetResult>>();
				foreach (UserRole userRole in ptr.Roles)
				{
					if (userRole.UserId != EndlessServices.Instance.CloudService.ActiveUserId)
					{
						list.Add(MonoBehaviourSingleton<RightsManager>.Instance.SetRightForUserOnAsset(finalProp.AssetID, userRole.UserId, userRole.Role.IsGreaterThanOrEqualTo(Roles.Editor) ? Roles.Editor : Roles.Viewer));
						list.Add(MonoBehaviourSingleton<RightsManager>.Instance.SetRightForUserOnAsset(newScript.AssetID, userRole.UserId, userRole.Role.IsGreaterThanOrEqualTo(Roles.Editor) ? Roles.Editor : Roles.Viewer));
					}
				}
				await Task.WhenAll<SetUserRoleForAssetResult>(list);
			}
			await MonoBehaviourSingleton<GameEditor>.Instance.AddPropToGameLibrary(finalProp);
			return finalProp;
		}

		// Token: 0x0600128C RID: 4748 RVA: 0x0005F674 File Offset: 0x0005D874
		protected void SetupComponents(string name, string description, out Script newScript, out Prop newProp)
		{
			newScript = this.script.Clone();
			newProp = this.prop.Clone();
			newProp.Name = name;
			newProp.Description = description;
			newProp.AssetID = SerializableGuid.NewGuid();
			newProp.AssetVersion = "0.0.0";
			newProp.AssetType = "prop";
			newProp.InternalVersion = Prop.INTERNAL_VERSION.ToString();
			newProp.ApplyXOffset = false;
			newProp.ApplyZOffset = false;
			newProp.ScriptAsset = new AssetReference
			{
				AssetID = SerializableGuid.NewGuid(),
				AssetVersion = "0.0.0",
				AssetType = "script"
			};
			newProp.SetComponentIds(this.baseType.ComponentId, this.components.Select((ComponentDefinition component) => component.ComponentId).ToList<string>());
			newScript.Name = name + " Script";
			newScript.Description = description;
			newScript.AssetID = newProp.ScriptAsset.AssetID;
			newScript.AssetVersion = "0.0.0";
			newScript.AssetType = "script";
			newScript.InternalVersion = Script.INTERNAL_VERSION.ToString();
			newScript.SetComponentIds(this.baseType.ComponentId, this.components.Select((ComponentDefinition component) => component.ComponentId).ToList<string>());
		}

		// Token: 0x0600128D RID: 4749 RVA: 0x0005F810 File Offset: 0x0005DA10
		private async Task<FileUploadResult> UploadIcon(string name, Texture2D capturedIcon)
		{
			byte[] array = ((capturedIcon != null) ? capturedIcon.EncodeToPNG() : base.Icon.texture.EncodeToPNG());
			FileUploadData fileUploadData = new FileUploadData
			{
				Bytes = array,
				Filename = name,
				MimeType = "image/png"
			};
			return await CloudUploader.UploadFileBytesAsync(EndlessServices.Instance.CloudService, fileUploadData, "endstar");
		}

		// Token: 0x04000F49 RID: 3913
		[SerializeField]
		private Prop prop;

		// Token: 0x04000F4A RID: 3914
		[SerializeField]
		private Script script;

		// Token: 0x04000F4B RID: 3915
		[SerializeField]
		private BaseTypeDefinition baseType;

		// Token: 0x04000F4C RID: 3916
		[SerializeField]
		private List<ComponentDefinition> components;
	}
}
