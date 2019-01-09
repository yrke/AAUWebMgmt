using ITSWebMgmt.Helpers;
using System;
using System.DirectoryServices;
using System.Text;

namespace ITSWebMgmt
{
    public class RawADGridGenerator
    {

        public string buildRawSegment(DirectoryEntry result)
        {
            //builder.Append((result.Properties["aauStaffID"][0])).ToString();
            var builder = new StringBuilder();

            builder.Append("<table class=\"ui celled structured table\"><thead><tr><th>Key</th><th>Value</th></tr></thead><tbody>");

            foreach (string k in result.Properties.PropertyNames)
            {
                //Don't display admin password in raw data
                if (k.Equals("ms-Mcs-AdmPwd"))
                {
                    continue;
                }

                builder.Append("<tr>");
                
                var a = result.Properties[k];
                if (a != null && a.Count > 0)
                {
                    builder.Append("<td rowspan=\""+a.Count+"\">" + k + "</td>");

                    var v = a[0];
                    if (v.GetType().Equals(typeof(DateTime)))
                    {
                        v = DateTimeConverter.Convert((DateTime)v);
                    }

                    if (a.Count == 1)
                    {
                        builder.Append("<td>" + v + "</td></tr>");
                    }
                    else
                    {
                        builder.Append("<td>" + v + "</td>");
                        for (int i = 1; i < a.Count; i++)
                        {
                            v = a[i];
                            if (v.GetType().Equals(typeof(DateTime)))
                            {
                                v = DateTimeConverter.Convert((DateTime)v);
                            }
                            builder.Append("<tr><td>" + v + "</td></tr>");
                        }

                    }

                }
                else
                {
                    builder.Append("<td></td></tr>");
                }

            }

            builder.Append("</tbody></table>");

            return builder.ToString();
        }


    }
}