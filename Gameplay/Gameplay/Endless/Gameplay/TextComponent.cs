using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using Endless.Shared;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000360 RID: 864
	public class TextComponent : EndlessNetworkBehaviour, IStartSubscriber, IComponentBase, IScriptInjector
	{
		// Token: 0x170004A6 RID: 1190
		// (get) Token: 0x06001609 RID: 5641 RVA: 0x00068387 File Offset: 0x00066587
		// (set) Token: 0x0600160A RID: 5642 RVA: 0x0006838F File Offset: 0x0006658F
		internal LocalizedString Text
		{
			get
			{
				return this.text;
			}
			set
			{
				if (this.text != value)
				{
					this.text = value;
					this.textObject.text = this.text.GetLocalizedString();
				}
			}
		}

		// Token: 0x170004A7 RID: 1191
		// (get) Token: 0x0600160B RID: 5643 RVA: 0x000683B7 File Offset: 0x000665B7
		// (set) Token: 0x0600160C RID: 5644 RVA: 0x000683C4 File Offset: 0x000665C4
		internal global::UnityEngine.Color Color
		{
			get
			{
				return this.color.Value;
			}
			set
			{
				this.color.Value = value;
			}
		}

		// Token: 0x170004A8 RID: 1192
		// (get) Token: 0x0600160D RID: 5645 RVA: 0x000683D2 File Offset: 0x000665D2
		// (set) Token: 0x0600160E RID: 5646 RVA: 0x000683DF File Offset: 0x000665DF
		internal TextAlignmentOptions TextAlignmentOptions
		{
			get
			{
				return this.textAlignment.Value;
			}
			set
			{
				this.textAlignment.Value = value;
			}
		}

		// Token: 0x170004A9 RID: 1193
		// (get) Token: 0x0600160F RID: 5647 RVA: 0x000683ED File Offset: 0x000665ED
		// (set) Token: 0x06001610 RID: 5648 RVA: 0x000683FA File Offset: 0x000665FA
		internal LocalizedString RuntimeText
		{
			get
			{
				return this.runtimeText.Value;
			}
			set
			{
				this.runtimeText.Value = value;
			}
		}

		// Token: 0x170004AA RID: 1194
		// (get) Token: 0x06001611 RID: 5649 RVA: 0x00068408 File Offset: 0x00066608
		// (set) Token: 0x06001612 RID: 5650 RVA: 0x00068415 File Offset: 0x00066615
		internal float Alpha
		{
			get
			{
				return this.alpha.Value;
			}
			set
			{
				this.alpha.Value = value;
			}
		}

		// Token: 0x170004AB RID: 1195
		// (get) Token: 0x06001613 RID: 5651 RVA: 0x00068423 File Offset: 0x00066623
		// (set) Token: 0x06001614 RID: 5652 RVA: 0x00068430 File Offset: 0x00066630
		internal float CharacterSpacing
		{
			get
			{
				return this.characterSpacing.Value;
			}
			set
			{
				this.characterSpacing.Value = value;
			}
		}

		// Token: 0x170004AC RID: 1196
		// (get) Token: 0x06001615 RID: 5653 RVA: 0x0006843E File Offset: 0x0006663E
		// (set) Token: 0x06001616 RID: 5654 RVA: 0x0006844B File Offset: 0x0006664B
		internal float LineSpacing
		{
			get
			{
				return this.lineSpacing.Value;
			}
			set
			{
				this.lineSpacing.Value = value;
			}
		}

		// Token: 0x170004AD RID: 1197
		// (get) Token: 0x06001617 RID: 5655 RVA: 0x00068459 File Offset: 0x00066659
		// (set) Token: 0x06001618 RID: 5656 RVA: 0x00068466 File Offset: 0x00066666
		internal bool DisplayText
		{
			get
			{
				return this.display.Value;
			}
			set
			{
				this.display.Value = value;
			}
		}

		// Token: 0x170004AE RID: 1198
		// (get) Token: 0x06001619 RID: 5657 RVA: 0x00068474 File Offset: 0x00066674
		// (set) Token: 0x0600161A RID: 5658 RVA: 0x00068481 File Offset: 0x00066681
		internal bool EnableAutoSizing
		{
			get
			{
				return this.useAutoSizing.Value;
			}
			set
			{
				this.useAutoSizing.Value = value;
			}
		}

		// Token: 0x170004AF RID: 1199
		// (get) Token: 0x0600161B RID: 5659 RVA: 0x0006848F File Offset: 0x0006668F
		// (set) Token: 0x0600161C RID: 5660 RVA: 0x0006849C File Offset: 0x0006669C
		internal float FontSize
		{
			get
			{
				return this.fontSize.Value;
			}
			set
			{
				this.fontSize.Value = value;
			}
		}

		// Token: 0x170004B0 RID: 1200
		// (get) Token: 0x0600161D RID: 5661 RVA: 0x000684AA File Offset: 0x000666AA
		// (set) Token: 0x0600161E RID: 5662 RVA: 0x000684B7 File Offset: 0x000666B7
		internal float MinFontSize
		{
			get
			{
				return this.minFontSize.Value;
			}
			set
			{
				this.minFontSize.Value = value;
			}
		}

		// Token: 0x170004B1 RID: 1201
		// (get) Token: 0x0600161F RID: 5663 RVA: 0x000684C5 File Offset: 0x000666C5
		// (set) Token: 0x06001620 RID: 5664 RVA: 0x000684D2 File Offset: 0x000666D2
		internal float MaxFontSize
		{
			get
			{
				return this.maxFontSize.Value;
			}
			set
			{
				this.maxFontSize.Value = value;
			}
		}

		// Token: 0x06001621 RID: 5665 RVA: 0x000684E0 File Offset: 0x000666E0
		public override void OnNetworkSpawn()
		{
			this.textObject.text = this.Text.GetLocalizedString();
			LocalizedString localizedString = this.text;
			localizedString.OnTextChanged = (Action<Language>)Delegate.Combine(localizedString.OnTextChanged, new Action<Language>(delegate(Language _)
			{
				this.textObject.text = this.text.GetLocalizedString();
			}));
			this.textObject.color = this.Color;
			this.textObject.alignment = this.TextAlignmentOptions;
			this.textObject.alpha = this.Alpha;
			this.textObject.characterSpacing = this.CharacterSpacing;
			this.textObject.lineSpacing = this.LineSpacing;
			this.textObject.enabled = this.DisplayText;
			this.textObject.enableAutoSizing = this.EnableAutoSizing;
			this.textObject.fontSize = this.FontSize;
			this.textObject.fontSizeMin = this.MinFontSize;
			this.textObject.fontSizeMax = this.MaxFontSize;
		}

		// Token: 0x06001622 RID: 5666 RVA: 0x000685D4 File Offset: 0x000667D4
		private void OnTextValueChanged(LocalizedString previousValue, LocalizedString newValue)
		{
			if (previousValue != null)
			{
				previousValue.OnTextChanged = (Action<Language>)Delegate.Remove(previousValue.OnTextChanged, new Action<Language>(this.OnTextChanged));
			}
			if (newValue != null)
			{
				this.textObject.text = newValue.GetLocalizedString();
				newValue.OnTextChanged = (Action<Language>)Delegate.Combine(newValue.OnTextChanged, new Action<Language>(this.OnTextChanged));
			}
		}

		// Token: 0x06001623 RID: 5667 RVA: 0x0006863C File Offset: 0x0006683C
		private void OnMaxFontChanged(float _, float newValue)
		{
			this.textObject.fontSizeMax = newValue;
		}

		// Token: 0x06001624 RID: 5668 RVA: 0x0006864A File Offset: 0x0006684A
		private void OnMinFontChanged(float _, float newValue)
		{
			this.textObject.fontSizeMin = newValue;
		}

		// Token: 0x06001625 RID: 5669 RVA: 0x00068658 File Offset: 0x00066858
		private void OnFontSizeChanged(float _, float newValue)
		{
			this.textObject.fontSize = newValue;
		}

		// Token: 0x06001626 RID: 5670 RVA: 0x00068666 File Offset: 0x00066866
		private void OnUseAutoSizingChanged(bool _, bool newValue)
		{
			this.textObject.enableAutoSizing = newValue;
		}

		// Token: 0x06001627 RID: 5671 RVA: 0x00068674 File Offset: 0x00066874
		private void OnDisplayChanged(bool _, bool newValue)
		{
			this.textObject.enabled = newValue;
		}

		// Token: 0x06001628 RID: 5672 RVA: 0x00068682 File Offset: 0x00066882
		private void OnLineSpacingChanged(float _, float newValue)
		{
			this.textObject.lineSpacing = newValue;
		}

		// Token: 0x06001629 RID: 5673 RVA: 0x00068690 File Offset: 0x00066890
		private void OnCharacterSpacingChanged(float _, float newValue)
		{
			this.textObject.characterSpacing = newValue;
		}

		// Token: 0x0600162A RID: 5674 RVA: 0x0006869E File Offset: 0x0006689E
		private void OnAlphaChanged(float _, float newValue)
		{
			this.textObject.alpha = newValue;
		}

		// Token: 0x0600162B RID: 5675 RVA: 0x000686AC File Offset: 0x000668AC
		private void OnTextChanged(Language language)
		{
			this.textObject.text = this.RuntimeText.GetLocalizedString();
		}

		// Token: 0x0600162C RID: 5676 RVA: 0x000686C4 File Offset: 0x000668C4
		private void OnTextAlignmentChanged(TextAlignmentOptions _, TextAlignmentOptions newValue)
		{
			this.textObject.alignment = newValue;
		}

		// Token: 0x0600162D RID: 5677 RVA: 0x000686D2 File Offset: 0x000668D2
		private void OnColorChanged(global::UnityEngine.Color _, global::UnityEngine.Color newValue)
		{
			this.textObject.color = newValue;
		}

		// Token: 0x0600162E RID: 5678 RVA: 0x000686E0 File Offset: 0x000668E0
		public void SetAlpha(float newAlpha)
		{
			newAlpha = Mathf.Clamp01(newAlpha);
			this.Alpha = newAlpha;
		}

		// Token: 0x0600162F RID: 5679 RVA: 0x000686F1 File Offset: 0x000668F1
		public void SetText(Context context, LocalizedString newText)
		{
			this.RuntimeText = newText;
			this.textObject.text = this.RuntimeText.GetLocalizedString();
		}

		// Token: 0x06001630 RID: 5680 RVA: 0x00068710 File Offset: 0x00066910
		public void SetRawText(Context context, string newText)
		{
			LocalizedString localizedString = new LocalizedString();
			localizedString.SetStringValue(newText, LocalizedString.ActiveLanguage);
			this.RuntimeText = localizedString;
		}

		// Token: 0x06001631 RID: 5681 RVA: 0x00068738 File Offset: 0x00066938
		public void EndlessStart()
		{
			if (base.IsServer)
			{
				this.runtimeText.Value = this.Text;
			}
			NetworkVariable<global::UnityEngine.Color> networkVariable = this.color;
			networkVariable.OnValueChanged = (NetworkVariable<global::UnityEngine.Color>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<global::UnityEngine.Color>.OnValueChangedDelegate(this.OnColorChanged));
			NetworkVariable<TextAlignmentOptions> networkVariable2 = this.textAlignment;
			networkVariable2.OnValueChanged = (NetworkVariable<TextAlignmentOptions>.OnValueChangedDelegate)Delegate.Combine(networkVariable2.OnValueChanged, new NetworkVariable<TextAlignmentOptions>.OnValueChangedDelegate(this.OnTextAlignmentChanged));
			LocalizedString localizedString = this.RuntimeText;
			localizedString.OnTextChanged = (Action<Language>)Delegate.Combine(localizedString.OnTextChanged, new Action<Language>(this.OnTextChanged));
			NetworkVariable<LocalizedString> networkVariable3 = this.runtimeText;
			networkVariable3.OnValueChanged = (NetworkVariable<LocalizedString>.OnValueChangedDelegate)Delegate.Combine(networkVariable3.OnValueChanged, new NetworkVariable<LocalizedString>.OnValueChangedDelegate(this.OnTextValueChanged));
			NetworkVariable<float> networkVariable4 = this.alpha;
			networkVariable4.OnValueChanged = (NetworkVariable<float>.OnValueChangedDelegate)Delegate.Combine(networkVariable4.OnValueChanged, new NetworkVariable<float>.OnValueChangedDelegate(this.OnAlphaChanged));
			NetworkVariable<float> networkVariable5 = this.characterSpacing;
			networkVariable5.OnValueChanged = (NetworkVariable<float>.OnValueChangedDelegate)Delegate.Combine(networkVariable5.OnValueChanged, new NetworkVariable<float>.OnValueChangedDelegate(this.OnCharacterSpacingChanged));
			NetworkVariable<float> networkVariable6 = this.lineSpacing;
			networkVariable6.OnValueChanged = (NetworkVariable<float>.OnValueChangedDelegate)Delegate.Combine(networkVariable6.OnValueChanged, new NetworkVariable<float>.OnValueChangedDelegate(this.OnLineSpacingChanged));
			NetworkVariable<bool> networkVariable7 = this.display;
			networkVariable7.OnValueChanged = (NetworkVariable<bool>.OnValueChangedDelegate)Delegate.Combine(networkVariable7.OnValueChanged, new NetworkVariable<bool>.OnValueChangedDelegate(this.OnDisplayChanged));
			NetworkVariable<bool> networkVariable8 = this.useAutoSizing;
			networkVariable8.OnValueChanged = (NetworkVariable<bool>.OnValueChangedDelegate)Delegate.Combine(networkVariable8.OnValueChanged, new NetworkVariable<bool>.OnValueChangedDelegate(this.OnUseAutoSizingChanged));
			NetworkVariable<float> networkVariable9 = this.fontSize;
			networkVariable9.OnValueChanged = (NetworkVariable<float>.OnValueChangedDelegate)Delegate.Combine(networkVariable9.OnValueChanged, new NetworkVariable<float>.OnValueChangedDelegate(this.OnFontSizeChanged));
			NetworkVariable<float> networkVariable10 = this.minFontSize;
			networkVariable10.OnValueChanged = (NetworkVariable<float>.OnValueChangedDelegate)Delegate.Combine(networkVariable10.OnValueChanged, new NetworkVariable<float>.OnValueChangedDelegate(this.OnMinFontChanged));
			NetworkVariable<float> networkVariable11 = this.maxFontSize;
			networkVariable11.OnValueChanged = (NetworkVariable<float>.OnValueChangedDelegate)Delegate.Combine(networkVariable11.OnValueChanged, new NetworkVariable<float>.OnValueChangedDelegate(this.OnMaxFontChanged));
		}

		// Token: 0x170004B2 RID: 1202
		// (get) Token: 0x06001632 RID: 5682 RVA: 0x00068932 File Offset: 0x00066B32
		// (set) Token: 0x06001633 RID: 5683 RVA: 0x0006893A File Offset: 0x00066B3A
		public WorldObject WorldObject { get; private set; }

		// Token: 0x170004B3 RID: 1203
		// (get) Token: 0x06001634 RID: 5684 RVA: 0x00068943 File Offset: 0x00066B43
		public Type ComponentReferenceType
		{
			get
			{
				return typeof(TextReferences);
			}
		}

		// Token: 0x06001635 RID: 5685 RVA: 0x00068950 File Offset: 0x00066B50
		public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
		{
			TextReferences textReferences = (TextReferences)referenceBase;
			this.textObject = textReferences.textObject;
			this.inlineTextSpanFinder.RegisterTextComponent(this.textObject);
			this.textObject.spriteAsset = this.spriteAsset;
		}

		// Token: 0x06001636 RID: 5686 RVA: 0x00068992 File Offset: 0x00066B92
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x170004B4 RID: 1204
		// (get) Token: 0x06001637 RID: 5687 RVA: 0x0006899C File Offset: 0x00066B9C
		public object LuaObject
		{
			get
			{
				Text text;
				if ((text = this.luaTextInterface) == null)
				{
					text = (this.luaTextInterface = new Text(this));
				}
				return text;
			}
		}

		// Token: 0x170004B5 RID: 1205
		// (get) Token: 0x06001638 RID: 5688 RVA: 0x000689C2 File Offset: 0x00066BC2
		public Type LuaObjectType
		{
			get
			{
				return typeof(Text);
			}
		}

		// Token: 0x170004B6 RID: 1206
		// (get) Token: 0x06001639 RID: 5689 RVA: 0x000689CE File Offset: 0x00066BCE
		public List<Type> EnumTypes
		{
			get
			{
				return new List<Type> { typeof(TextAlignmentOptions) };
			}
		}

		// Token: 0x0600163A RID: 5690 RVA: 0x000689E5 File Offset: 0x00066BE5
		public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
		}

		// Token: 0x0600163D RID: 5693 RVA: 0x00068ADC File Offset: 0x00066CDC
		protected override void __initializeVariables()
		{
			bool flag = this.display == null;
			if (flag)
			{
				throw new Exception("TextComponent.display cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.display.Initialize(this);
			base.__nameNetworkVariable(this.display, "display");
			this.NetworkVariableFields.Add(this.display);
			flag = this.color == null;
			if (flag)
			{
				throw new Exception("TextComponent.color cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.color.Initialize(this);
			base.__nameNetworkVariable(this.color, "color");
			this.NetworkVariableFields.Add(this.color);
			flag = this.alpha == null;
			if (flag)
			{
				throw new Exception("TextComponent.alpha cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.alpha.Initialize(this);
			base.__nameNetworkVariable(this.alpha, "alpha");
			this.NetworkVariableFields.Add(this.alpha);
			flag = this.textAlignment == null;
			if (flag)
			{
				throw new Exception("TextComponent.textAlignment cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.textAlignment.Initialize(this);
			base.__nameNetworkVariable(this.textAlignment, "textAlignment");
			this.NetworkVariableFields.Add(this.textAlignment);
			flag = this.runtimeText == null;
			if (flag)
			{
				throw new Exception("TextComponent.runtimeText cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.runtimeText.Initialize(this);
			base.__nameNetworkVariable(this.runtimeText, "runtimeText");
			this.NetworkVariableFields.Add(this.runtimeText);
			flag = this.characterSpacing == null;
			if (flag)
			{
				throw new Exception("TextComponent.characterSpacing cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.characterSpacing.Initialize(this);
			base.__nameNetworkVariable(this.characterSpacing, "characterSpacing");
			this.NetworkVariableFields.Add(this.characterSpacing);
			flag = this.lineSpacing == null;
			if (flag)
			{
				throw new Exception("TextComponent.lineSpacing cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.lineSpacing.Initialize(this);
			base.__nameNetworkVariable(this.lineSpacing, "lineSpacing");
			this.NetworkVariableFields.Add(this.lineSpacing);
			flag = this.fontSize == null;
			if (flag)
			{
				throw new Exception("TextComponent.fontSize cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.fontSize.Initialize(this);
			base.__nameNetworkVariable(this.fontSize, "fontSize");
			this.NetworkVariableFields.Add(this.fontSize);
			flag = this.useAutoSizing == null;
			if (flag)
			{
				throw new Exception("TextComponent.useAutoSizing cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.useAutoSizing.Initialize(this);
			base.__nameNetworkVariable(this.useAutoSizing, "useAutoSizing");
			this.NetworkVariableFields.Add(this.useAutoSizing);
			flag = this.minFontSize == null;
			if (flag)
			{
				throw new Exception("TextComponent.minFontSize cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.minFontSize.Initialize(this);
			base.__nameNetworkVariable(this.minFontSize, "minFontSize");
			this.NetworkVariableFields.Add(this.minFontSize);
			flag = this.maxFontSize == null;
			if (flag)
			{
				throw new Exception("TextComponent.maxFontSize cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.maxFontSize.Initialize(this);
			base.__nameNetworkVariable(this.maxFontSize, "maxFontSize");
			this.NetworkVariableFields.Add(this.maxFontSize);
			base.__initializeVariables();
		}

		// Token: 0x0600163E RID: 5694 RVA: 0x0001E813 File Offset: 0x0001CA13
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x0600163F RID: 5695 RVA: 0x00068E41 File Offset: 0x00067041
		protected internal override string __getTypeName()
		{
			return "TextComponent";
		}

		// Token: 0x040011F3 RID: 4595
		[SerializeField]
		[HideInInspector]
		private TextMeshPro textObject;

		// Token: 0x040011F4 RID: 4596
		[SerializeField]
		private InlineTextSpanFinder inlineTextSpanFinder;

		// Token: 0x040011F5 RID: 4597
		[SerializeField]
		private LocalizedString text;

		// Token: 0x040011F6 RID: 4598
		[SerializeField]
		private TMP_SpriteAsset spriteAsset;

		// Token: 0x040011F7 RID: 4599
		private readonly NetworkVariable<bool> display = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x040011F8 RID: 4600
		private readonly NetworkVariable<global::UnityEngine.Color> color = new NetworkVariable<global::UnityEngine.Color>(global::UnityEngine.Color.black, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x040011F9 RID: 4601
		private readonly NetworkVariable<float> alpha = new NetworkVariable<float>(1f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x040011FA RID: 4602
		private readonly NetworkVariable<TextAlignmentOptions> textAlignment = new NetworkVariable<TextAlignmentOptions>(TextAlignmentOptions.Center, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x040011FB RID: 4603
		private readonly NetworkVariable<LocalizedString> runtimeText = new NetworkVariable<LocalizedString>(new LocalizedString(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x040011FC RID: 4604
		private readonly NetworkVariable<float> characterSpacing = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x040011FD RID: 4605
		private readonly NetworkVariable<float> lineSpacing = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x040011FE RID: 4606
		private readonly NetworkVariable<float> fontSize = new NetworkVariable<float>(0.2f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x040011FF RID: 4607
		private readonly NetworkVariable<bool> useAutoSizing = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04001200 RID: 4608
		private readonly NetworkVariable<float> minFontSize = new NetworkVariable<float>(1f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04001201 RID: 4609
		private readonly NetworkVariable<float> maxFontSize = new NetworkVariable<float>(72f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04001203 RID: 4611
		internal EndlessScriptComponent scriptComponent;

		// Token: 0x04001204 RID: 4612
		private Text luaTextInterface;
	}
}
