using System.Collections.Generic;

namespace ITSWebMgmt.WebMgmtErrors
{
    public class WebMgmtErrorList
    {
        private List<WebMgmtError> errors;
        private int[] ErrorCount = { 0, 0, 0 };
        public string ErrorMessages;

        public WebMgmtErrorList(List<WebMgmtError> errors)
        {
            this.errors = errors;
            processErrors();
        }

        private void processErrors()
        {
            foreach (WebMgmtError error in errors)
            {
                if (error.HaveError())
                {
                    ErrorCount[error.Severeness]++;
                    ErrorMessages += generateMessage(error);
                }
            }
            if (ErrorMessages == null)
            {
                ErrorMessages = "No warnings found";
            }
        }

        private string generateMessage(WebMgmtError error)
        {
            string messageType = "";

            switch (error.Severeness)
            {
                case Severity.Error:
                    messageType = "negative";
                    break;
                case Severity.Warning:
                    messageType = "warning";
                    break;
                case Severity.Info:
                    messageType = "info";
                    break;
            }

            return $"<div class=\"ui {messageType} message\" runat= \"server\">" +
                    $"<div class=\"header\">{error.Heading}</div>" +
                    $"<p>{error.Description}</p>" +
                    $"</div>";
        }

        public string getErrorCountMessage()
        {
            string messageType = "";
            string heading = "";

            if (ErrorCount[Severity.Error] > 0)
            {
                messageType = "negative";
                heading = "Errors";
            }
            else if (ErrorCount[Severity.Warning] > 0)
            {
                messageType = "warning";
                heading = "Warnings";
            }
            else if (ErrorCount[Severity.Info] > 0)
            {
                messageType = "info";
                heading = "Infos";
            }

            return messageType == "" ? "" : $"<div class=\"ui {messageType} message\" runat= \"server\">" +
                    $"<div class=\"header\">{heading} found</div>" +
                    $"<p>Found {ErrorCount[Severity.Error]} errors, {ErrorCount[Severity.Warning]} warnings, and {ErrorCount[Severity.Info]} infos.</p>" +
                    $"</div>";
        }
    }
}