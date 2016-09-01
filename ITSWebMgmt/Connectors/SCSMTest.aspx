<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SCSMTest.aspx.cs" Inherits="ITSWebMgmt.SCSMTest" Async="true" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div><h1>Test</h1>
        <asp:TextBox runat="server" TextMode="Password" ID="passwordTextBx"></asp:TextBox>
        <asp:Button runat="server" OnClick="button_Click" Text="DoStuff" />
        <asp:Label runat="server" ID="responseLbl"></asp:Label>
    </div>
    </form>
</body>
</html>
