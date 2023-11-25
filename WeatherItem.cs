using Microsoft.EntityFrameworkCore;

namespace net.zmau.weatherarchiver
{
    enum PrecipitationType
    {
        Rain,
        Snow,
        Unknown
    }
    class PrecipitationData
    {
        public PrecipitationType Type { get; set; }
        public int Amount { get; set; }
        public PrecipitationData()
        {
            Type = PrecipitationType.Unknown;
        }
        public PrecipitationData(PrecipitationType type, int amount) {
            this.Type = type;
            this.Amount = amount;
        }

        public string Unit { 
            get { 
                switch(Type)
                {
                    case PrecipitationType.Rain: return "mm";
                    case PrecipitationType.Snow: return "cm";
                    default: return "";
                }
               
            }
        }
    }

    [PrimaryKey(nameof(Town), nameof(Date))]
    internal class WeatherItem
    {
        public string Town { get; set; }
        public DateTime Date { get; set; }
        public int HiTemp { get; set; }
        public int SunshineHours { get; set; }
        public int RainMM { get; set; }
        public int SnowCM { get; set; }
        public int WindSpeed { get; set; }

        public override string ToString()
        {
            return $"{Town} {Date.ToString("dd.MM.")} {HiTemp}°C {SunshineHours}h of sun {RainMM}mm {WindSpeed}km/h wind";
        }

        public List<Object> AsSheetRow()
        {
            return new List<Object>() { Date.ToString("dd.MM.yy"), Town, HiTemp.ToString(), SunshineHours.ToString(), RainMM.ToString(), SnowCM.ToString(), WindSpeed.ToString() };
        }
    }
}
