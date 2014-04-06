<%@ Page Title="Home Page" Language="C#" AutoEventWireup="true" %>

<%@ Import Namespace="System" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Web" %>
<%@ Import Namespace="Ucam" %>

<script runat="server"> 
     
    private void Page_Load(object sender, System.EventArgs e) 
    {
        Dictionary<string, string> args = new Dictionary<string,string>();
                
        // args.Add("auth_service", "https://demo.raven.cam.ac.uk/auth/authenticate.html");
        args.Add("hostname", "localhost:2438");
        args.Add("log_file", "C:/Users/User/Documents/Visual Studio 2013/WebSites/Ucam_Webauth/outputlog.txt");
        args.Add("key_dir", "C:/wamp/www/raven");
        args.Add("cookie_key", "Random string");
                
        var oUcam_Webauth = new Ucam_Webauth(args, Request, Response);        
        
        if (Request.ServerVariables["QUERY_STRING"] == "Action=Logout")
        {
            oUcam_Webauth.logout();
            Response.Write("Logged out of Raven (local)");
            Response.Write("<br/><a href=\"https://raven.cam.ac.uk/auth/logout.html\">Logout Raven (remote)</a>" +
                           "<br/><a href=\"Default.aspx\">Access Raven authenticated page</a>");            
            return;
        }

        if (oUcam_Webauth.authenticate())
        {
            if (oUcam_Webauth.success())
            {
                Response.Write("SUCCESS. You are " + oUcam_Webauth.principal());
                Response.Write("<br/>Ptags = " + oUcam_Webauth.ptags());
                NameValueCollection qscoll = HttpUtility.ParseQueryString(Request.ServerVariables["QUERY_STRING"]);
                foreach (String s in qscoll.AllKeys) Response.Write("<br>GET variable: " + s + "=" + qscoll[s]);
                Response.Write("<br/><a href=\"Default.aspx?Action=Logout\">Logout Raven (local)</a>");                
            }
            else Response.Write("FAIL - " + oUcam_Webauth.status() + ':' + oUcam_Webauth.msg());
        }
        else Response.Write("ERROR: Ucam_Webauth returned early");
    }
  
</script>
