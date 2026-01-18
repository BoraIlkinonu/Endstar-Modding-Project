using System;

namespace Endless.Creator.Test.LuaParsing
{
	// Token: 0x0200031B RID: 795
	public class ParsingError
	{
		// Token: 0x1700020F RID: 527
		// (get) Token: 0x06000E63 RID: 3683 RVA: 0x00044243 File Offset: 0x00042443
		public string Message { get; }

		// Token: 0x17000210 RID: 528
		// (get) Token: 0x06000E64 RID: 3684 RVA: 0x0004424B File Offset: 0x0004244B
		public int Line { get; }

		// Token: 0x17000211 RID: 529
		// (get) Token: 0x06000E65 RID: 3685 RVA: 0x00044253 File Offset: 0x00042453
		public int CharacterIndex { get; }

		// Token: 0x17000212 RID: 530
		// (get) Token: 0x06000E66 RID: 3686 RVA: 0x0004425B File Offset: 0x0004245B
		// (set) Token: 0x06000E67 RID: 3687 RVA: 0x00044263 File Offset: 0x00042463
		public bool GlobalError { get; set; }

		// Token: 0x06000E68 RID: 3688 RVA: 0x0004426C File Offset: 0x0004246C
		public ParsingError(string message, int line, int characterIndex)
		{
			this.Message = message;
			this.Line = line;
			this.CharacterIndex = characterIndex;
		}

		// Token: 0x06000E69 RID: 3689 RVA: 0x0004428C File Offset: 0x0004248C
		public override string ToString()
		{
			if (!this.GlobalError)
			{
				return string.Format("Line: {0}, Character Index: {1}, Message: {2}", this.Line, this.CharacterIndex, this.Message);
			}
			return "Global Error, Message: " + this.Message;
		}
	}
}
