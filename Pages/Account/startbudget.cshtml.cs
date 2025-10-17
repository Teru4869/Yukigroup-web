using Microsoft.AspNetCore.Mvc.RazorPages;
using Yukigroup_WEB.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
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

        [BindProperty] public string EditId { get; set; }
        [BindProperty] public DateTime? StartDate { get; set; }
        [BindProperty] public string StartConfimer { get; set; }
        [BindProperty] public decimal? Itimanen { get; set; }
        [BindProperty] public decimal? Gosenen { get; set; }
        [BindProperty] public decimal? Nisenen { get; set; }
        [BindProperty] public decimal? Senen { get; set; }
        [BindProperty] public decimal? Gohyakuen { get; set; }
        [BindProperty] public decimal? Hyakuen { get; set; }
        [BindProperty] public decimal? Gozyuen { get; set; }
        [BindProperty] public decimal? Zyuen { get; set; }
        [BindProperty] public decimal? Goen { get; set; }
        [BindProperty] public decimal? Itien { get; set; }

        public async Task OnGetAsync()
        {
            Rows = await _sheetService.GetSheetAsync("startbudget!A:Z");

            if (Rows != null && Rows.Count > 0)
            {
                // 全行を逆順に
                var reversedRows = Rows.Reverse().ToList();

                // 直近3か月（12行）だけ残す
                var limitedRows = reversedRows.Take(12).ToList();

                Rows = limitedRows;
            }
        }

        public async Task<IActionResult> OnPostEditAsync(string id)
        {
            await OnGetAsync();
            var target = Rows.FirstOrDefault(r => r.Count > 0 && r[0]?.ToString() == id);
            if (target != null)
            {
                EditId = id;

                if (DateTime.TryParse(target[1]?.ToString(), out var parsedDate))
                    StartDate = parsedDate;
                else
                    StartDate = null;

                StartConfimer = target[2]?.ToString();
                Itimanen = ParseDecimal(target.ElementAtOrDefault(3));
                Gosenen = ParseDecimal(target.ElementAtOrDefault(4));
                Nisenen = ParseDecimal(target.ElementAtOrDefault(5));
                Senen = ParseDecimal(target.ElementAtOrDefault(6));
                Gohyakuen = ParseDecimal(target.ElementAtOrDefault(7));
                Hyakuen = ParseDecimal(target.ElementAtOrDefault(8));
                Gozyuen = ParseDecimal(target.ElementAtOrDefault(9));
                Zyuen = ParseDecimal(target.ElementAtOrDefault(10));
                Goen = ParseDecimal(target.ElementAtOrDefault(11));
                Itien = ParseDecimal(target.ElementAtOrDefault(12));
            }
            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            Console.WriteLine("[OnPostSaveAsync] 実行開始（startbudget）");

            // EditId のエラーを除外（新規時に空でもOKにする）
            ModelState.Remove(nameof(EditId));

            if (!ModelState.IsValid)
            {
                Console.WriteLine("[OnPostSaveAsync] ModelState 無効 → ページ再表示");
                foreach (var kvp in ModelState)
                {
                    if (kvp.Value.Errors.Count > 0)
                        Console.WriteLine($"  ❌ {kvp.Key}: {string.Join(", ", kvp.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                await OnGetAsync();
                return Page();
            }

            Console.WriteLine("[OnPostSaveAsync] 入力値 OK。新規/更新処理へ進む。");

            // --- 金種の集計 ---
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

            var total = (itimanen * 10000) + (gosenen * 5000) + (nisenen * 2000) + (senen * 1000)
                        + (gohyakuen * 500) + (hyakuen * 100) + (gozyuen * 50)
                        + (zyuen * 10) + (goen * 5) + (itien * 1);

            Console.WriteLine($"[OnPostSaveAsync] 合計計算完了: {total}");

            // --- ID 採番処理 ---
            string id;
            if (string.IsNullOrEmpty(EditId))
            {
                // Googleシート上の最大IDを調べて +1
                int nextId = await _sheetService.GetNextIdAsync("startbudget");
                id = nextId.ToString("D6"); // 例: 000047
                Console.WriteLine($"[OnPostSaveAsync] 新規ID採番: {id}");
            }
            else
            {
                id = EditId;
                Console.WriteLine($"[OnPostSaveAsync] 既存ID使用: {id}");
            }

            // --- 行データ構築 ---
            var newRow = new List<object>
    {
        id,
        StartDate?.ToString("yyyy/MM/dd"),
        StartConfimer,
        itimanen, gosenen, nisenen, senen,
        gohyakuen, hyakuen, gozyuen, zyuen, goen, itien,
        total
    };
            Console.WriteLine($"[OnPostSaveAsync] newRow 作成完了: {string.Join(", ", newRow)}");

            // --- Google Sheets 登録処理 ---
            if (string.IsNullOrEmpty(EditId))
            {
                Console.WriteLine("[OnPostSaveAsync] 新規登録処理 → AppendStartBudgetRowAsync()");
                await _sheetService.AppendStartBudgetRowAsync(newRow);
                Console.WriteLine("[OnPostSaveAsync] AppendStartBudgetRowAsync() 完了");
            }
            else
            {
                Console.WriteLine("[OnPostSaveAsync] 既存更新処理 → UpdateRowByIdAsync()");
                await _sheetService.UpdateRowByIdAsync("startbudget", id, newRow);
                Console.WriteLine("[OnPostSaveAsync] UpdateRowByIdAsync() 完了");
            }

            Console.WriteLine("[OnPostSaveAsync] Redirect 実行");
            return RedirectToPage();
        }


        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            await _sheetService.DeleteRowByIdAsync("startbudget", id);
            return RedirectToPage();
        }

        private decimal? ParseDecimal(object? obj)
            => decimal.TryParse(obj?.ToString(), out var d) ? d : (decimal?)null;
    }
}
