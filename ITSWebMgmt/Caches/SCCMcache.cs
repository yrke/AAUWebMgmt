using System;
using System.ComponentModel;
using System.Linq;
using System.Management;

namespace ITSWebMgmt.Caches
{
    public class SCCMcache
    {
        public string ResourceID;

        public SCCMcache()
        {
        }

        private static ManagementScope ms = new ManagementScope("\\\\srv-cm12-p01.srv.aau.dk\\ROOT\\SMS\\site_AA1");

        private ManagementObjectCollection[] _cache = new ManagementObjectCollection[20]; //TODO fix size
        private ManagementObjectCollection[] _cache2 = new ManagementObjectCollection[20]; //TODO fix size

        private string[] DBEntry = {
            "SMS_G_System_PHYSICAL_MEMORY",
            "SMS_G_System_LOGICAL_DISK",
            "SMS_G_System_PC_BIOS" ,
            "SMS_G_System_VIDEO_CONTROLLER",
            "SMS_G_System_PROCESSOR",
            "SMS_G_System_DISK",
            "SMS_G_System_INSTALLED_SOFTWARE",
            "SMS_G_System_COMPUTER_SYSTEM",
            "SMS_G_System_Threats",
            "SMS_R_System",
            "SMS_FullCollectionMembership"
        };

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
        public ManagementObjectCollection RAM { get => getQuery(0); private set {} }
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
        public ManagementObjectCollection LogicalDisk { get => getQuery(1); private set { } }
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
        public ManagementObjectCollection BIOS { get => getQuery(2); private set { } }
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
        public ManagementObjectCollection VideoController { get => getQuery(3); private set { } }
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
        public ManagementObjectCollection Processor { get => getQuery(4); private set { } }
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
        public ManagementObjectCollection Disk { get => getQuery(5); private set {} }
        #region Software
        /*
        [DisplayName("Installed Software"), dynamic: ToInstance, provider("ExtnProv")]
        class SMS_G_System_INSTALLED_SOFTWARE : SMS_G_System_Current
        {
            [ResDLL("SMS_RXPL.dll"), ResID(15)] uint32 ResourceID = NULL;
            [ResDLL("SMS_RXPL.dll"), ResID(16)] uint32 GroupID = NULL;
            [ResDLL("SMS_RXPL.dll"), ResID(17)] uint32 RevisionID = NULL;
            [ResDLL("SMS_RXPL.dll"), ResID(12)] datetime TimeStamp = NULL;
            string ARPDisplayName;
            string ChannelCode;
            string ChannelID;
            string CM_DSLID;
            string EvidenceSource;
            datetime InstallDate;
            uint32 InstallDirectoryValidation;
            string InstalledLocation;
            string InstallSource;
            uint32 InstallType;
            uint32 Language;
            string LocalPackage;
            string MPC;
            uint32 OsComponent;
            string PackageCode;
            string ProductID;
            string ProductName;
            string ProductVersion;
            string Publisher;
            string RegisteredUser;
            string ServicePack;
            string SoftwareCode;
            string SoftwarePropertiesHash;
            string SoftwarePropertiesHashEx;
            string UninstallString;
            string UpgradeCode;
            uint32 VersionMajor;
            uint32 VersionMinor;
        };
        */
        #endregion
        public ManagementObjectCollection Software { get => getQuery(6); private set { } }
        //Missing class
        public ManagementObjectCollection Computer { get => getQuery(7); private set { } }
        #region Antivirus
        /*             
            instance of SMS_G_System_Threats
            {
                ActionSuccess = TRUE;
                ActionTime = "20181206092351.150000+***";
                Category = "";
                CategoryID = 27;
                CleaningAction = 2;
                DetectionID = "{04155F79-EB84-4828-9CEC-AC0749C4EDA6}";
                DetectionSource = 3;
                DetectionTime = "20181206092345.703000+***";
                ErrorCode = -2142207965;
                ExecutionStatus = 1;
            *    Path = "file:_C:\\OneDriveTemp\\S-1-5-21-1950982312-1110734968-986239597-1661\\2ce71f67d80e4f848ce20184ec987e52-8c19be29fc224df0aa8af25df55650dd-33f2835be60c42e1812107e28db565e3-d250bdce6b36dbf4e5459435ccabee25f9b05e46.temp";
            *    PendingActions = 0;
            *    Process = "C:\\Users\\lat\\AppData\\Local\\Microsoft\\OneDrive\\OneDrive.exe";
                ProductVersion = "4.18.1810.5";
                ResourceID = 16787705;
                Severity = "";
            *    SeverityID = 5;
                ThreatID = "227086";
            *    ThreatName = "PUA:Win32/Reimage";
            *    UserName = "ET\\lat";
            };
        */
        #endregion
        public ManagementObjectCollection Antivirus { get => getQuery(8); private set { } }
        //Missing class
        public ManagementObjectCollection System { get => getQuery(9); private set { } }
        //Missing class
        public ManagementObjectCollection Collection { get => getQuery(10); private set { } }

        public ManagementObjectCollection getResourceIDFromComputerName(string computerName) => getQuery(0, new WqlObjectQuery("select ResourceID from SMS_CM_RES_COLL_SMS00001 where name like '" + computerName + "'"));
        public ManagementObjectCollection getUserMachineRelationshipFromUserName(string userName) => getQuery(1, new WqlObjectQuery("SELECT * FROM SMS_UserMachineRelationship WHERE UniqueUserName = \"" + userName + "\""));

        private ManagementObjectCollection getQuery(int i, WqlObjectQuery wqlq)
        {
            if (_cache2[i] == null)
            {
                var searcher = new ManagementObjectSearcher(ms, wqlq);
                _cache2[i] = searcher.Get();
            }

            return _cache2[i];
        }

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
    
    public static class ManagementObjectCollectionExtension
    {
        public static dynamic GetProperty(this ManagementObjectCollection moc, string property)
        {
            return moc.OfType<ManagementObject>().FirstOrDefault()?.Properties[property]?.Value;
        }

        public static T GetPropertyAs<T>(this ManagementObjectCollection moc, string property)
        {
            var tc = TypeDescriptor.GetConverter(typeof(T));
            var temp = GetPropertyAsString(moc, property);
            if (temp == "")
            {
                return default(T);
            }
            return (T)(tc.ConvertFromInvariantString(temp));
        }

        public static int GetPropertyInGB(this ManagementObjectCollection moc, string property)
        {
            return GetPropertyAs<int>(moc, property) / 1024;
        }

        public static string GetPropertyAsString(this ManagementObjectCollection moc, string property)
        {
            var temp = GetProperty(moc, property);
            return temp == null ? ""  : temp.ToString();
        }
    }
}