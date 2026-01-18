using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using Endless.Shared;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class TextComponent : EndlessNetworkBehaviour, IStartSubscriber, IComponentBase, IScriptInjector
{
	[SerializeField]
	[HideInInspector]
	private TextMeshPro textObject;

	[SerializeField]
	private InlineTextSpanFinder inlineTextSpanFinder;

	[SerializeField]
	private LocalizedString text;

	[SerializeField]
	private TMP_SpriteAsset spriteAsset;

	private readonly NetworkVariable<bool> display = new NetworkVariable<bool>(value: true);

	private readonly NetworkVariable<UnityEngine.Color> color = new NetworkVariable<UnityEngine.Color>(UnityEngine.Color.black);

	private readonly NetworkVariable<float> alpha = new NetworkVariable<float>(1f);

	private readonly NetworkVariable<TextAlignmentOptions> textAlignment = new NetworkVariable<TextAlignmentOptions>(TextAlignmentOptions.Center);

	private readonly NetworkVariable<LocalizedString> runtimeText = new NetworkVariable<LocalizedString>(new LocalizedString());

	private readonly NetworkVariable<float> characterSpacing = new NetworkVariable<float>(0f);

	private readonly NetworkVariable<float> lineSpacing = new NetworkVariable<float>(0f);

	private readonly NetworkVariable<float> fontSize = new NetworkVariable<float>(0.2f);

	private readonly NetworkVariable<bool> useAutoSizing = new NetworkVariable<bool>(value: true);

	private readonly NetworkVariable<float> minFontSize = new NetworkVariable<float>(1f);

	private readonly NetworkVariable<float> maxFontSize = new NetworkVariable<float>(72f);

	internal EndlessScriptComponent scriptComponent;

	private Text luaTextInterface;

	internal LocalizedString Text
	{
		get
		{
			return text;
		}
		set
		{
			if (text != value)
			{
				text = value;
				textObject.text = text.GetLocalizedString();
			}
		}
	}

	internal UnityEngine.Color Color
	{
		get
		{
			return color.Value;
		}
		set
		{
			color.Value = value;
		}
	}

	internal TextAlignmentOptions TextAlignmentOptions
	{
		get
		{
			return textAlignment.Value;
		}
		set
		{
			textAlignment.Value = value;
		}
	}

	internal LocalizedString RuntimeText
	{
		get
		{
			return runtimeText.Value;
		}
		set
		{
			runtimeText.Value = value;
		}
	}

	internal float Alpha
	{
		get
		{
			return alpha.Value;
		}
		set
		{
			alpha.Value = value;
		}
	}

	internal float CharacterSpacing
	{
		get
		{
			return characterSpacing.Value;
		}
		set
		{
			characterSpacing.Value = value;
		}
	}

	internal float LineSpacing
	{
		get
		{
			return lineSpacing.Value;
		}
		set
		{
			lineSpacing.Value = value;
		}
	}

	internal bool DisplayText
	{
		get
		{
			return display.Value;
		}
		set
		{
			display.Value = value;
		}
	}

	internal bool EnableAutoSizing
	{
		get
		{
			return useAutoSizing.Value;
		}
		set
		{
			useAutoSizing.Value = value;
		}
	}

	internal float FontSize
	{
		get
		{
			return fontSize.Value;
		}
		set
		{
			fontSize.Value = value;
		}
	}

	internal float MinFontSize
	{
		get
		{
			return minFontSize.Value;
		}
		set
		{
			minFontSize.Value = value;
		}
	}

	internal float MaxFontSize
	{
		get
		{
			return maxFontSize.Value;
		}
		set
		{
			maxFontSize.Value = value;
		}
	}

	[field: SerializeField]
	public WorldObject WorldObject { get; private set; }

	public Type ComponentReferenceType => typeof(TextReferences);

	public object LuaObject => luaTextInterface ?? (luaTextInterface = new Text(this));

	public Type LuaObjectType => typeof(Text);

	public List<Type> EnumTypes => new List<Type> { typeof(TextAlignmentOptions) };

	public override void OnNetworkSpawn()
	{
		textObject.text = Text.GetLocalizedString();
		LocalizedString localizedString = text;
		localizedString.OnTextChanged = (Action<Language>)Delegate.Combine(localizedString.OnTextChanged, (Action<Language>)delegate
		{
			textObject.text = text.GetLocalizedString();
		});
		textObject.color = Color;
		textObject.alignment = TextAlignmentOptions;
		textObject.alpha = Alpha;
		textObject.characterSpacing = CharacterSpacing;
		textObject.lineSpacing = LineSpacing;
		textObject.enabled = DisplayText;
		textObject.enableAutoSizing = EnableAutoSizing;
		textObject.fontSize = FontSize;
		textObject.fontSizeMin = MinFontSize;
		textObject.fontSizeMax = MaxFontSize;
	}

	private void OnTextValueChanged(LocalizedString previousValue, LocalizedString newValue)
	{
		if (previousValue != null)
		{
			previousValue.OnTextChanged = (Action<Language>)Delegate.Remove(previousValue.OnTextChanged, new Action<Language>(OnTextChanged));
		}
		if (newValue != null)
		{
			textObject.text = newValue.GetLocalizedString();
			newValue.OnTextChanged = (Action<Language>)Delegate.Combine(newValue.OnTextChanged, new Action<Language>(OnTextChanged));
		}
	}

	private void OnMaxFontChanged(float _, float newValue)
	{
		textObject.fontSizeMax = newValue;
	}

	private void OnMinFontChanged(float _, float newValue)
	{
		textObject.fontSizeMin = newValue;
	}

	private void OnFontSizeChanged(float _, float newValue)
	{
		textObject.fontSize = newValue;
	}

	private void OnUseAutoSizingChanged(bool _, bool newValue)
	{
		textObject.enableAutoSizing = newValue;
	}

	private void OnDisplayChanged(bool _, bool newValue)
	{
		textObject.enabled = newValue;
	}

	private void OnLineSpacingChanged(float _, float newValue)
	{
		textObject.lineSpacing = newValue;
	}

	private void OnCharacterSpacingChanged(float _, float newValue)
	{
		textObject.characterSpacing = newValue;
	}

	private void OnAlphaChanged(float _, float newValue)
	{
		textObject.alpha = newValue;
	}

	private void OnTextChanged(Language language)
	{
		textObject.text = RuntimeText.GetLocalizedString();
	}

	private void OnTextAlignmentChanged(TextAlignmentOptions _, TextAlignmentOptions newValue)
	{
		textObject.alignment = newValue;
	}

	private void OnColorChanged(UnityEngine.Color _, UnityEngine.Color newValue)
	{
		textObject.color = newValue;
	}

	public void SetAlpha(float newAlpha)
	{
		newAlpha = Mathf.Clamp01(newAlpha);
		Alpha = newAlpha;
	}

	public void SetText(Context context, LocalizedString newText)
	{
		RuntimeText = newText;
		textObject.text = RuntimeText.GetLocalizedString();
	}

	public void SetRawText(Context context, string newText)
	{
		LocalizedString localizedString = new LocalizedString();
		localizedString.SetStringValue(newText, LocalizedString.ActiveLanguage);
		RuntimeText = localizedString;
	}

	public void EndlessStart()
	{
		if (base.IsServer)
		{
			runtimeText.Value = Text;
		}
		NetworkVariable<UnityEngine.Color> networkVariable = color;
		networkVariable.OnValueChanged = (NetworkVariable<UnityEngine.Color>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<UnityEngine.Color>.OnValueChangedDelegate(OnColorChanged));
		NetworkVariable<TextAlignmentOptions> networkVariable2 = textAlignment;
		networkVariable2.OnValueChanged = (NetworkVariable<TextAlignmentOptions>.OnValueChangedDelegate)Delegate.Combine(networkVariable2.OnValueChanged, new NetworkVariable<TextAlignmentOptions>.OnValueChangedDelegate(OnTextAlignmentChanged));
		LocalizedString localizedString = RuntimeText;
		localizedString.OnTextChanged = (Action<Language>)Delegate.Combine(localizedString.OnTextChanged, new Action<Language>(OnTextChanged));
		NetworkVariable<LocalizedString> networkVariable3 = runtimeText;
		networkVariable3.OnValueChanged = (NetworkVariable<LocalizedString>.OnValueChangedDelegate)Delegate.Combine(networkVariable3.OnValueChanged, new NetworkVariable<LocalizedString>.OnValueChangedDelegate(OnTextValueChanged));
		NetworkVariable<float> networkVariable4 = alpha;
		networkVariable4.OnValueChanged = (NetworkVariable<float>.OnValueChangedDelegate)Delegate.Combine(networkVariable4.OnValueChanged, new NetworkVariable<float>.OnValueChangedDelegate(OnAlphaChanged));
		NetworkVariable<float> networkVariable5 = characterSpacing;
		networkVariable5.OnValueChanged = (NetworkVariable<float>.OnValueChangedDelegate)Delegate.Combine(networkVariable5.OnValueChanged, new NetworkVariable<float>.OnValueChangedDelegate(OnCharacterSpacingChanged));
		NetworkVariable<float> networkVariable6 = lineSpacing;
		networkVariable6.OnValueChanged = (NetworkVariable<float>.OnValueChangedDelegate)Delegate.Combine(networkVariable6.OnValueChanged, new NetworkVariable<float>.OnValueChangedDelegate(OnLineSpacingChanged));
		NetworkVariable<bool> networkVariable7 = display;
		networkVariable7.OnValueChanged = (NetworkVariable<bool>.OnValueChangedDelegate)Delegate.Combine(networkVariable7.OnValueChanged, new NetworkVariable<bool>.OnValueChangedDelegate(OnDisplayChanged));
		NetworkVariable<bool> networkVariable8 = useAutoSizing;
		networkVariable8.OnValueChanged = (NetworkVariable<bool>.OnValueChangedDelegate)Delegate.Combine(networkVariable8.OnValueChanged, new NetworkVariable<bool>.OnValueChangedDelegate(OnUseAutoSizingChanged));
		NetworkVariable<float> networkVariable9 = fontSize;
		networkVariable9.OnValueChanged = (NetworkVariable<float>.OnValueChangedDelegate)Delegate.Combine(networkVariable9.OnValueChanged, new NetworkVariable<float>.OnValueChangedDelegate(OnFontSizeChanged));
		NetworkVariable<float> networkVariable10 = minFontSize;
		networkVariable10.OnValueChanged = (NetworkVariable<float>.OnValueChangedDelegate)Delegate.Combine(networkVariable10.OnValueChanged, new NetworkVariable<float>.OnValueChangedDelegate(OnMinFontChanged));
		NetworkVariable<float> networkVariable11 = maxFontSize;
		networkVariable11.OnValueChanged = (NetworkVariable<float>.OnValueChangedDelegate)Delegate.Combine(networkVariable11.OnValueChanged, new NetworkVariable<float>.OnValueChangedDelegate(OnMaxFontChanged));
	}

	public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
	{
		TextReferences textReferences = (TextReferences)referenceBase;
		textObject = textReferences.textObject;
		inlineTextSpanFinder.RegisterTextComponent(textObject);
		textObject.spriteAsset = spriteAsset;
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}

	public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
	{
		scriptComponent = endlessScriptComponent;
	}

	protected override void __initializeVariables()
	{
		if (display == null)
		{
			throw new Exception("TextComponent.display cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		display.Initialize(this);
		__nameNetworkVariable(display, "display");
		NetworkVariableFields.Add(display);
		if (color == null)
		{
			throw new Exception("TextComponent.color cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		color.Initialize(this);
		__nameNetworkVariable(color, "color");
		NetworkVariableFields.Add(color);
		if (alpha == null)
		{
			throw new Exception("TextComponent.alpha cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		alpha.Initialize(this);
		__nameNetworkVariable(alpha, "alpha");
		NetworkVariableFields.Add(alpha);
		if (textAlignment == null)
		{
			throw new Exception("TextComponent.textAlignment cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		textAlignment.Initialize(this);
		__nameNetworkVariable(textAlignment, "textAlignment");
		NetworkVariableFields.Add(textAlignment);
		if (runtimeText == null)
		{
			throw new Exception("TextComponent.runtimeText cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		runtimeText.Initialize(this);
		__nameNetworkVariable(runtimeText, "runtimeText");
		NetworkVariableFields.Add(runtimeText);
		if (characterSpacing == null)
		{
			throw new Exception("TextComponent.characterSpacing cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		characterSpacing.Initialize(this);
		__nameNetworkVariable(characterSpacing, "characterSpacing");
		NetworkVariableFields.Add(characterSpacing);
		if (lineSpacing == null)
		{
			throw new Exception("TextComponent.lineSpacing cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		lineSpacing.Initialize(this);
		__nameNetworkVariable(lineSpacing, "lineSpacing");
		NetworkVariableFields.Add(lineSpacing);
		if (fontSize == null)
		{
			throw new Exception("TextComponent.fontSize cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		fontSize.Initialize(this);
		__nameNetworkVariable(fontSize, "fontSize");
		NetworkVariableFields.Add(fontSize);
		if (useAutoSizing == null)
		{
			throw new Exception("TextComponent.useAutoSizing cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		useAutoSizing.Initialize(this);
		__nameNetworkVariable(useAutoSizing, "useAutoSizing");
		NetworkVariableFields.Add(useAutoSizing);
		if (minFontSize == null)
		{
			throw new Exception("TextComponent.minFontSize cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		minFontSize.Initialize(this);
		__nameNetworkVariable(minFontSize, "minFontSize");
		NetworkVariableFields.Add(minFontSize);
		if (maxFontSize == null)
		{
			throw new Exception("TextComponent.maxFontSize cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		maxFontSize.Initialize(this);
		__nameNetworkVariable(maxFontSize, "maxFontSize");
		NetworkVariableFields.Add(maxFontSize);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected internal override string __getTypeName()
	{
		return "TextComponent";
	}
}
