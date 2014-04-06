
/// <summary>
/// This C# class implements a Raven v3 agent for the University of Cambridge
/// Web Authentication System
///
/// See http://raven.cam.ac.uk/project/ for more details
///
/// Code is a direct C# port of PHP module for Raven
/// https://wiki.cam.ac.uk/raven/PHP_library
///
/// Copyright (c) 2004, 2005, 2008, 2014 University of Cambridge
///
/// This module is free software; you can redistribute it and/or modify
/// it under the terms of the GNU Lesser General Public License as
/// published by the Free Software Foundation; either version 2.1 of the
/// License, or (at your option) any later version.
///
/// The module is distributed in the hope that it will be useful, but
/// WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
/// Lesser General Public License for more details.
///
/// You should have received a copy of the GNU Lesser General Public
/// License along with this toolkit; if not, write to the Free Software
/// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307
/// USA
///
/// $Id: ucam_webauth.cs, v1.0 2014/04/05 08:13:00 sh801 
///
/// Version 1.0
/// </summary>

namespace Ucam
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Web;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using OpenSSL;
    using OpenSSL.Core;
    using OpenSSL.Crypto;

    /// <summary>
    /// Implements a Raven v3 agent for the University of Cambridge
    /// Web Authentication System.
    /// 
    /// See http://raven.cam.ac.uk/project/ for more details
    ///
    /// Code is a direct C# port of PHP module for Raven
    /// https://wiki.cam.ac.uk/raven/PHP_library
    ///
    /// Version 1.0
    /// </summary>
    public class Ucam_Webauth
    {

        // 'status_codes' is associative array of status codes of the form {"CODE" => "Description", ..}

        Dictionary<string, string> status_codes;

        private string PROTOCOL_VERSION = "3";
        private string AUTHENTICATION_RESPONSE_VERSION = "3";
        private string DEFAULT_AUTH_SERVICE = "https://raven.cam.ac.uk/auth/authenticate.html";
        private string DEFAULT_KEY_DIR = "/etc/httpd/conf/webauth_keys";
        private string DEFAULT_COOKIE_NAME = "Ucam-WebAuth-Session";
        private string DEFAULT_TIMEOUT_MESSAGE = "your logon to the site has expired";
        private string DEFAULT_HOSTNAME = null;		// must be supplied explicitly
        private int COMPLETE = 1;
        private string TESTSTRING = "Test";
        private string WLS_LOGOUT = "Not-authenticated";
        private string DEFAULT_LOG_FILE = "log.txt";

        // Index numbers for the 'Authentication response' fields
        // See Raven specification documentation for descriptions of each parameter

        private int AUTHENTICATION_RESPONSE_VER = 0;
        private int AUTHENTICATION_RESPONSE_STATUS = 1;
        private int AUTHENTICATION_RESPONSE_MSG = 2;
        private int AUTHENTICATION_RESPONSE_ISSUE = 3;
        private int AUTHENTICATION_RESPONSE_EXPIRE = 4;
        private int AUTHENTICATION_RESPONSE_ID = 5;
        private int AUTHENTICATION_RESPONSE_PRINCIPAL = 6;
        private int AUTHENTICATION_RESPONSE_PTAGS = 7;
        private int AUTHENTICATION_RESPONSE_AUTH = 8;
        private int AUTHENTICATION_RESPONSE_SSO = 9;
        private int AUTHENTICATION_RESPONSE_PARAMS = 10;
        private int AUTHENTICATION_RESPONSE_SIG = 11;
        private int AUTHENTICATION_RESPONSE_SIZE = 12; // Size of required array

        // ********* Why do we need AUTHENTICATION_RESPONSE_XXX and WLS_RESPONSE_XXX???

        private int WLS_RESPONSE_VER = 0;
        private int WLS_RESPONSE_STATUS = 1;
        private int WLS_RESPONSE_MSG = 2;
        private int WLS_RESPONSE_ISSUE = 3;
        private int WLS_RESPONSE_ID = 4;
        private int WLS_RESPONSE_URL = 5;
        private int WLS_RESPONSE_PRINCIPAL = 6;
        private int WLS_RESPONSE_PTAGS = 7;
        private int WLS_RESPONSE_AUTH = 8;
        private int WLS_RESPONSE_SSO = 9;
        private int WLS_RESPONSE_LIFE = 10;
        private int WLS_RESPONSE_PARAMS = 11;
        private int WLS_RESPONSE_KID = 12;
        private int WLS_RESPONSE_SIG = 13;
        private int WLS_RESPONSE_SIZE = 14; // Size of required array

        private string[] _authentication_response;

        private bool _do_session;
        private string _cookie_key;
        private string _cookie_path;
        private string _auth_service;
        private string _description;
        private int _response_timeout;
        private int _clock_skew;
        private string _hostname;
        private string _key_dir;
        private int _max_session_life;
        private string _timeout_message;
        private string _cookie_name;
        private string _cookie_domain;
        private bool _fail;
        // private string status;
        // private string headers;    
        private string _forced_reauth_message;
        private bool _interact;
        private string _log_file;
        private string _aauth;

        // Extra variables not used in PHP class

        private HttpRequest _httprequest;
        private HttpResponse _httpresponse;
        private bool _use_interact;

        /// <summary>
        /// Constructor for Ucam_Webauth
        /// 
        /// To supply arguments:
        /// 
        /// <c>
        /// Dictionary<string, string> args = new Dictionary<string,string>();                
        /// args.Add("auth_service", "https://...");
        /// args.Add("hostname", "www...");
        /// args.Add("log_file", "C:/logfile.txt");
        /// args.Add("key_dir", "C:/raven");
        /// args.Add("cookie_key", "Random string");
        /// </c>
        /// 
        /// </summary>
        /// <param name="args">An associative array of arguments as name,value pairs, eg. {"auth_service" => "http://..", "hostname" => "www...", "log_file" => }</param>
        /// <param name="httprequest">Provided by main webserver process in order to allow server variables to be read</param>
        /// <param name="httpresponse">Provided by main webserver process in order to allow cookies to be set</param>
        public Ucam_Webauth(Dictionary<string, string> args, HttpRequest httprequest = null, HttpResponse httpresponse = null)
        {
            // Set up 'status_codes' associative array
            // See Raven specification documentation for descriptions of each parameter

            status_codes = new Dictionary<string, string>();
            status_codes.Add("200", "Successful authentication");
            status_codes.Add("410", "The user cancelled the authentication request");
            status_codes.Add("510", "No mutually acceptable authentication types available");
            status_codes.Add("520", "Unsupported protocol version");
            status_codes.Add("530", "General request parameter error");
            status_codes.Add("540", "Interaction would be required");
            status_codes.Add("560", "WAA not authorised");
            status_codes.Add("570", "Authentication declined");

            this._authentication_response = new string[this.AUTHENTICATION_RESPONSE_SIZE];

            this.httprequest = httprequest;
            this.httpresponse = httpresponse;

            if (args.ContainsKey("log_file")) this.log_file = args["log_file"];
            else this.log_file = this.DEFAULT_LOG_FILE;

            if (args.ContainsKey("auth_service")) this.auth_service = args["auth_service"];
            else this.auth_service = this.DEFAULT_AUTH_SERVICE;

            if (args.ContainsKey("description")) this.description = args["description"];

            if (args.ContainsKey("response_timeout")) this.response_timeout = Convert.ToInt32(args["response_timeout"]);
            else this.response_timeout = 30;

            if (args.ContainsKey("clock_skew")) this.clock_skew = Convert.ToInt32(args["clock_skew"]);
            else this.clock_skew = 5;

            if (args.ContainsKey("key_dir")) this.key_dir = args["key_dir"];
            else this.key_dir = this.DEFAULT_KEY_DIR;

            if (args.ContainsKey("do_session")) this.do_session = Convert.ToBoolean(args["do_session"]);
            else this.do_session = true;

            if (args.ContainsKey("max_session_life")) this.max_session_life = Convert.ToInt32(args["max_session_life"]);
            else this.max_session_life = 2 * 60 * 60;

            if (args.ContainsKey("timeout_message")) this.timeout_message = args["timeout_message"];
            else this.timeout_message = this.DEFAULT_TIMEOUT_MESSAGE;

            if (args.ContainsKey("cookie_key")) this.cookie_key = args["cookie_key"];

            if (args.ContainsKey("cookie_name")) this.cookie_name = args["cookie_name"];
            else this.cookie_name = this.DEFAULT_COOKIE_NAME;

            if (args.ContainsKey("cookie_path")) this.cookie_path = args["cookie_path"];
            else this.cookie_path = ""; // *** SHOULD BE PATH RELATIVE PATH TO SCRIPT BY DEFAULT ***

            if (args.ContainsKey("hostname")) this.hostname = args["hostname"];
            else this.hostname = this.DEFAULT_HOSTNAME;

            // COOKIE PATH CAN BE NULL, DEFAULTS TO CURRENT DIRECTORY (TEST)

            if (args.ContainsKey("cookie_domain")) this.cookie_domain = args["cookie_domain"];
            else this.cookie_domain = "";

            if (args.ContainsKey("fail")) this.fail = Convert.ToBoolean(args["fail"]);
            else this.fail = false;

            if (args.ContainsKey("forced_reauth_message")) this.forced_reauth_message = args["forced_reauth_message"];

            if (args.ContainsKey("interact"))
            {
                this.interact = Convert.ToBoolean(args["interact"]);
                this.use_interact = true;
            }
            else
            {
                this.use_interact = false;
            }

        }

        /// <summary>
        /// Get/set variable
        /// </summary>
        /// <returns></returns>
        public bool do_session
        {
            get { return this._do_session; }
            set { this._do_session = value; }
        }

        /// <summary>
        /// Get/set variable
        /// </summary>
        /// <returns></returns>
        public string cookie_key
        {
            get { return this._cookie_key; }
            set { this._cookie_key = value; }
        }

        /// <summary>
        /// Get/set variable
        /// </summary>
        /// <returns></returns>
        public string cookie_path
        {
            get { return this._cookie_path; }
            set { this._cookie_path = value; }
        }

        /// <summary>
        /// Get/set variable
        /// </summary>
        /// <returns></returns>
        public string auth_service
        {
            get { return this._auth_service; }
            set { this._auth_service = value; }
        }

        /// <summary>
        /// Get/set variable
        /// </summary>
        /// <returns></returns>
        public string description
        {
            get { return this._description; }
            set { this._description = value; }
        }

        /// <summary>
        /// Get/set variable
        /// </summary>
        /// <returns></returns>
        public int response_timeout
        {
            get { return this._response_timeout; }
            set { this._response_timeout = value; }
        }

        /// <summary>
        /// Get/set variable
        /// </summary>
        /// <returns></returns>
        public int clock_skew
        {
            get { return this._clock_skew; }
            set { this._clock_skew = value; }
        }

        /// <summary>
        /// Get/set variable
        /// </summary>
        /// <returns></returns>
        public string hostname
        {
            get { return this._hostname; }
            set { this._hostname = value; }
        }

        /// <summary>
        /// Get/set variable
        /// </summary>
        /// <returns></returns>
        public string key_dir
        {
            get { return this._key_dir; }
            set { this._key_dir = value; }
        }

        /// <summary>
        /// Get/set variable
        /// </summary>
        /// <returns></returns>
        public int max_session_life
        {
            get { return this._max_session_life; }
            set { this._max_session_life = value; }
        }

        /// <summary>
        /// Get/set variable
        /// </summary>
        /// <returns></returns>
        public string timeout_message
        {
            get { return this._timeout_message; }
            set { this._timeout_message = value; }
        }

        /// <summary>
        /// Get/set variable
        /// </summary>
        /// <returns></returns>
        public string cookie_name
        {
            get { return this._cookie_name; }
            set { this._cookie_name = value; }
        }

        /// <summary>
        /// Get/set variable
        /// </summary>
        /// <returns></returns>
        public string cookie_domain
        {
            get { return this._cookie_domain; }
            set { this._cookie_domain = value; }
        }

        /// <summary>
        /// Get/set variable
        /// </summary>
        /// <returns></returns>
        public bool fail
        {
            get { return this._fail; }
            set { this._fail = value; }
        }

        /// <summary>
        /// Get/set variable
        /// </summary>
        /// <returns></returns>
        public string forced_reauth_message
        {
            get { return this._forced_reauth_message; }
            set { this._forced_reauth_message = value; }
        }

        /// <summary>
        /// Get/set variable
        /// </summary>
        /// <returns></returns>
        public bool interact
        {
            get { return this._interact; }
            set { this._interact = value; }
        }

        // Extra functions not used in PHP class

        /// <summary>
        /// Get/set variable
        /// </summary>
        /// <returns></returns>
        public bool use_interact
        {
            get { return this._use_interact; }
            set { this._use_interact = value; }
        }

        /// <summary>
        /// Get/set variable
        /// </summary>
        /// <returns></returns>
        public HttpRequest httprequest
        {
            get { return this._httprequest; }
            set { this._httprequest = value; }

        }

        /// <summary>
        /// Get/set variable
        /// </summary>
        /// <returns></returns>
        public HttpResponse httpresponse
        {
            get { return this._httpresponse; }
            set { this._httpresponse = value; }

        }

        /// <summary>
        /// Get/set variable
        /// </summary>
        /// <returns></returns>
        public string log_file
        {
            get { return this._log_file; }
            set { this._log_file = value; }

        }

        /// <summary>
        /// Get/set variable
        /// </summary>
        /// <returns></returns>
        public string aauth
        {
            get { return this._aauth; }
            set { this._aauth = value; }

        }

        // read-only functions to access the authentication state

        /// <summary>
        /// Read-only authentication state
        /// </summary>
        /// <returns></returns>
        public string status()
        {
            try { return this._authentication_response[this.AUTHENTICATION_RESPONSE_STATUS]; }
            catch (NullReferenceException e) { return null; }
        }

        /// <summary>
        /// Read-only authentication state
        /// </summary>
        /// <returns></returns>
        public bool success()
        {
            try { return this._authentication_response[this.AUTHENTICATION_RESPONSE_STATUS].Equals("200", StringComparison.Ordinal); }
            catch (NullReferenceException e) { return false; }
        }

        /// <summary>
        /// Read-only authentication state
        /// </summary>
        /// <returns></returns>
        public string msg()
        {
            try { return this._authentication_response[this.AUTHENTICATION_RESPONSE_MSG]; }
            catch (NullReferenceException e) { return null; }
        }

        /// <summary>
        /// Read-only authentication state
        /// </summary>
        /// <returns></returns>
        public string issue()
        {
            try { return this._authentication_response[this.AUTHENTICATION_RESPONSE_ISSUE]; }
            catch (NullReferenceException e) { return null; }
        }

        /// <summary>
        /// Read-only authentication state
        /// </summary>
        /// <returns></returns>
        public string expire()
        {
            try { return this._authentication_response[this.AUTHENTICATION_RESPONSE_EXPIRE]; }
            catch (NullReferenceException e) { return null; }
        }

        /// <summary>
        /// Read-only authentication state
        /// </summary>
        /// <returns></returns>
        public string id()
        {
            try { return this._authentication_response[this.AUTHENTICATION_RESPONSE_ID]; }
            catch (NullReferenceException e) { return null; }
        }

        /// <summary>
        /// Read-only authentication state
        /// </summary>
        /// <returns></returns>
        public string principal()
        {
            try { return this._authentication_response[this.AUTHENTICATION_RESPONSE_PRINCIPAL]; }
            catch (NullReferenceException e) { return null; }
        }

        /// <summary>
        /// Read-only authentication state
        /// </summary>
        /// <returns></returns>
        public string auth()
        {
            try { return this._authentication_response[this.AUTHENTICATION_RESPONSE_AUTH]; }
            catch (NullReferenceException e) { return null; }
        }

        /// <summary>
        /// Read-only authentication state
        /// </summary>
        /// <returns></returns>
        public string sso()
        {
            try { return this._authentication_response[this.AUTHENTICATION_RESPONSE_SSO]; }
            catch (NullReferenceException e) { return null; }
        }

        /// <summary>
        /// Read-only authentication state
        /// </summary>
        /// <returns></returns>
        public string webauth_params()
        {
            // Original PHP function was 'params()' but this created problems within C# 
            // so changed to 'webauth_params()'

            try { return this._authentication_response[this.AUTHENTICATION_RESPONSE_PARAMS]; }
            catch (NullReferenceException e) { return null; }
        }

        /// <summary>
        /// Read-only authentication state
        /// </summary>
        /// <returns></returns>
        public string ptags()
        {
            try { return this._authentication_response[this.AUTHENTICATION_RESPONSE_PTAGS]; }
            catch (NullReferenceException e) { return null; }
        }

        /// <summary>
        /// Get the url to redirect the browser to after a successful authentication attempt.
        /// 
        /// This is typically identical to the url that first calls this script. 
        /// But due to the possibility of faking 'HOST' variables in a server response, 
        /// the 'hostname' must be supplied as a setup / default parameter in the code. 
        ///
        /// In versions 2 and 3, we must include the query component (GET parameters) of the 
        /// original calling url if they exist.
        /// </summary>
        /// <returns></returns>
        public string url()
        {
            // Strip out port number from hostname
                
            string basichostname = Regex.Replace(this.hostname, ":[0-9]+$", "");

            string port = this.httprequest.ServerVariables["SERVER_PORT"];
            string protocol = "http://";

            if (this.using_https())
            {
                protocol = "https://";
                if (port == "443") port = "";
            }
            else
            {
                if (port == "80") port = "";
            }

            string url = protocol + basichostname;
            if (port != "") url += ':' + port;

            url += this.httprequest.ServerVariables["REQUEST_URI"];

            return url;
        }

        /// <summary>
        /// Get name of cookie, typically 'Ucam-WebAuth-Session'
        /// If 'https', then distinguish the name of the cookie by suffixing '-S'
        /// </summary>
        /// <returns></returns>
        public string full_cookie_name()
        {
            if (this.using_https())
            {
                return this.cookie_name + "-S";
            }

            return this.cookie_name;
        }

        /// <summary>
        /// Determines whether we're using https or not
        /// </summary>
        /// <returns></returns>
        public bool using_https()
        {
            if (this.httprequest != null)
            {
                try
                {
                    string servertype = this.httprequest.ServerVariables["HTTPS"];
                    if (servertype.Equals("on", StringComparison.OrdinalIgnoreCase)) { return true; }
                }
                catch { }
            }

            return false;
        }

        /// <summary>
        /// Convert a Unix timestamp into the format required by Raven. 
        /// Format based on RFC 3339, but see Raven documentation for 
        /// a full description.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public string time2iso(long t)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            origin = origin.AddSeconds(t);

            return origin.ToString("yyyyMMddTHHmmssZ");
        }

        /// <summary>
        /// Convert a time in Raven format into a Unix timestamp 
        /// ie. the number of seconds since epoch.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public long iso2time(string t)
        {
            try
            {
                int year = Convert.ToInt32(t.Substring(0, 4));
                int month = Convert.ToInt32(t.Substring(4, 2));
                int day = Convert.ToInt32(t.Substring(6, 2));
                int hour = Convert.ToInt32(t.Substring(9, 2));
                int minute = Convert.ToInt32(t.Substring(11, 2));
                int second = Convert.ToInt32(t.Substring(13, 2));

                DateTime date = new DateTime(year, month, day, hour, minute, second);
                DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                TimeSpan diff = date - origin;

                return (int)diff.TotalSeconds;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns content of a certificate key as a string
        /// </summary>
        /// <param name="key_id"></param>
        /// <returns></returns>
        public string load_key(string key_id)
        {
            string key_filename = this.key_dir + "/" + key_id + ".crt";

            if (System.IO.File.Exists(key_filename)) { return System.IO.File.ReadAllText(key_filename); }

            return null;
        }

        /// <summary>
        /// Checks that the 'sig'(nature) provided by the WLS when it signed 'data' 
        /// is a valid signature for the data. This ensures the data has not been 
        /// tampered with.
        /// </summary>
        /// <param name="data">data that has been signed</param>
        /// <param name="sig">signature provided by WLS</param>
        /// <param name="key_id">id of key file from which the certificate file can be located</param>
        /// <returns></returns>
        public bool check_sig(string data, string sig, string key_id)
        {
            string key_str = this.load_key(key_id);

            // Load the certificate into an X509Certificate object.

            OpenSSL.X509.X509Certificate key_cert = new OpenSSL.X509.X509Certificate(new BIO(key_str));

            // Hash method

            MessageDigest md = MessageDigest.SHA1;

            // Prepare text to verify
            BIO b = new BIO(data);

            // Verify:
            // It throws an exception if it can't be verified.

            bool isVerified = false;

            try
            {
                isVerified = MessageDigestContext.Verify(md, b, this.wls_decode(sig), key_cert.PublicKey);
            }
            catch { }

            return isVerified;
        }

        /// <summary>
        /// Encode a byte array of data in a way that can be 
        /// easily sent to the WLS
        /// </summary>
        /// <param name="plainTextBytes">data as array of bytes</param>
        /// <returns></returns>
        public string wls_encode(byte[] plainTextBytes)
        {
            string result = System.Convert.ToBase64String(plainTextBytes);

            result = result.Replace("+", "-");
            result = result.Replace("/", ".");
            result = result.Replace("=", "_");

            return result;
        }

        /// <summary>
        /// Decode a string of data received from the WLS 
        /// into a byte array of data
        /// </summary>
        /// <param name="str">data to decode as a string</param>
        /// <returns></returns>
        public byte[] wls_decode(string str)
        {
            string result = str;

            result = result.Replace("-", "+");
            result = result.Replace(".", "/");
            result = result.Replace("_", "=");

            return System.Convert.FromBase64String(result);
        }

        /// <summary>
        /// Create HMACSHA1 hash value for 'data' using public 'key'.
        /// </summary>
        /// <param name="key">raw content of a certificate file as string</param>
        /// <param name="data">data to create hash value from as string</param>
        /// <returns></returns>
        public string hmac_sha1(string key, string data)
        {
            byte[] tmpSource = ASCIIEncoding.ASCII.GetBytes(data);
            byte[] keyByte = ASCIIEncoding.ASCII.GetBytes(key);

            HMACSHA1 hmacsha1 = new HMACSHA1(keyByte);

            //Compute hash based on source data
            byte[] hashmessage = hmacsha1.ComputeHash(tmpSource);

            string sbinary = "";

            for (int i = 0; i < hashmessage.Length; i++)
            {
                sbinary += hashmessage[i].ToString("x2"); // hex format
            }

            return this.wls_encode(ASCIIEncoding.ASCII.GetBytes(sbinary));
        }

        /// <summary>
        /// Verify that 'data' has been signed by public 'key'
        /// 
        /// Compute HMACSHAI hash value for 'data' using public 'key'
        /// then compare the value to 'sig'(nature).
        /// 
        /// </summary>
        /// <param name="key">Full text for public key</param>
        /// <param name="data">Data to be verified</param>
        /// <param name="sig">Signature of signed data (ie. hash value generated at WLS)</param>
        /// <returns></returns>
        public bool hmac_sha1_verify(string key, string data, string sig)
        {
            return (sig == this.hmac_sha1(key, data));
        }

        /// <summary>
        /// Write a message to the log file with a timestamp prefix on every line
        /// 
        /// Note relative paths, eg. 'logfile.txt', will be relative 
        /// to the IIS server directories where permissions to create
        /// new files may not be available. So provide an absolute
        /// path in the initial arguments to Ucam_Webauth ("log_file" => Path)      
        /// </summary>
        /// <param name="message"></param>
        /// <param name="message_type"></param>
        public void write_log(string message, int message_type = 0)
        {
            try
            {
                StreamWriter sw = new StreamWriter(this.log_file, true);

                string date_prefix = DateTime.UtcNow.ToString("[yyyy-MM-dd HH:mm:ss] ");

                sw.WriteLine(date_prefix + message);
                sw.Flush();
                sw.Close();
            }
            catch (IOException ex)
            {
                // Major problem if unable to write to 'log_file'
                // raise a breakpoint

                if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
            }
        }

        /// <summary>
        /// Sets cookie.
        /// </summary>
        /// <param name="name">Name of cookie.</param>
        /// <param name="value">Value of cookie.</param>
        /// <param name="expire">Expiry of cookie. Set to DateTime.MinValue to make it a session cookie</param>
        /// <param name="path">Path of cookie.</param>
        /// <param name="domain">Domain</param>
        /// <param name="secure">Only allow on secure domains.</param>
        /// <param name="httponly">Only allow on http non-secure domains.</param>
        public void setcookie(string name, string value, int expire, string path, string domain, bool secure = false, bool httponly = false)
        {
            HttpCookie cookie = new HttpCookie(name);

            DateTime expire_datetime;

            if (expire == 0)
            {
                expire_datetime = DateTime.MinValue;
            }
            else
            {
                expire_datetime = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
                expire_datetime.AddSeconds(expire);
            }

            cookie.Value = value;
            cookie.Expires = expire_datetime;
            cookie.Path = path;
            cookie.Domain = domain;
            cookie.Secure = secure;
            cookie.HttpOnly = httponly;

            this.httpresponse.Cookies.Add(cookie);
        }

        /// <summary>
        /// To partially logout the Raven session we delete the local Raven cookie
        ///
        /// This will not completely log you out of Raven as the Raven WLS 
        /// also stores a session cookie that you cannot delete remotely.
        /// So accessing Raven after logout may elicit a 
        /// "You are already logged in" from the Raven WLS 
        ///
        /// As both local and WLS cookies are session cookies, the safest 
        /// thing to do is to quit the browser to remove both session cookies. 
        /// Note that some browsers with 'Restore session' functionality, 
        /// eg. Firefox, may not remove session cookies properly.
        /// </summary>
        /// <returns>'true' whatever happens.</returns>
        public bool logout()
        {
            HttpCookie cookie = new HttpCookie(this.full_cookie_name());
            cookie.Expires = DateTime.Now.AddDays(-1d);
            this.httpresponse.Cookies.Add(cookie);

            return true;
        }

        /// <summary>
        /// Authenticate using WLS.
        /// 
        /// This function consists of three phases ordered in the code as:
        /// 1. General/Subsequent Processing.
        /// 2. Authentication Response.
        /// 3. Authentication Request.
        /// 
        /// The process of authenticating the user 
        /// happens as follows:
        /// 
        /// (a) If no authentication cookie is found during the 
        /// 'General/Subsequent Processing' phase and there is 
        /// no 'GET' variable with the name 'WLS-Response' 
        /// found during the 'Authentication Response' phase, 
        /// then it's assumed the user is neither authenticated nor
        /// is in the middle of a WAA<->WLS callback. So the 
        /// user's browser is redirected to the WLS during the
        /// 'Authentication Request' phase. In other words, 
        /// they're sent off from the original website to the 
        /// Raven authentication website.
        /// 
        /// (b) The user attempts to authenticate themselves
        /// on the remote WLS site. If authentication is successful,
        /// the user's browser is redirected with signed data in the 
        /// 'GET' field 'WLS-Response'.
        /// 
        /// (c) The script processes this data during the 
        /// 'Authentication Response' phase. If the data has 
        /// been correctly signed and has not expired (plus some
        /// other criteria), then an authentication cookie is set 
        /// and the user is redirected.
        /// 
        /// (d) The authentication cookie will then be detected 
        /// during the 'General/Subsequent Processing' phase and 
        /// will return 'true', ie. authenticated, back to the user.
        /// 
        /// </summary>
        /// <param name="authassertionid">Not sure why this is a parameter.</param>
        /// <param name="testauthonly">If set to 'true' then the WAA only checks for the existence of a valid cookie and doesn't set any cookies or do any redirection.</param>
        /// <returns></returns>
        public bool authenticate(string authassertionid = null, bool testauthonly = false)
        {
            string current_timeout_message = null;
            string http_host = this.httprequest.ServerVariables["HTTP_HOST"];
            string query_string = this.httprequest.ServerVariables["QUERY_STRING"];
            string request_method = this.httprequest.ServerVariables["REQUEST_METHOD"];

            // ****************************************
            // Do some preamble checks first
            // ****************************************

            // consistency check

            if ((testauthonly) && (!this.do_session))
            {
                this._authentication_response[this.AUTHENTICATION_RESPONSE_STATUS] = "600";
                this._authentication_response[this.AUTHENTICATION_RESPONSE_MSG] = "Requested dummy run but session cookie not managed";
                return true;
            }

            // check that someone defined cookie key and if we are doing session management

            if ((this.do_session) && (this.cookie_key == null))
            {
                this._authentication_response[this.AUTHENTICATION_RESPONSE_STATUS] = "600";
                this._authentication_response[this.AUTHENTICATION_RESPONSE_MSG] = "No key defined for session cookie";
                return true;
            }

            // log a warning if being used to authenticate POST requests

            if (request_method == "POST") write_log("Ucam_Webauth PHP Agent invoked for POST request, which it doesn't really support");           

            // Check that the hostname is set explicitly (since we cannot trust
            // the Host: header); if it returns false (i.e. not set).

            if ((this.hostname == null) | (this.hostname == ""))
            {
                this._authentication_response[this.AUTHENTICATION_RESPONSE_MSG] = "Ucam_Webauth configuration error - mandatory hostname not defined";
                this._authentication_response[this.AUTHENTICATION_RESPONSE_STATUS] = "600";
                write_log("hostname not set in Ucam_Webauth object, but is mandatory");
                return true;
            }

            // ****************************************
            // General/Subsequent Processing
            //
            // If the user has been authorized and the appropriate 
            // cookie has been set (assuming we're doing session 
            // management), then nothing else needs to happen.
            //
            // Note that if the stored status isn't 200 (OK) 
            // then we destroy the cookie so that if
            // we come back through here again we will fall through and repeat
            // the authentication. We do this first so that replaying an old
            // authentication response won't trigger a 'stale authentication' error
            //
            // It's only during this 'General/Subsequent Processing' 
            // phase that a possible successful authentication 
            // reaches its conclusion and the user is presented 
            // with an authenticated resource.
            //
            // ****************************************


            write_log("General Processing: Starting...");

            if (this.do_session)
            {
                write_log("General Processing: Session management=ON");

                if ((this.httprequest.Cookies[this.full_cookie_name()] != null) &&
                    (this.httprequest.Cookies[this.full_cookie_name()].Value != this.TESTSTRING) &&
                    (this.httprequest.Cookies[this.full_cookie_name()].Value != this.WLS_LOGOUT))
                {
                    write_log("General Processing: Found authentication cookie=" + HttpUtility.UrlDecode(this.httprequest.Cookies[this.full_cookie_name()].Value));

                    this._authentication_response = HttpUtility.UrlDecode(this.httprequest.Cookies[this.full_cookie_name()].Value).Split("!".ToCharArray());

                    // Check authentication cookie has been correctly signed

                    string[] values_for_verify = new string[this._authentication_response.Length];

                    Array.Copy(this._authentication_response, values_for_verify, this._authentication_response.Length);
                    string sig = values_for_verify[values_for_verify.Length - 1];
                    Array.Resize(ref values_for_verify, values_for_verify.Length - 1);

                    if (!this.hmac_sha1_verify(this.cookie_key, String.Join("!", values_for_verify), sig))
                    {
                        write_log("General Processing: AUTHENTICATION FAILED, session cookie sig invalid");
                        this._authentication_response[this.AUTHENTICATION_RESPONSE_MSG] = "Session cookie signature invalid";
                        this._authentication_response[this.AUTHENTICATION_RESPONSE_STATUS] = "600";
                        return false;
                    }

                    write_log("General Processing: Existing authentication cookie verified");

                    // Check authentication cookie hasn't expired

                    long issue = this.iso2time(this._authentication_response[this.AUTHENTICATION_RESPONSE_ISSUE]);
                    long expire = this.iso2time(this._authentication_response[this.AUTHENTICATION_RESPONSE_EXPIRE]);
                    long now = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds; // Get current time as Unix timestamp

                    if ((issue <= now) && (now < expire))
                    {
                        if ((authassertionid == null) | (authassertionid != this._authentication_response[this.AUTHENTICATION_RESPONSE_ID]))
                        {
                            if (this._authentication_response[this.AUTHENTICATION_RESPONSE_STATUS] != "200")
                            {
                                if (!testauthonly) this.setcookie(this.full_cookie_name(),
                                                                    "",
                                                                    1,
                                                                    this.cookie_path,
                                                                    this.cookie_domain,
                                                                    this.using_https());

                            }
                            write_log("General Processing: AUTHENTICATION COMPLETE");
                            return true;
                        }
                        else
                        {
                            current_timeout_message = this.forced_reauth_message;
                            write_log("General Processing: Detection of 'stale' authassertionid requested and found: " + authassertionid);
                            write_log("General Processing: Authentication using current session ticket denied");
                        }
                    }
                    else
                    {
                        current_timeout_message = this.timeout_message;
                        write_log("General Processing: Local session cookie expired");
                        write_log("General Processing: Issue/now/expire: " + issue.ToString() + "/" + now.ToString() + "/" + expire.ToString());
                    }
                }
            }

            // ****************************************
            // Authentication Response
            //
            // Having called 'Authentication Request' previously, 
            // the user will be prompted to enter their 
            // login details through the WLS. Once that process 
            // is complete, the WLS will then issue a callback
            // through the browser to this current script. We now process
            // this callback during the 'Authentication Response' 
            // phase. 
            //
            // We validate the response to see what happened - 
            // whether the user successfully logged in or whether 
            // there were other problems. If login was successful 
            // and we are not doing session management, we can just return. 
            // If we are doing session management, we check that the 
            // session cookie already exists with a test value (because 
            // otherwise we probably don"t have cookies
            // enabled), set it, and redirect back to the original 
            // URL to clear the browser's location bar of the WLS response.
            //
            // 'Authentication Response' is typically the SECOND 
            // step in the process of authorization.
            //
            // ****************************************


            write_log("Authentication Response: Starting...");

            bool wls_response_value_exists = false;
            string wls_response_value = "";

            if (query_string != null)
            {
                // We're processing urls with possible name-value pairs
                // in addition to WLS-Response=... so we need to extract all 
                // name-value pairs and check specifically for WLS-Response. 

                string[] namevaluepairs = query_string.Split('&');

                for(int i = 0; i < namevaluepairs.Length; i++)
                {
                    string[] pair = namevaluepairs[i].Split('=');

                    if (pair.Length == 2)
                    {
                        if (pair[0] == "WLS-Response")
                        {
                            wls_response_value = HttpUtility.UrlDecode(pair[1]);
                            wls_response_value_exists = true;
                        }
                    }
                }
            }

            if (wls_response_value_exists)
            {
                write_log("Authentication Response: WLS response=" + wls_response_value);

                string[] wls_response = wls_response_value.Split("!".ToCharArray());

                this._authentication_response[this.AUTHENTICATION_RESPONSE_STATUS] = "200";
                this._authentication_response[this.AUTHENTICATION_RESPONSE_MSG] = "";

                string sig = wls_response[wls_response.Length - 1];
                string key_id = wls_response[wls_response.Length - 2];
                Array.Resize(ref wls_response, wls_response.Length - 2);
                
                if (wls_response[this.WLS_RESPONSE_STATUS] == "410")
                {
                    this._authentication_response[this.AUTHENTICATION_RESPONSE_MSG] = this.status_codes["410"];
                    this._authentication_response[this.AUTHENTICATION_RESPONSE_STATUS] = "410";
                }
                else if (wls_response[this.WLS_RESPONSE_VER] != this.PROTOCOL_VERSION)
                {
                    this._authentication_response[this.AUTHENTICATION_RESPONSE_MSG] = "Wrong protocol version in authentication service reply";
                    this._authentication_response[this.AUTHENTICATION_RESPONSE_STATUS] = "600";
                }
                else if (wls_response[this.WLS_RESPONSE_STATUS] != "200")
                {
                    this._authentication_response[this.AUTHENTICATION_RESPONSE_MSG] = this.status_codes[wls_response[this.WLS_RESPONSE_STATUS]];
                    if (wls_response[this.WLS_RESPONSE_MSG] != null) this._authentication_response[this.AUTHENTICATION_RESPONSE_MSG] += wls_response[this.WLS_RESPONSE_MSG];
                    this._authentication_response[this.AUTHENTICATION_RESPONSE_STATUS] = wls_response[this.WLS_RESPONSE_STATUS];
                }
                else if (this.check_sig(String.Join("!", wls_response), sig, key_id) != true)
                {
                    // Signature is not correct for the wls_response

                    this._authentication_response[this.AUTHENTICATION_RESPONSE_MSG] = "Invalid WLS wls_response signature";
                    this._authentication_response[this.AUTHENTICATION_RESPONSE_STATUS] = "600";
                    return true;
                }
                else
                {
                    long now = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                    long issue = this.iso2time(wls_response[this.WLS_RESPONSE_ISSUE]);

                    if (issue == null)
                    {
                        this._authentication_response[this.AUTHENTICATION_RESPONSE_MSG] = "Unable to read issue time in authentication service reply";
                        this._authentication_response[this.AUTHENTICATION_RESPONSE_STATUS] = "600";
                    }
                    else if (issue > (now + this.clock_skew + 1))
                    {
                        this._authentication_response[this.AUTHENTICATION_RESPONSE_MSG] = "Authentication service reply aparently issued in the future: " + wls_response[this.WLS_RESPONSE_ISSUE];
                        this._authentication_response[this.AUTHENTICATION_RESPONSE_STATUS] = "600";
                    }
                    else if ((now - this.clock_skew - 1) > (issue + this.response_timeout))
                    {
                        this._authentication_response[this.AUTHENTICATION_RESPONSE_MSG] = "Stale authentication service reply issue at " + wls_response[this.WLS_RESPONSE_ISSUE];
                        this._authentication_response[this.AUTHENTICATION_RESPONSE_STATUS] = "600";
                    }

                    string response_url = wls_response[this.WLS_RESPONSE_URL];
                    response_url = Regex.Replace(response_url, @"\?.*$", "");
                    string this_url = Regex.Replace(this.url(), @"\?.+$", "");

                    if (this_url != response_url)
                    {
                        this._authentication_response[this.AUTHENTICATION_RESPONSE_MSG] = "URL in response ticket doesn't match this URL: " + response_url + " != " + this_url;
                        this._authentication_response[this.AUTHENTICATION_RESPONSE_STATUS] = "600";
                    }
                }

                // work out session expiry time

                int expiry = this.max_session_life;

                if ((wls_response[this.WLS_RESPONSE_LIFE] != null) && (wls_response[this.WLS_RESPONSE_LIFE] != ""))
                {
                    int wls_token_life = Convert.ToInt32(wls_response[this.WLS_RESPONSE_LIFE]);
                    if ((wls_token_life > 0) && (wls_token_life < expiry)) expiry = wls_token_life;
                }

                // Expand 'wls_response' array to maximum size just in case it's too small 
                // and subsequent calls to wls_response[this.WLS_RESPONSE_XXX] raise 'IndexOutOfRangeException'

                Array.Resize(ref wls_response, this.WLS_RESPONSE_SIZE);

                // populate authentication response with information collected so far

                this._authentication_response[this.AUTHENTICATION_RESPONSE_ISSUE] = this.time2iso((long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString();
                this._authentication_response[this.AUTHENTICATION_RESPONSE_EXPIRE] = this.time2iso((long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds + expiry).ToString();
                this._authentication_response[this.AUTHENTICATION_RESPONSE_ID] = wls_response[this.WLS_RESPONSE_ID];
                this._authentication_response[this.AUTHENTICATION_RESPONSE_PRINCIPAL] = wls_response[this.WLS_RESPONSE_PRINCIPAL];
                this._authentication_response[this.AUTHENTICATION_RESPONSE_AUTH] = wls_response[this.WLS_RESPONSE_AUTH];
                this._authentication_response[this.AUTHENTICATION_RESPONSE_SSO] = wls_response[this.WLS_RESPONSE_SSO];
                this._authentication_response[this.AUTHENTICATION_RESPONSE_PARAMS] = wls_response[this.WLS_RESPONSE_PARAMS];
                this._authentication_response[this.AUTHENTICATION_RESPONSE_PTAGS] = wls_response[this.WLS_RESPONSE_PTAGS];

                // return complete if we are not doing session management

                if (!this.do_session) return true;

                // otherwise establish a session cookie to maintain the
                // session ticket. First check that the cookie actually exists
                // with a test value, because it should have been created
                // previously and if its not there we'll probably end up in
                // a redirect loop.

                if ((this.httprequest.Cookies[this.full_cookie_name()] == null) |
                    ((this.httprequest.Cookies[this.full_cookie_name()] != null) &&
                        (this.httprequest.Cookies[this.full_cookie_name()].Value != this.TESTSTRING)))
                {
                    this._authentication_response[this.AUTHENTICATION_RESPONSE_STATUS] = "610";
                    this._authentication_response[this.AUTHENTICATION_RESPONSE_MSG] = "Browser is not accepting session cookie";
                    return true;
                }

                this._authentication_response[this.AUTHENTICATION_RESPONSE_VER] = this.AUTHENTICATION_RESPONSE_VERSION;

                // This used to use ksort and implode, but appeared to produce
                // broken results from time to time. This does the work the hard
                // way.

                string cookie = "";
                for (int i = 0; i < this.AUTHENTICATION_RESPONSE_PARAMS; i++)
                {
                    if (this._authentication_response[i] != null) { cookie += this._authentication_response[i]; }
                    cookie += "!";
                }

                if (this._authentication_response[this.AUTHENTICATION_RESPONSE_PARAMS] != null)
                {
                    cookie += this._authentication_response[this.AUTHENTICATION_RESPONSE_PARAMS];
                }

                // Add signature to cookie as final parameter

                sig = this.hmac_sha1(this.cookie_key, cookie);
                cookie += "!" + sig;
                write_log("Authentication Response: Setting cookie=" + cookie);

                // End

                if (!testauthonly) this.setcookie(this.full_cookie_name(),
                                                    cookie,
                                                    0,
                                                    this.cookie_path,
                                                    this.cookie_domain,
                                                    this.using_https());

                write_log("Authentication Response: Session cookie established, redirecting...");

                // Clean up the URL in browser location bar, i.e., remove WLS stuff
                // in query string, and, inevitably, redo original request a second time?
                if (!testauthonly) this.httpresponse.Redirect(wls_response[this.WLS_RESPONSE_URL]);

                return false;
            }


            // ****************************************
            // Authentication Request
            //
            // Send an authentication request from the WAA to the WLS. 
            // If we are doing session management then set a test value cookie 
            // so we can test that it's still available when we get back.
            //
            // For an unauthorised user with a fresh browser, this is 
            // the FIRST step in the process of authorization.
            //
            // ****************************************

            write_log("Authentication Request: STEP 1...");

            if (this.do_session)
            {
                // If the hostname from the request (Host: header) does not match the
                // server's preferred name for itself (which should be what's configured
                // as hostname), cookies are likely to break "randomly" (or more
                // accurately, the cookie may not be sent by the browser since it"s for
                // a different hostname) as a result of following links that use the
                // preferred name, or server-level redirects e.g. to fix "directory"
                // URLs lacking the trailing "/". Attempt to avoid that by redirecting
                // to an equivalent URL using the configured hostname.

                if ((http_host != null) && (this.hostname.ToLower()) != http_host.ToLower())
                {
                    write_log("Authentication Request: Redirect to tidy up hostname mismatch");
                    if (!testauthonly) this.httpresponse.Redirect(this.url());
                    return false;
                }

                write_log("Authentication Request: Setting pre-session cookie");

                if (!testauthonly) this.setcookie(  this.full_cookie_name(),
                                                    this.TESTSTRING,
                                                    0,
                                                    this.cookie_path,
                                                    this.cookie_domain,
                                                    this.using_https());
            }


            // Now build the full 'Authentication Request' URL that will 
            // redirect the user to the WLS. Full information about each 
            // of these parameters can be found in Raven v3 documentation.

            string                                  dest = this.auth_service;
                                                    dest += "?ver=" + this.PROTOCOL_VERSION;
                                                    dest += "&url=" + HttpUtility.UrlEncode(this.url());
            if (this.description != null)           dest += "&desc=" + HttpUtility.UrlEncode(this.description);
            if (this.aauth != null)                 dest += "&aauth=" + HttpUtility.UrlEncode(this.aauth); 
            if (this.use_interact)                  dest += "&iact=" + (this.interact == true ? "yes" : "no");
            if (current_timeout_message != null)    dest += "&msg=" + HttpUtility.UrlEncode(current_timeout_message);
            if (this.webauth_params() != null)      dest += "&params" + HttpUtility.UrlEncode(this.webauth_params());
                                                    dest += "&date=" + HttpUtility.UrlEncode(this.time2iso((long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds));
            // if (this.clock_skew != 0)            dest += "&skew=" + HttpUtility.UrlEncode(Convert.ToString(this.clock_skew)); // 'skew' parameter deprecated in v3
            if (this.fail == true)                  dest += "&fail=yes";

            write_log("Authentication Request: Redirecting to WLS with URL=" + dest);

            if (!testauthonly) this.httpresponse.Redirect(dest);

            return false;
        }
    }
}
