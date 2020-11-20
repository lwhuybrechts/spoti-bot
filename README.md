# Spoti-bot

This app is a Telegram bot that works with webhooks.

It can be added to a group chat. When a link to a Spotify track is posted in the chat, the track is added to the playlist _Tussen muzieknoten en melodiën_.

### App functions:
There are two functions: Update and Callback. They are both executed with an http request.
- Update is called by the Telegram webhook, and is executed every time a message is sent in the chat.
- Callback is called by the Spotify login page to get an accesstoken, which is used to edit Spotify playlists and the player-state with.

Startup is executed every time one of the functions are executed, and sets up the dependency injection for their execution.

### Spotify authorization flow:
1. The user request an url from the bot, to setup their spotify account connection.
2. Bot generates an url to the Spotify login page, and send it to the chat with the user.
3. After the user clicks the link and succesfully logs in, Spotify calls our callback uri with a code.
4. The code can be used to request an access token and refresh token, which are saved in storage. From now on the access token can be used to do calls to the Spotify api.
5. When the access token expires, the Spotify library that we use will automatically refresh it.

## How to debug/test this app with Telegram:
You need to change the webhook of the Telegram chatbot to call our local environment instead of the live bot.
The webhook can be set by executing a http GET or POST request to the Telegram api.

#### Create local settings.
To run this app locally you need to add a file to the root of the solution: local.settings.json

Add to the file, and fill in missing parameters:
```json
{
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "PlaylistOptions:Id": "",
    "PlaylistOptions:Name": "",
    "SentryOptions:Dsn": "",
    "SpotifyOptions:ClientId": "",
    "SpotifyOptions:Secret": "",
    "TelegramOptions:AccessToken": "",
    "TelegramOptions:BotUsername": "",
    "AzureOptions:FunctionAppUrl": "",
    "AzureOptions:StorageAccountConnectionString": ""
  }
}
```

#### Use **ngrok** to create a http tunnel to our local app.
1. Download [ngrok](https://ngrok.com/download), run nrok.exe.
2. Execute command: `ngrok http 8443`. This runs ngrok and listens to port 8443 for http(s) requests.
3. Open http://localhost:4040/ to open the ngrok interface. Two urls are displayed, use the https one when setting the webhook.

#### Then, update the Telegram chatbot **webhook**.
1. Get the Telegram access token from the appsettings.
2. Execute the following http GET and POST requests to the Telegram api.
  * For example, [Postman](https://www.postman.com/downloads/) can be used to execute http requests.
3. Get the current webhook url with a GET request to https://api.telegram.org/bot{telegramAccessToken}/getWebhookInfo.
  * Save the url for later, when you're done testing and need to change it back.
4. Do a POST request to https://api.telegram.org/bot{telegramAccessToken}/setWebhook.
  * As a body, add a form-data parameter 'url' and set it's value to: `{ngrok https url}/api/update`.

#### Then, test the app.
1. Run this app on port 8443.
2. Telegram messages that are sent to the bot will now be directed to your local app, and you can debug their requests.
3. After testing, post the old webhook url to the setWebhook url to switch back to the live bot.

### New feature idea's:
- Web front-end with addedBy, creationDate, browsing, sorting, etc.
- Only add tracks to the playlist at certain upvotes??
- Send a wacky message for a certain genre. The different genres of tracks that are being added are logged to Sentry. Maybe look into genre-seeds: https://developer.Spotify.com/console/get-available-genre-seeds/
- Currently the bot is only authorized with my personal Spotify account. Use personal/direct messages or inline queries to request accesstokens for all users.
- Listen to youtube video's, find their tracks in the Spotify api and add them.
- Check the Telegram api for more inspiration.
- Add Discord functions for groovybot channel?