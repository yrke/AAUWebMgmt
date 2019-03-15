<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ComputerInfo.aspx.cs" Inherits="ITSWebMgmt.ComputerInfo" MasterPageFile="~/Site.Master" %>
<%@ Register TagPrefix="uc" TagName="Changes" Src="~/Controls/ActiveChanges.ascx" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <script>
        $(document).ready(function () {
            $('.menu .item').tab({ history: false });
            var active_tab = '<%= Session["ActiveTab"] %>';
            $('.menu .item').tab('change tab', active_tab);
        });
    </script>
    <div id="loader">
        <div class="ui active dimmer" style="display: none">
            <div class="ui text loader">Loading</div>
        </div>
    </div>

    <%-- <uc:Changes ID="changes" runat="server" /> --%>

    <h1>Computer Info</h1>
    <div>
        <form method="get">
            Computername: 
        <div class="ui action input">

            <input name="computername" class="ui input focus" value="<% =ComputerName %>" />

            <input type="submit" value="Search" onclick='$("#loader > div").show("fast");'>
        </div>
        </form>
        <br />
    </div>

    <div id="content">
        <form runat="server">
            <asp:hiddenfield id="tabName" value="test" runat="server"/>
            <asp:Button runat="server" ID="tabChangedButton" Text="" style="visibility:hidden" OnClick="TabChanged_Click" />
            <script>
                $(document).ready(function () {
                    $('.menu .item').tab({
                        'onVisible': function ()
                        {
                            var dataTabName = $(this).attr("data-tab");
                            document.getElementById("<%= tabName.ClientID %>").value = dataTabName; //$('#<%= tabName.ClientID %>').val(dataTabName); does the same
                            $("#load").show("fast");
                            document.getElementById('<%= tabChangedButton.ClientID %>').click();

                            // TODO: Make only load tab first time in JS to avoid blink on button click and server contact
                            // The code below should do it, but it returns false for all tabs after first case where it is true
                            // The hidden field is properly only updated on postback and it will therefore not be possible to do the above
                            // alert(dataTabName + " " + document.getElementById("<%= tabName.ClientID %>").value +" " +'<%= tabName.Value %>');
                            // if ('<%= Session[tabName.Value + "build"] == null %>' == 'True') {
                            //     document.getElementById('<%= tabChangedButton.ClientID %>').click();
                            // }
                        }
                    });
                });
            </script>
            <h2><asp:Label ID="UserNameLabel" runat="server"></asp:Label></h2>
            <asp:Label ID="ResultLabel" runat="server"></asp:Label>
            <div runat="server" id="ResultDiv">
                <div runat="server" id="warningsAndErrorDIV">
                    <asp:Label ID="ErrorCountMessageLabel" runat="server"></asp:Label>
                </div>
                <div class="ui grid">
                    <div class="four wide column">
                        <div class="ui vertical fluid tabular menu">
                            <a class="active item" data-tab="basicinfo">Basic Info</a>
                            <!--<a class="item" data-tab="userInformation">User Information</a>-->
                            <!--<a class="item" data-tab="advancedinfo">Advanced Info</a> -->
                            <a class="item" data-tab="groups">Groups</a>
                            <a class="item" data-tab="sccmInfo">SCCM Info</a>
                            <a class="item" data-tab="sccmInventory">Inventory</a>
                            <a class="item" data-tab="sccmAV">Antivirus</a>
                            <a class="item" data-tab="sccmHW">Hardware inventory</a>
                            <!--<a class="item" data-tab="networkdrives">Networkdrives</a>-->
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
                                        <td>Domain:</td>
                                        <td>
                                            <asp:Label runat="server" ID="labelDomain" /></td>
                                    </tr>
                                    <tr>
                                        <td>Admin Pwd Expire:</td>
                                        <td>
                                            <asp:Label runat="server" ID="labelPwdExpireDate" /></td>
                                    </tr>
                                    <tr>
                                        <td>PC Config:</td>
                                        <td>
                                            <asp:Label runat="server" ID="labelBasicInfoPCConfig" /></td>
                                    </tr>
                                    <tr>
                                        <td>Bitlocker Enabled:</td>
                                        <td>
                                            <asp:Label runat="server" ID="labelBasicInfoExtraConfig" /></td>
                                    </tr>
                                    <tr>
                                        <td>Managed By:</td>
                                        <td>
                                            <asp:Label runat="server" ID="labelManagedBy" />
                                            <asp:Button ID="EditManagedByButton" runat="server" value="" Text="Edit" OnClick="EditManagedBy_Click" CssClass="ui button" style="float:right"/>
                                            <asp:TextBox ID="labelManagedByText" runat="server" TextMode="SingleLine" Visible = "false"></asp:TextBox>
                                            <asp:Button ID="SaveEditManagedByButton" runat="server" value="" Text="Save" OnClick="SaveEditManagedBy_Click" CssClass="ui button" Visible = "false" style="float:right"/>
                                            <br>
                                            <asp:Label runat="server" ID="labelManagedByError" />
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                            <br />
                            <asp:Button ID="ResultGetPassword2" runat="server" value="" Text="Get Local Admin Password" OnClick="ResultGetPassword_Click" CssClass="ui button" />
                        </div>

                        <div class="ui tab segment" data-tab="userInformation">
                        </div>
                        <div class="ui tab segment" data-tab="rawdata">
                            <h2>Raw AD data</h2>
                            <asp:Label ID="ResultLabelRaw" runat="server"></asp:Label>
                        </div>
                        <div class="ui tab segment" data-tab="tasks">
                            <h2>Tasks</h2>
                            <br />
                            <asp:Button ID="ResultGetPassword" runat="server" value="" Text="Get Local Admin Password" OnClick="ResultGetPassword_Click" CssClass="ui button" />
                            <br />

                            <asp:Button ID="buttonEnableBitlockerEncryption" runat="server" value="" Text="Enable Bitlocker Encryption" OnClick="buttonEnableBitlockerEncryption_Click" CssClass="ui button" />
                            <br />
                            <div id="MoveComputerOUdiv" runat="server">
                                <asp:Button ID="MoveComputerOU" runat="server" value="" Text="Move computer to OU Clients" OnClick="MoveOU_Click" CssClass="ui button" />
                                Only do this if you know what you are doing!
                            </div>

                        </div>
                        <div class="ui tab segment" data-tab="sccmInfo">
                            <h2>SCCM Info</h2>
                            <h3>Computer Details</h3>
                            <asp:Label runat="server" ID="labelSCCMCollecionsTable" />

                            <h3>Collections</h3>
                            <asp:Label runat="server" ID="labelSCCMComputers" />

                            <h3>Raw data</h3>
                            <asp:Label runat="server" ID="labelSCCMCollections" />
                        </div>

                        <div class="ui tab segment" data-tab="sccmInventory">
                            <h2>SCCM Info</h2>
                            <asp:Label runat="server" ID="labelSSCMInventoryTable" />

                            <h3>Software Details</h3>
                            <asp:Label runat="server" ID="labelSCCMCollecionsSoftware" />

                            <h3>Raw</h3>
                            <asp:Label runat="server" ID="labelSCCMInventory" />
                        </div>

                        <div class="ui tab segment" data-tab="sccmAV">
                            <h2>Antivirus Info</h2>
                            <asp:Label runat="server" ID="labelSCCMAV" />
                        </div>

                        <div class="ui tab segment" data-tab="sccmHW">
                            <div class="ui active loader" style="display: none" id="load"></div>
                            <h2>Hardware Info</h2>
                            <h3>RAM</h3>
                            <asp:Label runat="server" ID="labelSCCMRAM" />
                            <h3>Processor</h3>
                            <asp:Label runat="server" ID="labelSCCMProcessor" />
                            <h3>Video controller</h3>
                            <asp:Label runat="server" ID="labelSCCMVC" />
                            <h3>BIOS</h3>
                            <asp:Label runat="server" ID="labelSCCMBIOS" />
                            <h3>Logical Disk</h3>
                            <asp:Label runat="server" ID="labelSCCMLD" />
                            <h3>Disk</h3>
                            <asp:Label runat="server" ID="labelSCCMDisk" />
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
                        <div class="ui tab segment" data-tab="warnings">
                            <h2>Warnings</h2>
                            <asp:Label ID="ErrorMessagesLabel" runat="server"></asp:Label>
                        </div>
                    </div>
                </div>
            </div>
        </form>
    </div>
</asp:Content>
