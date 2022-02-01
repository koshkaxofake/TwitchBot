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
      AddHtmlFront("<HTML><img src=\"" + "https://i.imgur.com/IbpurRL.png" + "\" style=\"width:300px;height:auto;\"><<p style=\"color: red; font - size: 40px; \">&emsp;" + username +"</p><meta http-equiv=\"refresh\" content=\"5\"></HTML>");
      soundInterface.AddSound("wow.mp3");
      chatManager.GivePoints(username, 10000);
    }
    private void Client_OnUnFollow(string username)
    {
      Console.WriteLine(username + " unfollowed");
      chatManager.SpendPoints(username, chatManager.numPoints(username));
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
      chatManager.OnMessage(e.ChatMessage.Username, e);
      List<string> parsed_commands = ParseCommand(e.ChatMessage.Message, "!");
      foreach (string com in parsed_commands)
        commands[com.Split(' ')[0].ToLower()].ExecuteCommand(com, e);

      parsed_commands = ParseCommand(e.ChatMessage.Message, "$");
      foreach (string com in parsed_commands)
        commands[com.Split(' ')[0].ToLower()].CostExplain(com, e);
    }

    private void Client_OnWhisperReceived(object sender, OnWhisperReceivedArgs e)
    {
      if (e.WhisperMessage.Username == "my_friend")
        client.SendWhisper(e.WhisperMessage.Username, "Hey! Whispers are so cool!!");
    }

    private void Client_OnNewSubscriber(object sender, OnNewSubscriberArgs e)
    {
      if (e.Subscriber.SubscriptionPlan == SubscriptionPlan.Prime)
        client.SendMessage(e.Channel, $"Welcome {e.Subscriber.DisplayName}, you are not possible. U R Hack6");
      else
        client.SendMessage(e.Channel, $"Welcome {e.Subscriber.DisplayName}, you are not possible. U R Hack");
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
