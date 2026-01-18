using System.Threading.Tasks;
using Endless.Gameplay;
using Endless.Props.Assets;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Creator.DynamicPropCreation;

[CreateAssetMenu(menuName = "ScriptableObject/Dynamic Prop Creation/AbstractPropCreationScreenData")]
public class AbstractPropCreationScreenData : PropCreationScreenData
{
	[SerializeField]
	private IconDefinition defaultAbstractIcon;

	public string DefaultIconGuid => defaultAbstractIcon.IconId;

	public async Task<Prop> UploadProp(string name, string description, bool shareWithGameOwners, SerializableGuid iconId, Texture2D capturedIconTexture)
	{
		SetupComponents(name, description, out var newScript, out var newProp);
		newProp.SetPropMetaData("AbstractIcon", iconId);
		return await UploadProp(newProp, newScript, capturedIconTexture, shareWithGameOwners);
	}
}
