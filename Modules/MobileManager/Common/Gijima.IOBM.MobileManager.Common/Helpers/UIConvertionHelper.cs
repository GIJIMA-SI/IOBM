using System;

namespace Gijima.IOBM.MobileManager.Common.Helpers
{
    public class UIConvertionHelper
    {
        public UIConvertionHelper()
        { }

        /// <summary>
        /// Converts CM to pixels
        /// </summary>
        /// <param name="CM"></param>
        /// <returns></returns>
        public string ConvertCmToPixels(double CM)
        {
            return Convert.ToString(Math.Round(CM * 37.8, 0));
        }
    }
}
