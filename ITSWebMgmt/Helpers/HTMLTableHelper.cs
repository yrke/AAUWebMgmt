using System;
using System.Text;

namespace ITSWebMgmt
{
    public class HTMLTableHelper
    {

        int colums;
        public HTMLTableHelper(int colums)
        {
            this.colums = colums;
        }

        public string printStart()
        {
            return "<table class=\"ui celled table\">";
        }

        public string printRow(string[] args, bool isHeader) {

            if (!(args.Length == this.colums)) { throw new ArgumentOutOfRangeException(); };
            
            if (isHeader)
            {
                var sb = new StringBuilder();
                sb.Append("<thead>");
                sb.Append(printRow(args));
                sb.Append("</thead>");
                return sb.ToString();
            }
            else
            {
                return printRow(args);
            }
        
        }
        

        public string printRow(params string[] args)
        {
            var sb = new StringBuilder();
            if (!(args.Length == this.colums)) { throw new ArgumentOutOfRangeException(); };
            sb.Append("<tr>");

            foreach (string s in args)
            {
                sb.Append("<td>");
                sb.Append(s);
                sb.Append("</td>");
            }

            sb.Append("</tr>");
            return sb.ToString();
        }

        public string printEnd()
        {
            return "</table>";
        }


    }
}