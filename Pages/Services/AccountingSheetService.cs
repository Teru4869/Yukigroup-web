using Google.Apis.Sheets.v4;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Google.Apis.Sheets.v4.Data;
using System;

namespace Yukigroup_WEB.Services
{
    public class AccountingSheetService
    {
        private readonly SheetsService _service;
        private readonly string _spreadsheetId = "1PyB66kBkKy1kXHlrHcXkMM1W9Yq5PKd5bEmwLOx-LpI";

        public AccountingSheetService()
        {
            GoogleCredential credential;
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(SheetsService.Scope.Spreadsheets);
            }

            _service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Yukigroup Accounting",
            });
        }

        public async Task<IList<IList<object>>> GetSheetAsync(string range)
        {
            var request = _service.Spreadsheets.Values.Get(_spreadsheetId, range);
            var response = await request.ExecuteAsync();
            return response.Values;
        }
        public async Task AppendRowAsync(string sheetName, IList<object> rowData)
        {
            try
            {
                Console.WriteLine($"[AppendRowAsync] {sheetName} に行を追加中...");
                Console.WriteLine($"[AppendRowAsync] データ: {string.Join(", ", rowData)}");

                var range = $"{sheetName}!A:Z"; // 対象シート全体
                var valueRange = new Google.Apis.Sheets.v4.Data.ValueRange
                {
                    Values = new List<IList<object>> { rowData }
                };

                if (rowData != null && rowData.Count > 0)
                {
                    Console.WriteLine($"[AppendRowAsync] Sheet: {sheetName}, ID: {rowData[0]}");
                }
                else
                {
                    Console.WriteLine($"[AppendRowAsync] Sheet: {sheetName}, rowData is null or empty");
                }

                var appendRequest = _service.Spreadsheets.Values.Append(valueRange, _spreadsheetId, range);
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;

                var result = await appendRequest.ExecuteAsync();
                Console.WriteLine($"[AppendRowAsync] Google Sheets 追加結果: {result.Updates?.UpdatedRange}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AppendRowAsync] エラー発生: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

        }

        // IDから行番号を取得
        public async Task<int?> FindRowNumberByIdAsync(string sheetName, string id)
        {
            var values = await GetSheetAsync($"{sheetName}!A:Z");
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i].Count > 0 && values[i][0]?.ToString() == id)
                    return i + 1; // Google Sheetsは1始まり
            }
            return null;
        }

        // ID指定で更新
        public async Task UpdateRowByIdAsync(string sheetName, string id, IList<object> newRow)
        {
            var rowNumber = await FindRowNumberByIdAsync(sheetName, id);
            if (rowNumber == null) return;
            var range = $"{sheetName}!A{rowNumber}:Z{rowNumber}";
            var valueRange = new ValueRange { Values = new List<IList<object>> { newRow } };

            var updateRequest = _service.Spreadsheets.Values.Update(valueRange, _spreadsheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            await updateRequest.ExecuteAsync();
        }

        // ID指定で削除
        public async Task DeleteRowByIdAsync(string sheetName, string id)
        {
            var rowNumber = await FindRowNumberByIdAsync(sheetName, id);
            if (rowNumber == null) return;

            var batchUpdate = new BatchUpdateSpreadsheetRequest
            {
                Requests = new List<Request>
                {
                    new Request
                    {
                        DeleteDimension = new DeleteDimensionRequest
                        {
                            Range = new DimensionRange
                            {
                                SheetId = await GetSheetIdAsync(sheetName),
                                Dimension = "ROWS",
                                StartIndex = rowNumber.Value - 1,
                                EndIndex = rowNumber.Value
                            }
                        }
                    }
                }
            };
            await _service.Spreadsheets.BatchUpdate(batchUpdate, _spreadsheetId).ExecuteAsync();
        }

        // 次のIDを取得
        public async Task<int> GetNextIdAsync(string sheetName)
        {
            var allRows = await GetSheetAsync($"{sheetName}!A:A");
            int lastId = 0;
            foreach (var row in allRows.Skip(1))
            {
                if (row.Count > 0 && int.TryParse(row[0].ToString(), out int parsedId))
                {
                    if (parsedId > lastId) lastId = parsedId;
                }
            }
            return lastId + 1;
        }


        // Sheet名からSheetIdを取得
        private async Task<int> GetSheetIdAsync(string sheetName)
        {
            var spreadsheet = await _service.Spreadsheets.Get(_spreadsheetId).ExecuteAsync();
            var sheet = spreadsheet.Sheets.FirstOrDefault(s => s.Properties.Title == sheetName);
            return sheet.Properties.SheetId.Value;
        }
        // 各シート専用メソッド
        public async Task AppendDayBudgetRowAsync(IList<object> rowData) =>
            await AppendRowAsync("daybudget", rowData);

        public async Task AppendStartBudgetRowAsync(IList<object> rowData) =>
            await AppendRowAsync("startbudget", rowData);

        public async Task AppendEndBudgetRowAsync(IList<object> rowData) =>
            await AppendRowAsync("endbudget", rowData);

    }
}
