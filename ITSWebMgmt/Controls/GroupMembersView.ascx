<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="GroupMembersView.ascx.cs" Inherits="ITSWebMgmt.Controls.GroupMembersView" %>

<h2>Members</h2>
<asp:Label ID="memberInfoLabel" runat="server"></asp:Label>
<div class="ui two item menu">
    <a data-tab="groups-direct" class="item">Direct Groups</a> 
    <a data-tab="groups-all" class="item"> Recursive groups</a>
</div>

<div class="ui active tab segment" data-tab="groups-direct">
    <asp:Label ID="groupssegmentLabel" runat="server"></asp:Label>
    </div>

<div class="ui tab segment" data-tab="groups-all">
    <asp:Label ID="groupsAllsegmentLabel" runat="server"></asp:Label>
</div>