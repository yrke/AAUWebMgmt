using System;
using System.Collections.Generic;
using ITSWebMgmt.Controllers;
using ITSWebMgmt.Connectors.Active_Directory;
using ITSWebMgmt.Helpers;
using ITSWebMgmt.WebMgmtErrors;
using System.Management;
using System.Threading;
using System.Web;
using ITSWebMgmt.Caches;

namespace ITSWebMgmt.Models
{
    public class ComputerModel
    {
        //SCCMcache
        private SCCMcache SCCMcache;
        public ManagementObjectCollection RAM { get => SCCMcache.RAM; private set { } }
        public ManagementObjectCollection LogicalDisk { get => SCCMcache.LogicalDisk; private set { } }
        public ManagementObjectCollection BIOS { get => SCCMcache.BIOS; private set { } }
        public ManagementObjectCollection VideoController { get => SCCMcache.VideoController; private set { } }
        public ManagementObjectCollection Processor { get => SCCMcache.Processor; private set { } }
        public ManagementObjectCollection Disk { get => SCCMcache.Disk; private set { } }
        public ManagementObjectCollection Software { get => SCCMcache.Software; private set { } }
        public ManagementObjectCollection Computer { get => SCCMcache.Computer; private set { } }
        public ManagementObjectCollection Antivirus { get => SCCMcache.Antivirus; private set { } }
        public ManagementObjectCollection System { get => SCCMcache.System; private set { } }
        public ManagementObjectCollection Collection { get => SCCMcache.Collection; private set { } }


        //ADcache
        private ComputerADcache ADcache;
        public string ComputerNameAD { get => ADcache.ComputerName; }
        public string Domain { get => ADcache.Domain; }
        public bool ComputerFound { get => ADcache.ComputerFound; }
        public string AdminPasswordExpirationTime { get => ADcache.getProperty("ms-Mcs-AdmPwdExpirationTime"); }
        public string ManagedByAD { get => ADcache.getProperty("managedBy"); set => ADcache.saveProperty("managedBy", value); }
        public string DistinguishedName { get => ADcache.getProperty("distinguishedName"); }
        public string adpath { get => ADcache.adpath; }

        //Display
        public ComputerController computer;
        public string ComputerName = "ITS\\AAU804396";
        public string ManagedBy;
        public string Raw;
        public string Result;
        public string PasswordExpireDate;
        public string SSCMInventoryTable;
        public string SCCMCollecionsSoftware;
        public string SCCMInventory;
        public string BasicInfoPCConfig;
        public string BasicInfoExtraConfig;
        public string SCCMComputers;
        public string SCCMCollectionsTable;
        public string SCCMCollections;
        public string SCCMAV;
        public string SCCMLD;
        public string SCCMRAM;
        public string SCCMBIOS;
        public string SCCMVC;
        public string SCCMProcessor;
        public string SCCMDisk;
        public string ErrorCountMessage;
        public string ErrorMessages;
        public string GroupSegment;
        public string GroupsAllSegment;

        private List<string> build = new List<string>();
        private string ActiveTab = "basicinfo";
        public bool ShowResultDiv = false;
        public bool ShowResultGetPassword = false;
        public bool ShowResultGetPassword2 = false;
        public bool ShowMoveComputerOUdiv = false;

        public ComputerModel(ComputerController controller, string computername)
        {
            if (computername != null)
            {
                computer = controller;
                computer.ComputerModel = this;
                ADcache = new ComputerADcache(computername, computer.ControllerContext.HttpContext.User.Identity.Name);
                SCCMcache = new SCCMcache();
                SCCMcache.ResourceID = getSCCMResourceIDFromComputerName(ComputerNameAD);
                ComputerName = computername;
                buildlookupComputer();
            }
        }

        public string getSCCMResourceIDFromComputerName(string computername)
        {
            string resourceID = "";
            //XXX use ad path to get right object in sccm, also dont get obsolite
            foreach (ManagementObject o in SCCMcache.getResourceIDFromComputerName(computername))
            {
                resourceID = o.Properties["ResourceID"].Value.ToString();
                break;
            }

            return resourceID;
        }

        private void addIfNotContained(string tabName)
        {
            if (!build.Contains(tabName))
            {
                build.Add(tabName);
            }
        }

        public void buildlookupComputer()
        {
            Result = "";

            if (!ComputerFound)
            {
                Result = "Computer Not Found";
                return;
            }

            if (AdminPasswordExpirationTime != null)
            {
                PasswordExpireDate = AdminPasswordExpirationTime;
            }
            else
            {
                PasswordExpireDate = "LAPS not Enabled";
            }

            buildBasicInfo();
            buildSCCMInfo();
            buildWarningSegment();

            List<string> loadedTapNames = new List<string> { "basicinfo", "sccmInfo", "tasks", "warnings" };

            foreach (string TabName in loadedTapNames)
            {
                addIfNotContained(TabName);
            }

            //XXX check resourceID

            // Load the data for the other tabs in background. I am not sure if this work properly if the user presses a tab before all tabs is loaded in the background
            // If not it does not the load time will be longer
            // backgroundLoadedTapNames = "groups", "sccmInventory", "sccmAV", "sccmHW", "rawdata"

            //Load data into ADcache in the background
            ThreadPool.QueueUserWorkItem(_ =>
            {
                ADcache.getGroups("memberOf");
                ADcache.getGroupsTransitive("memberOf");
                ADcache.getAllProperties();
            }, null);

            //Load data into SCCMcache in the background
            ThreadPool.QueueUserWorkItem(_ =>
            {
                SCCMcache.LoadAllIntoCache();
            }, null);

            ShowResultDiv = true;

            if (!computer.checkComputerOU(adpath))
            {
                ShowMoveComputerOUdiv = true;
            }
        }

        protected void TabChanged_Click(object sender, EventArgs e)
        {
            string TabName = "TEST";//TODO fix this
            if (TabName == "groups-all")
            {
                TabName = "groups";
            }
            ActiveTab = TabName;
            LoadTab(TabName);
        }

        protected void LoadTab(string TabName)
        {
            //Do not build if allready build
            if (build.Contains(TabName))
            {
                return;
            }

            switch (TabName)
            {
                case "groups":
                    buildGroupsSegments();
                    break;
                case "sccmInventory":
                    buildSCCMInventory();
                    break;
                case "sccmAV":
                    buildSCCMAntivirus();
                    break;
                case "sccmHW":
                    biuldSCCMHardware();
                    break;
                case "rawdata":
                    Raw = TableGenerator.buildRawTable(computer.getAllProperties());
                    break;
            }

            addIfNotContained(TabName);
        }

        protected void buildBasicInfo()
        {
            if (AdminPasswordExpirationTime != null)
            {
                PasswordExpireDate = AdminPasswordExpirationTime;
            }
            else
            {
                PasswordExpireDate = "LAPS not Enabled";
            }

            //Managed By
            ManagedBy = "none";

            if (ManagedByAD != null)
            {
                string managerVal = ManagedByAD;

                if (!string.IsNullOrWhiteSpace(managerVal))
                {
                    string email = ADHelpers.DistinguishedNameToUPN(managerVal);
                    ManagedBy = email;
                }
            }

            if (AdminPasswordExpirationTime != null)
            {
                ShowResultGetPassword = true;
                ShowResultGetPassword2 = true;
            }
        }

        protected void MoveOU_Click(object sender, EventArgs e)
        {
            computer.moveOU(computer.ControllerContext.HttpContext.User.Identity.Name, adpath);
        }
        
        protected void ResultGetPassword_Click(object sender, EventArgs e)
        {
            ComputerController.logger.Info("User {0} requesed localadmin password for computer {1}", computer.ControllerContext.HttpContext.User.Identity.Name, computer.adpath);

            var passwordRetuned = ComputerController.getLocalAdminPassword(computer.adpath);

            if (string.IsNullOrEmpty(passwordRetuned))
            {
                Result = "Not found";
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

                Result = "<code>" + passwordWithColor + "</code><br /> Password will expire in 8 hours";
            }

            ShowResultGetPassword = false;
        }

        public void buildGroupsSegments()
        {
            //XXX is memeber of an attribute
            GroupSegment = TableGenerator.BuildgroupssegmentLabel(ADcache.getGroups("memberOf"));
            GroupsAllSegment = TableGenerator.BuildgroupssegmentLabel(ADcache.getGroupsTransitive("memberOf"));
        }

        public void buildSCCMInventory()
        {
            var tableAndList = TableGenerator.CreateTableAndRawFromDatabase(Computer, new List<string>() { "Manufacturer", "Model", "SystemType", "Roles" }, "No inventory data");
            SSCMInventoryTable = tableAndList.Item1; //Table
            SCCMCollecionsSoftware = TableGenerator.CreateTableFromDatabase(Software,
                new List<string>() { "SoftwareCode", "ProductName", "ProductVersion", "TimeStamp" },
                new List<string>() { "Product ID", "Name", "Version", "Install date" },
                "Software information not found");
            SCCMInventory += tableAndList.Item2; //List
        }

        public void buildSCCMInfo()
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
            var names = computer.setConfig(Collection);

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

            BasicInfoPCConfig = computer.ConfigPC;
            BasicInfoExtraConfig = computer.ConfigExtra;

            //Basal Info
            var tableAndList = TableGenerator.CreateTableAndRawFromDatabase(System, new List<string>() { "LastLogonUserName", "IPAddresses", "MACAddresses", "Build", "Config" }, "Computer not found i SCCM");

            SCCMComputers = error + groupTableHelper.GetTable();
            SCCMCollectionsTable = tableAndList.Item1; //Table
            SCCMCollections = tableAndList.Item2; //List
        }

        public void buildSCCMAntivirus()
        {
            //DetectionID is required for UserName (SELECT * FROM SMS_G_System_Threats WHERE DetectionID='{04155F79-EB84-4828-9CEC-AC0749C4EDA6}' AND ResourceID=16787705)
            //Only few computers with data, one them is AAU112782
            SCCMAV = TableGenerator.CreateTableFromDatabase(Antivirus,
                new List<string>() { "ThreatName", "PendingActions", "Process", "SeverityID", "Path" },
                "Antivirus information not found");
        }

        public void biuldSCCMHardware()
        {
            SCCMLD = TableGenerator.CreateVerticalTableFromDatabase(LogicalDisk,
                new List<string>() { "DeviceID", "FileSystem", "Size", "FreeSpace" },
                new List<string>() { "DeviceID", "File system", "Size (GB)", "FreeSpace (GB)" },
                "Disk information not found");

            if (Database.HasValues(RAM))
            {
                int total = 0;
                int count = 0;

                foreach (ManagementObject o in RAM) //Has one!
                {
                    total += int.Parse(o.Properties["Capacity"].Value.ToString()) / 1024;
                    count++;
                }

                SCCMRAM = $"{total} GB RAM in {count} slot(s)";
            }
            else
            {
                SCCMRAM = "RAM information not found";
            }

            SCCMBIOS = TableGenerator.CreateVerticalTableFromDatabase(BIOS,
                new List<string>() { "BIOSVersion", "Description", "Manufacturer", "Name", "SMBIOSBIOSVersion" },
                "BIOS information not found");

            SCCMVC = TableGenerator.CreateVerticalTableFromDatabase(VideoController,
                new List<string>() { "AdapterRAM", "CurrentHorizontalResolution", "CurrentVerticalResolution", "DriverDate", "DriverVersion", "Name" },
                "Video controller information not found");

            SCCMProcessor = TableGenerator.CreateVerticalTableFromDatabase(Processor,
                new List<string>() { "Is64Bit", "IsMobile", "IsVitualizationCapable", "Manufacturer", "MaxClockSpeed", "Name", "NumberOfCores", "NumberOfLogicalProcessors" },
                "Processor information not found");

            SCCMDisk = TableGenerator.CreateVerticalTableFromDatabase(Disk,
                new List<string>() { "Caption", "Model", "Partitions", "Size", "Name" },
                "Video controller information not found");

        }

        public void buildRaw()
        {
            Raw = TableGenerator.buildRawTable(ADcache.getAllProperties());
        }
        
        protected void buttonEnableBitlockerEncryption_Click(object sender, EventArgs e)
        {
            computer.EnableBitlockerEncryption(adpath);
        }

        private void buildWarningSegment()
        {
            List<WebMgmtError> errors = new List<WebMgmtError>
            {
                new DriveAlmostFull(computer),
                new NotStandardComputerOU(computer),
            };

            var errorList = new WebMgmtErrorList(errors);
            ErrorCountMessage = errorList.getErrorCountMessage();
            ErrorMessages = errorList.ErrorMessages;

            //Password is expired and warning before expire (same timeline as windows displays warning)
        }
    }
}