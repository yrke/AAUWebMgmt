<%@ Page Title="Home Page" Language="C#" AutoEventWireup="true" CodeBehind="UserInfo.aspx.cs" Inherits="ITSWebMgmt.WebForm1" MasterPageFile="~/Site.Master" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <h1>User Info</h1>
    <div>
        Username
        <div class="ui action input">
            <asp:TextBox ID="UserNameBox" runat="server" Text="kyrke@its.aau.dk" CssClass="ui input focus" placeholder="Search..." />
            <asp:Button runat="server" CssClass="ui button" ID="sumbit" OnClick="lookupUser" Text="Søg" />
        </div>
        <asp:RegularExpressionValidator ID="regexEmailValid" runat="server" ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" ControlToValidate="UserNameBox" ErrorMessage="Invalid format"></asp:RegularExpressionValidator>
        <asp:RequiredFieldValidator runat="server" ID="reqName" ControlToValidate="UserNameBox" ErrorMessage="Please enter a value" />
        <br />
        <br />
        <script>
            $(document).ready(function () {
                $('.menu .item').tab({ history: false });
            });
        </script>

        <div runat="server" id="ResultDiv">
        <h2><asp:Label ID="UserNameLabel" runat="server"></asp:Label></h2>


        <div class="ui grid">
            <div class="four wide column">
                <div class="ui vertical fluid tabular menu">
                    <a class="active item" data-tab="basicinfo">Basic Info</a>
                    <!--<a class="item" data-tab="advancedinfo">Advanced Info</a>
                    <a class="item" data-tab="groups">Groups</a>
                    <a class="item" data-tab="fileshares">Fileshares</a>
                    <a class="item" data-tab="networkdrives">Networkdrives</a>-->
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
                                <td>Email:</td>
                                <td></td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div class="ui tab segment" data-tab="advancedinfo">
                    advanced
                </div>
                <div class="ui tab segment" data-tab="groups">
                    groups
                </div>
                <div class="ui tab segment" data-tab="fileshares">
                    fileshares
                </div>
                <div class="ui tab segment" data-tab="networkdrives">
                    networkdrives
                </div>
                <div class="ui tab segment" data-tab="rawdata">
                    <asp:Label ID="ResultLabel" runat="server"></asp:Label>
                </div>
                <div class="ui tab segment" data-tab="tasks">
                    Tasks<br />

                    Roaming Profile (enable/disable): 
                    <asp:Button runat="server" CssClass="ui button" ID="ToggleRoaming" Text="ToggleRoaming" OnClick="button_toggle_userprofile" />

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
