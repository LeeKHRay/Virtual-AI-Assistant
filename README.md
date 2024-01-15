# Virtual AI Assitant
Virtual AI Assistant is a Windows application developed using Unity 2019.4.9f1.

## How to use
1. Unzip `Virtual AI Assistant.zip`.
2. Run `Virtual AI Assistant.exe` in the unzipped folder.
3. Type the message in the textbox or click the "Record" button to use speech recognition.
4. Press Enter or click the "Send" button to send the message and then receive the response from the assistant.

## Features
The assistant can do the following things:
- Answer queries about date, time and weather
- Search websites
	- Clicking on the search result can visit the corresponding website in the browser
- Search images
	- Clicking on the image can download it to the `downloaded_images` folder, which is located in the application directory
- Display a map which is able to:
	- Show the route between two locations
	- Display points of interest
	- Show different levels of traffic congestion
	- Show and move back to the home location set by the user
- Send emails
	- Configure your Gmail account in "Settings" first
	- The password is your app password (see https://support.google.com/mail/answer/185833?hl=en)
- Play games (Snake and Tetris) with the user

There are two modes in the application:
- Fullscreen mode
- Desktop mode

## How to build the project:
Before building the project, you should first set the access tokens and API keys.

##### Access token for Dialogflow
1. Create a project in Google Cloud Platform (https://console.cloud.google.com/).
2. Create an agent in Dialogflow (https://dialogflow.cloud.google.com/).
3. In settings, go to the "Export and Import" tab and click "IMPORT FROM ZIP" to import the file `Virtual_AI_Assistant.zip` in `Dialogflow_agent`.
4. In Google Cloud Platform, search "Dialogflow API" and enable it if it is not enabled.
5. In "Service Accounts" under "IAM & Admin", click "+ CREATE SERVICE ACCOUNT".
6. In "Grant this service account access to project", select the role "Dialogflow API Client" and then click "DONE".
7. Click the 3 dots under "Actions" of this service account and click "Manage keys".
8. Click "ADD KEY", choose "Create new key" and choose P12 for the key type.
9. Create a folder named `DialogflowV2` in `Assets/Resources`.
10. Put the downloaded .p12 file into this folder and change its extension from `.p12` to `.bytes`.
11. In Unity, select `Assets/DF2AccessSettings` in the project hierarchy.
12. Put the ID of the project created in Google Cloud Platform, the filename of the .p12 file (without extension) and the service account email to `DF2AccessSettings` in the inspector.

##### Access token for Mapbox
1. Create a Mapbox account in https://www.mapbox.com/.
2. Copy the default public token in https://account.mapbox.com/access-tokens/.
3. In Unity, go to Mapbox > Setup, paste the token to the "Access Token" field and click submit.

##### Access tokens/API keys for the other services
See the comments in:
- `Assets\Scripts\AzureMapsUtils\AzureMaps.cs`
- `Assets\Scripts\AudioUtils\SpeechToText.cs`
- `Assets\Scripts\OpenCage\OpenCage.cs`
- `Assets\Scripts\ProgrammableSearchEngine\CustomSearch.cs`

##### Build the project
1. In Unity, go to File > Build Settings.
2. Click Build.