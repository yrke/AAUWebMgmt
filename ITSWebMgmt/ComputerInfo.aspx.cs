using ITSWebMgmt.Connectors.Active_Directory;
using ITSWebMgmt.Controllers;
using ITSWebMgmt.Helpers;
using ITSWebMgmt.WebMgmtErrors;
using System;
using System.Collections.Generic;
using System.Management;
using System.Web;
using System.Web.UI.WebControls;

namespace ITSWebMgmt
{
    public partial class ComputerInfo : System.Web.UI.Page
    {
        protected string ComputerName = "ITS\\AAU114811"; //Test computer
        private ComputerController computer;

        public void Page_Init(object o, EventArgs e)
        {

        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ResultGetPassword.Visible = false;
            ResultGetPassword2.Visible = false;
            MoveComputerOUdiv.Visible = false;
            ResultLabel.Text = "";

            ResultDiv.Visible = false;

            if (!IsPostBack)
            {
                string computername = Request.QueryString["computername"];

                if (computername != null)
                {
                    ComputerName = HttpUtility.HtmlEncode(computername);
                    computer = new ComputerController(ComputerName, HttpContext.Current.User.Identity.Name);
                    buildlookupComputer();

                    Session["adpath"] = computer.adpath;
                    Session["computer"] = computer;
                }
            }
            else
            {
                computer = (ComputerController)Session["computer"];
                buildlookupComputer();
            }
        }

        protected void buildlookupComputer()
        {
            if (!computer.ComputerFound)
            {
                ResultLabel.Text = "Computer Not Found";
                return;
            }

            labelDomain.Text = computer.Domain;

            if (computer.AdminPasswordExpirationTime != null)
            {
                labelPwdExpireDate.Text = computer.AdminPasswordExpirationTime;
            }
            else
            {
                labelPwdExpireDate.Text = "LAPS not Enabled";
            }

            ResultLabelRaw.Text = TableGenerator.buildRawTable(computer.getAllProperties());

            buildBasicInfo();

            //XXX check resourceID 
            buildSCCMInfo();
            buildSCCMInventory();
            buildSCCMAntivirus();
            biuldSCCMHardware();
            buildGroupsSegments();
            buildWarningSegment();

            ResultDiv.Visible = true;

            if (!computer.checkComputerOU())
            {
                MoveComputerOUdiv.Visible = true;
            }
        }

        protected void buildBasicInfo()
        {
            if (computer.AdminPasswordExpirationTime != null)
            {
                labelPwdExpireDate.Text = computer.AdminPasswordExpirationTime;
            }
            else
            {
                labelPwdExpireDate.Text = "LAPS not Enabled";
            }

            //Managed By
            labelManagedBy.Text = "none";

            if (computer.ManagedBy != null)
            {
                string managerVal = computer.ManagedBy;

                if (!string.IsNullOrWhiteSpace(managerVal))
                {
                    string email = ADHelpers.DistinguishedNameToUPN(managerVal);
                    labelManagedBy.Text = email;
                }
            }

            if (computer.AdminPasswordExpirationTime != null)
            {
                ResultGetPassword.Visible = true;
                ResultGetPassword2.Visible = true;
            }
        }

        protected void MoveOU_Click(object sender, EventArgs e)
        {
            computer.moveOU(HttpContext.Current.User.Identity.Name);
        }

        protected void EditManagedBy_Click(object sender, EventArgs e)
        {
            labelManagedByText.Text = labelManagedBy.Text;
            tuggleVisibility();
        }

        protected void SaveEditManagedBy_Click(object sender, EventArgs e)
        {
            ManagedByChanger managedByChanger = new ManagedByChanger(computer.ADcache);
            managedByChanger.SaveEditManagedBy(labelManagedByText.Text);
            labelManagedByError.Text = managedByChanger.ErrorMessage;
            if (managedByChanger.ErrorMessage == "")
            {
                tuggleVisibility();
                labelManagedBy.Text = labelManagedByText.Text;
            }
        }

        private void tuggleVisibility()
        {
            labelManagedBy.Visible = !labelManagedBy.Visible;
            EditManagedByButton.Visible = !EditManagedByButton.Visible;
            labelManagedByText.Visible = !labelManagedByText.Visible;
            SaveEditManagedByButton.Visible = !SaveEditManagedByButton.Visible;
        }

        protected void ResultGetPassword_Click(object sender, EventArgs e)
        {
            string adpath = (string)Session["adpath"];
            ComputerController.logger.Info("User {0} requesed localadmin password for computer {1}", System.Web.HttpContext.Current.User.Identity.Name, adpath);
            
            var passwordRetuned = ComputerController.getLocalAdminPassword(adpath);

            if (string.IsNullOrEmpty(passwordRetuned))
            {
                ResultLabel.Text = "Not found";
            }
            else
            {
                Func<string, string, string> appendColor = (string x, string color) => { return "<font color=\"" + color + "\">" + x + "</font>"; };

                string passwordWithColor = "";
                foreach (char c in passwordRetuned)
                {
                    var color = "green";
                    if (char.IsNumber(c))
                    {
                        color = "blue";
                    }

                    passwordWithColor += appendColor(c.ToString(), color);

                }

                ResultLabel.Text = "<code>" + passwordWithColor + "</code><br /> Password will expire in 8 hours";
            }

            ResultGetPassword.Visible = false;
        }

        private void buildGroupsSegments()
        {
            //XXX is memeber of an attribute
            TableGenerator.BuildGroupsSegments(computer.getGroups("memberOf"), computer.getGroupsTransitive("memberOf"), groupssegmentLabel, groupsAllsegmentLabel);
        }

        protected void buildSCCMInventory()
        {
            var tableAndList = TableGenerator.CreateTableAndRawFromDatabase(computer.Computer, new List<string>() { "Manufacturer", "Model", "SystemType", "Roles" }, "No inventory data");
            labelSSCMInventoryTable.Text = tableAndList.Item1; //Table
            labelSCCMCollecionsSoftware.Text = TableGenerator.CreateTableFromDatabase(computer.Software,
                new List<string>() { "SoftwareCode", "ProductName", "ProductVersion", "TimeStamp" },
                new List<string>() { "Product ID", "Name", "Version", "Install date" },
                "Software information not found");
            labelSCCMInventory.Text += tableAndList.Item2; //List
        }

        protected void buildSCCMInfo()
        {
            /*
             *     strQuery = "SELECT * FROM SMS_FullCollectionMembership WHERE ResourceID="& computerID 
                    for each fc in foundCollections
                       Set collection = SWbemServices.Get ("SMS_Collection.CollectionID=""" & fc.CollectionID &"""")
                       stringResult = stringResult & "<li> "  & collection.Name & "<br />"
                Next
            
             * SMS_Collection.CollectionID = 
             * 
             */

            //XXX: remeber to filter out computers that are obsolite in sccm (not active)
            string error = "";
            HTMLTableHelper groupTableHelper = new HTMLTableHelper(new string[] { "Collection Name" });
            var names = computer.setConfig();
            
            if (names != null)
            {
                foreach (var name in names)
                {
                    groupTableHelper.AddRow(new string[] { name });
                }
            }
            else
            {
                error = "Computer not found i SCCM";
            }

            labelBasicInfoPCConfig.Text = computer.ConfigPC;
            labelBasicInfoExtraConfig.Text = computer.ConfigExtra;

            //Basal Info
            var tableAndList = TableGenerator.CreateTableAndRawFromDatabase(computer.System, new List<string>() { "LastLogonUserName", "IPAddresses", "MACAddresses", "Build", "Config" }, "Computer not found i SCCM");

            labelSCCMComputers.Text = error + groupTableHelper.GetTable();
            labelSCCMCollecionsTable.Text = tableAndList.Item1; //Table
            labelSCCMCollections.Text = tableAndList.Item2; //List
        }

        protected void buildSCCMAntivirus()
        {
            //DetectionID is required for UserName (SELECT * FROM SMS_G_System_Threats WHERE DetectionID='{04155F79-EB84-4828-9CEC-AC0749C4EDA6}' AND ResourceID=16787705)
            //Only few computers with data, one them is AAU112782
            labelSCCMAV.Text = TableGenerator.CreateTableFromDatabase(computer.Antivirus,
                new List<string>() { "ThreatName", "PendingActions", "Process", "SeverityID", "Path" },
                "Antivirus information not found");
        }

        protected void biuldSCCMHardware()
        {
            labelSCCMLD.Text = TableGenerator.CreateVerticalTableFromDatabase(computer.LogicalDisk,
                new List<string>() { "DeviceID", "FileSystem", "Size", "FreeSpace" },
                new List<string>() { "DeviceID", "File system", "Size (GB)", "FreeSpace (GB)" },
                "Disk information not found");

            if (Database.HasValues(computer.RAM))
            {
                int total = 0;
                int count = 0;

                foreach (ManagementObject o in computer.RAM) //Has one!
                {
                    total += int.Parse(o.Properties["Capacity"].Value.ToString()) / 1024;
                    count++;
                }

                labelSCCMRAM.Text = $"{total} GB RAM in {count} slot(s)";
            }
            else
            {
                labelSCCMRAM.Text = "RAM information not found";
            }

            labelSCCMBIOS.Text = TableGenerator.CreateVerticalTableFromDatabase(computer.BIOS,
                new List<string>() { "BIOSVersion", "Description", "Manufacturer", "Name", "SMBIOSBIOSVersion" },
                "BIOS information not found");

            labelSCCMVC.Text = TableGenerator.CreateVerticalTableFromDatabase(computer.VideoController,
                new List<string>() { "AdapterRAM", "CurrentHorizontalResolution", "CurrentVerticalResolution", "DriverDate", "DriverVersion", "Name" },
                "Video controller information not found");

            labelSCCMProcessor.Text = TableGenerator.CreateVerticalTableFromDatabase(computer.Processor,
                new List<string>() { "Is64Bit", "IsMobile", "IsVitualizationCapable", "Manufacturer", "MaxClockSpeed", "Name", "NumberOfCores", "NumberOfLogicalProcessors" },
                "Processor information not found");

            labelSCCMDisk.Text = TableGenerator.CreateVerticalTableFromDatabase(computer.Disk,
                new List<string>() { "Caption", "Model", "Partitions", "Size", "Name" },
                "Video controller information not found");

        }

        protected void buttonEnableBitlockerEncryption_Click(object sender, EventArgs e)
        {
            computer.EnableBitlockerEncryption();
        }

        private void buildWarningSegment()
        {
            List<WebMgmtError> errors = new List<WebMgmtError>
            {
                new DriveAlmostFull(computer)
            };

            var errorList = new WebMgmtErrorList(errors);
            ErrorCountMessageLabel.Text = errorList.getErrorCountMessage();
            ErrorMessagesLabel.Text = errorList.ErrorMessages;

            //Password is expired and warning before expire (same timeline as windows displays warning)
        }
    }
}