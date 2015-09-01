<%@ Page Title="Home Page" Language="C#" AutoEventWireup="true" CodeBehind="UserInfo.aspx.cs" Inherits="ITSWebMgmt.WebForm1" MasterPageFile="~/Site.Master"%>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <h1> User Info</h1>
    <div>

        Username
        <div class="ui action input">
         <asp:TextBox ID="UserNameBox" runat="server"  Text="kyrke@its.aau.dk" CssClass="ui input focus" placeholder="Search..." /> <asp:Button runat="server" CssClass="ui button" ID="sumbit" OnClick="lookupUser" Text="Søg" />
        </div>
            <asp:RegularExpressionValidator ID="regexEmailValid" runat="server" ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" ControlToValidate="UserNameBox" ErrorMessage="Invalid format"></asp:RegularExpressionValidator>
            <asp:RequiredFieldValidator runat="server" id="reqName" controltovalidate="UserNameBox" errormessage="Please enter a value" />
        <br />
        
        <div class="ui grid">
  <div class="four wide column">
    <div class="ui vertical fluid tabular menu">
      <a class="active item">
        Bio
      </a>
      <a class="item">
        Pics
      </a>
      <a class="item">
        Companies
      </a>
      <a class="item">
        Links
      </a>
    </div>
  </div>
  <div class="twelve wide stretched column">
    <div class="ui segment">
      This is an stretched grid column. This segment will always match the tab height
    </div>
  </div>
</div>

        <h2><asp:Label ID="UserNameLabel" runat="server"></asp:Label></h2>
        <asp:Label ID="ResultLabel" runat="server"></asp:Label>
        <!--<asp:TextBox ID="ResultBox" TextMode="MultiLine" Width="700" Height="200" runat="server"></asp:TextBox>-->
        
    </div>

</asp:Content>