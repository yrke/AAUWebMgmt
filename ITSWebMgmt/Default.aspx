<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ITSWebMgmt.Default" MasterPageFile="~/Site.Master" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <div class="ui segment">
          <div class="ui right internal attached rail" style="margin-top:50px;">
    <div class="ui segment" style="">
      <h3>Nyheder</h3>
        Webmgmt understøtter nu at vise en brugers aktive sager fra scsm.
    </div>
  </div>
    <h1>ITSWebMgmt</h1>

    <a href="/UserInfo.aspx">User Info</a><br />
    <a runat="server" href="~/ComputerInfo.aspx">Computer Info</a><br />

  
        </div>
    <div class="ui segment">
        Du kan bidrage til udviklingen af Webmgmt på siden <a href="https://github.com/yrke/AAUWebMgmt">github.com/yrke/AAUWebMgmt</a>
    </div>

</asp:Content>