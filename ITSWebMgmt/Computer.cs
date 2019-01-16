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

        public ComputerData(string resourceID)
        {
            ResourceID = resourceID;
            SCCMcache = new SCCMcache(resourceID);
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
    }
}