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
    }
}