using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;

namespace ITSWebMgmt.Helpers
{
    public partial class ActiveChanges : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {


            SqlConnection myConnection = new SqlConnection("Data Source = ad-sql2-misc.aau.dk; Initial Catalog = webmgmt; Integrated Security=SSPI;");
            try
            {
                myConnection.Open();
                string command = @"SELECT TOP (1000) [ChangeID]
      ,[Navn]
      ,[Beskrivelse]
      ,[Start]
      ,[Slut]
      ,[Ansvarlig]
  FROM[webmgmt].[dbo].[Changes] where Try_CONVERT(datetime, Start) < CURRENT_TIMESTAMP  and Try_CONVERT(datetime, Slut) > CURRENT_TIMESTAMP";

                SqlCommand readSQLCommand = new SqlCommand(command, myConnection);
                //readSQLCommand.Parameters.AddWithValue("@ChangeID", id);
                //var reader = readSQLCommand.ExecuteReader();
                //reader.Read();
                //string f = reader["ChangeID"].ToString();

                bool hasValues = false;
                StringBuilder sb = new StringBuilder();
                using (var reader = readSQLCommand.ExecuteReader())
                {



                    foreach (DbDataRecord r in reader)
                    {
                        hasValues = true;
                        string changeid = r["ChangeID"].ToString();
                        sb.Append("<a href=\"https://srv-webmgmt01.srv.aau.dk/IDtoSystem.aspx?id=" + changeid + "\">" + changeid + "</a>: " + r["Navn"].ToString() + "<br/>");
                    }
                }




                /*
                resultstring.Append("<h1>Change " + reader["ChangeID"].ToString() + "</h1>");


                resultstring.Append("");
                resultstring.Append("<br/>");
                resultstring.Append("<b>" + reader["Navn"].ToString() + "</b>");
                resultstring.Append("<br/>");
                resultstring.Append(reader["Beskrivelse"].ToString());
                resultstring.Append("<br/>");
                resultstring.Append("<b>Start dato:</b> " + reader["Start"].ToString());
                resultstring.Append("<br/>");
                resultstring.Append("<b> End dato:</b>  " + reader["Slut"].ToString());
                resultstring.Append("<br/>");
                resultstring.Append("<b>Ansvarlig:</b> " + reader["Ansvarlig"].ToString());
                */




                string markup = @"
                <div class=""ui warning message"" id=""errorUserDisabled"" >
                <div class=""header"">
                    Active Changes
                </div>
                <p>@@message@@</p>
            </div>
            ";

                if (hasValues)
                {


                    var message = markup.Replace("@@message@@", sb.ToString());


                    result.Text = message;

                }





                myConnection.Close();
            }
            catch (SqlException ex)
            {

            }


        }
    }
}