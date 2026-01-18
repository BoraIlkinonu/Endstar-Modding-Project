using System;
using Newtonsoft.Json.Linq;

namespace Endless.Shared
{
	// Token: 0x0200000C RID: 12
	public class UserRole
	{
		// Token: 0x06000051 RID: 81 RVA: 0x00003209 File Offset: 0x00001409
		public UserRole(int userId, Roles role)
		{
			this.UserId = userId;
			this.Role = role;
		}

		// Token: 0x06000052 RID: 82 RVA: 0x00003228 File Offset: 0x00001428
		public UserRole(JObject source)
		{
			JToken jtoken = source.SelectToken("user_id");
			int? num = ((jtoken != null) ? new int?(jtoken.Value<int>()) : null);
			if (num != null)
			{
				this.UserId = num.Value;
			}
			else
			{
				this.IsValid = false;
			}
			JToken jtoken2 = source.SelectToken("role");
			int? num2;
			if (jtoken2 == null)
			{
				num2 = null;
			}
			else
			{
				JToken jtoken3 = jtoken2.SelectToken("id");
				num2 = ((jtoken3 != null) ? new int?(jtoken3.Value<int>()) : null);
			}
			int? num3 = num2;
			if (num3 != null)
			{
				this.Role = (Roles)num3.Value;
			}
			else
			{
				this.IsValid = false;
				this.Role = Roles.None;
			}
			JToken jtoken4 = source.SelectToken("inherited_from_parent");
			bool? flag = ((jtoken4 != null) ? new bool?(jtoken4.Value<bool>()) : null);
			if (flag != null)
			{
				this.InheritedFromParent = flag.Value;
				return;
			}
			this.IsValid = false;
			this.InheritedFromParent = false;
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000053 RID: 83 RVA: 0x00003335 File Offset: 0x00001535
		public int UserId { get; }

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000054 RID: 84 RVA: 0x0000333D File Offset: 0x0000153D
		// (set) Token: 0x06000055 RID: 85 RVA: 0x00003345 File Offset: 0x00001545
		public Roles Role { get; set; }

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000056 RID: 86 RVA: 0x0000334E File Offset: 0x0000154E
		// (set) Token: 0x06000057 RID: 87 RVA: 0x00003356 File Offset: 0x00001556
		public bool InheritedFromParent { get; set; }

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000058 RID: 88 RVA: 0x0000335F File Offset: 0x0000155F
		public bool IsValid { get; } = true;

		// Token: 0x06000059 RID: 89 RVA: 0x00003368 File Offset: 0x00001568
		public override string ToString()
		{
			string text = (this.InheritedFromParent ? "inherited" : "not inherited");
			return string.Format("User {0} has role {1}, which is {2}", this.UserId, this.Role, text);
		}
	}
}
