<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DefendpointChallengeResponse.aspx.cs" Inherits="ITSWebMgmt.DefendpointChallengeResponse" MasterPageFile="~/Site.Master" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <form runat="server">

        <h1>Defendpoint Challenge/Response</h1>
        <div>
            Generate a response code:


            <div class="ui fluid input">
                <input runat="server" id="reasonInput"  type="text" placeholder="Reason">
            </div>
            <br />
            <div class="ui fluid action input">
                <input runat="server" id="challanageInput" type="text" placeholder="Challenge  (XXXX XXXX)">
                <button runat="server" class="ui button">Get Code</button>
            </div>

        
            <asp:Label runat="server" ID="resultLbl2" />
            
            



        </div>
    </form>
</asp:Content>