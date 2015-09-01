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
        <div class="ui grid">
            <div class="four wide column">
                <div class="ui vertical fluid tabular menu">
                    <a class="active item" data-tab="basicinfo">Basic Info</a>
                    <a class="item" data-tab="advancedinfo">Advanced Info</a>
                    <a class="item" data-tab="groups">Groups</a>
                    <a class="item" data-tab="fileshares">Fileshares</a>
                    <a class="item" data-tab="networkdrives">Networkdrives</a>
                    <a class="item" data-tab="rawdata">Raw Data</a>
                    <a class="item" data-tab="tasks">Tasks</a>
                </div>
            </div>
            <div class="twelve wide stretched column">
                <div class="ui tab segment">
                    none<!-- spacer as the fist elemen else is placed differencet -->
                </div>
                <div class="ui active tab segment" data-tab="basicinfo">
                    basis
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
                    raw
                </div>
                <div class="ui tab segment" data-tab="tasks">
                    Tasks
                </div>
            </div>
        </div>

        <h2>
            <asp:Label ID="UserNameLabel" runat="server"></asp:Label></h2>
        <asp:Label ID="ResultLabel" runat="server"></asp:Label>
        <!--<asp:TextBox ID="ResultBox" TextMode="MultiLine" Width="700" Height="200" runat="server"></asp:TextBox>-->

    </div>

</asp:Content>
