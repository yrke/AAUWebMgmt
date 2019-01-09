using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Web;

namespace ITSWebMgmt.Computer
{
    public class SCCMcache
    {
        public string ResourceID;

        public SCCMcache(string resourceID)
        {
            ResourceID = resourceID;
        }

        private static ManagementScope ms = new ManagementScope("\\\\srv-cm12-p01.srv.aau.dk\\ROOT\\SMS\\site_AA1");

        private ManagementObjectCollection[] _cache = new ManagementObjectCollection[6];
        private string[] DBEntry = { "SMS_G_System_LOGICAL_DISK", "SMS_G_System_PHYSICAL_MEMORY", "SMS_G_System_PC_BIOS" , "SMS_G_System_VIDEO_CONTROLLER", "SMS_G_System_PROCESSOR", "SMS_G_System_DISK"};

        public ManagementObjectCollection RAM { get => getQuery(0); private set {} }
        public ManagementObjectCollection LogicalDisk { get => getQuery(1); private set { } }
        public ManagementObjectCollection BIOS { get => getQuery(2); private set { } }
        public ManagementObjectCollection VideoController { get => getQuery(3); private set { } }
        public ManagementObjectCollection Processor { get => getQuery(4); private set { } }
        public ManagementObjectCollection Disk { get => getQuery(5); private set {} }

        private ManagementObjectCollection getQuery(int i)
        {
            if (_cache[i] == null)
            {
                var wqlq = new WqlObjectQuery($"SELECT * FROM {DBEntry[i]} WHERE ResourceID=" + ResourceID);
                var searcher = new ManagementObjectSearcher(ms, wqlq);
                _cache[i] = searcher.Get();
            }

            return _cache[i];
        }
    }
}