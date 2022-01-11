To make the chatbot work you will need to create a new class called SecretDontOpenOnStreamFile that has a static function AccessToken() that returns a string of your TwitchLib api token(see https://github.com/TwitchLib/TwitchLib to find out how to get the twitch lib token). You can also just replace the function call with the api token, you do you.

You are going to also want to set it to be your account when creating ConnectionCredentials. Also can change what channel the bot connects to by changing client.Initialize(credentials, "koshkaxofake"); 

Sound files for the play command need to be where the .exe is located.

Hit me up on discord if you got questions. https://discord.gg/sxeeByVQvP
