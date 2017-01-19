using System;
using System.Windows.Data;

namespace Gijima.IOBM.MobileManager.Common
{
    public class UIDataConvertionHelper : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string val = "";
            String direction = parameter as string;

            if (value != null)
            {
                if (value.GetType() == typeof(System.Boolean))
                    val = bool.Parse(value.ToString()) == true ? "1" : "0";
                else
                    val = value.ToString();
            }

            if (direction == "State")
            {
                return val == "1" ? "Active" : "In-Active";
            }

            if (direction == "StatusLink")
            {
                switch ((StatusLink)Enum.Parse(typeof(StatusLink), val))
                {
                    case StatusLink.Contract:
                        return StatusLink.Contract.ToString();
                    case StatusLink.Device:
                        return StatusLink.Device.ToString();
                    case StatusLink.Simm:
                        return StatusLink.Simm.ToString();
                    default:
                        return "All";
                }
            }

            if (direction == "PackageType")
            {
                switch ((PackageType)Enum.Parse(typeof(PackageType), val))
                {
                    case PackageType.VOICE:
                        return PackageType.VOICE.ToString();
                    case PackageType.DATA:
                        return PackageType.DATA.ToString();
                    default:
                        return PackageType.DATA.ToString();
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
