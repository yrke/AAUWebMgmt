using System.Collections.Generic;
using ITSWebMgmt.Controllers;
using System.Management;
using System.Threading;
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
        public ComputerADcache ADcache;
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
        public bool ShowResultDiv = false;
        public bool ShowResultGetPassword = false;
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
                LoadDataInbackground();
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

        private void LoadDataInbackground()
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

            // List<string> loadedTapNames = new List<string> { "basicinfo", "sccmInfo", "tasks", "warnings" };
            // Load the data for the other tabs in background.
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
    }
}