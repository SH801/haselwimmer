About the module 
================
- This C# Raven Authentication module is a direct code port of the PHP Raven Authentication class provided at http://raven.cam.ac.uk/project/
- For a full description of the latest Raven specification, go to http://raven.cam.ac.uk/project/
- For a graphical overview of how Raven Authentication works, see the included image "Raven_Authentication_Process_v3.jpg""

Platform requirements
---------------------
This module has been tested on .NET Framework 4.5. There may be problems running the code on earlier platforms.

Required libraries - OpenSSL.NET
--------------------------------
OpenSSL.NET can be downloaded from http://openssl-net.sourceforge.net/
OpenSSL.NET provides a C# wrapper for OpenSSL. Instructions for installing and enabling the OpenSSL.NET library within your C# project are provided with the OpenSSL.NET download.

Getting started
---------------
To use the C# class, create an instance of the class from within a C# server page:

var oUcam_Webauth = new Ucam_Webauth(args, Request, Response);        

- 'args' is an associative array of arguments of the form new Dictionary<string,string>(). 
- 'Request' and 'Response' are HTTP objects provided by the server page that Ucam_Webauth needs access to in order to query server variables and set cookies.

You create/add arguments for Ucam_Webauth as follows:

Dictionary<string, string> args = new Dictionary<string,string>();              
args.Add("hostname", "localhost");
args.Add("auth_service", "https://demo.raven.cam.ac.uk/auth/authenticate.html");
args.Add("key_dir", "C:/keydir");

You then call authenticate() and check for success():

if (oUcam_Webauth.authenticate())
{
	if (oUcam_Webauth.success())
	{
		... SUCCESS

A sample of a simple server page that loads the module can be found in the 'Default.aspx' file provided.

How does the module work?
-------------------------
The authenticate() function does all the heavy-lifting. It consists of three key phases, ordered as they appear in the code:

- 1. General/Subsequent Processing
- 2. Authentication Response
- 3. Authentication Request

The authenticate() function starts with some basic checks:
- If a dummy run is to be attempted, then we need do_session, ie. use session cookie, to be true.
- If we're using a session authentication cookie, we need cookie_key, the key we'll be using to sign our cookie, to be non-empty.
- Reject 'POST' requests.
- Check for valid 'hostname'.

With these checks out the way we proceed to 'General/Subsequent Processing'.

### General/Subsequent Processing
This section of code checks for a valid authentication cookie. While it's at the start of the authenticate() function, it's often the piece of code that is not fully utilised until right at the end of the authentication process, when a user has been verified and a session authentication cookie has been set. 
So let's assume a user has been verified, an authentication cookie has been set and we're quite advanced through the authentication process. During the body of the 'General/Subsequent Processing' code chunk, the following happens:
- Check that the session management cookie isn't set to TESTSTRING or WLS_LOGOUT. If it's neither, we have a potentially valid authentication cookie.
- Split the cookie up into an array of elements using the '!' delimiter. The array will be placed in the _authentication_response member variable which means values can easily be retrieved by other member functions later on.
- Remove the last element of the array as 'sig'.
- Join the remaining elements as a string using the '!' delimiter, then hmac_sha1_verify it using 'sig'. This basically creates a hash of the string and compares 'sig' with the hash value to check that the cookie has been correctly signed.
- If the authentication cookie has been correctly signed, check to see it hasn't expired. If not, then authentication is complete and we return true to the user.
- In the event of errors, we either return false to the user (if cookie not signed correctly) or we allow the program to continue on so that reauthentication may proceed (typically if the cookie has expired).

### Authenticated Response
If we hit the 'Authenticated Response' section of code, that means that no valid authentication cookie exists. This could be because the Web Login Service (WLS) has only just redirected the user back to the main website with a 'WLS-Response' variable. So our next step is to check to see if 'WLS-Response' exists in our current URL.
We run through all name/value pairs searching for 'WLS-Response'. This is necessary as the Raven Web Login Service may include other GET variables in its callback to the main website. If the 'WLS-Response' GET variable is found, the following happens:
- We split the 'WLS-Response' value into an array of elements using the '!' delimiter. 
- Remove the last two elements of this array as 'sig' and 'key_id'.
- Use the WLS_RESPONSE_STATUS field of the array to provide the status of the authentication provided from the Web Login Service.
- Set AUTHENTICATION_RESPONSE_MSG depending on the WLS_RESPONSE_STATUS.
- Check the 'sig'(nature) end portion of the 'WLS-Response' is correct for the remaining non-signature data within 'WLS-Response'. If it's not correct, return 'true' to the user.
- Check the time parameters of the 'WLS-Response' are sufficiently close to the current time. NOTE: clocks must by synced via NTP.
- Check the URL parameter of the 'WLS-Response' matches the current URL (once query parameters are removed).
- Expand the 'WLS-Response' array to its maximum size to avoid possible 'IndexOutOfRangeException'.
- Population _authentication_response member with information collected so far.
- If we're not using cookies to hold authentication state, then we return 'true' to the user to indicate the user's been authenticated.
- If we are intending to use cookies, we check that a test cookie with the same name already exists. This should have been set in the 'Authentication Request' part of the code during a previous iteration of 'authenticate()'. If no test cookie can be found, this suggests setting cookies is problematic. So we don't bother trying to and simply return 'true' (= authenticated) to the user.
- If a test authentication cookie is found, we add in some extra parameters into the WLS-Response array, bundle the array up as a string, get the hmac_sha1 hash signature for this string and then append this signature onto the string to form the new value of the authentication cookie - which will subsequently be verified in the 'General/Subsequent Processing' section of code, above.
- Finally we redirect the user to the 'url' field of the 'WLS-Response'. 

### Authentication Request
If no 'WLS-Response' GET variable was found during 'Authenticated Response', it's likely the user has only just tried to access an authenticated response and has yet to verify themselves through the Web Login Service. So we're right at the start of the authentication process. The following happens:
- Check to see if the HTTP_HOST as reported by the server is different from the 'hostname' provided in the code. If different, we redirect the user to the code-supplied hostname.
- Set a test authentication cookie. This will be used during the subsequent 'Authenticated Response' phase (see above) to check that cookies can in fact be set on the browser.
- Build a Web Login Server URL with different GET parameters then redirect the user to this URL; see 'Authentication Request' section within Raven documentation for full details of all correct parameters.

### Other important functions

#### url()
Get the url to redirect the browser to after a successful authentication attempt.
This is typically identical to the url that first calls this script. But due to the possibility of faking 'HOST' variables in a server response, the 'hostname' must be supplied as a setup / default parameter in the code. 

#### check_sig(string data, string sig, string key_id)
Checks that the 'sig'(nature) provided by the WLS when it signed 'data' is a valid signature for the data. This ensures the data has not been tampered with.

#### wls_encode(byte[] plainTextBytes)
Encode a byte array of data in a way that can be easily sent to the WLS.

#### wls_decode(string str)
Decode a string of data received from the WLS into a byte array of data.

#### hmac_sha1(string key, string data)
Create HMACSHA1 hash value for 'data' using public 'key'.

#### hmac_sha1_verify(string key, string data, string sig)
Verify that 'data' has been signed by public 'key' with 'sig'.

