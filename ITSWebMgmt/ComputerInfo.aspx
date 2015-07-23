<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ComputerInfo.aspx.cs" Inherits="ITSWebMgmt.ComputerInfo" MasterPageFile="~/Site.Master" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">


    <h1> Computer Info</h1>
    <div>
        ComputerName <asp:TextBox ID="ComputerNameInput" runat="server"  Text="AAU804396" /> <asp:Button runat="server" ID="sumbit" OnClick="lookupComputer" Text="Søg"/>
            <asp:RequiredFieldValidator runat="server" id="reqName" controltovalidate="ComputerNameInput" errormessage="Please enter a value" />

        <br />
        
        <h2><asp:Label ID="UserNameLabel" runat="server"></asp:Label></h2>
        <asp:Label ID="ResultLabel" runat="server"></asp:Label>
        <asp:Button ID="ResultGetPassword" runat="server" value="" Text="Get Local Admin Password" OnClick="ResultGetPassword_Click"/>
        <!--<asp:TextBox ID="ResultBox" TextMode="MultiLine" Width="700" Height="200" runat="server"></asp:TextBox>-->
        
    </div>

</asp:Content>
