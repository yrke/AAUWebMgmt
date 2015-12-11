<%@ Page Title="Home Page" Language="C#" AutoEventWireup="true" CodeBehind="UserInfo.aspx.cs" Inherits="ITSWebMgmt.UserInfo" MasterPageFile="~/Site.Master" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

<div class="ui dimmer modals page transition hidden" style="display: block !important;">
<div id="modalConfirmOUmove" class="ui small test modal transition hidden" style="margin-top: -92px; display: block !important;">
    <div class="header">
      Fix user OU
    </div>
    <div class="content">
      <p>This task will move the user to the right OU. This will affect applied GPOs.</p>
    </div>
    <div class="actions">
      <div class="ui negative button">
        No
      </div>

          <div runat="server" class="ui positive right labeled icon button" onclick="$('#MainContent_btnPostbackfixUserOUButton').click()">
       
           Yes
           <i class="checkmark icon"></i>
          </div>
    </div>
  </div>
</div>

<div id="loader">
<div class="ui active dimmer" style="display:none">
    <div class="ui text loader">Loading</div>
</div>
</div>

    <h1>User Info</h1>
    <div>


        Username
        <div class="ui action input">
            <asp:TextBox ID="UserNameBox" runat="server" Text="kyrke@its.aau.dk" CssClass="ui input focus" placeholder="Search..." />
            <asp:Button runat="server" CssClass="ui button" ID="sumbit" OnClick="lookupUser" Text="Søg"  OnClientClick='$("#loader > div").show("fast");'/>
        </div>
        <!--<asp:RegularExpressionValidator ID="regexEmailValid" runat="server" ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" ControlToValidate="UserNameBox" ErrorMessage="Invalid format"></asp:RegularExpressionValidator>
        <asp:RequiredFieldValidator runat="server" ID="reqName" ControlToValidate="UserNameBox" ErrorMessage="Please enter a value" />-->
        <br />
        <br />
        <script>
            $(document).ready(function () {
                $('.menu .item').tab({ history: false });
            });
        </script>

        <div runat="server" id="ResultDiv">
        
            
        <div runat="server" id="warningsAndErrorDIV">
        
            <div class="ui negative message" id="errorUserDisabled" style="display:none" runat="server">
            <div class="header">
                User is diabled
            </div>
            <p>The user is disabled in AD, user can't login. User is expired in AdmDB or disabled by a administrator, see <a href="onenote:https://docs.its.aau.dk/Documentation/Info%20til%20Service%20Desk/Disablet%20Users.one#Disabled%20users%20in%20AD&section-id={062F945F-AF8F-4E1C-8151-6C87AA1F134B}&page-id={86CE4A52-90A9-4A5C-A189-9402B9B6153B}&object-id={441C8DED-9C4E-4561-B184-186C63174D6D}&EB">onenote</a></p>
            </div>
        
            <div class="ui negative message" id="errorUserLockedDiv" style="display:none" runat="server">
            <div class="header">
                User account is locked
            </div>
            <p>The user account is locked, used tasks unlock account to unlock it.</p>
            </div>

            <div class="ui negative message" id="errorMissingAAUAttr" style="display:none" runat="server">
            <div class="header">
                User is missing AAU Attributes
            </div>
            <p>The user is missing one or more of the AAU attributes. The user will not be able to login via login.aau.dk. Check CPR is correct in ADMdb</p>
            </div>
            
             
            <div class="ui warning message" id="warningNotStandardOU" style="display:none" runat="server">
              <i class="close icon"></i>
              <div class="header">
                User is in a non standard OU
              </div>
              This might not be a problem. User can be affected by non-stadard group policy. User can be a service user or admin account.  
            </div>
               

        </div>
            
        <h2><asp:Label ID="UserNameLabel" runat="server"></asp:Label></h2>


        <div class="ui grid">
            <div class="four wide column">
                <div class="ui vertical fluid tabular menu">
                    <a class="active item" data-tab="basicinfo">Basic Info</a>
                    <a class="item" data-tab="servicemanager">Service Manager</a>
                    <a class="item" data-tab="computerInformation">Computer Information</a>
                    <!--<a class="item" data-tab="advancedinfo">Advanced Info</a> -->
                    <a class="item" data-tab="groups">Groups</a>
                    <a class="item" data-tab="fileshares">Fileshares</a>
                    <!--<a class="item" data-tab="networkdrives">Networkdrives</a>-->
                    <a class="item" data-tab="rawdata">Raw Data</a>
                    <a class="item" data-tab="tasks">Tasks</a>
<!--                    <a class="item" data-tab="statustest">Statustest</a> -->

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
                                <td>Navn</td>
                                <td><asp:Label runat="server" id="displayName"></asp:Label></td>
                            </tr>
                            <tr>
                                <td>ADMdb Expire Date</td>
                                <td><asp:Label runat="server" id="basicInfoAdmDBExpireDate"></asp:Label></td>
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
                        </tbody>
                    </table>
                </div>
                <div class="ui tab segment" data-tab="advancedinfo">
                    advanced
                </div>
                <div class="ui tab segment" data-tab="groups">
                    <h2>groups</h2>
                    <asp:Label ID="groupssegmentLabel" runat="server"></asp:Label>
                </div>
                <div class="ui tab segment" data-tab="fileshares">
                    <h2>fileshares</h2>
                    <asp:Label ID="filesharessegmentLabel" runat="server"></asp:Label>
                    
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
                

                <div class="ui tab segment" data-tab="rawdata">
                    <asp:Label ID="ResultLabel" runat="server"></asp:Label>
                </div>
                <div class="ui tab segment" data-tab="tasks">
                    Tasks<br />

                    Unlock Account (if locked by wrong password):
                    <asp:Button runat="server" CssClass="ui button" ID="unlockUserAccountButton" Text="Unlock Account" OnClick="unlockUserAccountButton_Click" /><br />
                    
                    <br /><br />
                    Roaming Profile (enable/disable): 
                    <asp:Button runat="server" CssClass="ui button" ID="ToggleRoaming" Text="ToggleRoaming" OnClick="button_toggle_userprofile" /><br />
                    <br /><br />
                    
                    <div runat="server" id="divFixuserOU">
                    Fix user OU (only do this if you know what you are doing): 
                    <asp:Button runat="server" OnClick="fixUserOUButton" Text="Fix User OU" id="btnPostbackfixUserOUButton" style="display:none"/>                    
                    <button type="button" Class="ui button" ID="btnFixOUShowModal" onclick="$('#modalConfirmOUmove').modal('show');" >Fix User OU</button><br />
                    </div>
                </div>
                <div class="ui tab segment" data-tab="statustest">
                    <table class="ui celled table">
                        <thead>
                            <tr>
                                <th>Name</th>
                                <th>Status</th>
                                <th>Notes</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>
                                <td>No Name Specified</td>
                                <td>Unknown</td>
                                <td class="negative">None</td>
                            </tr>
                            <tr class="positive">
                                <td>Jimmy</td>
                                <td><i class="icon checkmark"></i>Approved</td>
                                <td>None</td>
                            </tr>
                            <tr>
                                <td>Jamie</td>
                                <td>Unknown</td>
                                <td class="positive"><i class="icon close"></i>Requires call</td>
                            </tr>
                            <tr class="negative">
                                <td>Jill</td>
                                <td>Unknown</td>
                                <td>None</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
        </div>

        
        <!--<asp:TextBox ID="ResultBox" TextMode="MultiLine" Width="700" Height="200" runat="server"></asp:TextBox>-->

    </div>
</asp:Content>
