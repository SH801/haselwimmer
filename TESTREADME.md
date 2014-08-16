University of Cambridge Event Registration App 1.0
==================================================


Overview
--------
This HTML5 application provides an example of a Cambridge University card 
processing application based around 'Card reader', an Android application that 
handles the specifics of Cambridge University card scanning. 'Card reader' 
functions as a 'keyboard wedge' sending card information to other applications 
via the keyboard buffer.

Using a card scanning 'keyboard wedge' to perform the actual card scanning 
has the following advantages:

- Build and test card applications quickly, with only a basic knowledge 
of HTML and Javascript. 

- Multiple configurations: an HTML5 application can be compiled as a mobile 
app and run on the same device as 'Card reader'; it can also be run within 
a web browser on a desktop PC or tablet, receiving card data from a separate 
mobile phone running 'Card reader'.

In the following documentation, 'user' refers to the user operating the 
software / performing the card scanning, while 'attendee(s)' refers to the 
individual(s) attending the particular event. Ideally the attendee should have 
their University card with them, although the software will allow for 
some manual data input when a card is not present.


Specifics - Card scanning
-------------------------
When a University card is scanned, the 'Card reader' Android app sends the 
card's 'Mifare Card ID' number to a local / remote keyboard buffer as 
a sequence of keystrokes terminated with a carriage return, eg. '0123456789\n'. 
Assuming the "Event Registration" screen of this application is activated, 
the app receives these sequence of keystrokes into an HTML input field. 

The application then queries a database table to see if the specific 
'Mifare Card ID' that was sent corresponds to an attendee in a list of 
pre-registered attendees. If it does, the card is logged and the input 
field is cleared, ready to receive the next card ID. 

The list of pre-registered attendees is downloaded from the central Careers 
database by specifying an 'eventcode' through "Set event code" and then 
clicking "Download attendees". To view the list of attendees and their 
status (registered or not), select "View attendees".

If a scanned card doesn't correspond to one of the pre-registered attendees, 
the app will prompt the user for further information as follows:

- Ambiguous attendees: Some pre-registered attendees may be considered 
'Ambiguous' in that they lack a 'Mifare Card ID'*. Whenever a card id is 
not found in the list of pre-registered attendees, the app will prompt the 
user to select an attendee from the list of 'Ambiguous attendees'. After 
the user has verified the name of the attendee (either using the attendee's 
card or through another form of identification), the card id will then be 
attached to that attendee. 

- If the attendee is not in the list of ambiguous attendees (which may 
be empty), the user will be prompted to add the attendee to the event. 
If the user taps "OK", the card id will be added to the list of registered 
attendees.
 
NOTE: For some events you may ONLY want pre-registered attendees to be 
allowed entry, in which case you should ALWAYS tap "Cancel" when prompted 
to add an attendee.

* Ambiguous attendees will occur when the Careers database holds both a 
non-standard identifier for the user (starting with '9') and lacks a 
valid CRSID - one or other of these items of information must be standard / 
valid to connect the UIS card database to the Careers database. 

Specifics - Card not available
------------------------------
If the attendee does not have their card with them, the user can tap 
"Card not available" on the "Event Registration" screen. This will 
present a screen of all the pre-registered attendees who have not yet 
registered. The user can then select an attendee from this list. 

If the attendee has neither pre-registered nor has their card with them, 
the user should tap "Card not available" and then "NOT LISTED ABOVE" 
to enter a CRSID for the user. It is also possible to enter a CRSID directly 
in the main input field of the "Event Registration" screen. 

Specifics - Uploading data
--------------------------
Once event registration for the event has finished, the data can be 
uploaded to a central server script (via JSON) by clicking "Upload data". 
The server script is currently located at:

http://card.careers.cam.ac.uk/events/
 
The server script saves the data as a timestamped CSV file and 
imports the data into the 'DC_CardUpload' table, ready to be imported 
or cross-queried into other tables. The same server script also handles 
the download of pre-registered attendees. In both cases, the script 
validates each web request using the username and password set in 
"Server settings".

After the data has been successfully uploaded, the user can delete all card 
data in the local database by going to "Server settings" and tapping 
"Delete data". As a security precaution, it is only possible to delete data 
once it has all been successfully uploaded.

Logging cards without pre-registered attendees
----------------------------------------------
If the event does not have pregistered attendees or you want to record 
every card that is swiped, simply ensure the list of attendees is empty. 
The application will then be unable to verify each swiped card against 
a predefined list and will record each card that is presented.

To ensure the list of attendees is empty:

- Upload the latest data via "Upload data".

- Tap "Server settings" and then "Delete data" to clear the local database.

- Tap "Home" and then "Set event code" to set the correct event code for 
the event that you are monitoring. 

NOTE: You will need to set a valid event code before every event.


How this application is built
-----------------------------
The application comprises a single section of Javascript code, which 
handles all the application logic and database processing, and a single 
section of HTML which provides the screens/pages of the user interface. 

SQLite is used to provide a robust local SQL database that maintains state 
across browser sessions. So even if the browser or app crashes before data 
is uploaded, the data will be safely stored away and can be subsequently 
uploaded.

NOTE: Only Chrome browsers support SQLite, the local browser database, so 
a Chrome browser must be used for mobile or PC testing and live use.


User interface
--------------
The application consists of three screens:

1. Home screen
--------------

- Start registration: Start event registration.

- Set event code: Set the event code of the event to be monitored. 
Once this is set, you will be prompted to download attendees. The 
"Set event code" button will be greyed-out if data has been recorded that 
has not yet been uploaded. This is because setting a new event code will 
delete any local data.

- Download attendees: Download the list of pre-registered attendees 
for the event that was specified when you selected "Set event code". The 
"Download attendees" button will be greyed-out if data has been recorded 
that has not yet been uploaded. This is because downloading attendees will 
delete any local data.

- View attendees: View the list of pre-registered attendees. The 
list will show you what attendees have actually attended and the time 
/ date their card was scanned. NOTE: if you are using multiple phones to 
register attendees, there will be NO data reconciliation / consolidation 
between them.

- Upload data: Upload data to the central server. The username/password 
and upload URL can be changed via the "Server settings" screen. The 
"Upload data" button will be greyed-out if there is no data to upload.

- Server settings: Modify the server settings.

- About: General information about this application including version 
number.

2. Event Registration screen
----------------------------
This screen must be active to record card data. The event code for the 
current event will be displayed at the top of the screen along with the 
number of attendees scanned / registered. The main black box at the top 
displays status, eg. "Waiting for card", "Attendee not registered", etc.

The lower half of the screen will display information about cards recently 
scanned.  If the user does not have a card, tap "Card not available".

3. Server settings screen
-------------------------
Use this screen to modify the settings for uploading to the central 
server. You can also delete any local data from this screen, assuming 
all the data's been uploaded.


Creating mobile app
-------------------
1. Download and install Intel XDK from http://xdk-software.intel.com/
2. Run Intel XDK and load "EventRegistrationApp" from supplied source folder. 
You should see this same "index.html" page appear when you click on the 
"Develop" tab across the top of the screen.
3. Click on the "Build" tab and create a mobile app on one of the platforms 
listed.
4. If you're creating an Android app, relevant app icons are provided in the 
"EventRegistrationApp\icons" folder.

-------------------------------------------------------------------------------
Event Registration App (c) Cambridge University Careers Service 2014

sh801, v1.0, 16/08/2014
-------------------------------------------------------------------------------
