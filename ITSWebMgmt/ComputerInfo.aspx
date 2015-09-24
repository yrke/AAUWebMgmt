<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ComputerInfo.aspx.cs" Inherits="ITSWebMgmt.ComputerInfo" MasterPageFile="~/Site.Master" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">


    <h1> Computer Info</h1>
    <div>
        ComputerName: 
        
        <div class="ui action input">
            <asp:TextBox ID="ComputerNameInput" runat="server"  Text="AAU804396" CssClass="ui input focus" /> 
            <asp:Button runat="server" ID="sumbit" OnClick="lookupComputer" Text="Søg" CssClass="ui button"/>
        </div>
        
            <asp:RequiredFieldValidator runat="server" id="reqName" controltovalidate="ComputerNameInput" errormessage="Please enter a value" />

        <br />
        
        <h2><asp:Label ID="UserNameLabel" runat="server"></asp:Label></h2>
        <asp:Label ID="ResultLabel" runat="server"></asp:Label>
        <br />
        <asp:Button ID="ResultGetPassword" runat="server" value="" Text="Get Local Admin Password" OnClick="ResultGetPassword_Click" CssClass="ui button"/>
        <br />
        <div id="MoveComputerOUdiv" runat="server">
        <asp:Button ID="MoveComputerOU" runat="server" value="" Text="Move computer to OU Clients" OnClick="MoveOU_Click" CssClass="ui button"/> Only do this if you know what you are doing!
        </div>
        
    </div>

</asp:Content>
