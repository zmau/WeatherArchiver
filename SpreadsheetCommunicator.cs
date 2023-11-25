using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;


namespace net.zmau.weatherarchiver
{
    //sheetwriter@standardwriter.iam.gserviceaccount.com
    // Use this key in your application by passing it with the key=API_KEY parameter : AIzaSyCsijtF8yMA7YJNHHYaKPPLwYiiatUg1pA
    internal class SpreadsheetCommunicator
    {
        protected string[] _scopes = { SheetsService.Scope.Spreadsheets };
        protected string _applicationName = "Weather Archiver";
        protected string _spreadsheetId = "1i2snDQNWi4PPmHdZIceystg2ll59tjY09CR1C_r6TwU";
        protected SheetsService _sheetsService;

        protected DateTime _lastDateCovered;
        protected int _lastRowWritten;
        protected List<ValueRange> _updateData;

        public SpreadsheetCommunicator()
        {
            GoogleCredential credential;
            using (var stream = new FileStream(Path.Combine(Environment.CurrentDirectory, "standardwriter-64bea66251ab.json"),
                FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream);
                credential = credential.CreateScoped(_scopes);
            }
            _sheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _applicationName
            });
            _updateData = new List<ValueRange>();
        }

        public bool AlreadyDoneForToday()
        {
            var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, $"weather!B1:D1");
            var response = request.Execute();
            if (response.Values == null)
            {
                _lastRowWritten = 2;
                return false;
            }
            else
            {
                var row = response.Values[0];
                _lastDateCovered = DateTime.ParseExact(row[0].ToString(), "dd.MM.yy", null);
                _lastRowWritten = Convert.ToInt32(row[2]);
                return _lastDateCovered == DateTime.Today;
            }
        }
        public virtual string Export(List<IList<object>> data)
        {
            string valueInputOption = "USER_ENTERED";

            var headerRange = new ValueRange();
            headerRange.Range = $"weather!A1:G2";
            _lastDateCovered = DateTime.Today;
            var statusRow = new List<object> { "last change :", _lastDateCovered.ToString("dd.MM.yy"), "total records :", _lastRowWritten + data.Count  };
            headerRange.Values = new List<IList<object>>();
            headerRange.Values.Insert(0, statusRow);
            var header = new List<object> { "date", "town", "hi T(°C)", "sunshine (H)", "rain(mm)", "snow(cm)", "wind(km/h)" };
            headerRange.Values.Insert(1, header);
            _updateData.Add(headerRange);

            var dataValueRange = new ValueRange();
            var nextRange = $"weather!A{_lastRowWritten + 1}:Y10000";
            dataValueRange.Range = nextRange;
            dataValueRange.Values = data;
            _updateData.Add(dataValueRange);

            BatchUpdateValuesRequest requestBody = new BatchUpdateValuesRequest();
            requestBody.ValueInputOption = valueInputOption;
            requestBody.Data = _updateData;
            var request = _sheetsService.Spreadsheets.Values.BatchUpdate(requestBody, _spreadsheetId);

            BatchUpdateValuesResponse response = request.Execute();
            // Data.BatchUpdateValuesResponse response = await request.ExecuteAsync(); // For async 

            return JsonConvert.SerializeObject(response);
        }
    }
}
