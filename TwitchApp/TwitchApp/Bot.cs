using System;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Http.Headers;
using System.Media;
using NAudio;
using System.Threading;
using NAudio.Wave;
using System.Windows.Forms;

namespace TwitchApp
{
  class Bot
  {
    public TwitchClient client;
    public ChatterManager chatManager = new ChatterManager("chatdata.txt");
    List<string> eroMsg = new List<string>();
    Random rand = new Random();
    Dictionary<string, uint> wallets = new Dictionary<string, uint>();
    Dictionary<string, Bet> bets = new Dictionary<string, Bet>();
    Dictionary<string, Command> commands = new Dictionary<string, Command>();
    public SoundInterface soundInterface;
    string baseHtml = "<HTML><meta http-equiv=\"refresh\" content=\"1\"><HTML>";
    //string baseHtml = "<HTML><script>$(document).ready(function(e) {var refresher = setInterval(\"update_content();\", 3000);})function update_content(){$.ajax({type: \"GET\",url: \"index\",cache: false,}).done(function(page_html) {alert(\"LOADED\");var newDoc = document.open(\"text/html\", \"replace\");newDoc.write(page_html);newDoc.close();});}<script></HTML>";
    List<string> htmlQueue = new List<string>();
    Mutex webserverMutex = new Mutex();
    void InitCommands()
    {
      commands.Add("q", new UseQCommand(this));
      commands.Add("w", new UseWCommand(this));
      commands.Add("e", new UseECommand(this));
      commands.Add("r", new UseRCommand(this));
      commands.Add("f", new UseFCommand(this));
      commands.Add("mouse", new MouseCommand(this));
      commands.Add("chat", new ChatCommand(this));
      commands.Add("type", new TypeCommand(this));
      commands.Add("write", new WriteCommand(this));
      commands.Add("input", new InputCommand(this));
      WalletCommand wc = new WalletCommand(this);
      commands.Add("wallet", wc);
      commands.Add("points", wc);
      YouTubeCommand ytc = new YouTubeCommand(this);
      commands.Add("play", ytc);
      commands.Add("video", ytc);
      commands.Add("playsound", new PlaySoundCommand(this));
      commands.Add("image", new ImageCommand(this));
      commands.Add("stop", new StopSoundCommand(this));
      commands.Add("mia", new MiaPingCommand(this));
      BetCommand bet = new BetCommand(this);
      commands.Add("bet", bet);
      commands.Add("betstop", bet);
      commands.Add("win", bet);
      commands.Add("give", new GivePoints(this));
      TTSCommand tts = new TTSCommand(this);
      commands.Add("tts", tts);
      commands.Add("speak", tts);
      commands.Add("read", tts);
      commands.Add("anounce", tts);
      eroMsg.Add("With my back against the wall, I watch as he slowly approaches me. His ramrod swinging like a pendulum. Every second its swings all the way to the right flicking the precum across the room onto a portrait of my mom. He stops right in front of me and stares directly into my eyes. The pendulum motion is slowly dying down as his blood sword fills up. He wipes away my tears, then uses them to lube up his penetrator. It has finally reached my velvet lounge, but it hasnt finished growing, im on my tippy toes now and its parallel with the floor. He turns his head and spits a massive loogie onto my micheal jordon figurine. My feet are no longer touching the ground and his flesh tyne is lifting me even higher. He turns his head back to me, he has a smug look on his face as if he is proud of spitting on my figurine. Suddenly his red headed dragon flys into my baby cave and my feet touch the ground.");
      eroMsg.Add("Out of the corner of my eye I can see his scrotum totem sneaking out from under his kilt, I prepare myself for the dick to face fight that is to come. I try to make a preemptive attack but he's too fast, his hairy canary has already high-fived my uvula. His hand is exploring my badonkadonk. His super soaker has turned my throat into a musky swamp of mayonnaise. He throws me onto the ottoman and pins me with his hands on my back. I'm dying in anticipation as his jigger is kissing my log cutter, his rod creeps into my dark tunnel like a creeper in minecraft. I feel his throbbing creeper explode inside of me, during his moment of euphoria I muster all my strength to push him off of me. I quickly leap onto him and pin his arms with my legs, I stuff my badonkadonk into his face. I can feel the bristle of his beard as my balls rub across his chin.");
      eroMsg.Add("He tickled my baby valley with his throbbing $5 footlong. I let out a squeal that sounded like a walrus at the dentist, when he drove his custard luancher so deep that I no longer felt hungry. 5 minutes? 30 minutes? an hour? I dont know I can't think straight anymore, all I know is he is sending millions of planktons into me trying to find my kraby patty recipe.");
      eroMsg.Add("He lightly rubs my buns with his fingers before parting my buns, sliding in his big hot wiener and filling it with his mayonnaise. I lick all the mayonnaise off his wiener, then swallow his entire wiener. The hotdog vendor looks at me in disgust.");
      eroMsg.Add("In seconds all of her clothes are on the ground and she is sprawled naked across my bed. My breathing is heavy and my glasses are fogging up. She grabs my shirt and pulls me onto her bare body, she flips me onto my back and stradles me. She starts unbuttoning my shirt, but gives up after four buttons. Her hand moves down to my crotch, after a little fondling she unzips my pants and pulls out my fire hose. Unfortunately for her though, I had already came minutes ago when she first touched me, and I have now passed out from embarrassment.");
      eroMsg.Add("His massive serpent is covered in thick love slime. I've never seen one this big and its not even hard yet. I can barely fit it into my mouth, and after a couple minutes of slurping his gherkin, hes still all bendy wendy. I flip around getting ready to put this monster inside of me. His  glasses are fogged over, hiding his pretty blue eyes. I slowly start feeding his limp lingam to my beaver, until my beaver can't take anymore. I am bouncing on his pogo stick at the resonance frequency of his mattress. Each bounce getting higher and higher, until the force of the mattress launched his glasses off his face, shattering them on the ceiling revealing his unconscious face. I confirm his dick still has a pulse before continuing.");
      eroMsg.Add("The ground shakes every time he rhythmically thrusts his magic love wand into me, synchronized with the crashing of the waves on the sand. I grab the ground in pleasure, the smooth white sand flows around my fingers. He brushes the hair out of my eyes, and with his hand on my cheek he kisses me on the lips. Tongues intertwined, he runs his hand down my arm and into my hand. Hand in hand we reach climax together. He grabs a big handful of sand and pours it over my bearded clam and rubs the rest on his gooey duck. The sand mixes with our love juices making a coarse paste. He quickly pushes the paste into my deepest parts with his spam javelin, while we are both still at max sensitivity.");
      eroMsg.Add("He looks odd with no upper body muscles but with legs like tree trunks, but I'm excited to see what a professional cyclist can do in bed. He undoes his fly and displays his eight inch ding-a-ling, but it looks puny next to the telephone polls that are his quadriceps. He rams my baby maker with his love stake at full force. The flex of his quadriceps femoris ripped his jeans so violently that the denim whipped me right on my meat curtains.");
      eroMsg.Add("He grabs my hand while I am taking his pants off. \"Sorry, but we can't do this, you won't be able to handle it\" he says. \"I can handle it, I'm not like other girls\" I reply. \"I have two vaginas\",\"I have two penises\" we say at the same time. We both pause for a second. \"I have herpes\" we both say at the same time. We pause again, I look up at his face. \"We're meant to be together\" I say. \"I was lying, stay the fuck away from me you monster\" he says");
      eroMsg.Add("He grabs the collar of his shirt, pulls it over his head, and tosses it into his laundry basket in one fluid motion. My mouth starts watering looking at his six pack that looks like kings hawaiian sweet rolls. With a flick of his wrist his belt is loosened and his pants drop to the floor. He hops onto his bed and pushes his mega man body pillow aside. He slides his trousers down to his knees, and begins playing with his johnson. He looks so hot with his eyes closed, one hand behind his head, and one hand on his other head, I wish I could do more than just watch through his webcam. Maybe tomorrow i'll accidently bump into him and ask him if he plays mega man.");
      //msg.Add();
      Erotic ero = new Erotic(this, eroMsg);
      commands.Add("ero", ero);
      commands.Add("erotic", ero);
      commands.Add("sex", ero);
      commands.Add("george", ero);
      commands.Add("naked", ero);
      commands.Add("ttsex", ero);
    }
    public Bot(SoundInterface sound)
    {
      //start webserver
      Thread thread1 = new Thread(webserver);
      thread1.Start();

      soundInterface = sound;

      InitCommands();

      ConnectionCredentials credentials = new ConnectionCredentials("koshkaxofake", SecretDontOpenOnStreamFile.AccessToken());
      var clientOptions = new ClientOptions
      {
        MessagesAllowedInPeriod = 750,
        ThrottlingPeriod = TimeSpan.FromSeconds(30)
      };
      WebSocketClient customClient = new WebSocketClient(clientOptions);
      client = new TwitchClient(customClient);
      client.Initialize(credentials, "koshkaxofake");

      client.OnLog += Client_OnLog;
      client.OnJoinedChannel += Client_OnJoinedChannel;
      client.OnMessageReceived += Client_OnMessageReceived;
      client.OnWhisperReceived += Client_OnWhisperReceived;
      client.OnNewSubscriber += Client_OnNewSubscriber;
      client.OnConnected += Client_OnConnected;
      client.OnUserJoined += Client_OnUserJoined;
      client.OnUserLeft += Client_OnUserLeft;
      client.OnExistingUsersDetected += Client_OnUserExists;
      client.OnGiftedSubscription += Client_OnGiftedSub;
      chatManager.followerEvent += Client_OnFollow;
      chatManager.unfollowerEvent += Client_OnUnFollow;
      
      client.Connect();

      Thread thread2 = new Thread(BotLoop);
      thread2.Start();
    }
    //update loop for points and other thingys
    void BotLoop()
    {
      while(true)
      {
        Thread.Sleep(10000);
        
        //check for new followers
        chatManager.CheckForFollowers();

        chatManager.GivePoints(1);
        chatManager.Save();
      }
    }
    private void Client_OnFollow(string username)
    {
      //play a sound and show an image
      AddHtmlFront("<HTML><img src=\"" + "https://i.imgur.com/Wt28FXv.png" + "\" style=\"width:600px;height:auto;\"><p style=\"color: red; font - size: 72px; \">" + username +"</p><meta http-equiv=\"refresh\" content=\"5\"></HTML>");
      soundInterface.AddSound("wow.mp3");
      chatManager.GivePoints(username, 10000);
    }
    private void Client_OnUnFollow(string username)
    {
      Console.WriteLine(username + " unfollowed");
      chatManager.SpendPoints(username, chatManager.numPoints(username));
    }
    private void Client_OnGiftedSub(object sender, OnGiftedSubscriptionArgs e)
    {
    }

    private void Client_OnUserExists(object sender, OnExistingUsersDetectedArgs e)
    {
      foreach (string user in e.Users)
      {
        Console.WriteLine(user + " was in chat");
        chatManager.ChatterJoined(user);
      }
    }
    private void Client_OnUserJoined(object sender, OnUserJoinedArgs e)
    {
      Console.WriteLine(e.Username + " joined chat");
      chatManager.ChatterJoined(e.Username);
    }
    private void Client_OnUserLeft(object sender, OnUserLeftArgs e)
    {
      Console.WriteLine(e.Username + " left chat");
      chatManager.ChatterLeft(e.Username);

    }
    private void Client_OnLog(object sender, OnLogArgs e)
    {
      Console.WriteLine($"{e.DateTime.ToString()}: {e.BotUsername} - {e.Data}");
    }

    private void Client_OnConnected(object sender, OnConnectedArgs e)
    {
      Console.WriteLine($"Connected to {e.AutoJoinChannel}");
    }

    private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
    {
      Console.WriteLine("Bot is Live");
      client.SendMessage(e.Channel, "Bot is Live!");
    }

    List<string> ParseCommand(string msg, string commandChar)
    {
      List<string> ret = new List<string>();
      int index = msg.LastIndexOf(commandChar);
      string extra = "";
      while (index != -1)
      {
        string inner = msg.Substring(index + 1); //remove the !
        string command = inner.Split(' ')[0].ToLower();
        if (commands.ContainsKey(command))
        {
          ret.Add(inner + extra);
          extra = "";
          msg = msg.Substring(0, index);
          index = msg.LastIndexOf(commandChar);
        }
        else
        {
          //include the !
          extra = msg.Substring(index) + extra;
          msg = msg.Substring(0, index);
          index = msg.LastIndexOf(commandChar);
        }
      }
      ret.Reverse();
      return ret;
    }

    private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
    {
      MessageData msg = new MessageData(e.ChatMessage);
      chatManager.OnMessage(e.ChatMessage.Username, msg);
      List<string> parsed_commands = ParseCommand(e.ChatMessage.Message, "!");
      //hard code an anti steven
      if (e.ChatMessage.Username.Contains("pluto") == false)
      {
        foreach (string com in parsed_commands)
          commands[com.Split(' ')[0].ToLower()].ExecuteCommand(com, msg);
      }
      else if (parsed_commands.Count > 0)
      {
        client.SendMessage(e.ChatMessage.Channel, "Strange seems like the bot isnt working...");
      }

      //tts messages with bits
      if (e.ChatMessage.Bits > 0 && parsed_commands.Count == 0)
      {
        soundInterface.AddTTS(e.ChatMessage.Message);
      }

      parsed_commands = ParseCommand(e.ChatMessage.Message, "$");
      foreach (string com in parsed_commands)
        commands[com.Split(' ')[0].ToLower()].CostExplain(com, msg);
    }

    private void Client_OnWhisperReceived(object sender, OnWhisperReceivedArgs e)
    {
      MessageData msg = new MessageData(e.WhisperMessage);

      chatManager.OnMessage(e.WhisperMessage.Username, msg);
      List<string> parsed_commands = ParseCommand(e.WhisperMessage.Message, "!");
      //hard code an anti steven
      if (e.WhisperMessage.Username != "pluto5578")
      {
        foreach (string com in parsed_commands)
          commands[com.Split(' ')[0].ToLower()].ExecuteCommand(com, msg);
      }
      else if (parsed_commands.Count > 0)
      {
        client.SendWhisper(e.WhisperMessage.Username, "Strange seems like the bot isnt working...");
      }

      parsed_commands = ParseCommand(e.WhisperMessage.Message, "$");
      foreach (string com in parsed_commands)
        commands[com.Split(' ')[0].ToLower()].CostExplain(com, msg);

      client.SendWhisper(e.WhisperMessage.Username, "This is an automated message. I love you " + e.WhisperMessage.Username);
    }

    private void Client_OnNewSubscriber(object sender, OnNewSubscriberArgs e)
    {
      if (e.Subscriber.SubscriptionPlan == SubscriptionPlan.Tier2)
      {
        //play a sound and show an image
        AddHtmlFront("<HTML><img src=\"" + "https://i.imgur.com/HFw7vAX.png" + "\" style=\"width:600px;height:auto;\"><p style=\"color: red; font - size: 72px; \">" + e.Subscriber.DisplayName + "</p><meta http-equiv=\"refresh\" content=\"5\"></HTML>");
        soundInterface.AddSound("wow.mp3");
        soundInterface.AddSound("wow.mp3");
        string personalMsg = eroMsg[rand.Next(eroMsg.Count)];
        personalMsg = personalMsg.Replace(" he ", " " + e.Subscriber.DisplayName+" ");
        personalMsg = personalMsg.Replace(" her ", " " + e.Subscriber.DisplayName+" ");
        personalMsg = personalMsg.Replace(" his ", " " + e.Subscriber.DisplayName+"'s ");
        personalMsg = personalMsg.Replace(" hers ", " " + e.Subscriber.DisplayName + "'s ");
        soundInterface.AddTTS(personalMsg);
        chatManager.GivePoints(e.Subscriber.DisplayName, 100000);
      }
      else if (e.Subscriber.SubscriptionPlan == SubscriptionPlan.Tier3)
      {
        soundInterface.AddTTS(e.Subscriber.DisplayName + " has made a grave mistake");
      }
      else
      {
        //play a sound and show an image
        AddHtmlFront("<HTML><img src=\"" + "https://i.imgur.com/CmeFXFt.png" + "\" style=\"width:600px;height:auto;\"><p style=\"color: red; font - size: 72px; \">" + e.Subscriber.DisplayName + "</p><meta http-equiv=\"refresh\" content=\"5\"></HTML>");
        soundInterface.AddSound("wow.mp3");
        soundInterface.AddTTS(e.Subscriber.DisplayName + " has subscribed");
        soundInterface.AddTTS("Thats a bold strategy cotton, lets see if it pays off for them");
        chatManager.GivePoints(e.Subscriber.DisplayName, 100000);
      }
    }

    public void AddHtml(string html)
    {
      webserverMutex.WaitOne();
      htmlQueue.Add(html);
      webserverMutex.ReleaseMutex();
      Console.WriteLine("add html " + html.Substring(10, 10));
    }
    public void AddHtmlFront(string html)
    {
      webserverMutex.WaitOne();
      htmlQueue.Insert(0, html);
      webserverMutex.ReleaseMutex();
      Console.WriteLine("add html " + html.Substring(10, 10));
    }
    public string GetHtml()
    {
      string html = baseHtml;
      webserverMutex.WaitOne();
      if (htmlQueue.Count > 0)
      {
        html = htmlQueue[0];
        htmlQueue.Remove(html);
      }
      webserverMutex.ReleaseMutex();
      Console.WriteLine("get html " + html.Substring(10, 10));
      return html;
    }

    public void webserver()
    {
      Program.httpListener.Prefixes.Add("http://localhost:8080/index/");
      Program.httpListener.Start();
      while (true)
      {
        HttpListenerContext context = Program.httpListener.GetContext();
        HttpListenerRequest request = context.Request;
        // Obtain a response object.
        HttpListenerResponse response = context.Response;


        // Construct a response.
        //string responseString = "<HTML><BODY> Hello world!</BODY><img src=\"https://alchetron.com/cdn/petre-mshvenieradze-e19a0604-28ed-461d-9b67-76edbe6e711-resize-750.jpeg\" alt =\"Italian Trulli\"><meta http-equiv=\"refresh\" content=\"5\"></HTML>";
        string responseString = GetHtml();
        Console.WriteLine("web refresh");

        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        // Get a response stream and write the response to it.
        response.ContentLength64 = buffer.Length;
        System.IO.Stream output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        // You must close the output stream.
        output.Close();
        //Program.httpListener.Stop();
      }
    }
  }
}
