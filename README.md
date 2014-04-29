University of Cambridge C# Raven Authentication Module - v.1.0 (29/04/2014)
============================================================================

Description
-----------
The software comprises a C# class 'Ucam_Webauth' and sample files that 
provide a C# implementation of a 'Raven' authentication module. Raven is 
the web authentication protocol used by the University of Cambridge, UK. 
The logic and code of the 'Ucam_Webauth' class are loosely based on the 
PHP Raven authentication class provided at http://raven.cam.ac.uk/project/. 
- For a full description of the latest Raven specification and an explanation 
of how Raven works, go to http://raven.cam.ac.uk/project/.
- This software was originally created for the Careers Service, University 
of Cambridge by sh801@cam.ac.uk

Files and folders
-----------------
[App_Code]: Contains the main C# class 'Ucam_Webauth'.
[bin]: The location for the binary DLLs for OpenSSL and OpenSSL.NET.
[certificates]: Temporary location for Raven public key certificates.
[docs]: Contains supporting documentation.
[logs]: The location for log files created by the module.
Default.aspx: A sample file showing how 'Ucam_Webauth' is used to provide 
Raven authentication.
Test.aspx: A test file for unit testing the 'Ucam_Webauth' module using the 
'Ucam_RavenWLS' dummy Raven server (separate project, not included).
Ucam_Webauth.sln: A Microsoft Visual Studio Solution file for the module.

Platform requirements
---------------------
This module has been tested on .NET Framework 2.0. 

Installation
------------

### Install OpenSSL.NET library
OpenSSL.NET provides a C# wrapper for OpenSSL and can be downloaded, along 
with the necessary OpenSSL binaries, from http://openssl-net.sourceforge.net/
Instructions for installing and enabling the OpenSSL.NET library within your 
C# project are provided with the download. 
- If you experience problems accessing the OpenSSL.NET library, ensure you 
have installed the correct binaries for your platform (32bit or 64bit). 
Copying both 32bit and 64bit DLLs onto the same machine may cause problems.

### Install Raven certificates
The authentication module uses the Raven public key certificate at 
https://raven.cam.ac.uk/project/keys/ to verify authentication responses. 
Download the certificate from https://raven.cam.ac.uk/project/keys/ and copy 
to the 'certificates' folder provided with the download - the 'certificates' 
folder is a temporary location for the certificate while you get the module 
up and running. You will need to supply the full path to the 'certificates' 
folder as either a 'key_dir' argument supplied to the 'Ucam_Webauth' 
constructor or by modifying the 'Ucam_Webauth.cs' variable 
'DEFAULT_KEY_DIR' directly. 

Once you have everything running correctly, move the 'certificates' folder 
to a new location on your webserver that is not web- or publicly-accessible 
and modify the 'key_dir' string accordingly.
 
- NOTE: you may have to change the name of the key file from 'pubkey2.crt' 
to '2.crt'. 

If you're using the Raven test server 
(http://raven.cam.ac.uk/project/test-demo/) for testing purposes, make sure 
you install the test server keys instead, but ensure you remove these keys 
before using the authentication module in a production environment. This is 
following advice on the demo webpage:
>> It is vital to keep these demo keys seperate from keys 
>> used with a production service - failure to do so could 
>> allow an attacker to successfully replay a response 
>> from the demonstration server, which anyone can easily 
>> obtain, against a production service. 


Getting started
---------------

To use the 'Ucam_Webauth' C# class:

- 1. Ensure the 'Ucam_Webauth.cs' class file is included in your project's 
'App_Code' folder.
- 2. Reference the 'Ucam_Webauth.cs' namespace by including a reference 
within your ASP.NET or C# file:

<%@ Import Namespace="Ucam" %>

OR

using Ucam;

- 3. Set up the initial parameters for the 'Ucam_Webauth' class:

Dictionary<string, string> args = new Dictionary<string,string>();              
args.Add("hostname", "localhost");
args.Add("auth_service", "https://demo.raven.cam.ac.uk/auth/authenticate.html");
args.Add("key_dir", "C:/Ucam_Webauth/certificates");

'args' is an associative array of *text* strings so parameter values must 
be converted into strings - numbers and booleans must be supplied within 
quotes, ie. "23", "TRUE", "FALSE".
A full list of allowed parameters is provided at the end of this README. 

- 4. Create an instance of the class from within a C# server page:

var oUcam_Webauth = new Ucam_Webauth(args, Request, Response);        

'Request' and 'Response' are HTTP server variables that must be supplied to 
'Ucam_Webauth' to allow it to check/set headers and server variables.

- 5. Call 'Authenticate()' on the Ucam_Webauth object and act according to 
the value returned:

switch (oUcam_Webauth.Authenticate())
{
    case Ucam_Webauth.AUTHENTICATE_INCOMPLETE:

		... 
        break;                                               

    case Ucam_Webauth.AUTHENTICATE_COMPLETE_AUTHENTICATED:

		...                
        break;

    case Ucam_Webauth.AUTHENTICATE_COMPLETE_NOT_AUTHENTICATED:

		...
		break;

    case Ucam_Webauth.AUTHENTICATE_COMPLETE_ERROR:

		...                
        break;                
}

The four possible return values of 'Authenticate()' are:

AUTHENTICATE_INCOMPLETE : The authentication process has yet to complete. 
It may be waiting on user interaction at the WLS server.
AUTHENTICATE_COMPLETE_AUTHENTICATED : The authentication process completed 
and the user has been successfully authenticated.
AUTHENTICATE_COMPLETE_NOT_AUTHENTICATED : The authentication process 
completed and the user was not successfully authenticated. 
The user may have clicked 'Cancel' at the WLS server.
AUTHENTICATE_COMPLETE_ERROR : There was an error during the authentication 
process forcing the authentication cycle to terminate early.

As the 'Authenticate()' function may need to send HTTP headers, it must be 
called before any output (e.g. HTML, HTTP headers) is sent to the browser.

A sample of a simple server page using the 'Ucam_Webauth' C# module can be 
found in the 'Default.aspx' file provided.

Overview of Raven process
-------------------------
A diagram of the Raven authentication process is located within the 'docs' 
folder as "I - Overview of Raven Authentication Process.pdf". 

The authentication cycle consists of the following key stages:

### User first tries to access authenticated page
User tries to load an authenticated page on a particular website. 
The 'Ucam_Webauth' class is loaded and the 'Authenticate()' function is called. 
If no authentication cookie is found to indicate the user is authenticated, 
the user's browser is redirected to a separate Raven server using a special 
'Authentication Request'. The authentication request consists of a series of 
authentication parameters encoded into the URL redirect as name/value pairs.

### User is prompted for login information
The Raven server interprets the authentication request sent by the main 
website and prompts the user for their username and password. The user may 
then be successfully authenticated or may decide to click 'Cancel'. They are 
redirected back to the main website with a series of 'Authentication Response' 
parameters encoded into a 'WLS-Response' GET variable.

### User redirected back to main webserver
The user's original page is loaded again and 'Authenticate()' is called a 
second time. 'Ucam_Webauth' processes the new 'WLS-Response' GET value and, 
if it's valid, sets an authentication cookie on the user's browser. The 
user's original page is then loaded again. 

### User redirected back to main webserver  
With an authentication cookie now set, 'Authenticate()' returns either 
'AUTHENTICATE_COMPLETE_AUTHENTICATED' or 
'AUTHENTICATE_COMPLETE_NOT_AUTHENTICATED'. If 
'AUTHENTICATE_COMPLETE_AUTHENTICATED', the original page can go on to 
display authenticated content to the user.

Specifics of module
-------------------
The 'Authenticate()' function is the overarching authentication function of 
'Ucam_Webauth'. It starts off by performing some basic sanity checks using 
'CheckSetup()' then uses 'GetCurrentState()' to establish the current state 
of the authentication process:

### STATE_NEW_AUTHENTICATION
A completely fresh authentication. 'SendAuthenticationRequest()' [*1*] is 
called which sends an authentication request to the Raven authentication 
server. 'SendAuthenticationRequest()' performs some basic data checks, sets 
the authentication cookie to 'AUTHENTICATIONCOOKIE_REDIRECT_WLS' (to record 
where we are in the authentication process) and redirects the users browser 
to the Raven authentication server with a series of authentication parameters 
encoded as name/value pairs.

### STATE_WLS_RESPONSE_RECEIVED
The Raven authentication server has processed the user and has returned the 
user's browser back to the original website with a series of authentication 
response parameters encoded into the 'WLS-Response' GET variable. 
'ProcessAuthenticationResponse()' [*2*] is then called which checks the 
validity of the 'WLS-Response' value, sets an authentication cookie and 
redirects the user back to the original page.

### STATE_WAA_AUTHENTICATIONCOOKIE_SET
A valid authentication cookie has been set 
(<> AUTHENTICATIONCOOKIE_REDIRECT_WLS). 
'ProcessAuthenticationCookie()' [*3*] is then called which checks the 
validity of the cookie. If the cookie has expired, 
'SendAuthenticationRequest()' is called again in case the user needs to 
reauthenticate themselves. If the cookie is still valid, an 
'AUTHENTICATE_COMPLETE_XXX' value is returned to the user indicating that 
the authentication cycle has completed successfully. 
NOTE: this may be true if the user has cancelled the authentication process, 
in which case 'AUTHENTICATE_COMPLETE_NOT_AUTHENTICATED' will be returned.

### STATE_ERROR
An error occurred, breaking the authentication cycle and returning 
AUTHENTICATE_COMPLETE_ERROR to user.

Detailed diagrams of the Raven process flow for (i) a successful 
authentication (ii) a cancelled authentication are located in the 'docs' 
folder as "II - Ucam_Webauth - Flowchart for Valid Authentication" and 
"III - Ucam_Webauth - Flowchart for Cancelled Authentication", respectively. 

The numbers on these diagrams correspond to the three key secondary function 
described above:
- *1*. SendAuthenticationRequest()
- *2*. ProcessAuthenticationResponse()
- *3*. ProcessAuthenticationCookie()

Other important functions include:

### ResetState()
Attempts to reset state as if a new user has just loaded a fresh browser 
window. This is typically used when a user has experienced an error and we 
want to give them a fresh opportunity to try again. 


### check_signature(...)
Checks the signature provided by the Raven server when it signed the 
'WLS-Response' variable is a valid signature for the data. This ensures the 
data has not been tampered with.

### hmac_sha1(...)
Creates a hash value for signing the local authentication cookie.

### wls_encode/decode(...)
Encoding/decoding functions to allow base64 signatures to be sent within URLs.

Possible arguments to 'Ucam_Webauth'
------------------------------------
(Based on documentation for PHP authentication module)

- log_file : 
The location for a log file that will record progress and track possible 
errors. The folder containing the file must be read/writable by the webserver.
Default: log.txt

- response_timeout : 
Responses from the central authentication server are time-stamped. 
This parameter sets the period of time in seconds during which these 
responses are considered valid. 
Default: 30 seconds.

- key_dir : 
The name of the directory containing the public key certificate(s) required 
to validate the authentication responses sent by the server. 
Default: '/etc/httpd/conf/webauth_keys'.

- max_session_life : 
The maximum period of time in seconds for which an established session will 
be valid. This may be overriden if the authentication reply contains a 
shorter 'life' parameter. Note that this does NOT define an expiry time for 
the session cookie. Session cookies are always set without an expiry time, 
causing them to expire when the browser session finishes. 
Default: 7200 (2 hours).

- timeout_message : 
A re-authentication by the authentication service will be triggered when an 
established session expires. This option sets a text string which is sent to 
the authentication server to explain to the user why they are being asked to 
authenticate again. HTML markup is suppressed as for the description 
parameter described below. 
Default: 'your login to the site has expired'.

- hostname (required) :
The fully-qualified TCP/IP hostname that should be used in request URLs 
referencing the Ucam_Webauth-enabled application. This *must* be set, as it 
is needed for multiple reasons - primarily security but also to avoid varying 
hostnames in URLs leading to failed or inconsistent authentication. 
No default.

- cookie_key (required): 
A random key used to protect session cookies from tampering. Any reasonably 
unpredictable string (for example the MD5 checksum of a rapidly changing 
logfile) will be satisfactory. This key must be the same for all uses of the 
web authentication system that will receive the same session cookies (see the 
cookie_name, cookie_path and cookie_domain parameters below). 
No default.

- cookie_name :
The name used for the session cookie. 
When used for access to resources over HTTPS the string '-S' is appended to 
this name. 
Default: 'Ucam-Webauth-Session'.

- cookie_path :
The 'Path' attribute for the session cookie. The default is the directory 
component of the path to the script currently being executed. This should 
result in the cookie being returned for future requests for this script and 
for the other resources in the same 'directory'; see the important 
information about the cookie_key parameter above. 
Default: '/'.

- cookie_domain :
The 'Domain' attribute for the session cookie. By default the 'Domain' 
attribute is omitted when setting the cookie. This should result in the 
cookie being returned only to the server running the script. Be aware that 
some people may treat with suspicion cookies with domain attributes that are 
wider than the host setting the cookie. 
No default.

- auth_service : The full URL for the web login service to be used. 
Default: 'https://raven.cam.ac.uk/auth/authenticate.html' 

### The following parameters prefixed with 'authrequest_' relate to properties 
sent to the authentication server as part of an authentication request:

- authrequest_desc : A text description of the resource that is requesting 
authentication. This may be displayed to the user by the authentication 
service. It is restricted to printable ASCII characters (0x20 - 0x7e) though 
it may contain HTML entities representing other characters. The characters 
'<' and '>' will be converted into HTML entities before being sent to the 
browser and so this text cannot usefully contain HTML markup.
No default.

- authrequest_params : Data that will be returned unaltered to the WAA in 
any 'authentication response message' issued as a result of this request. 
This could be used to carry the identity of the resource originally requested 
or other WAA state, or to associate authentication requests with their 
eventual replies. When returned, this data will be protected by the digital 
signature applied to the authentication response message but nothing else is 
done to ensure the integrity or confidentiality of this data - the WAA MUST 
take responsibility for this if necessary.
No default.

- authrequest_skew : Interpretation of response_timeout is difficult if the 
clocks on the server running the PHP agent and on the authentication server 
are out of step. Both servers should use NTP to synchronize their clocks, 
but if they don't then this parameter should be set to an estimate of the 
maximum expected difference between them (in seconds). 
Default: 0.

- authrequest_fail :
If TRUE, sets the fail parameter in any authentication request sent to the 
authentication server to 'yes'. This has the effect of requiring the 
authentication server itself to report any errors that it encounters, rather 
than returning an error indication. Note however that even with this parameter 
set errors may be detected by this module that will result in authentication 
failing here. 
Default: FALSE.

- authrequest_iact :
If TRUE, then the 'iact' parameter provided to the authentication server is 
set to 'yes'. If FALSE, then the 'iact' parameter is set to 'no'. If no value 
is provided for 'authrequest_iact', the 'iact' parameter is left blank. 
The value 'yes' for 'iact' requires that a re-authentication exchange takes 
place with the user. This could be used prior to a sensitive transaction in 
an attempt to ensure that a previously authenticated user is still present 
at the browser. The value 'no' requires that the authentication request will 
only succeed if the user's identity can be returned without interacting with 
the user. This could be used as an optimisation to take advantage of any 
existing authentication but without actively soliciting one. If omitted or 
empty, then a previously established identity may be returned if the WLS 
supports doing so, and if not then the user will be prompted as necessary.
Default: omitted.
