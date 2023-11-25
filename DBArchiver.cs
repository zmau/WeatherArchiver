namespace net.zmau.weatherarchiver
{
    internal class DBArchiver
    {
        public static void WriteToDB(List<WeatherItem> weatherItems) {
            using (var context = new WeatherDBContext())
            {
                context.Database.EnsureCreated();
                context.Weather.AddRange(weatherItems);
                context.SaveChanges();
            }
        }
    }
}
