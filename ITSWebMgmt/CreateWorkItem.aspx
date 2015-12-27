<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CreateWorkItem.aspx.cs" Inherits="ITSWebMgmt.CreateWorkItem" MasterPageFile="~/Site.Master" %>


<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="ui form">
        <div class="field">
            <label>Affected User</label>
            <asp:TextBox runat="server" ID="tb_affectedUser" ReadOnly="true"></asp:TextBox>
        </div>
        <div class="field">
            <label>Title</label>
            <asp:TextBox runat="server" ID="tb_title" placeholder="Title"></asp:TextBox>
        </div>
        <div class="field">
            <label>Desription</label>
            <asp:TextBox TextMode="MultiLine" runat="server" ID="tb_description" placeholder="Description" Rows="10"></asp:TextBox>
        </div>

        <asp:Button Text="Create SR" runat="server" OnClick="createSR_OnClick" CssClass="ui button" />
        <asp:Button Text="Create IR" runat="server" OnClick="createIR_OnClick" CssClass="ui button" />
    </div>
</asp:Content>
