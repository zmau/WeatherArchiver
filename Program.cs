namespace net.zmau.weatherarchiver
{
    internal class Program
    {
        // Written in collaboration with Chat GPT, for curiosity's sake.
        static async Task Main(string[] args)
        {
            SpreadsheetCommunicator spreadsheetCommunicator = new SpreadsheetCommunicator();
            if (!spreadsheetCommunicator.AlreadyDoneForToday())
            {
                MeteoReader reader = new MeteoReader();
                await reader.ReadAsync();
                //await            
                spreadsheetCommunicator.Export(reader.WeatherForSheet);
                DBArchiver.WriteToDB(reader.WeatherAsList);
            }
        }
    }
}