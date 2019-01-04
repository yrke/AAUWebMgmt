using System;
using System.Text;

namespace ITSWebMgmt
{
    public class HTMLTableHelper
    {
        private StringBuilder table = new StringBuilder();
        private bool tableEnded = false;
        private int colums;

        public HTMLTableHelper(int columbs)
        {
            colums = columbs;
            table.Append("<table class=\"ui celled table\">");
        }

        public HTMLTableHelper(string[] headers)
        {
            colums = headers.Length;
            table.Append("<table class=\"ui celled table\">");

            table.Append("<thead>");
            AddRow(headers);
            table.Append("</thead>");
        }

        public void AddRow(string[] args)
        {
            if (!(args.Length == colums)) { throw new ArgumentOutOfRangeException(); };

            table.Append("<tr>");

            foreach (string s in args)
            {
                table.Append("<td>");
                table.Append(s);
                table.Append("</td>");
            }

            table.Append("</tr>");
        }

        public void EndTable()
        {
            if (!tableEnded)
                table.Append("</table>");
        }

        public string GetTable()
        {
            if (!tableEnded)
                EndTable();
            return table.ToString();    
        }
    }
}