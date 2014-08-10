University of Cambridge Event Registration App 1.0
==================================================

Overview
--------
This HTML5 application provides an example of a Cambridge University card 
processing application based around 'Card reader', an Android application that 
handles the specifics of Cambridge University card scanning. 

Separating the specifics of card scanning from the more general card 
application has the following advantages:

- Build and test card applications quickly, with only a basic knowledge 
of HTML and Javascript. 

- Card processing application can be run in a variety of configurations, 
depending on the application. 

- For mobile events, an HTML5 application can be compiled as a mobile app and 
run on the same device as the card reader. 

- When the application is to be used in a fixed location, eg. reception desk, 
the application can be loaded as a webpage on a desktop PC while the mobile 
device acts as a separate card reader. 


Specifics
---------
When a University card is scanned, the 'Card reader' Android app sends the 
card's 'Mifare Card ID' number to a local or remote keyboard buffer as 
a sequence of keystrokes terminated with a carriage return, for example 
'0123456789\n'. Assuming the 'Event Registration' screen of this application 
is activated, it receives these sequence of keystrokes into an HTML input 
field. This application then queries a database table to see if the specific 
'Mifare Card ID' corresponds to an attendee in a list of preregistered 
attendees. If it does, the card is logged and the input field is cleared, 
ready to receive the next card ID. If the 'Mifare Card ID' doesn't 
correspond to one of the preregistered attendees, the screen flashes red.

The list of preregistered attendees is downloaded from the central Careers 
database by specifying an 'eventcode' through "Set event code" and then 
clicking "Download attendees"; the list of attendees will then be 
displayed. To revisit this list at any time, select "View attendees".

In some instances, preregistered attendees may lack a 'Mifare Card ID' 
number. This will occur when the Careers database holds both a 
non-standard identifier for the user (starting with '9') and lacks a 
valid CRSID - both of these items of information are necessary to 
connect the UIS card database to the Careers database. 

In those instances where a preregistered attendee lacks a 'Mifare Card 
ID', the following will happen:

- The application will be unable to find their 'Mifare Card ID' in the 
local database and will prompt the user to select the attendee from 
the list of 'ambiguous' attendees (attendees without 'Mifare Card IDs') 
held in the database. 

- The user will need to confirm their firstname and surname independently, 
either by showing the name on their card or by providing some other form 
of identification.

- Once the operator is satisfied the user has confirmed their identity, 
they can select the preregistered attendee from the list on screen. The 
'Mifare Card ID' will then be attached to the user's record in the 
local database. Once this information is uploaded, it can be used to 
match the user to their card in the future.

Once event registration for the event has finished, the data can be 
uploaded to a central server script (via JSON) by clicking "Upload data". 
The server script is currently located at:

http://card.careers.cam.ac.uk/events/
 
The server script saves the data as a timestamped CSV file and 
imports the data into the 'DC_CardUpload' table, ready to be imported 
or cross-queried into other tables. The same server script also handles 
the download of preregistered attendees. In both cases, the script 
validates each web request using the username and password set in 
"Server settings".

After the data has been successfully uploaded, the user can delete all card 
data in the local database by going to "Server settings" and tapping 
"Delete data". As a security precaution, it's only possible to delete data 
once it has all been successfully uploaded.


Logging cards without preregistered attendees
---------------------------------------------
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

- Download attendees: Download the list of preregistered attendees 
for the event that was specified when you selected "Set event code". The 
"Download attendees" button will be greyed-out if data has been recorded 
that has not yet been uploaded. This is because downloading attendees will 
delete any local data.

- View attendees: View the list of preregistered attendees. The 
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
current event will be displayed at the top of the screen while the lower 
half of the screen will display information about cards recently scanned. 
The main black box at the top of the screen displays status, eg. "Waiting 
for card", "Attendee not registered", etc.

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

sh801, v1.0, 10/08/2014
-------------------------------------------------------------------------------
