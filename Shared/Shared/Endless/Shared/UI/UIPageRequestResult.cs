using System;
using Endless.Shared.Pagination;
using Newtonsoft.Json;

namespace Endless.Shared.UI
{
	// Token: 0x02000193 RID: 403
	public class UIPageRequestResult<T>
	{
		// Token: 0x060009F1 RID: 2545 RVA: 0x0002AB1E File Offset: 0x00028D1E
		public UIPageRequestResult(T[] items, Pagination pagination)
		{
			this.Items = items;
			this.Pagination = pagination;
		}

		// Token: 0x170001C5 RID: 453
		// (get) Token: 0x060009F2 RID: 2546 RVA: 0x0002AB34 File Offset: 0x00028D34
		// (set) Token: 0x060009F3 RID: 2547 RVA: 0x0002AB3C File Offset: 0x00028D3C
		[JsonProperty("data")]
		public T[] Items { get; set; }

		// Token: 0x170001C6 RID: 454
		// (get) Token: 0x060009F4 RID: 2548 RVA: 0x0002AB45 File Offset: 0x00028D45
		// (set) Token: 0x060009F5 RID: 2549 RVA: 0x0002AB4D File Offset: 0x00028D4D
		[JsonProperty("pagination")]
		public Pagination Pagination { get; set; }

		// Token: 0x170001C7 RID: 455
		// (get) Token: 0x060009F6 RID: 2550 RVA: 0x0002AB56 File Offset: 0x00028D56
		public int Page
		{
			get
			{
				return this.Pagination.Offset / this.Pagination.Limit + 1;
			}
		}

		// Token: 0x060009F7 RID: 2551 RVA: 0x0002AB74 File Offset: 0x00028D74
		public override string ToString()
		{
			return string.Format("{0}: {1}\n", "Items", (this.Items == null) ? "null" : this.Items.Length) + string.Format("{0}: {1}\n", "Pagination", this.Pagination) + string.Format("{0}: {1}", "Page", this.Page);
		}

		// Token: 0x060009F8 RID: 2552 RVA: 0x0002ABE0 File Offset: 0x00028DE0
		public static UIPageRequestResult<T> Parse(string json)
		{
			return JsonConvert.DeserializeObject<UIPageRequestResult<T>>(json);
		}
	}
}
