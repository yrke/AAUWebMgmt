<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ITSWebMgmt.Default" MasterPageFile="~/Site.Master" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>ITSWebMgmt</h1>

    <a href="/UserInfo.aspx">User Info</a><br />
    <a runat="server" href="~/ComputerInfo.aspx">Computer Info</a><br />
</asp:Content>