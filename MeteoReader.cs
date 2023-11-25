using HtmlAgilityPack;
using System.Configuration;

namespace net.zmau.weatherarchiver;

internal class MeteoReader
{
    string basicUrl = "https://www.meteoblue.com/en/weather/week/";
    private HttpClient _httpClient;
    private HtmlDocument _htmlDocument;
    private List<WeatherItem> _weather;
    private string _errorMessage;

    public MeteoReader() { 
        _weather = new List<WeatherItem>();
        _htmlDocument = new HtmlDocument();
        _httpClient = new HttpClient();
    }
    public async Task ReadAsync()
    {
        string[] towns = ConfigurationManager.AppSettings["towns"].Split("|");
        foreach(string townCode in towns)
        {
            var townWeatherItem = await ReadForTown(townCode);
            if(townWeatherItem != null)
            {
                _weather.Add(townWeatherItem);
            }
        }
    }

    public List<WeatherItem> WeatherAsList
    {
        get { return _weather; }
    }

    public List<IList<object>> WeatherForSheet { 
        get {
            List<IList<object>> weatherForSheet = new List<IList<object>>();
            _weather.ForEach(item => weatherForSheet.Add(item.AsSheetRow()));
            return weatherForSheet; 
        } 
    }
    private async Task<WeatherItem?> ReadForTown(string TownCode)
    {
        try
        {
            var townURL = basicUrl + TownCode;
            HttpResponseMessage response = await _httpClient.GetAsync(townURL);

            if (response.IsSuccessStatusCode)
            {
                WeatherItem weatherItem = new WeatherItem();
                weatherItem.Town = GetTownName(TownCode);
                weatherItem.Date = DateTime.Today;
                string htmlContent = await response.Content.ReadAsStringAsync();
                _htmlDocument.LoadHtml(htmlContent);
                weatherItem.SunshineHours = readDiv("tab-sun", "h");
                weatherItem.HiTemp = readDiv("tab-temp-max", "&nbsp;°C");
                
                var precipitation = readPrecDiv();
                if (precipitation.Type == PrecipitationType.Rain)
                    weatherItem.RainMM = precipitation.Amount;
                else if (precipitation.Type == PrecipitationType.Snow)
                    weatherItem.SnowCM = precipitation.Amount;

                weatherItem.WindSpeed = readRangeDiv("wind", "km/h");
                return weatherItem;
            }
            else
            {
                Console.WriteLine($"Neuspeli zahtev. Kod odgovora: {response.StatusCode}");
                return null;
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Error. Town {TownCode}; Message : {ex.Message}";
            return null;
        }
    }

    private int readDiv(string className, string addendum)
    {
        var valueAsString = readDivAsString(className, addendum);
        return Convert.ToInt32(valueAsString);
    }
    private PrecipitationData readPrecDiv()
    {
        string valueAsString = "";
        try
        {
            PrecipitationData precData = new PrecipitationData();
            HtmlNode node = _htmlDocument.DocumentNode.SelectSingleNode($"//div[@class='tab-precip']");
            if (node != null)
            {
                if (node.Attributes["title"].Value == "Snowfall")
                    precData.Type = PrecipitationType.Snow;
                else precData.Type = PrecipitationType.Rain;

                var text = node.InnerText.Trim();
                valueAsString = text.Replace(precData.Unit, "").Trim();
                if (valueAsString == "-")
                    precData.Amount = 0;
                if (valueAsString.Contains("-"))
                {
                    string[] values = valueAsString.Split("-");
                    var min = Convert.ToInt32(values[0]);
                    var max = Convert.ToInt32(values[1]);
                    precData.Amount = (min + max) / 2;
                }
                else precData.Amount = Convert.ToInt32(valueAsString);
            }
            else throw new Exception($"No data regarding precipitation");
            return precData;
        }
        catch (FormatException)
        {
            throw new FormatException($"Precipitation, {valueAsString}");
        }
    }

    private int readRangeDiv(string className, string addendum)
    {
        var valueAsString = readDivAsString(className, addendum);
        if (valueAsString == "-")
            return 0;
        if (valueAsString.Contains("-"))
        {
            string[] values = valueAsString.Split("-");
            var min = Convert.ToInt32(values[0]);
            var max = Convert.ToInt32(values[1]);
            return (min+max)/2;
        }
        return Convert.ToInt32(valueAsString);
    }
    private string readDivAsString(string className, string addendum)
    {
        HtmlNode node = _htmlDocument.DocumentNode.SelectSingleNode($"//div[@class='{className}']");
        if (node != null)
        {
            string valueAsString = node.InnerText.Trim().Replace(addendum, "").Trim();
            return valueAsString;
        }
        else throw new Exception($"No data regarding {className}");
    }

    private string GetTownName(string TownCode)
    {
        int underscorePosition = TownCode.IndexOf("_");
        return TownCode.Substring(0, underscorePosition);
    }
}
