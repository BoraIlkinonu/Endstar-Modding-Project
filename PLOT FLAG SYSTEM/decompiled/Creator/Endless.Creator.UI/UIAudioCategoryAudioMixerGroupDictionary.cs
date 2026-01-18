using Endless.Props.Assets;
using Endless.Shared;
using UnityEngine;
using UnityEngine.Audio;

namespace Endless.Creator.UI;

[CreateAssetMenu(menuName = "ScriptableObject/UI/Creator/Dictionaries/Audio Category Audio Mixer Group Dictionary", fileName = "Audio Category Audio Mixer Group Dictionary")]
public class UIAudioCategoryAudioMixerGroupDictionary : BaseEnumKeyScriptableObjectDictionary<AudioCategory, AudioMixerGroup>
{
}
