<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="test.aspx.cs" Inherits="ITSWebMgmt.test" %>
<%@ Register TagPrefix="WebMgmt" TagName="UserInfoBoxControl" Src="~/DynamicData/FieldTemplates/GroupEditor_Edit.ascx" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>

        <script src="https://cdnjs.cloudflare.com/ajax/libs/jquery/3.0.0-alpha1/jquery.min.js" ></script>

</head>
<body>
  
    <div>
        



<form id="createSCSMform" target="_blank" method="post">




        <input title="" name="title" />
        
        <input type="text" name="description" />
        <input name="vm" value='{"Description":"fisk fisk","RequestedWorkItem":{  
                "BaseId":"008f492b-df58-6e9c-47c5-bd4ae81028af",
                "DisplayName":"Kenneth Yrke Jørgensen",
         }}'/>
        <input id="createSR" type="submit" value="Submit new SR" data-url="https://service.aau.dk/ServiceRequest/New/">
	    <input id="createIR" type="submit" value="Submit new IR" data-url="https://service.aau.dk/Incident/New/">

	    
        </form>

          <script>
              $("#createSR, #createIR").click(function (e) {
                  e.preventDefault();

                  var form = $("#createSCSMform");

                  form.prop("action", $(this).data("url"));
                  form.submit();
              });
          </script>

    </div>
  
</body>
</html>
