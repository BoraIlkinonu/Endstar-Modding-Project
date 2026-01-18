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

namespace Endless.Creator.DynamicPropCreation;

public abstract class PropCreationScreenData : PropCreationData
{
	[SerializeField]
	private Prop prop;

	[SerializeField]
	private Script script;

	[SerializeField]
	private BaseTypeDefinition baseType;

	[SerializeField]
	private List<ComponentDefinition> components;

	public override bool IsSubMenu => false;

	public string DefaultName => prop.Name;

	public string DefaultDescription => prop.Description;

	protected async Task<Prop> UploadProp(Prop newProp, Script newScript, Texture2D capturedIconTexture, bool shareWithGameOwners)
	{
		FileUploadResult fileUploadResult = await UploadIcon(base.name + " Icon", capturedIconTexture);
		if (fileUploadResult.HasErrors)
		{
			throw fileUploadResult.Exception;
		}
		newProp.IconFileInstanceId = fileUploadResult.FileInstanceId;
		GraphQlResult graphQlResult = await EndlessServices.Instance.CloudService.CreateAssetAsync(newScript);
		if (graphQlResult.HasErrors)
		{
			throw graphQlResult.GetErrorMessage();
		}
		GraphQlResult graphQlResult2 = await EndlessServices.Instance.CloudService.CreateAssetAsync(newProp);
		if (graphQlResult2.HasErrors)
		{
			throw graphQlResult2.GetErrorMessage();
		}
		Prop finalProp = JsonConvert.DeserializeObject<Prop>(graphQlResult2.GetDataMember().ToString());
		if (shareWithGameOwners)
		{
			GetAllRolesResult obj = await MonoBehaviourSingleton<RightsManager>.Instance.GetAllUserRolesForAssetAsync(MonoBehaviourSingleton<GameEditor>.Instance.ActiveGame.AssetID);
			List<Task<SetUserRoleForAssetResult>> list = new List<Task<SetUserRoleForAssetResult>>();
			foreach (UserRole role in obj.Roles)
			{
				if (role.UserId != EndlessServices.Instance.CloudService.ActiveUserId)
				{
					list.Add(MonoBehaviourSingleton<RightsManager>.Instance.SetRightForUserOnAsset(finalProp.AssetID, role.UserId, role.Role.IsGreaterThanOrEqualTo(Roles.Editor) ? Roles.Editor : Roles.Viewer));
					list.Add(MonoBehaviourSingleton<RightsManager>.Instance.SetRightForUserOnAsset(newScript.AssetID, role.UserId, role.Role.IsGreaterThanOrEqualTo(Roles.Editor) ? Roles.Editor : Roles.Viewer));
				}
			}
			await Task.WhenAll(list);
		}
		await MonoBehaviourSingleton<GameEditor>.Instance.AddPropToGameLibrary(finalProp);
		return finalProp;
	}

	protected void SetupComponents(string name, string description, out Script newScript, out Prop newProp)
	{
		newScript = script.Clone();
		newProp = prop.Clone();
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
		newProp.SetComponentIds(baseType.ComponentId, ((IEnumerable<ComponentDefinition>)components).Select((Func<ComponentDefinition, string>)((ComponentDefinition component) => component.ComponentId)).ToList());
		newScript.Name = name + " Script";
		newScript.Description = description;
		newScript.AssetID = newProp.ScriptAsset.AssetID;
		newScript.AssetVersion = "0.0.0";
		newScript.AssetType = "script";
		newScript.InternalVersion = Script.INTERNAL_VERSION.ToString();
		newScript.SetComponentIds(baseType.ComponentId, ((IEnumerable<ComponentDefinition>)components).Select((Func<ComponentDefinition, string>)((ComponentDefinition component) => component.ComponentId)).ToList());
	}

	private async Task<FileUploadResult> UploadIcon(string name, Texture2D capturedIcon)
	{
		byte[] bytes = ((capturedIcon != null) ? capturedIcon.EncodeToPNG() : base.Icon.texture.EncodeToPNG());
		FileUploadData fileUploadData = new FileUploadData
		{
			Bytes = bytes,
			Filename = name,
			MimeType = "image/png"
		};
		return await CloudUploader.UploadFileBytesAsync(EndlessServices.Instance.CloudService, fileUploadData);
	}
}
