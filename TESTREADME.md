University of Cambridge Android Card Reader - v.1.0 (29/06/2014)
================================================================

Description
-----------
The software comprises a series of Android Java and XML files that compile 
together to form an Android NFC card reader application. The application 
reads a user-defined field from the card, eg. CRSID, Barcode, USN, and 
sends the retrieved data as a sequence of keystrokes to the device's local 
keyboard buffer or to an external PC / tablet (depending on the 'Destination' 
setting of the application). By creating a card reader 'keyboard wedge', it's 
possible to create card applications quickly and easily without having to 
worry about specifics of card reading. 
- Sending to the local device buffer is achieved using an Android Input Method: 
http:developer.android.com/guide/topics/text/creating-input-method.html
- Sending to an external PC / tablet is achieved using the InputStick 
Bluetooth-to-USB dongle: http:www.inputstick.com

In the event of missing card information, the application uses a webservice to 
retrieve card information from a central database using the card's unique ID.

The software has been specifically designed to read University of Cambridge 
Mifare 4K NFC cards but can be easily modified to read other types of card. 
For more information about the specification of University of Cambridge NFC 
cards, contact the University Card Office:
http:www.misd.admin.cam.ac.uk/services-and-support/university-card

This software was originally created for the Careers Service, University 
of Cambridge by sh801@cam.ac.uk

Important files and folders
---------------------------
- [bin]: Contains compiled versions of the code including 'UcamCardreader.apk', 
the final Android application.
- [res]: Contains resources for the application, eg. icons (within 
'drawable-XXXX'), text strings ('values\strings.xml'), menus (within 'menu'), 
dialogs, preferences ('xml\preferences.xml') and layouts (within 'layout').
- [src\com]: Source files for the 'SoftKeyboard' IME and InputStick objects 
that process keystrokes sent from the main card reader application. Apart from 
modifications to 'com.example.android.softkeyboard.SoftKeyboard.java', all 
source files for 'src\com' are taken directly from the Android 'SoftKeyboard' 
SDK sample and the InputStick SDK 
(http:www.inputstick.com/index.php/developers/download)
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
https:play.google.com/store/apps/details?id=de.syss.MifareClassicTool&hl=en_GB 

Installation and getting started
--------------------------------
Copy the latest 'UcamCardreader.apk' application to your device and follow the 
instructions provided at:
http:card.careers.cam.ac.uk/apps

Application settings
--------------------
The application has a number of user-defined settings, accessible by tapping 
on the three dots, top right of the screen, and selecting "Settings":

## Card scanning

### Card information
Selects which piece of information to read off the card. The options are:
- CRSID (default)
- UL Barcode
- Cardholder ID
- USN (Unique Student Number)
- Staff number
- Mifare Unique Card ID

## Card processing

### Check expiry date
If this box is ticked, the application will check the expiry date of the card 
against the current date and will either output nothing or "EXPIRED" (if 
"Describe errors in output" is set) if the card has expired.

### Fallback URL
You can supply the website address of a web service to be used in the event a 
particular card field is empty; the Mifare Unique Card ID is appended to the 
URL before it's submitted to the webservice, eg. 
http:fallbackurl/retrieveCRSID/0123456789. This is particularly useful when 
reading CRSIDs as cards are often sent out to users before they've been 
assigned a CRSID. 

## Output

### Destination
Specify where output keys should be sent after a card has been scanned. The 
options are:
- Separate PC
- Local device

### Output ID if empty
If this box is ticked, the application will insert the Mifare Unique Card ID 
(prefixed with 'ID') if no card information can be found, eg. ID0123456789

### Describe errors in output
If the card has expired or no card information can be found (assuming "Output 
ID if empty" is off and the fallback URL returned nothing), then output 
meaningful text rather than doing nothing, eg. "EXPIRED" or "EMPTY".

### Add delimiter
Supply a sequence of text delimiters to be inserted after the card data and 
before any possible carriage return.

### Add carriage return
Add a carriage return after the card data whenever any text is output to the 
local device / PC.

## Authentication

### Web service URL
Supply the URL of the webservice for authenticating the application - the 
application must be authenticated before it can be used. The central 
authentication server provides a way to distribute encrypted versions of the 
crucial 'card key' that is required to read University of Cambridge cards. 
For specific instructions on authenticating the app, go to: 
http:card.careers.cam.ac.uk/apps


Compiling and debugging application
-----------------------------------
1. On your Android device, go to "Settings -> Developer options". 
- If you're using Android 4.2, developer options may be hidden by default. 
To enable them, go to "Settings -> About phone" and tap "Build number" seven 
times. Return to previous screen and "Developer options" should be visible.
2. In "Developer options", ensure "USB Debugging" is ON (has a tick by it).
3. Connect your Android device to your development machine with a USB cable. 
You may need to install OEM USB drivers from: 
http:developer.android.com/tools/extras/oem-usb.html 
Tap "OK" on any dialogs requesting permission.
4. Download and install "Android SDK" from: 
http:developer.android.com/sdk/index.html
5. Launch "eclipse" from within the "adt-bundle-XXX\eclipse" folder to start 
the "Android Developer Tools".
6. Go to "File -> Import -> General: Existing Projects Into Workspace" 
and navigate to the main "UcamCardreader" folder. 
7. Click on "UcamCardreader" and go to "Run -> Debug As -> Android 
Application". You should see your Android device listed in the top of the 
dialog box. Click "OK" to install the card reader application on your Android 
device and start debugging. 
8. To check the application is working correctly, follow the instructions at:
http:card.careers.cam.ac.uk/apps

Specifics of the application
----------------------------
The 'AndroidManifest.xml' activates different services / activities (coded as 
separate .java files) according to different system messages. Some files 
manage background tasks like card scanning while others directly handle user 
interface screens.

## Authentication.java
'Authentication' class manages authentication screen for authenticating with 
central server. The authentication screen works as follows: the user enters 
their username, password and passphrase and clicks 'Authenticate'. Assuming 
the username/password are valid, the central server returns the Cambridge 
University card key encrypted with the supplied passphrase.

## AuthenticationService.java
'AuthenticationService' class accesses webservice to retrieve an encrypted 
id card passphrase. The webservice should return the information in JSON 
format as:

{"status":"XXX","data":"_value_"}

where 'status' is the status of the query; '200' = Success and 'data' and 
_value_ are the name/value of the returned data field.

Uses JSON parser (c) Mini Sharma from: 
http:mrbool.com/how-to-use-json-to-parse-data-into-android-application/28944#ixzz33HP5lE46

## BootCompleted.java
'BootCompleted' class responds to 'android.intent.action.BOOT_COMPLETE' intent. 
We respond to BOOT_COMPLETE intent in order to start 'Core' service on startup. 
'Core' handles all global variables and must be loaded first.

## Card.java
'Card' activity manages Mifare 4K card scanning and processing. A decrypted 
card key will typically be used to read the sectors off the card. If card 
information is missing, the class/activity can call a webservice to look up 
card information based on the card's id. 

The activity is activated after 'android.nfc.action.TECH_DISCOVERED' intent 
and typically results in keys being sent to a local or remote device during 
the 'processCardInfo' call. In the event of an asynchronous webservice look 
up, however, 'processCardInfo' may be called on a separate thread after the 
async call has finished.

## ConnectivityChange.java
'ConnectivityChange' class responds to 'android.net.conn.CONNECTIVITY_CHANGE' 
intents. The appearance of a CONNECTIVITY_CHANGE intent means the status of 
the wifi connection has changed, in which case we use the 'Internet' class to 
test whether the connection's a fully functioning internet connection.

## Conversion.java
'Conversion' class handles general conversion between base64, hex and byte 
arrays.

## Core.java
'Core' service manages global variables and sets up objects that must persist 
through the lifetime of the application, eg. InputStick. The 'Core' service is 
run on startup as a result of the 'BootCompleted' class receiving the 
'android.intent.action.BOOT_COMPLETED' intent. 

Global variables that 'Core' manages include 'passphrase', used to decrypt 
the University of Cambridge's unique card key, stored as encrypted data when 
the app is authenticated. 'Core' also handles the sending of keys to the 
output device - either the local keyboard buffer in the form of a custom input 
device (com.example.android.softkeyboard), or an external PC via the 
Bluetooth-to-USB InputStick dongle.

Other tasks the 'Core' service performs:
- Creating a login timer that resets logins after a period of time.
- Creating an ADB monitor that checks to see whether someone is trying 
to connect to the phone via USB debugging. If there is, the encrypted 
card key is deleted.

## Decryption.java
'Decryption' class handles decryption of card key. Based on "Interoperable AES 
encryption with Java and JavaScript" https:github.com/mpetersen/aes-example

## FallbackService.java
'FallbackService' class accesses webservice to retrieve a value based on the 
Mifare ID of the card. The webservice should provide the information in JSON 
format as:

{"status":"XXX","_name_":"_value_"}

where 'status' is the status of the query; '200' = Success and _name_ and 
_value_ are the name/value of the data field.

Uses JSON parser (c) Mini Sharma from: 
http:mrbool.com/how-to-use-json-to-parse-data-into-android-application/28944#ixzz33HP5lE46

## InputStick.java
'InputStick' class manages connection to the Bluetooth-to-USB InputStick 
dongle (www.inputstick.com).

## Interface.java
'Interface' activity handles UI. Provides screen output showing current status 
of card processing and also access to application settings and authentication. 

## Internet.java
'Internet' class tests whether fully-functioning internet connection in place. 
It may be possible for an internet connection to only be 'partly' in place if 
there is a live connection to a Raven-authenticated access point but the 
authentication with Raven has not been completed. So we search the page that's 
retrieved (google.com) and look for relevant keywords to see whether that's 
the case.

## JSONParser.java
'JSONParser' class makes JSON request and returns data as JSON object.

## Login.java
'Login' class manages login prompt. After entering a passphrase through 
login prompt, the system checks it is the correct passphrase for the encrypted 
card key. If it is, it sets the global passphrase variable (via 'Core') 
accordingly. If not, it increments the login counter and prompts the user 
again.

## PrefFragment.java
'PrefFragment' class loads PreferenceScreen-compliant XML application settings.

## Settings.java
'Settings' class handles access to application settings. Application settings 
are automatically synced to preferences UI through a PreferenceScreen-compliant 
XML preferences file. For example a list of choices is defined within 
'preferences.xml' as 

<ListPreference
	android:key="setting_card_info"
	android:title="@string/title_card_info"
	android:summary="@string/summary_card_info"
	android:entries="@array/entries_card_info"
	android:entryValues="@array/entryvalues_card_info"
	android:dialogTitle="@string/title_card_info" />

This settings can be retrieved from code using the 'android:key' value:
pref.getString("setting_card_info", "");      

## TextOutput.java
TextOutput class handles simple text output generated by app as it progresses. 
The text data for TextOutput is stored as a global variable in 'Core'.