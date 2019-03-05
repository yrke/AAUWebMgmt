<%@ Page Title="Home Page" Language="C#" AutoEventWireup="true" CodeBehind="UserInfo.aspx.cs" Inherits="ITSWebMgmt.UserInfo" MasterPageFile="~/Site.Master" Async="true" %>
<%@ Register TagPrefix="uc" TagName="Changes" Src="~/Controls/ActiveChanges.ascx" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="ui dimmer modals page transition hidden" style="display: block !important;">
        <div id="modalConfirmOUmove" class="ui small test modal transition hidden" style="margin-top: -92px; display: block !important;">
            <div class="header">
                Move user to Default OU
            </div>
            <div class="content">
                <p>
                    The user is placed in a non standard OU, this means applied GPOs and settings might differ from the standard. 
                    Run this task to move the user to the default configuration. 
                    <br />
                    <br />
                    <b><i>Warning: This action will affect the user, only run this it in agreement with the user!</i></b>
                </p>
            </div>
            <div class="actions">
                <div class="ui negative button">
                    No
                </div>
                <div runat="server" class="ui positive right labeled icon button" onclick="$('#MainContent_btnPostbackfixUserOUButton').click()">
                    Yes <i class="checkmark icon"></i>
                </div>
            </div>
        </div>
    </div>

    <div id="loader">
        <div class="ui active dimmer" style="display: none">
            <div class="ui text loader">Loading</div>
        </div>
    </div>

    <h1>User Info</h1>
    <div>
        <form method="get">
            Username
        <div class="ui action input">

            <input name="search" class="ui input focus" value="<%=UserName %>" />

            <input type="submit" value="Search" onclick='$("#loader > div").show("fast");' />
        </div>
        </form>
        <br />
        <br />
        <script>
            $(document).ready(function () {
                $('.menu .item').tab({ history: false });
            });
        </script>

        <div id="errordiv" runat="server">
            <asp:Label ID="ResultErrorLabel" runat="server"></asp:Label>
        </div>

        <uc:Changes ID="changes" runat="server" />

        <div runat="server" id="ResultDiv">
            <div runat="server" id="warningsAndErrorDIV">
                <asp:Label ID="ErrorCountMessageLabel" runat="server"></asp:Label>
            </div>

            <form runat="server">
                <h2><asp:Label ID="UserNameLabel" runat="server"></asp:Label></h2>
                <div class="ui grid">
                    <div class="four wide column">
                        <div class="ui vertical fluid tabular menu">
                            <a class="active item" data-tab="basicinfo">Basic Info</a>
                            <a class="item" data-tab="servicemanager">Service Manager</a>
                            <a class="item" data-tab="calAgenda">Calendar Agenda</a>
                            <a class="item" data-tab="computerInformation">Computer Information</a>
                            <!--<a class="item" data-tab="advancedinfo">Advanced Info</a> -->
                            <a class="item" data-tab="groups">Groups</a>
                            <a class="item" data-tab="fileshares">Fileshares</a>
                            <a class="item" data-tab="exchange">Exchange Resources</a>
                            <!--<a class="item" data-tab="networkdrives">Networkdrives</a>-->
                            <a class="item" data-tab="loginscript" runat="server" id="menuLoginScript">Loginscript</a>
                            <a class="item" data-tab="print" runat="server" id="menuPrint">Print</a>
                            <a class="item" data-tab="rawdata">Raw Data</a>
                            <a class="item" data-tab="tasks">Tasks</a>
                            <a class="item" data-tab="warnings">Warnings</a>
                            <!--<a class="item" data-tab="statustest">Statustest</a> -->
                        </div>
                    </div>
                    <div class="twelve wide stretched column">
                        <div class="ui tab segment">
                            none<!-- spacer as the fist elemen else is placed differencet -->
                        </div>
                        <div class="ui active tab segment" data-tab="basicinfo">
                            <table class="ui definition table">
                                <tbody>
                                    <tr>
                                        <td>Name</td>
                                        <td><asp:Label runat="server" ID="displayName"></asp:Label></td>
                                    </tr>
                                    <tr>
                                        <td>ADMdb Expire Date</td>
                                        <td><asp:Label runat="server" ID="basicInfoAdmDBExpireDate"></asp:Label></td>
                                    </tr>
                                    <tr>
                                        <td>Department (PDS)</td>
                                        <td><asp:Label runat="server" ID="lblbasicInfoDepartmentPDS"></asp:Label></td>
                                    </tr>
                                    <tr>
                                        <td>Office (PDS)</td>
                                        <td><asp:Label runat="server" ID="lblbasicInfoOfficePDS"></asp:Label></td>
                                    </tr>
                                    <tr>
                                        <td>Password Expired</td>
                                        <td><asp:Label runat="server" ID="basicInfoPasswordExpired"></asp:Label></td>
                                    </tr>
                                    <tr>
                                        <td>Password Expire Date</td>
                                        <td><asp:Label runat="server" ID="basicInfoPasswordExpireDate"></asp:Label></td>
                                    </tr>
                                    <asp:Label runat="server" ID="labelBasicInfoTable" />
                                    <tr>
                                        <td>Romaing Profile</td>
                                        <td><asp:Label runat="server" ID="labelBasicInfoRomaing" /></td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                        <div class="ui tab segment" data-tab="advancedinfo">
                            advanced
                        </div>
                        <div class="ui tab segment" data-tab="print">
                            <h2>Print</h2>
                            <asp:Label ID="lblPrint" runat="server"></asp:Label>
                        </div>
                        <div class="ui tab segment" data-tab="groups">
                            <h2>Groups</h2>
                            <div class="ui two item menu">
                                <a data-tab="groups-direct" class="item">Direct Groups</a>
                                <a data-tab="groups-all" class="item">Recursive groups</a>
                            </div>
                            <div class="ui active tab segment" data-tab="groups-direct">
                                <asp:Label ID="groupssegmentLabel" runat="server"></asp:Label>
                            </div>
                            <div class="ui tab segment" data-tab="groups-all">
                                <asp:Label ID="groupsAllsegmentLabel" runat="server"></asp:Label>
                            </div>
                        </div>
                        <div class="ui tab segment" data-tab="fileshares">
                            <h2>Fileshares</h2>
                            <asp:Label ID="filesharessegmentLabel" runat="server"></asp:Label>
                        </div>
                        <div class="ui tab segment" data-tab="calAgenda">
                            <h2>Calendar Agenda (today)</h2>
                            <asp:Label runat="server" ID="lblcalAgenda" />
                        </div>
                        <div class="ui tab segment" data-tab="exchange">
                            <h2>Exchange</h2>
                            <asp:Label runat="server" ID="lblexchange" />
                        </div>

                        <div class="ui tab segment" data-tab="networkdrives">
                            <h2>networkdrives</h2>
                            <h3>GPO mounted</h3>
                            not inplemented
                            <h3>Logonscript</h3>
                            not inplemented
                        </div>
                        <div class="ui tab segment" data-tab="servicemanager">
                            <h2>SCSM Information</h2>
                            <asp:Button ID="buttonCreateIRSR" runat="server" OnClick="createNewIRSR_Click" Text="Create new IR/SR" />
                            <asp:Label ID="divServiceManager" runat="server"></asp:Label>
                        </div>

                        <div class="ui tab segment" data-tab="computerInformation">
                            <h2>Computer Information</h2>
                            <!-- Information from managerOf computers and logon machines from SCCM -->
                            <asp:Label ID="divComputerInformation" runat="server"></asp:Label>
                        </div>

                        <div class="ui tab segment" data-tab="loginscript" id="divLoginscript">
                            <h2>Loginscript</h2>
                            <asp:Label ID="labelLoginscript" runat="server"></asp:Label>
                        </div>

                        <div class="ui tab segment" data-tab="rawdata">
                            <h2>Raw Data</h2>
                            <asp:Label ID="labelRawdata" runat="server"></asp:Label>
                        </div>
                        <div class="ui tab segment" data-tab="tasks">
                            <h2>Tasks</h2>
                            <br />
                            Unlock Account (if locked by wrong password):
                            <asp:Button runat="server" CssClass="ui button" ID="unlockUserAccountButton" Text="Unlock Account" OnClick="unlockUserAccountButton_Click" /><br />
                            <br />
                            <br />
                            Roaming Profile (enable/disable): 
                            <asp:Button runat="server" CssClass="ui button" ID="ToggleRoaming" Text="ToggleRoaming" OnClick="button_toggle_userprofile" /><br />
                            <br />
                            <br />
                            <div runat="server" id="divFixuserOU">
                                Move user to default OU: 
                                <asp:Button runat="server" OnClick="fixUserOUButton" Text="Move user to default OU" ID="btnPostbackfixUserOUButton" Style="display: none" />
                                <button type="button" class="ui button" id="btnFixOUShowModal" onclick="$('#modalConfirmOUmove').modal('show');">Move user to default OU</button><br />
                            </div>
                        </div>
                        <div class="ui tab segment" data-tab="warnings">
                            <h2>Warnings</h2>
                            <asp:Label ID="ErrorMessagesLabel" runat="server"></asp:Label>
                        </div>
                    </div>
                </div>
            </form>
        </div>
    </div>
</asp:Content>
