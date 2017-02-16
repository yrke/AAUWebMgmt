using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ITSWebMgmt
{
    public partial class IDtoSystem : System.Web.UI.Page
    {


        protected void printChangeInformation(string id)
        {

            SqlConnection myConnection = new SqlConnection("Data Source = ad-sql2-misc.aau.dk; Initial Catalog = webmgmt; Integrated Security=SSPI;");
            myConnection.Open();
            string command = "SELECT TOP (1) [ChangeID] ,[Navn] ,[Beskrivelse] ,[Start] ,[Slut] ,[Ansvarlig] FROM[webmgmt].[dbo].[Changes] WHERE changeid like @ChangeID";

            SqlCommand readSQLCommand = new SqlCommand(command, myConnection);
            readSQLCommand.Parameters.AddWithValue("@ChangeID", id);
            var reader = readSQLCommand.ExecuteReader();
            reader.Read();
            //string f = reader["ChangeID"].ToString();


            StringBuilder resultstring = new StringBuilder();


            resultstring.Append("<h1>Change "+ reader["ChangeID"].ToString() + "</h1>");

            
            resultstring.Append("");
            resultstring.Append("<br/>");
            resultstring.Append("<b>" + reader["Navn"].ToString() + "</b>");
            resultstring.Append("<br/>");
            resultstring.Append(reader["Beskrivelse"].ToString());
            resultstring.Append("<br/>");
            resultstring.Append("<b>Start dato:</b> " + reader["Start"].ToString());
            resultstring.Append("<br/>");
            resultstring.Append("<b> End dato:</b>  " +reader["Slut"].ToString());
            resultstring.Append("<br/>");
            resultstring.Append("<b>Ansvarlig:</b> " + reader["Ansvarlig"].ToString());

            result.Text = resultstring.ToString();

            myConnection.Close();

        }


        protected void Page_Load(object sender, EventArgs e)
        {

            string id = Request.QueryString["id"] ?? ""; // ?? "": return empty string if null value


            if (id.StartsWith("IR", StringComparison.CurrentCultureIgnoreCase)){
                //Is incident
                Response.Redirect("https://service.aau.dk/Incident/Edit/" + id);
            }
            else if (id.StartsWith("SR", StringComparison.CurrentCultureIgnoreCase)) {
                Response.Redirect("https://service.aau.dk/ServiceRequest/Edit/" + id);
            }
            else if (id.StartsWith("C-", StringComparison.CurrentCultureIgnoreCase)) {
                printChangeInformation(id);
            }
            else if (id.StartsWith("EC-", StringComparison.CurrentCultureIgnoreCase)) {
                printChangeInformation(id);
            }
            else if (id.StartsWith("SC-", StringComparison.CurrentCultureIgnoreCase)) {
                printChangeInformation(id);
            }

            else {
                result.Text = "<h1>Type not found</h1>";
            }


        }




    }
}