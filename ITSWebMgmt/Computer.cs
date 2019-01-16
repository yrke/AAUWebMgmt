using ITSWebMgmt.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Web;

namespace ITSWebMgmt.Computer
{
    public class ComputerData
    {
        public string ResourceID;
        private SCCMcache SCCMcache;

        public ComputerData(string computername)
        {
            SCCMcache = new SCCMcache();
            ResourceID = getSCCMResourceIDFromComputerName(computername);
            SCCMcache.ResourceID = ResourceID;
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
    }
}