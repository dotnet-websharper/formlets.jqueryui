<%@ Page Language="C#"  Trace="false" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>WebSharper Playground</title>
    <WebSharper:ScriptManager runat="server" />
    <style type="text/css">
        .formlet div 
        {
        	border : inherit;
        	padding : inherit;
        }
    </style>
</head>
<body>
    <Sample:SampleControl runat="server" />
</body>
</html>
