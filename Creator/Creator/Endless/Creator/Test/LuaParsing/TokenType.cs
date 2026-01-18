using System;

namespace Endless.Creator.Test.LuaParsing
{
	// Token: 0x02000326 RID: 806
	public enum TokenType
	{
		// Token: 0x04000C62 RID: 3170
		LeftParenthesis,
		// Token: 0x04000C63 RID: 3171
		RightParenthesis,
		// Token: 0x04000C64 RID: 3172
		LeftBrace,
		// Token: 0x04000C65 RID: 3173
		RightBrace,
		// Token: 0x04000C66 RID: 3174
		LeftBracket,
		// Token: 0x04000C67 RID: 3175
		RightBracket,
		// Token: 0x04000C68 RID: 3176
		Dot,
		// Token: 0x04000C69 RID: 3177
		Minus,
		// Token: 0x04000C6A RID: 3178
		Plus,
		// Token: 0x04000C6B RID: 3179
		Star,
		// Token: 0x04000C6C RID: 3180
		Slash,
		// Token: 0x04000C6D RID: 3181
		Modulo,
		// Token: 0x04000C6E RID: 3182
		Semicolon,
		// Token: 0x04000C6F RID: 3183
		Colon,
		// Token: 0x04000C70 RID: 3184
		Comma,
		// Token: 0x04000C71 RID: 3185
		Hash,
		// Token: 0x04000C72 RID: 3186
		Carrot,
		// Token: 0x04000C73 RID: 3187
		Tilde,
		// Token: 0x04000C74 RID: 3188
		Bang,
		// Token: 0x04000C75 RID: 3189
		BangEqual,
		// Token: 0x04000C76 RID: 3190
		Equal,
		// Token: 0x04000C77 RID: 3191
		EqualEqual,
		// Token: 0x04000C78 RID: 3192
		Greater,
		// Token: 0x04000C79 RID: 3193
		GreaterEqual,
		// Token: 0x04000C7A RID: 3194
		Less,
		// Token: 0x04000C7B RID: 3195
		LessEqual,
		// Token: 0x04000C7C RID: 3196
		Concatenate,
		// Token: 0x04000C7D RID: 3197
		Identifier,
		// Token: 0x04000C7E RID: 3198
		String,
		// Token: 0x04000C7F RID: 3199
		Number,
		// Token: 0x04000C80 RID: 3200
		Comment,
		// Token: 0x04000C81 RID: 3201
		Print,
		// Token: 0x04000C82 RID: 3202
		And,
		// Token: 0x04000C83 RID: 3203
		Break,
		// Token: 0x04000C84 RID: 3204
		Do,
		// Token: 0x04000C85 RID: 3205
		If,
		// Token: 0x04000C86 RID: 3206
		Else,
		// Token: 0x04000C87 RID: 3207
		Elseif,
		// Token: 0x04000C88 RID: 3208
		End,
		// Token: 0x04000C89 RID: 3209
		True,
		// Token: 0x04000C8A RID: 3210
		False,
		// Token: 0x04000C8B RID: 3211
		For,
		// Token: 0x04000C8C RID: 3212
		Function,
		// Token: 0x04000C8D RID: 3213
		In,
		// Token: 0x04000C8E RID: 3214
		Local,
		// Token: 0x04000C8F RID: 3215
		Nil,
		// Token: 0x04000C90 RID: 3216
		Not,
		// Token: 0x04000C91 RID: 3217
		Or,
		// Token: 0x04000C92 RID: 3218
		Repeat,
		// Token: 0x04000C93 RID: 3219
		Return,
		// Token: 0x04000C94 RID: 3220
		Then,
		// Token: 0x04000C95 RID: 3221
		Until,
		// Token: 0x04000C96 RID: 3222
		While,
		// Token: 0x04000C97 RID: 3223
		EndOfLine,
		// Token: 0x04000C98 RID: 3224
		EndOfFile
	}
}
