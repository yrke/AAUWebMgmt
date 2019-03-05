<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="FileshareInfo.aspx.cs" Inherits="ITSWebMgmt.FileshareInfo" %>
<%@ Register TagPrefix="uc" TagName="GroupMembersView" Src="~/Controls/GroupMembersView.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <form runat="server">
        <!-- Loader -->
        <div id="loader">
            <div class="ui active dimmer" style="display: none">
                <div class="ui text loader">Loading</div>
            </div>
        </div>

        <!-- page seach header -->
        <div>
            <h1><asp:Label runat="server" ID="headingLabel" /></h1>

        </div>



        <!-- page content -->


        <script>
            $(document).ready(function () {
                $('.menu .item').tab({ history: false });
            });
        </script>

        <br />
        <br />
        <div runat="server" id="ResultDiv">




            <div class="ui grid">
                <div class="four wide column">
                    <div class="ui vertical fluid tabular menu">
                        <a class="active item" data-tab="basicinfo">Basic Info</a>
                        <a class="item" data-tab="members">Members</a>
                    </div>
                </div>
                <div class="twelve wide stretched column">
                    <div class="ui tab segment">
                        none<!-- spacer as the fist elemen else is placed differencet -->
                    </div>
                    <div class="ui active tab segment" data-tab="basicinfo">
                        <h2>Basic Info</h2>
                            <table class="ui definition table">
                                <tbody>
                                    <tr>
                                        <td>Name:</td>
                                        <td>
                                            <asp:Label runat="server" ID="labelName" /></td>
                                    </tr>
                                    <tr>
                                        <td>Domain:</td>
                                        <td>
                                            <asp:Label runat="server" ID="labelDomain" /></td>
                                    </tr>
                                    <tr>
                                        <td>Is Security group:</td>
                                        <td>
                                            <asp:Label runat="server" ID="labelSecurityGroup" /></td>
                                    </tr>
                                    <tr>
                                        <td>Group Scope:</td>
                                        <td>
                                            <asp:Label runat="server" ID="labelGroupScope" /></td>
                                    </tr>
                                    <tr>
                                        <td>Group Description:</td>
                                        <td>
                                            <asp:Label runat="server" ID="labelGroupDescription" /></td>
                                    </tr>
                                    <tr>
                                        <td>Group Info:</td>
                                        <td>
                                            <asp:Label runat="server" ID="labelGroupInfo" /></td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    <div class="ui tab segment" data-tab="members">
                         <uc:GroupMembersView ID="members" runat="server" />
                    </div>
                </div>
            </div>
        </div>
    </form>
</asp:Content>

