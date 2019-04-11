using ITSWebMgmt.Caches;
using ITSWebMgmt.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace ITSWebMgmt.Models
{
    public class UserModel
    {
        public UserController user;
        public UserADcache ADcache;
        public SCCMcache SCCMcache;
        public string adpath { get => ADcache.adpath; set { ADcache = new UserADcache(value); ADcache.adpath = value; } }
        public string Guid { get => new Guid((byte[])(ADcache.getProperty("objectGUID"))).ToString(); }
        public string UserPrincipalName { get => ADcache.getProperty("userPrincipalName"); }
        public string DisplayName { get => ADcache.getProperty("displayName"); }
        public string[] ProxyAddresses
        {
            get
            {
                var temp = ADcache.getProperty("proxyAddresses");
                return temp.GetType().Equals(typeof(string)) ? (new string[] { temp }) : temp;
            }
        }
        public int UserAccountControlComputed { get => ADcache.getProperty("msDS-User-Account-Control-Computed"); }
        public int UserAccountControl { get => ADcache.getProperty("userAccountControl"); }
        public string UserPasswordExpiryTimeComputed { get => ADcache.getProperty("msDS-UserPasswordExpiryTimeComputed"); }
        public string GivenName { get => ADcache.getProperty("givenName"); }
        public string SN { get => ADcache.getProperty("sn"); }
        public string AAUStaffID { get => ADcache.getProperty("aauStaffID").ToString(); }
        public string AAUStudentID { get => ADcache.getProperty("aauStudentID").ToString(); }
        public object Profilepath { get => ADcache.getProperty("profilepath"); }
        public string AAUUserClassification { get => ADcache.getProperty("aauUserClassification"); }
        public string AAUUserStatus { get => ADcache.getProperty("aauUserStatus").ToString(); }
        public string ScriptPath { get => ADcache.getProperty("scriptPath"); }
        public bool IsAccountLocked { get => ADcache.getProperty("IsAccountLocked"); }
        public string AAUAAUID { get => ADcache.getProperty("aauAAUID"); }
        public string AAUUUID { get => ADcache.getProperty("aauUUID"); }
        public string TelephoneNumber { get => ADcache.getProperty("telephoneNumber"); set => ADcache.saveProperty("telephoneNumber", value); }
        public string LastLogon { get => ADcache.getProperty("lastLogon"); }
        public string DistinguishedName { get => ADcache.getProperty("distinguishedName"); }
        public ManagementObjectCollection getUserMachineRelationshipFromUserName(string userName) => SCCMcache.getUserMachineRelationshipFromUserName(userName);

        public string[] getUserInfo()
        {
            return new string[]
            {
                UserPrincipalName,
                AAUAAUID,
                AAUUUID,
                AAUUserStatus,
                AAUStaffID,
                AAUStudentID,
                AAUUserClassification,
                TelephoneNumber,
                LastLogon
            };
        }

        public string InfoAdmDBExpireDate;
        public string BasicInfoDepartmentPDS;
        public string BasicInfoOfficePDS;
        public string BasicInfoPasswordExpired;
        public string BasicInfoPasswordExpireDate;
        public string BasicInfoTable;
        public string BasicInfoRomaing;
        public string Print;
        public string GroupSegment;
        public string GroupsAllSegment;
        public string Filesharessegment;
        public string CalAgenda;
        public string Exchange;
        public string ServiceManager;
        public string ComputerInformation;
        public string Loginscript;
        public string Rawdata;
        public string ErrorMessages;
        public string ResultError;
        public string UserName = "kyrke@its.aau.dk";
        public string ErrorCountMessage;
        public bool ShowResultDiv = false;
        public bool ShowErrorDiv = false;
        public bool ShowFixUserOU = false;
        public bool ShowLoginScript = false;

        public UserModel(UserController controller, string adpath)
        {
            if (adpath != null)
            {
                user = controller;
                user.UserModel = this;
                ADcache = new UserADcache(adpath);
                SCCMcache = new SCCMcache();
                user.buildUserLookup(adpath);
            }
            else
            {
                user.buildUserNotFound();
            }
        }
    }
}
