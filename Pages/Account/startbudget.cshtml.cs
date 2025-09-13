using Microsoft.AspNetCore.Mvc.RazorPages;
using Yukigroup_WEB.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;

namespace Yukigroup_WEB.Pages.Account
{
    public class startbudgetModel : PageModel
    {
        private readonly AccountingSheetService _sheetService;

        public IList<IList<object>> Rows { get; set; }

        public startbudgetModel(AccountingSheetService sheetService)
        {
            _sheetService = sheetService;
        }

        [BindProperty]
        public string StartDate { get; set; }

        [BindProperty]
        public string StartConfimer { get; set; }  // B列プルダウン用

        [BindProperty]
        public decimal? Itimanen { get; set; }
        [BindProperty]
        public decimal? Gosenen { get; set; }
        [BindProperty]
        public decimal? Nisenen { get; set; }
        [BindProperty]
        public decimal? Senen { get; set; }
        [BindProperty]
        public decimal? Gohyakuen { get; set; }
        [BindProperty]
        public decimal? Hyakuen { get; set; }
        [BindProperty]
        public decimal? Gozyuen { get; set; }
        [BindProperty]
        public decimal? Zyuen { get; set; }
        [BindProperty]
        public decimal? Goen { get; set; }
        [BindProperty]
        public decimal? Itien { get; set; }
        public async Task OnGetAsync()
        {
            Rows = await _sheetService.GetSheetAsync("startbudget!A:Z");

            if (Rows != null && Rows.Count > 4)
            {
                // 1〜4行目を固定でそのまま保持
                var fixedRows = Rows.Take(4).ToList();

                // 5行目以降を逆順に
                var reversedRows = Rows.Skip(4).Reverse().ToList();

                // 結合
                var result = new List<IList<object>>();
                result.AddRange(fixedRows);
                result.AddRange(reversedRows);

                Rows = result;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(); // 再描画用にデータ取得
                return Page();
            }

            // 未入力を0に変換
            var itimanen = Itimanen ?? 0;
            var gosenen = Gosenen ?? 0;
            var nisenen = Nisenen ?? 0;
            var senen = Senen ?? 0;
            var gohyakuen = Gohyakuen ?? 0;
            var hyakuen = Hyakuen ?? 0;
            var gozyuen = Gozyuen ?? 0;
            var zyuen = Zyuen ?? 0;
            var goen = Goen ?? 0;
            var itien = Itien ?? 0;

            // 合計計算
            var total =
                (itimanen * 10000) +
                (gosenen * 5000) +
                (nisenen * 2000) +
                (senen * 1000) +
                (gohyakuen * 500) +
                (hyakuen * 100) +
                (gozyuen * 50) +
                (zyuen * 10) +
                (goen * 5) +
                (itien * 1);

            var newRow = new List<object>
            {
                StartDate,
                StartConfimer,
                itimanen,
                gosenen,
                nisenen,
                senen,
                gohyakuen,
                hyakuen,
                gozyuen,
                zyuen,
                goen,
                itien,
                total
            };

            await _sheetService.AppendStartBudgetRowAsync(newRow);

            return RedirectToPage(); // 再読み込みして一覧に反映
        }
    }
}
