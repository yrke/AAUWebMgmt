using System;
using System.Text;

namespace ITSWebMgmt
{
    public class HTMLTableHelper
    {
        private StringBuilder table = new StringBuilder();
        private bool tableEnded = false;

        int colums;
        public HTMLTableHelper(int colums, string[] headers)
        {
            this.colums = colums;
            table.Append("<table class=\"ui celled table\">");

            if (!(headers.Length == this.colums)) { throw new ArgumentOutOfRangeException(); };

            table.Append("<thead>");
            AddRow(headers);
            table.Append("</thead>");
        }

        public void AddRow(string[] args)
        {
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