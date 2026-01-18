using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Shared.DataTypes
{
	// Token: 0x02000012 RID: 18
	[Serializable]
	public class SemanticVersion : IComparable, IFormattable
	{
		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000069 RID: 105 RVA: 0x00003539 File Offset: 0x00001739
		// (set) Token: 0x0600006A RID: 106 RVA: 0x00003541 File Offset: 0x00001741
		[JsonIgnore]
		public int Major
		{
			get
			{
				return this._major;
			}
			private set
			{
				this._major = value;
			}
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x0600006B RID: 107 RVA: 0x0000354A File Offset: 0x0000174A
		// (set) Token: 0x0600006C RID: 108 RVA: 0x00003552 File Offset: 0x00001752
		[JsonIgnore]
		public int Minor
		{
			get
			{
				return this._minor;
			}
			private set
			{
				this._minor = value;
			}
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x0600006D RID: 109 RVA: 0x0000355B File Offset: 0x0000175B
		// (set) Token: 0x0600006E RID: 110 RVA: 0x00003563 File Offset: 0x00001763
		[JsonIgnore]
		public int Patch
		{
			get
			{
				return this._patch;
			}
			private set
			{
				this._patch = value;
			}
		}

		// Token: 0x0600006F RID: 111 RVA: 0x000022AB File Offset: 0x000004AB
		public SemanticVersion()
		{
		}

		// Token: 0x06000070 RID: 112 RVA: 0x0000356C File Offset: 0x0000176C
		public SemanticVersion(SemanticVersion version)
		{
			this.Major = version.Major;
			this.Minor = version.Minor;
			this.Patch = version.Patch;
		}

		// Token: 0x06000071 RID: 113 RVA: 0x00003598 File Offset: 0x00001798
		public SemanticVersion(int major, int minor, int patch)
		{
			if (major < 0)
			{
				throw new ArgumentOutOfRangeException("major", "Version parts cannot be negative.");
			}
			if (minor < 0)
			{
				throw new ArgumentOutOfRangeException("minor", "Version parts cannot be negative.");
			}
			if (patch < 0)
			{
				throw new ArgumentOutOfRangeException("patch", "Version parts cannot be negative.");
			}
			this.Major = major;
			this.Minor = minor;
			this.Patch = patch;
		}

		// Token: 0x06000072 RID: 114 RVA: 0x000035FC File Offset: 0x000017FC
		public static SemanticVersion Parse(string version)
		{
			if (string.IsNullOrEmpty(version))
			{
				throw new ArgumentException("Value cannot be null or an empty string.", "version");
			}
			SemanticVersion semanticVersion;
			if (!SemanticVersion.TryParse(version, out semanticVersion))
			{
				throw new ArgumentException(version + " is not a valid version string.", "version");
			}
			return semanticVersion;
		}

		// Token: 0x06000073 RID: 115 RVA: 0x00003642 File Offset: 0x00001842
		public static bool TryParse(string version, out SemanticVersion value)
		{
			return SemanticVersion.TryParseInternal(version, SemanticVersion._semanticVersionRegex, out value);
		}

		// Token: 0x06000074 RID: 116 RVA: 0x00003650 File Offset: 0x00001850
		private static bool TryParseInternal(string version, Regex regex, out SemanticVersion semVer)
		{
			semVer = null;
			if (string.IsNullOrEmpty(version))
			{
				return false;
			}
			Match match = regex.Match(version.Trim());
			int num;
			int num2;
			int num3;
			if (int.TryParse(match.Groups["major"].Value, out num) && int.TryParse(match.Groups["minor"].Value, out num2) && int.TryParse(match.Groups["patch"].Value, out num3))
			{
				semVer = new SemanticVersion(num, num2, num3);
				return true;
			}
			return false;
		}

		// Token: 0x06000075 RID: 117 RVA: 0x000036DE File Offset: 0x000018DE
		public SemanticVersion IncrementPatch()
		{
			return new SemanticVersion(this._major, this._minor, this._patch + 1);
		}

		// Token: 0x06000076 RID: 118 RVA: 0x000036F9 File Offset: 0x000018F9
		public static bool operator ==(SemanticVersion a, SemanticVersion b)
		{
			return a == b || (a != null && a.Equals(b));
		}

		// Token: 0x06000077 RID: 119 RVA: 0x0000370D File Offset: 0x0000190D
		public static bool operator !=(SemanticVersion a, SemanticVersion b)
		{
			return !(a == b);
		}

		// Token: 0x06000078 RID: 120 RVA: 0x00003719 File Offset: 0x00001919
		public static bool operator <(SemanticVersion a, SemanticVersion b)
		{
			return a.CompareTo(b) < 0;
		}

		// Token: 0x06000079 RID: 121 RVA: 0x00003725 File Offset: 0x00001925
		public static bool operator >(SemanticVersion a, SemanticVersion b)
		{
			return a.CompareTo(b) > 0;
		}

		// Token: 0x0600007A RID: 122 RVA: 0x00003731 File Offset: 0x00001931
		public static bool operator >=(SemanticVersion a, SemanticVersion b)
		{
			return a == b || a > b;
		}

		// Token: 0x0600007B RID: 123 RVA: 0x00003745 File Offset: 0x00001945
		public static bool operator <=(SemanticVersion a, SemanticVersion b)
		{
			return a == b || a < b;
		}

		// Token: 0x0600007C RID: 124 RVA: 0x0000375C File Offset: 0x0000195C
		public int CompareTo(object obj)
		{
			SemanticVersion semanticVersion = (SemanticVersion)obj;
			if (semanticVersion == null)
			{
				throw new ArgumentException("obj");
			}
			if (this.Major != semanticVersion.Major)
			{
				return this.Major.CompareTo(semanticVersion.Major);
			}
			if (this.Minor != semanticVersion.Minor)
			{
				return this.Minor.CompareTo(semanticVersion.Minor);
			}
			if (this.Patch != semanticVersion.Patch)
			{
				return this.Patch.CompareTo(semanticVersion.Patch);
			}
			return 0;
		}

		// Token: 0x0600007D RID: 125 RVA: 0x000037F0 File Offset: 0x000019F0
		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			SemanticVersion semanticVersion = obj as SemanticVersion;
			return !(semanticVersion == null) && (this == obj || (this.Major == semanticVersion.Major && this.Minor == semanticVersion.Minor && this.Patch == semanticVersion.Patch));
		}

		// Token: 0x0600007E RID: 126 RVA: 0x00003848 File Offset: 0x00001A48
		public override int GetHashCode()
		{
			return this.Major.GetHashCode() ^ this.Minor.GetHashCode() ^ this.Patch.GetHashCode();
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00003881 File Offset: 0x00001A81
		public override string ToString()
		{
			return this.ToString("G");
		}

		// Token: 0x06000080 RID: 128 RVA: 0x0000388E File Offset: 0x00001A8E
		public string ToString(string format)
		{
			return this.ToString(format, null);
		}

		// Token: 0x06000081 RID: 129 RVA: 0x00003898 File Offset: 0x00001A98
		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (formatProvider == null)
			{
				formatProvider = CultureInfo.CurrentCulture;
			}
			return string.Format(formatProvider, "{0}.{1}.{2}", this.Major, this.Minor, this.Patch);
		}

		// Token: 0x0400002E RID: 46
		private const RegexOptions _flags = RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled;

		// Token: 0x0400002F RID: 47
		private static readonly Regex _semanticVersionRegex = new Regex("^(?<major>0|[1-9]\\d*)\\.(?<minor>0|[1-9]\\d*)\\.(?<patch>0|[1-9]\\d*)$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled);

		// Token: 0x04000030 RID: 48
		[JsonProperty]
		[SerializeField]
		private int _major;

		// Token: 0x04000031 RID: 49
		[JsonProperty]
		[SerializeField]
		private int _minor;

		// Token: 0x04000032 RID: 50
		[JsonProperty]
		[SerializeField]
		private int _patch;
	}
}
