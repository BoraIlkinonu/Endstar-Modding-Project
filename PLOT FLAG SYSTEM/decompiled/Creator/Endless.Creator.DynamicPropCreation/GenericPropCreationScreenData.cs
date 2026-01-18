using System.Threading.Tasks;
using Endless.Props.Assets;
using UnityEngine;

namespace Endless.Creator.DynamicPropCreation;

[CreateAssetMenu(menuName = "ScriptableObject/Dynamic Prop Creation/GenericPropCreationScreenData")]
public class GenericPropCreationScreenData : PropCreationScreenData
{
	[SerializeField]
	private Texture2D propIcon;

	public Texture2D PropIcon => propIcon;

	public async Task<Prop> UploadProp(string name, string description, bool shareWithGameOwners)
	{
		SetupComponents(name, description, out var newScript, out var newProp);
		return await UploadProp(newProp, newScript, propIcon, shareWithGameOwners);
	}
}
