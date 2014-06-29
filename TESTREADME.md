University of Cambridge Card Reader (Android) - v.1.0 (29/06/2014)
==================================================================

Description
-----------
The Android card reader application uses Near Field Communication (NFC) to 
read a user-defined field, eg. CRSID, Barcode, USN, from a Cambridge 
University Mifare 4K card. It then sends the retrieved data as a sequence of 
keystrokes to the local keyboard buffer / external PC / tablet, depending on 
the application's 'Destination' setting. The application provides a card 
reader 'keyboard wedge', making it possible to create separate card 
applications quickly and easily without having to worry about card reading 
specifics. For an example of a separate application, see 'Card Logger' at:
http://card.careers.cam.ac.uk/apps

Keystrokes are sent to the local device buffer using the Android Input Method 
Editor (IME). Sending keys to an external PC / tablet is via the InputStick 
Bluetooth-to-USB dongle.
 
- http://developer.android.com/guide/topics/text/creating-input-method.html
- http://www.inputstick.com

In the event of missing card information, the application uses a web service to 
retrieve card information from a central database using the card's unique ID.

The software has been specifically designed to read University of Cambridge 
Mifare 4K cards but can be easily modified to read other types of card. 
For more information about the specification of University of Cambridge NFC 
cards, contact the University Card Office:
http://www.misd.admin.cam.ac.uk/services-and-support/university-card

This software was originally created for the Careers Service, University 
of Cambridge by sh801@cam.ac.uk

Important files and folders
---------------------------
- [bin]: Contains compiled versions of the code including 'UcamCardreader.apk', 
the final Android application.
- [res]: Contains resources for the application, eg. icons (within 
'drawable-XXXX'), text strings ('values\strings.xml'), menus (within 'menu'), 
dialogs, preferences ('xml\preferences.xml') and layouts (within 'layout').
- [src\com]: Source files for the 'SoftKeyboard' Input Method Editor (IME) 
and InputStick objects that process keystrokes sent from the main card reader 
application. Apart from modifications to 
'com.example.android.softkeyboard.SoftKeyboard.java', all source files for 
'src\com' are taken directly from the Android 'SoftKeyboard' SDK sample and 
the InputStick SDK (http://www.inputstick.com/index.php/developers/download)
- [src\uk]: Source files for the main card reader application (namespace 
'uk.ac.cam.cardreader'). A description of the function of each file is 
provided below under "Specifics of the application".
- AndroidManifest.xml: Specifies the services and activities of the 
application and how different application intents (system messages) should be 
processed by the application. 

Platform requirements
---------------------
This application has been developed and tested on the Sony Xperia M 
NFC-compliant Android phone running Android 4.2/4.3. It should run on all 
NFC-compliant Android 4.2+ phones providing they can read Mifare 4K cards. 

- To check an NFC phone is Mifare-4K-compliant, use the 'Mifare Classic Tool': 
https://play.google.com/store/apps/details?id=de.syss.MifareClassicTool&hl=en_GB 

Installation and getting started
--------------------------------
Copy the compiled 'UcamCardreader.apk' application to your device via email / 
web / USB / memorystick and follow the installation instructions provided at:
http://card.careers.cam.ac.uk/apps

Application settings
--------------------
The application has a number of customisable settings, accessible by tapping 
on the three dots, top right of the screen, and selecting "Settings":

### Card scanning \ Card information
Selects which piece of information to read off the card. The options are:

- CRSID (default)
- UL Barcode
- Cardholder ID
- USN (Unique Student Number)
- Staff number
- Mifare Unique Card ID

### Card processing \ Check expiry date
If this box is ticked, the application will check the expiry date of the card 
against the current date and will either output nothing or "EXPIRED" (if 
"Describe errors in output" is set) in the event the card has expired.

### Card processing \ Fallback URL
For entering the website address of a web service to be used in the event a 
card field is empty; the Mifare Unique Card ID is appended to the URL before 
it's submitted to the web service, eg. 
http://fallbackurl/retrieveCRSID/0123456789. This is useful when reading 
CRSIDs as cards are often sent out to users before they've been assigned a 
CRSID. 

### Output \ Destination
Specify where output keys should be sent after a card has been successfully 
scanned. The options are:

- Separate PC
- Local device

### Output \ Output ID if empty
If this box is ticked, the application will insert the Mifare Unique Card ID 
of the card prefixed with 'ID', eg. "ID0123456789", if no specific card 
information can be retrieved off the card.

### Output \ Describe errors in output
If the card has expired or no card information can be found (assuming "Output 
ID if empty" is off and the fallback URL returns nothing), then meaningful 
text will be output ("EXPIRED" or "EMPTY") if this box is ticked.

### Output \ Add delimiter
If this box is ticked, a sequence of text delimiters will be inserted after 
the card data and before any possible carriage return whenever text is output.

### Output \ Add carriage return
If this box is ticked, a carriage return will be added after the card data 
whenever any text is output to the local device / PC.

### Authentication \ Web service URL
For entering the URL of the authentication web service that authenticates the 
application; the application must be authenticated before it can be used. A 
central authentication server provides a way of distributing encrypted 
versions of the University of Cambridge 'card key', required to read 
University of Cambridge cards. For specific instructions on authenticating 
the app, go to: http://card.careers.cam.ac.uk/apps

Compiling and debugging application
-----------------------------------
1. On your Android device, go to "Settings -> Developer options". If you're 
using Android 4.2, developer options may be hidden by default. To enable them, 
go to "Settings -> About phone" and tap "Build number" seven times. Return to 
previous screen and "Developer options" should be visible.
2. In "Developer options", ensure "USB Debugging" is ON (has a tick by it).
3. Connect your Android device to your development machine with a USB cable. 
You may need to install OEM USB drivers from: 
http://developer.android.com/tools/extras/oem-usb.html 
Tap "OK" on any dialogs requesting permission.
4. Download and install "Android SDK" from: 
http://developer.android.com/sdk/index.html
5. Launch "eclipse" from within the "adt-bundle-XXX\eclipse" folder. This 
will launch "Android Developer Tools".
6. Go to "File -> Import -> General: Existing Projects Into Workspace", 
navigate to the main "UcamCardreader" folder and click "OK". 
7. Click on "UcamCardreader" in the "Package Explorer" window and go to 
"Run -> Debug As -> Android Application". You should see your Android device 
listed in the top of the dialog box. Select your device and click "OK" to 
install / debug the card reader application on your device. 
8. Check the application is working correctly by following the instructions at:
http://card.careers.cam.ac.uk/apps

Specifics of the application
----------------------------
The 'AndroidManifest.xml' configuration file activates different services / 
activities (coded as separate .java class files) according to different system 
messages. Some class files manage background tasks like card scanning while 
others directly handle user interface screens.

### Authentication.java
The 'Authentication' class manages the authentication screen for authenticating 
with the central server. The authentication screen works as follows: the user 
enters their username, password and passphrase and clicks 'Authenticate'. 
Assuming the username/password are valid, the central server returns the 
Cambridge University card key encrypted with the supplied passphrase.

### AuthenticationService.java
The 'AuthenticationService' class accesses the authentication web service to 
retrieve an encrypted card key, encrypted with the supplied passphrase. 
The web service should return the information in JSON format as:

```
{"status":"XXX","data":"value"}
```

where 'status' is the status of the query ('200' = Success) and 'data'/'value' 
is the name/value of the returned data field.

Uses JSON parser (c) Mini Sharma from: 
http://mrbool.com/how-to-use-json-to-parse-data-into-android-application/28944#ixzz33HP5lE46

### BootCompleted.java
The 'BootCompleted' class responds to 'android.intent.action.BOOT\_COMPLETE' 
intent. We respond to BOOT\_COMPLETE intent in order to start 'Core' service 
on startup. 'Core' handles all global variables and must be loaded first.

### Card.java
The 'Card' activity manages Mifare 4K card scanning and processing. A 
decrypted card key will typically be used to read the sectors off the card. If 
card information is missing, the class/activity can call a web service to look 
up card information based on the card's id. 

The activity is activated after 'android.nfc.action.TECH\_DISCOVERED' intent 
and typically results in keys being sent to a local or remote device during 
the 'processCardInfo' call. In the event of an asynchronous web service look 
up, however, 'processCardInfo' may be called on a separate thread after the 
async call has finished.

### ConnectivityChange.java
The 'ConnectivityChange' class responds to 
'android.net.conn.CONNECTIVITY\_CHANGE' intents. The appearance of a 
CONNECTIVITY\_CHANGE intent means the status of the wifi connection has 
changed, in which case we use the 'Internet' class to test whether the 
connection's a fully functioning internet connection.

### Conversion.java
The 'Conversion' class handles general conversion between base64, hex and byte 
arrays.

### Core.java
The 'Core' service class manages global variables and sets up objects that 
must persist through the lifetime of the application, eg. InputStick. The 
'Core' service is run on startup as a result of the 'BootCompleted' class 
receiving the 'android.intent.action.BOOT\_COMPLETED' intent. 

Global variables that 'Core' manages include 'passphrase', used to decrypt 
the University of Cambridge's unique card key; the encrypted card key is 
downloaded when the app is authenticated. 'Core' also handles the sending of 
keys to the output device - either the local keyboard buffer in the form of a 
custom input device (com.example.android.softkeyboard), or an external PC via 
the Bluetooth-to-USB InputStick dongle.

Other tasks the 'Core' service performs:

- Creating a login timer that resets logins after a period of time.
- Creating an ADB monitor that checks to see whether someone is trying 
to connect to the phone via USB debugging. If someone is trying to connect, 
the encrypted card key is deleted for security reasons.

### Decryption.java
The 'Decryption' class handles decryption of the encrypted card key. Based on 
"Interoperable AES encryption with Java and JavaScript" 
https://github.com/mpetersen/aes-example

### FallbackService.java
The 'FallbackService' class accesses a web service to retrieve a value based 
on the Mifare ID of the card. The web service should provide the information 
in JSON format as:

```
{"status":"XXX","data":"value"}
```

where 'status' is the status of the query (200' = Success) and 'data'/'value' 
is the name/value of the data field.

Uses JSON parser (c) Mini Sharma from: 
http://mrbool.com/how-to-use-json-to-parse-data-into-android-application/28944#ixzz33HP5lE46

### InputStick.java
The 'InputStick' class manages the connection to the Bluetooth-to-USB 
InputStick dongle (www.inputstick.com).

### Interface.java
The 'Interface' activity handles the application's main user interface, 
providing screen output that shows the current status of card processing. It 
also provides access to application settings and the authentication screen. 

### Internet.java
The 'Internet' class tests whether a fully-functioning internet connection 
is in place. It may be possible for an internet connection to only be 'partly' 
in place if there is a live connection to a Raven-authenticated access point 
but the authentication with Raven has not been completed. So we search the 
page that's retrieved (google.com) and look for relevant keywords to see 
whether that's the case.

### JSONParser.java
The 'JSONParser' class makes a JSON request and returns data as JSON object.

### Login.java
The 'Login' class manages the login prompt. After entering a passphrase 
through the login prompt, the system checks it is the correct passphrase for 
the encrypted card key. If it is, the application sets the global passphrase 
variable (via 'Core') accordingly. If it is not the correct passphrase, the 
application increments the login counter and prompts the user for the 
passphrase again.

### PrefFragment.java
The 'PrefFragment' class loads the PreferenceScreen-compliant XML application 
settings.

### Settings.java
The 'Settings' class handles access to application settings. Application 
settings are automatically synced to the preferences user interface through a 
PreferenceScreen-compliant XML preferences file. For example a list of 
choices is defined within 'preferences.xml' as 

```
<ListPreference
	android:key="setting_card_info"
	android:title="@string/title_card_info"
	android:summary="@string/summary_card_info"
	android:entries="@array/entries_card_info"
	android:entryValues="@array/entryvalues_card_info"
	android:dialogTitle="@string/title_card_info" />
```

This setting can then be retrieved from code using the 'android:key' value:

```
pref.getString("setting_card_info", "");      
```

### TextOutput.java
The 'TextOutput' class handles simple text output generated by app as it 
progresses. The text data for TextOutput is stored as a global variable in 
'Core'.
