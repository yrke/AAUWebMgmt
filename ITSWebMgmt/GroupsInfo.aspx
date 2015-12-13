<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="GroupsInfo.aspx.cs" Inherits="ITSWebMgmt.GroupsInfo" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Loader -->
    <div id="loader">
        <div class="ui active dimmer" style="display: none">
            <div class="ui text loader">Loading</div>
        </div>
    </div>

    <!-- page seach header -->
    <div>
        <h1>Group Info</h1>
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
                    <asp:Label runat="server" ID="lblBasicInfo" />
                </div>
                 <div class="ui tab segment" data-tab="members">
                    <h2>Members</h2>
                    <asp:Label runat="server" ID="lblMembers" />
                </div>
                <div class="ui tab segment" data-tab="memberOf">
                    <h2>Member Of</h2>
                    <asp:Label runat="server" ID="lblMemberOf" />
                </div>
                <div class="ui tab segment" data-tab="rawData">
                    <h2>Raw Data</h2>
                    <asp:Label runat="server" ID="labelRawData" />
                </div>
            </div>
        </div>


    </div>









</asp:Content>
