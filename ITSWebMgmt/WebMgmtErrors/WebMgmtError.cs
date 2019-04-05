using ITSWebMgmt.Controllers;
using ITSWebMgmt.Caches;
using System.Management;

namespace ITSWebMgmt.WebMgmtErrors
{
    public static class Severity
    {
        public const int Info = 2;
        public const int Warning = 1;
        public const int Error = 0;
    }

    public abstract class WebMgmtError
    {
        public string Heading;
        public string Description;
        public abstract bool HaveError();
        public int Severeness;
    }

    public abstract class UserWebMgmtError : WebMgmtError
    {
        protected UserController user;

        public UserWebMgmtError(UserController user)
        {
            this.user = user;
        }
    }

    public abstract class ComputerWebMgmtError : WebMgmtError
    {
        protected ComputerController computer;

        public ComputerWebMgmtError(ComputerController computer)
        {
            this.computer = computer;
        }
    }

    public class DriveAlmostFull : ComputerWebMgmtError
    {
        public DriveAlmostFull(ComputerController computer) : base(computer)
        {
            Heading = "Less than 5 GB space avilable";
            Description = "Having an almost full drive might cause troubles";
            Severeness = Severity.Warning;
        }

        public override bool HaveError()
        {
            int space = computer.ComputerModel.LogicalDisk.GetPropertyInGB("FreeSpace");
            if (space == 0) return false;
            return computer.ComputerModel.LogicalDisk.GetPropertyInGB("FreeSpace") <= 5;
        }
    }

    public class NotStandardOU : UserWebMgmtError
    {
        public NotStandardOU(UserController user) : base(user)
        {
            Heading = "User is in a non standard OU";
            Description = "This might not be a problem. User can be affected by non-stadard group policy. User can be a service user or admin account.";
            Severeness = Severity.Warning;
        }

        public override bool HaveError() => !user.userIsInRightOU();
    }

    public class NotStandardComputerOU : ComputerWebMgmtError
    {
        public NotStandardComputerOU(ComputerController computer) : base(computer)
        {
            Heading = "Computer is in a wrong OU";
            Description = "The computer is getting wroung GPO settings. Fix by using task \"Move computer to OU Clients.\" ";
            Severeness = Severity.Error;
        }

        public override bool HaveError() => !computer.computerIsInRightOU(computer.ComputerModel.DistinguishedName);
    }

    public class MissingAAUAttr : UserWebMgmtError
    {
        public MissingAAUAttr(UserController user) : base(user)
        {
            Heading = "User is missing AAU Attributes";
            Description = "The user is missing one or more of the AAU attributes. The user will not be able to login via login.aau.dk. Check CPR is correct in ADMdb.";
            Severeness = Severity.Error;
        }

        public override bool HaveError() => user.AAUUserClassification == null || user.AAUUserStatus == null || (user.AAUStaffID == null && user.AAUStudentID == null);
    }

    public class PasswordExpired : UserWebMgmtError
    {
        public PasswordExpired(UserController user) : base(user)
        {
            Heading = "Password Expired";
            Description = "The user account is locked due to an expired password. User must change password or reset the users password.";
            Severeness = Severity.Error;
        }

        public override bool HaveError()
        {
            const int UF_LOCKOUT = 0x0010;

            int userFlags = (int)user.UserAccountControlComputed;

            return (userFlags & UF_LOCKOUT) == UF_LOCKOUT;
        }
    }

    public class UserLockedDiv : UserWebMgmtError
    {
        public UserLockedDiv(UserController user) : base(user)
        {
            Heading = "User account is locked";
            Description = "The user account is locked, used tasks unlock account to unlock it.";
            Severeness = Severity.Error;
        }

        public override bool HaveError() => user.IsAccountLocked == true;
    }


    public class UserDisabled : UserWebMgmtError
    {
        public UserDisabled(UserController user) : base(user)
        {
            Heading = "User is diabled";
            Description = "The user is disabled in AD, user can't login. User is expired in AdmDB or disabled by a administrator, see <a href=\"onenote:https://docs.its.aau.dk/Documentation/Info%20til%20Service%20Desk/Disablet%20Users.one#Disabled%20users%20in%20AD&section-id={062F945F-AF8F-4E1C-8151-6C87AA1F134B}&page-id={86CE4A52-90A9-4A5C-A189-9402B9B6153B}&object-id={441C8DED-9C4E-4561-B184-186C63174D6D}&EB\">onenote</a>";
            Severeness = Severity.Error;
        }

        public override bool HaveError()
        {
            const int ufAccountDisable = 0x0002;
            return (user.UserAccountControl & ufAccountDisable) == ufAccountDisable;
        }
    }
}
