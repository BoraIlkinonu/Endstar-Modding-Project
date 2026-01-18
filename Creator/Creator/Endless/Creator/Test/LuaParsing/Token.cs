using System;

namespace Endless.Creator.Test.LuaParsing
{
	// Token: 0x02000323 RID: 803
	public class Token
	{
		// Token: 0x06000ECA RID: 3786 RVA: 0x00046814 File Offset: 0x00044A14
		public Token(TokenType type, string lexeme, object literal, int startIndex, int endIndex, int line)
		{
			this.Type = type;
			this.Lexeme = lexeme;
			this.Literal = literal;
			this.StartIndex = startIndex;
			this.Length = endIndex;
			this.Line = line;
		}

		// Token: 0x1700021E RID: 542
		// (get) Token: 0x06000ECB RID: 3787 RVA: 0x00046849 File Offset: 0x00044A49
		public int StartIndex { get; }

		// Token: 0x1700021F RID: 543
		// (get) Token: 0x06000ECC RID: 3788 RVA: 0x00046851 File Offset: 0x00044A51
		public int Length { get; }

		// Token: 0x17000220 RID: 544
		// (get) Token: 0x06000ECD RID: 3789 RVA: 0x00046859 File Offset: 0x00044A59
		public TokenType Type { get; }

		// Token: 0x17000221 RID: 545
		// (get) Token: 0x06000ECE RID: 3790 RVA: 0x00046861 File Offset: 0x00044A61
		public string Lexeme { get; }

		// Token: 0x17000222 RID: 546
		// (get) Token: 0x06000ECF RID: 3791 RVA: 0x00046869 File Offset: 0x00044A69
		public object Literal { get; }

		// Token: 0x17000223 RID: 547
		// (get) Token: 0x06000ED0 RID: 3792 RVA: 0x00046871 File Offset: 0x00044A71
		public int Line { get; }

		// Token: 0x06000ED1 RID: 3793 RVA: 0x0004687C File Offset: 0x00044A7C
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"Lexeme: <b>",
				this.Lexeme,
				"</b>, ",
				string.Format("{0}: {1}, ", "Type", this.Type),
				string.Format("{0}: {1}, ", "StartIndex", this.StartIndex),
				string.Format("EndIndex: {0}, ", this.StartIndex + this.Length),
				string.Format("{0}: {1}, ", "Length", this.Length),
				string.Format("{0}: {1}, ", "Line", this.Line),
				string.Format("{0}: {1}", "Literal", this.Literal)
			});
		}
	}
}
