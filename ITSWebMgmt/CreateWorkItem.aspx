<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CreateWorkItem.aspx.cs" Inherits="ITSWebMgmt.CreateWorkItem" MasterPageFile="~/Site.Master" %>


<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    User: <asp:TextBox runat="server" ID="tb_affectedUser" ReadOnly="true"></asp:TextBox> 
    <br />
    Title <asp:TextBox  runat="server" ID="tb_title"></asp:TextBox>
    <br />
    Description <asp:TextBox TextMode="MultiLine"  runat="server" ID="tb_description"></asp:TextBox>
    <br />
    <asp:Button Text="Create SR" runat="server" OnClick="createSR_OnClick"/>
    <asp:Button Text="Create IR" runat="server" OnClick="createIR_OnClick" />

</asp:Content>
