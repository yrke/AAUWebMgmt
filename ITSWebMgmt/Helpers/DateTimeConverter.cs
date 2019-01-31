using System;
using System.Management;

namespace ITSWebMgmt.Helpers
{
    public class DateTimeConverter
    {
        public static string Convert(DateTime? date)
        {
            if (date != null)
            {
                return ((DateTime)date).ToString("yyyy-MM-dd HH:mm:ss");
            }
            return "Date not found";
        }

        public static string Convert(long rawDate)
        {
            DateTime date = DateTime.FromFileTime(rawDate);
            return Convert(date);
        }

        public static string Convert(string CIM_DATETIME)
        {
            DateTime date = ManagementDateTimeConverter.ToDateTime(CIM_DATETIME);
            return Convert(date);
        }

        public static string Convert(object adsLargeInteger)
        {
            var highPart = (Int32)adsLargeInteger.GetType().InvokeMember("HighPart", System.Reflection.BindingFlags.GetProperty, null, adsLargeInteger, null);
            var lowPart = (Int32)adsLargeInteger.GetType().InvokeMember("LowPart", System.Reflection.BindingFlags.GetProperty, null, adsLargeInteger, null);
            var result = highPart * ((Int64)UInt32.MaxValue + 1) + lowPart;

            if (result == 9223372032559808511)
            {
                return null;
            }

            DateTime date = DateTime.FromFileTime(result);
            return Convert(date);
        }
    }
}