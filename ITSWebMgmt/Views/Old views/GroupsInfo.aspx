<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="GroupsInfo.aspx.cs" Inherits="ITSWebMgmt.GroupsInfo" %>

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
            <!--
        Group Name
        <div class="ui action input">
            <asp:TextBox ID="txtbxGroupName" runat="server" Text="" CssClass="ui input focus" placeholder="Search..." />
            <asp:Button runat="server" CssClass="ui button" ID="sumbit" OnClick="sumbit_Click" Text="Søg" OnClientClick='$("#loader > div").show("fast");' />
        </div>
        -->
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
                        <a class="item" data-tab="memberOf">Member Of</a>
                        <!--<a class="item" data-tab="">Recurs memeberof?</a>-->
                        <a class="item" data-tab="rawData">Raw Data</a>

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
                    </div>
                    <div class="ui tab segment" data-tab="memberOf">
                        <h2>Member Of</h2>
                        <asp:Label ID="memberOfInfoLabel" runat="server"></asp:Label>
                        <div class="ui two item menu">
                            <a data-tab="groups-direct" class="item">Direct Groups</a> 
                            <a data-tab="groups-all" class="item"> Recursive groups</a>
                        </div>

                        <div class="ui active tab segment" data-tab="groups-direct">
                            <asp:Label ID="groupofsegmentLabel" runat="server"></asp:Label>
                            </div>

                        <div class="ui tab segment" data-tab="groups-all">
                            <asp:Label ID="groupofAllsegmentLabel" runat="server"></asp:Label>
                        </div>
                    </div>
                    <div class="ui tab segment" data-tab="rawData">
                        <h2>Raw Data</h2>
                        <asp:Label runat="server" ID="labelRawData" />
                    </div>
                </div>
            </div>
        </div>
    </form>
</asp:Content>
