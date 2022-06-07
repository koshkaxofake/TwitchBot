To make the chatbot work you will need to create a new class called SecretDontOpenOnStreamFile that has a static function SetAccessToken() AccessToken() ClientSecret() and ClientId(). I messed around with that stuff a whole bunch and found the magical sauce that I forgot, so u will have to go figure that out. See here for info https://github.com/TwitchLib/TwitchLib or https://dev.twitch.tv/docs/authentication/getting-tokens-oauth#client-credentials-grant-flow. Best of luc.

You are going to also want to set it to be your account when creating ConnectionCredentials. Also can change what channel the bot connects to by changing client.Initialize(credentials, "koshkaxofake"); 

Sound files for the play command need to be where the .exe is located.

There are a few dependicies/packages that u will need. If u really want to run the app you probably just want to message me directly, I would be happy to help.

Hit me up on discord if you got questions. https://discord.gg/sxeeByVQvP
