using UnityEngine;

namespace Endless.Creator.UI;

[CreateAssetMenu(menuName = "ScriptableObject/UI/Creator/Dictionaries/Wire Shader Properties Dictionary")]
public class UIWireShaderPropertiesDictionary : ScriptableObject
{
	[field: SerializeField]
	public string Texture { get; private set; } = "_Texture";

	[field: SerializeField]
	public string Tiling { get; private set; } = "_Tiling";

	[field: SerializeField]
	public string Flip { get; private set; } = "_Flip";

	[field: SerializeField]
	public string StartColor { get; private set; } = "_Start_Color";

	[field: SerializeField]
	public string EndColor { get; private set; } = "_End_Color";

	[field: SerializeField]
	public string MainTex { get; private set; } = "_MainTex";
}
