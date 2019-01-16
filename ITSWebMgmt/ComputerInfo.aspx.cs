using ITSWebMgmt.Connectors.Active_Directory;
using ITSWebMgmt.Controllers.Computer;
using ITSWebMgmt.Helpers;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Management;
using System.Text;
using System.Web;

namespace ITSWebMgmt
{
    public partial class ComputerInfo : System.Web.UI.Page
    {
        protected string ComputerName = "ITS\\AAU804396";
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
                    computer = new ComputerController(ComputerName);
                    buildlookupComputer();
                }
            }
            else
            {

            }

        }

        protected void buildlookupComputer()
        {
            var resultLocal = computer.GetSearch(HttpContext.Current.User.Identity.Name);

            if (resultLocal == null)
            {
                ResultLabel.Text = "Computer Not Found";
                return;
            }

            labelDomain.Text = computer.Domain;

            if (resultLocal.Properties.Contains("ms-Mcs-AdmPwdExpirationTime"))
            {
                long rawDate = (long)resultLocal.Properties["ms-Mcs-AdmPwdExpirationTime"][0];
                labelPwdExpireDate.Text = DateTimeConverter.Convert(rawDate);
            }
            else
            {
                labelPwdExpireDate.Text = "LAPS not Enabled";
            }

            var rawbuilder = new RawADGridGenerator();
            ResultLabelRaw.Text = rawbuilder.buildRawSegment(resultLocal.GetDirectoryEntry());

            buildBasicInfo(resultLocal.GetDirectoryEntry());

            //XXX check resourceID 
            buildSCCMInfo();
            buildSCCMInventory();
            buildSCCMAntivirus();
            biuldSCCMHardware();

            buildGroupsSegments(resultLocal.GetDirectoryEntry());

            ResultDiv.Visible = true;

            if (!computer.checkComputerOU())
            {
                MoveComputerOUdiv.Visible = true;
            }
        }

        protected void buildBasicInfo(DirectoryEntry de)
        {
            if (de.Properties.Contains("ms-Mcs-AdmPwdExpirationTime"))
            {
                DateTime? expireDate = ADHelpers.convertADTimeToDateTime(de.Properties["ms-Mcs-AdmPwdExpirationTime"].Value);
                labelPwdExpireDate.Text = DateTimeConverter.Convert(expireDate);
            }
            else
            {
                labelPwdExpireDate.Text = "LAPS not Enabled";
            }

            //Managed By
            labelManagedBy.Text = "none";

            if (de.Properties.Contains("managedBy"))
            {
                string managerVal = de.Properties["managedBy"].Value.ToString();

                if (!string.IsNullOrWhiteSpace(managerVal))
                {
                    string email = ADHelpers.DistinguishedNameToUPN(managerVal);
                    labelManagedBy.Text = email;
                }
            }

            //builder.Append((string)Session["adpath"]);

            if (de.Properties.Contains("ms-Mcs-AdmPwdExpirationTime"))
            {
                ResultGetPassword.Visible = true;
                ResultGetPassword2.Visible = true;
            }
        }

        protected void MoveOU_Click(object sender, EventArgs e)
        {
            computer.moveOU(HttpContext.Current.User.Identity.Name);
        }

        protected void ResultGetPassword_Click(object sender, EventArgs e)
        {
            ComputerController.logger.Info("User {0} requesed localadmin password for computer {1}", System.Web.HttpContext.Current.User.Identity.Name, computer.adpath);

            var passwordRetuned = computer.getLocalAdminPassword();

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

        private void buildGroupsSegments(DirectoryEntry result)
        {
            //XXX is memeber of an attribute
            TableGenerator.BuildGroupsSegments("memberOf", result, groupssegmentLabel, groupsAllsegmentLabel);
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
    }
}