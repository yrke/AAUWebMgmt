<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ITSWebMgmt.Default" MasterPageFile="~/Site.Master" %>
<%@ Register TagPrefix="uc" TagName="Changes" Src="~/Controls/ActiveChanges.ascx" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

<%-- <uc:Changes ID="changes" runat="server" /> --%>
    
    <div class="ui segment">
          <div class="ui right internal attached rail" style="margin-top:50px;">
    <div class="ui segment" style="">
      <h3>Nyheder</h3>
        <li>2019-03-04 -  Now UserInfo searches on proxyAddress and UPN
        <br /><br />
        <li>2019-02-27 -  Managed By can now be set on computeres from webmgmt
        <br /><br />
        <li>2019-02-01 -  Inventory details added for computer
        <br /><br />
        <li>2017-12-04 -  Added feature to generate defendpoint challange/response keys
        <br /><br />
        <li>2017-07-05 -  Groups now shows direct and recursive memberships 
        <br /><br />
        <li>2017-03-31 -  More print information from EquiTrac
        <br /><br />
        

        
        
       
    </div>
  </div>
    <h1>ITSWebMgmt</h1>

    <a href="/UserInfo.aspx">User Info</a><br />
    <a href="/ComputerInfo.aspx">Computer Info</a><br />
    <a class="item" href="/DefendpointChallengeResponse.aspx">Defendpoint</a><br />
        <br /><br /><br />
        <br />
        <br />
        <br />
        <br />
        <br />
        <br />
        <br />
  <br /><br />
        </div>
    <div class="ui segment">
        Du kan bidrage til udviklingen af Webmgmt på siden <a href="https://github.com/yrke/AAUWebMgmt">github.com/yrke/AAUWebMgmt</a>
    </div>

</asp:Content>