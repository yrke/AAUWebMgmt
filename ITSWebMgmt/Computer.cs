using System;
using System.Collections.Generic;
using System.Linq;
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

        public void getHardware()
        {
            #region Logical Disk
            /*
             * [DisplayName("Logical Disk"), dynamic: ToInstance, provider("ExtnProv")]
            class SMS_G_System_LOGICAL_DISK : SMS_G_System_Current
            {
                [ResDLL("SMS_RXPL.dll"), ResID(15)] uint32 ResourceID = NULL;
                [ResDLL("SMS_RXPL.dll"), ResID(16)] uint32 GroupID = NULL;
                [ResDLL("SMS_RXPL.dll"), ResID(17)] uint32 RevisionID = NULL;
                [ResDLL("SMS_RXPL.dll"), ResID(12)] datetime TimeStamp = NULL;
                uint32 Access;
                uint32 Availability;
                uint64 BlockSize;
                string Caption;
                uint32 Compressed;
                uint32 ConfigManagerErrorCode;
                uint32 ConfigManagerUserConfig;
                string Description;
            *    string DeviceID;
                uint32 DriveType;
                uint32 ErrorCleared;
                string ErrorDescription;
                string ErrorMethodology;
            *    string FileSystem;
            *    uint64 FreeSpace;
                datetime InstallDate;
                uint32 LastErrorCode;
                uint32 MaximumComponentLength;
                uint32 MediaType;
                string Name;
                string NumberOfBlocks;
                string PNPDeviceID;
                string PowerManagementCapabilities;
                uint32 PowerManagementSupported;
                string ProviderName;
                string Purpose;
            *    uint64 Size;
                string Status;
                uint32 StatusInfo;
                uint32 SupportsFileBasedCompression;
                string SystemName;
                string VolumeName;
                string VolumeSerialNumber;
            };
           */
            #endregion
            var wqlq = new WqlObjectQuery("SELECT * FROM SMS_G_System_LOGICAL_DISK WHERE ResourceID=" + resourceID);
            labelSCCMLD.Text = DatabaseGetter.CreateVerticalTableFromDatabase(wqlq,
                new List<string>() { "DeviceID", "FileSystem", "Size", "FreeSpace" },
                new List<string>() { "DeviceID", "File system", "Size (GB)", "FreeSpace (GB)" },
                "Disk information not found");

            #region RAM
            /*
            SELECT * FROM SMS_G_System_PHYSICAL_MEMORY WHERE ResourceID=16787049
            instance of SMS_G_System_PHYSICAL_MEMORY
            {
                BankLabel = "";
            *    Capacity = "4096";
                Caption = "Physical Memory";
                CreationClassName = "Win32_PhysicalMemory";
                DataWidth = 64;
                Description = "Physical Memory";
                DeviceLocator = "PROC 2 DIMM 1D";
                FormFactor = 8;
                GroupID = 7;
                MemoryType = 0;
                Name = "Physical Memory";
                ResourceID = 16779367;
                RevisionID = 1;
                Speed = 1333;
                Tag = "Physical Memory 6";
                TimeStamp = "20141107155847.000000+***";
                TotalWidth = 72;
                TypeDetail = 128;
            };
            */
            #endregion
            wqlq = new WqlObjectQuery("SELECT * FROM SMS_G_System_PHYSICAL_MEMORY WHERE ResourceID=" + resourceID);
            var results = DatabaseGetter.getResults(wqlq);

            if (DatabaseGetter.HasValues(results))
            {
                int total = 0;
                int count = 0;

                foreach (ManagementObject o in results) //Has one!
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

            #region BIOS
            /*
            [DisplayName("BIOS"), dynamic: ToInstance, provider("ExtnProv")]
            class SMS_G_System_PC_BIOS : SMS_G_System_Current
            {
                [ResDLL("SMS_RXPL.dll"), ResID(15)] uint32 ResourceID = NULL;
                [ResDLL("SMS_RXPL.dll"), ResID(16)] uint32 GroupID = NULL;
                [ResDLL("SMS_RXPL.dll"), ResID(17)] uint32 RevisionID = NULL;
                [ResDLL("SMS_RXPL.dll"), ResID(12)] datetime TimeStamp = NULL;
                string BiosCharacteristics;
            *    string BIOSVersion;
                string BuildNumber;
                string Caption;
                string CodeSet;
                string CurrentLanguage;
            *    string Description;
                string IdentificationCode;
                uint32 InstallableLanguages;
                datetime InstallDate;
                string LanguageEdition;
                string ListOfLanguages;
            *    string Manufacturer;
            *    string Name;
                string OtherTargetOS;
                uint32 PrimaryBIOS;
                datetime ReleaseDate;
                string SerialNumber;
            *    string SMBIOSBIOSVersion;
            *    uint32 SMBIOSMajorVersion;
            *    uint32 SMBIOSMinorVersion;
                uint32 SMBIOSPresent;
                string SoftwareElementID;
                uint32 SoftwareElementState;
                string Status;
                uint32 TargetOperatingSystem;
                string Version;
            };
            */
            #endregion
            wqlq = new WqlObjectQuery("SELECT * FROM SMS_G_System_PC_BIOS WHERE ResourceID=" + resourceID);
            //Minor and Major not exist
            labelSCCMBIOS.Text = DatabaseGetter.CreateVerticalTableFromDatabase(wqlq,
                new List<string>() { "BIOSVersion", "Description", "Manufacturer", "Name", "SMBIOSBIOSVersion" },
                "BIOS information not found");

            #region Video controller
            /*
            class SMS_G_System_VIDEO_CONTROLLER : SMS_G_System_Current
            {
                [ResDLL("SMS_RXPL.dll"), ResID(15)] uint32 ResourceID = NULL;
                [ResDLL("SMS_RXPL.dll"), ResID(16)] uint32 GroupID = NULL;
                [ResDLL("SMS_RXPL.dll"), ResID(17)] uint32 RevisionID = NULL;
                [ResDLL("SMS_RXPL.dll"), ResID(12)] datetime TimeStamp = NULL;
                string AcceleratorCapabilities;
                string AdapterCompatibility;
                string AdapterDACType;
            *    uint32 AdapterRAM;
                uint32 Availability;
                string CapabilityDescriptions;
                string Caption;
                uint32 ColorTableEntries;
                uint32 ConfigManagerErrorCode;
                uint32 ConfigManagerUserConfig;
                uint32 CurrentBitsPerPixel;
            *    uint32 CurrentHorizontalResolution;
                string CurrentNumberOfColors;
                uint32 CurrentNumberOfColumns;
                uint32 CurrentNumberOfRows;
                uint32 CurrentRefreshRate;
                uint32 CurrentScanMode;
            *    uint32 CurrentVerticalResolution;
            *    string Description;
                string DeviceID;
                string DeviceSpecificPens;
                uint32 DitherType;
            *    datetime DriverDate;
            *    string DriverVersion;
                uint32 ErrorCleared;
                string ErrorDescription;
                uint32 ICMIntent;
                uint32 ICMMethod;
                string InfFilename;
                string InfSection;
                datetime InstallDate;
                string InstalledDisplayDrivers;
                uint32 LastErrorCode;
                uint32 MaxMemorySupported;
                uint32 MaxNumberControlled;
                uint32 MaxRefreshRate;
                uint32 MinRefreshRate;
                uint32 Monochrome;
            *    string Name;
                uint32 NumberOfColorPlanes;
                uint32 NumberOfVideoPages;
                string PNPDeviceID;
                string PowerManagementCapabilities;
                uint32 PowerManagementSupported;
                uint32 ProtocolSupported;
                uint32 ReservedSystemPaletteEntries;
                uint32 SpecificationVersion;
                string Status;
                uint32 StatusInfo;
                string SystemName;
                uint32 SystemPaletteEntries;
                datetime TimeOfLastReset;
                uint32 VideoArchitecture;
                uint32 VideoMemoryType;
                uint32 VideoMode;
                string VideoModeDescription;
                string VideoProcessor;
            };
            */
            #endregion
            wqlq = new WqlObjectQuery("SELECT * FROM SMS_G_System_VIDEO_CONTROLLER WHERE ResourceID=" + resourceID);
            labelSCCMVC.Text = DatabaseGetter.CreateVerticalTableFromDatabase(wqlq,
                new List<string>() { "AdapterRAM", "CurrentHorizontalResolution", "CurrentVerticalResolution", "DriverDate", "DriverVersion", "Name" },
                "Video controller information not found");

            #region Processor
            /*
            [DisplayName("Processor"), dynamic: ToInstance, provider("ExtnProv")]
            class SMS_G_System_PROCESSOR : SMS_G_System_Current
            {
                [ResDLL("SMS_RXPL.dll"), ResID(15)] uint32 ResourceID = NULL;
                [ResDLL("SMS_RXPL.dll"), ResID(16)] uint32 GroupID = NULL;
                [ResDLL("SMS_RXPL.dll"), ResID(17)] uint32 RevisionID = NULL;
                [ResDLL("SMS_RXPL.dll"), ResID(12)] datetime TimeStamp = NULL;
                uint32 AddressWidth;
                uint32 Architecture;
                uint32 Availability;
                uint32 BrandID;
                string Caption;
                uint32 ConfigManagerErrorCode;
                uint32 ConfigManagerUserConfig;
                string CPUHash;
                string CPUKey;
                uint32 CpuStatus;
                uint32 CurrentClockSpeed;
                uint32 CurrentVoltage;
                uint32 DataWidth;
                string Description;
                string DeviceID;
                uint32 ErrorCleared;
                string ErrorDescription;
                uint32 ExtClock;
                uint32 Family;
                datetime InstallDate;
            *    uint32 Is64Bit;
                uint32 IsHyperthreadCapable;
                uint32 IsHyperthreadEnabled;
            *    uint32 IsMobile;
                uint32 IsTrustedExecutionCapable;
            *    uint32 IsVitualizationCapable;
                uint32 L2CacheSize;
                uint32 L2CacheSpeed;
                uint32 L3CacheSize;
                uint32 L3CacheSpeed;
                uint32 LastErrorCode;
                uint32 Level;
                uint32 LoadPercentage;
            *    string Manufacturer;
            *    uint32 MaxClockSpeed;
            *    string Name;
                uint32 NormSpeed;
            *    uint32 NumberOfCores;
            *    uint32 NumberOfLogicalProcessors;
                string OtherFamilyDescription;
                uint32 PCache;
                string PNPDeviceID;
                string PowerManagementCapabilities;
                uint32 PowerManagementSupported;
                string ProcessorId;
                uint32 ProcessorType;
                uint32 Revision;
                string Role;
                string SocketDesignation;
                string Status;
                uint32 StatusInfo;
                string Stepping;
                string SystemName;
                string UniqueId;
                uint32 UpgradeMethod;
                string Version;
                uint32 VoltageCaps;
            };
             */
            #endregion
            wqlq = new WqlObjectQuery("SELECT * FROM SMS_G_System_PROCESSOR WHERE ResourceID=" + resourceID);
            labelSCCMProcessor.Text = DatabaseGetter.CreateVerticalTableFromDatabase(wqlq,
                new List<string>() { "Is64Bit", "IsMobile", "IsVitualizationCapable", "Manufacturer", "MaxClockSpeed", "Name", "NumberOfCores", "NumberOfLogicalProcessors" },
                "Processor information not found");

            #region Disk
            /*
            [DisplayName("Disk"), dynamic: ToInstance, provider("ExtnProv")]
            class SMS_G_System_DISK : SMS_G_System_Current
            {
                [ResDLL("SMS_RXPL.dll"), ResID(15)] uint32 ResourceID = NULL;
                [ResDLL("SMS_RXPL.dll"), ResID(16)] uint32 GroupID = NULL;
                [ResDLL("SMS_RXPL.dll"), ResID(17)] uint32 RevisionID = NULL;
                [ResDLL("SMS_RXPL.dll"), ResID(12)] datetime TimeStamp = NULL;
                uint32 Availability;
                uint32 BytesPerSector;
                string Capabilities;
                string CapabilityDescriptions;
            *    string Caption;
                string CompressionMethod;
                uint32 ConfigManagerErrorCode;
                uint32 ConfigManagerUserConfig;
                uint64 DefaultBlockSize;
                string Description;
                string DeviceID;
                uint32 ErrorCleared;
                string ErrorDescription;
                string ErrorMethodology;
                uint32 Index;
                datetime InstallDate;
                string InterfaceType;
                uint32 LastErrorCode;
                string Manufacturer;
                uint64 MaxBlockSize;
                uint64 MaxMediaSize;
                uint32 MediaLoaded;
                string MediaType;
                uint64 MinBlockSize;
            *    string Model;
                string Name;
                uint32 NeedsCleaning;
                uint32 NumberOfMediaSupported;
            *    uint32 Partitions;
                string PNPDeviceID;
                string PowerManagementCapabilities;
                uint32 PowerManagementSupported;
                uint32 SCSIBus;
                uint32 SCSILogicalUnit;
                uint32 SCSIPort;
                uint32 SCSITargetId;
                uint32 SectorsPerTrack;
                uint64 Size;
                string Status;
                uint32 StatusInfo;
                string SystemName;
                string TotalCylinders;
                uint32 TotalHeads;
                string TotalSectors;
                string TotalTracks;
                uint32 TracksPerCylinder;
            };
            */
            #endregion
            wqlq = new WqlObjectQuery("SELECT * FROM SMS_G_System_DISK WHERE ResourceID=" + resourceID);
            labelSCCMDisk.Text = DatabaseGetter.CreateVerticalTableFromDatabase(wqlq,
                new List<string>() { "Caption", "Model", "Partitions", "Size", "Name" },
                "Video controller information not found");
        }
    }
}