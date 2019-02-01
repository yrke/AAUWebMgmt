using System;
using System.Web.UI.WebControls;

namespace ITSWebMgmt
{
    public partial class SoftwareManager : System.Web.UI.Page
    {

        protected override void OnInit(EventArgs e)
        {

            if (!IsPostBack) { 
            /*Button button = new Button();
            button.Click += (se, ev) => { fisk.Text = idem("1"); };
            test.Controls.Add(button);*/
            Button button2 = new Button();
            button2.Click += (se, ev) => { fisk.Text = idem("2"); };
            test.Controls.Add(button2);
            Button button3 = new Button();
            button3.Click += (se, ev) => { fisk.Text = idem("3"); };
            test.Controls.Add(button3);
            }else
            {
                Button button = new Button();
                button.Click += (se, ev) => { fisk.Text = idem("1"); };
                test.Controls.Add(button);
                Button button2 = new Button();
                button2.Click += (se, ev) => { fisk.Text = idem("2"); };
                test.Controls.Add(button2);
                Button button3 = new Button();
                button3.Click += (se, ev) => { fisk.Text = idem("3"); };
                test.Controls.Add(button3);
            }
        }
         
        protected string idem(string id)
        {
            return "idem" + id;
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        void GreetingBtn_Click(Object sender,
                           EventArgs e)
        {
            // When the button is clicked,
            // change the button text, and disable it.

            Button clickedButton = (Button)sender;
            clickedButton.Text = "...button clicked...";
            clickedButton.Enabled = false;

            // Display the greeting label text.
            
        }

        private void CreateButton()
        {
            var button = new Button();
            button.Text = "test";
            button.Click += new EventHandler(this.GreetingBtn_Click);

            test.Controls.Add(button);
        }
    }
}