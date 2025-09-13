using Google.Apis.Sheets.v4;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

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
            var range = $"{sheetName}!A:Z"; // 対象シート全体
            var valueRange = new Google.Apis.Sheets.v4.Data.ValueRange
            {
                Values = new List<IList<object>> { rowData }
            };

            var appendRequest = _service.Spreadsheets.Values.Append(valueRange, _spreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;

            await appendRequest.ExecuteAsync();
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
