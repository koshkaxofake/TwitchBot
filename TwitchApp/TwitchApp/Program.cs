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


/// <summary>
/// 
/// 
/// 
/// 
/// </summary>

namespace TwitchApp
{
  class Program
  {
    public static readonly HttpClient httpclient = new HttpClient();
    public static readonly HttpListener httpListener = new HttpListener();
    static void Main(string[] args)                                                    
    {
      SoundInterface sounds = new SoundInterface();
      Bot bot = new Bot(sounds);
      sounds.SoundLoop();
    }
  }

  class SoundInterface
  {
    Mutex mut = new Mutex();
    List<string> sound_queue = new List<string>();
    public void AddSound(string sound)
    {
      mut.WaitOne();
      sound_queue.Add(sound);
      mut.ReleaseMutex();
    }
    public void Clear()
    {
      mut.WaitOne();
      sound_queue.Clear();
      mut.ReleaseMutex();
    }
    string GetNext()
    {
      string next = "";
      mut.WaitOne();
      if (sound_queue.Count > 0)
      {
        next = sound_queue[0];
        sound_queue.Remove(next);
      }
      mut.ReleaseMutex();
      return next;
    }
    public void SoundLoop()
    {
      while(true)
      {
        string sound = GetNext();
        if (sound != "")
        {
          string soundname = sound;
          if (soundname.Contains(".") == false)
            soundname += ".m4a";
          if (File.Exists(soundname))
          {
            using (var audioFile = new AudioFileReader(soundname))
            using (var outputDevice = new WaveOutEvent())
            {
              outputDevice.Init(audioFile);
              outputDevice.Volume = 0.1f;
              outputDevice.Play();
              while (outputDevice.PlaybackState == PlaybackState.Playing)
              {
                Thread.Sleep(100);
              }
            }
          }
        }
        else
        {
          Thread.Sleep(100);
        }
      }
    }
  }

  class Bet
  {
    public List<String> options = new List<string>();
    public Dictionary<string, KeyValuePair<uint, uint>> wagers = new System.Collections.Generic.Dictionary<string, KeyValuePair<uint, uint>>();
  }

  class Bot
  {
    TwitchClient client;
    Dictionary<string, uint> wallets = new Dictionary<string, uint>();
    Dictionary<string, Bet> bets = new Dictionary<string, Bet>();
    delegate void Command(string com, OnMessageReceivedArgs e);
    Dictionary<string, Command> commands = new Dictionary<string, Command>();
    SoundInterface soundInterface;
    string baseHtml = "<HTML><meta http-equiv=\"refresh\" content=\"1\"><HTML>";
    //string baseHtml = "<HTML><script>$(document).ready(function(e) {var refresher = setInterval(\"update_content();\", 3000);})function update_content(){$.ajax({type: \"GET\",url: \"index\",cache: false,}).done(function(page_html) {alert(\"LOADED\");var newDoc = document.open(\"text/html\", \"replace\");newDoc.write(page_html);newDoc.close();});}<script></HTML>";
    List<string> htmlQueue = new List<string>();
    Mutex webserverMutex = new Mutex();
    Dictionary<string, string> imageDic = new Dictionary<string, string>();
    void InitImageDic()
    {
      imageDic.Add("guy", "https://alchetron.com/cdn/petre-mshvenieradze-e19a0604-28ed-461d-9b67-76edbe6e711-resize-750.jpeg");
      imageDic.Add("horse", "https://upload.wikimedia.org/wikipedia/commons/thumb/8/85/Points_of_a_horse.jpg/330px-Points_of_a_horse.jpg");
      imageDic.Add("horse conk", "https://media.nationalgeographic.org/assets/photos/293/220/dc983ca5-23b3-496c-8cf8-40dbc33c4894.jpg");
      imageDic.Add("horse cock", "https://media.nationalgeographic.org/assets/photos/293/220/dc983ca5-23b3-496c-8cf8-40dbc33c4894.jpg");
    }
    void InitCommands()
    {
      commands.Add("q", UseQ);
      commands.Add("w", UseW);
      commands.Add("e", UseE);
      commands.Add("r", UseR);
      commands.Add("f", UseF);
      commands.Add("mouse", Mouse);
      commands.Add("chat", Chat);
      commands.Add("type", Type);
      commands.Add("write", Write);
      commands.Add("input", InputCommand);
      commands.Add("wallet", Wallet);
      commands.Add("play", YouTube);
      commands.Add("playsound", PlaySound);
      commands.Add("image", Image);
      commands.Add("video", YouTube);
      commands.Add("stop", StopSound);
    }
    public Bot(SoundInterface sound)
    {
      //start webserver
      Thread thread1 = new Thread(webserver);
      thread1.Start();

      soundInterface = sound;

      InitImageDic();

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

      client.Connect();
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

    List<string> ParseCommand(string msg)
    {
      List<string> ret = new List<string>();
      int index = msg.LastIndexOf('!');
      string extra = "";
      while (index != -1)
      {
        string inner = msg.Substring(index + 1); //remove the !
        string command = inner.Split(' ')[0].ToLower();
        if (commands.ContainsKey(command))
        {
          ret.Add(inner+extra);
          extra = "";
          msg = msg.Substring(0, index);
          index = msg.LastIndexOf('!');
        }
        else
        {
          //include the !
          extra = msg.Substring(index) + extra;
          msg = msg.Substring(0, index);
          index = msg.LastIndexOf('!');
        }
      }
      ret.Reverse();
      return ret;
    }

    private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
    {
      List<string> parsed_commands = ParseCommand(e.ChatMessage.Message);
      foreach (string com in parsed_commands)
        commands[com.Split(' ')[0].ToLower()](com, e);
    }

    private void Client_OnWhisperReceived(object sender, OnWhisperReceivedArgs e)
    {
      if (e.WhisperMessage.Username == "my_friend")
        client.SendWhisper(e.WhisperMessage.Username, "Hey! Whispers are so cool!!");
    }

    private void Client_OnNewSubscriber(object sender, OnNewSubscriberArgs e)
    {
      if (e.Subscriber.SubscriptionPlan == SubscriptionPlan.Prime)
        client.SendMessage(e.Channel, $"Welcome {e.Subscriber.DisplayName} to the substers! You just earned 500 points! So kind of you to use your Twitch Prime on this channel!");
      else
        client.SendMessage(e.Channel, $"Welcome {e.Subscriber.DisplayName} to the substers! You just earned 500 points!");
    }
    void StopSound(string com, OnMessageReceivedArgs e)
    {
      soundInterface.Clear();
    }
    void Type(string com, OnMessageReceivedArgs e)
    {
      string msg = com;
      if (msg.Length > 100)
        msg = msg.Substring(0, 100);
      string type_params = msg.Substring(5);
      bool escaped = false;
      foreach (char c in type_params)
      {
        if (escaped)
        {
          if (c == 'n')
            Input.SendInputWithAPIHardware("enter");
        }
        else
        {
          Input.SendInputWithAPIHardware(c.ToString());
        }
        escaped = c == '/';
      }
    }
    void InputCommand(string com, OnMessageReceivedArgs e)
    {
      string msg = com;
      if (msg.Length > 100)
        msg = msg.Substring(0, 100);
      string[] image_params = msg.Split(" ");
      foreach (string s in image_params)
        Input.SendInputWithAPIHardwareSleep(s);
    }
    void Mouse(string com, OnMessageReceivedArgs e)
    {
      string[] mouse_params = com.Split(" ");
      //mouse move x y
      //mouse set x y
      //mouse rightclick
      //mouse leftclick
      if (mouse_params.Length == 4)
      {
        int x = 0;
        int y = 0;
        int.TryParse(mouse_params[2], out x);
        int.TryParse(mouse_params[3], out y);
        if (com.ToLower().Contains("move"))
        {
          Input.MouseMove(x, y);
        }
        if (com.ToLower().Contains("set"))
        {
          Input.SetPosition(x, y);
        }
      }
      if (mouse_params.Length == 2)
      {
        if (com.ToLower().Contains("r"))
        {
          Input.RightMouseClick();
        }
        if (com.ToLower().Contains("f"))
        {
          Input.LeftMouseClick();
        }
      }
    }
    void Write(string com, OnMessageReceivedArgs e)
    {
      string msg = com;
      if (msg.Length > 100)
        msg = msg.Substring(0, 100);
      string image_params = msg.Substring(5);
      foreach (char c in image_params)
        Input.SendInputWithAPIHardware(c.ToString());
      Thread.Sleep(4);
      Input.SendInputWithAPIHardware("enter");
    }
    void Chat(string com, OnMessageReceivedArgs e)
    {
      Input.SendInputWithAPIHardwareHold("shift");
      Input.SendInputWithAPIHardware("enter");
      Thread.Sleep(1);
      string msg = com;
      if (msg.Length > 100)
        msg = msg.Substring(0, 100);
      string image_params = msg.Substring(4);
      foreach (char c in image_params)
        Input.SendInputWithAPIHardware(c.ToString());
      Input.SendInputWithAPIHardwareRelease("shift");
      Input.SendInputWithAPIHardware("enter");
    }
    void PlaySound(string com, OnMessageReceivedArgs e)
    {
      //format play soundname
      string[] image_params = com.Split(" ");
      for (int i = 1; i < image_params.Length; ++i)
      {
        string soundname = image_params[i];
        soundInterface.AddSound(soundname);
      }
    }
    void UseQ(string com, OnMessageReceivedArgs e)
    {
      Input.SendInputWithAPIHardware("q");
    }
    void UseW(string com, OnMessageReceivedArgs e)
    {
      Input.SendInputWithAPIHardware("w");
    }
    void UseE(string com, OnMessageReceivedArgs e)
    {
      Input.SendInputWithAPIHardware("e");
    }
    void UseR(string com, OnMessageReceivedArgs e)
    {
      Input.SendInputWithAPIHardware("r");
    }
    void UseF(string com, OnMessageReceivedArgs e)
    {
      Input.SendInputWithAPIHardware("f");
    }
    //not gonna work
    void Bet(string com, OnMessageReceivedArgs e)
    {
      if (bets.ContainsKey(e.ChatMessage.DisplayName))
      {
        client.SendMessage(e.ChatMessage.Channel, e.ChatMessage.DisplayName + " you already have a bet");
        return;
      }
      string optionstring = e.ChatMessage.Message.Substring(9);
      string[] options = optionstring.Split(",");
      if (options.Length >= 2)
      {
        Bet b = new Bet();
        foreach (string option in options)
        {
          b.options.Add(option);
        }
        bets.Add(e.ChatMessage.DisplayName, b);
        client.SendMessage(e.ChatMessage.Channel, e.ChatMessage.DisplayName + " started a bet");
      }
      else
      {
        client.SendMessage(e.ChatMessage.Channel, e.ChatMessage.DisplayName + " your bet need more options");
      }
    }
    void Wallet(string com, OnMessageReceivedArgs e)
    {
      if (wallets.ContainsKey(e.ChatMessage.DisplayName) == false)
      {
        wallets[e.ChatMessage.DisplayName] = 100;
        client.SendMessage(e.ChatMessage.Channel, e.ChatMessage.DisplayName + " now has 100 moneys");
      }
      else
      {
        client.SendMessage(e.ChatMessage.Channel, e.ChatMessage.DisplayName + " you have " + wallets[e.ChatMessage.DisplayName] + " moneys");
      }
    }
    void Image(string com, OnMessageReceivedArgs e)
    {
      string image = com.Substring(6);
      if (imageDic.ContainsKey(image))
        AddHtml("<HTML><img src=\""+imageDic[image]+ "\"style=\"width:250px;height:auto;\"><meta http-equiv=\"refresh\" content=\"5\"></HTML>");
    }

    void YouTube(string com, OnMessageReceivedArgs e)
    {
      //video url
      string[] args = com.Split(' ');
      string video_id = "";
      string video_time = "0";
      string video_length = "1800000";
      if (args.Length > 1)
      {
        if (args[1].Contains("v="))
        {
          //its full youtube link
          int index = args[1].IndexOf("v=") + 2;
          video_id = args[1].Substring(index);
          if (video_id.Contains("&"))
          {
            index = args[1].IndexOf("&");
            video_id = args[1].Substring(0, index);
          }
          if (args.Length >= 3)
          {
            video_length = args[2] + "000";
          }
        }
        else if (args[1].Contains("t="))
        {
          int index = args[1].IndexOf("t=");
          video_time = args[1].Substring(index + 2);
          int startindex = args[1].LastIndexOf("/");
          video_id = args[1].Substring(startindex + 1, index - startindex - 2);
          if (args.Length >= 3)
          {
            video_length = args[2] + "000";
          }
        }
        //https://youtu.be/v8umSweYgZQ
        else if (args[1].Contains("tu.be/"))
        {
          video_id = args[1].Substring(args[1].IndexOf("tu.be/")+6);
        }
        else
        {
          using (var client = new WebClient())
          {
            string result = client.DownloadString("https://www.youtube.com/results?search_query=" + com.Substring(6));
            int index1 = result.IndexOf("videoId");
            string temp = result.Substring(index1);
            video_id = temp.Split("\"")[2];
          }
        }
      }
      else if (args.Length > 1)
      {
        //just the id
        video_id = args[1];
      }

      string html = "";//"<HTML><script>player.setVolume(10);</script><meta http-equiv=\"refresh\" content=\"5\"><iframe name=\"player\" width=\"420\" height=\"315\"src = \"https://www.youtube.com/embed/tgbNymZ7vqY?autoplay=1\" ></ iframe ><script>player.setVolume(10);</script><meta http-equiv=\"refresh\" content=\"5\"> </HTML>";
      html += "<!DOCTYPE html>\n";
      html += "<html>\n";
      html += "<body>\n";
      html += "<!--1.The<iframe>(and video player) will replace this <div> tag. -->\n";
      html += "<div id = \"player\" ></div>\n";
      html += "<script>\n";
      html += "var tag = document.createElement('script');\n";
      html += "tag.src = \"https://www.youtube.com/iframe_api\";\n";
      html += "var firstScriptTag = document.getElementsByTagName('script')[0];\n";
      html += "firstScriptTag.parentNode.insertBefore(tag, firstScriptTag);\n";
      html += "var player;\n";
      html += "function onYouTubeIframeAPIReady()\n";
      html += "{\n";
      html += "player = new YT.Player('player', {\n";
      html += "height: '390',\n";
      html += "width: '640',\n";
      //html += "videoId: 'M7lc1UVf-VE',\n";
      html += "videoId: '"+video_id+"',\n";
      html += "playerVars:\n";
      html += "{\n";
      html += "'playsinline': 1,\n";
      html += "'controls': 0,\n";
      html += "'start': "+video_time+"\n";
      html += "},\n";
      html += "events:\n";
      html += "{\n";
      html += "'onReady': onPlayerReady,\n";
      html += "'onStateChange': onPlayerStateChange\n";
      html += "}\n";
      html += "});\n";
      html += "}\n";
      html += "function onPlayerReady(event)\n";
      html += "{\n";
      html += "event.target.playVideo();\n";
      html += "player.setVolume(5);\n";
      html += "}\n";
      html += "var done = false;\n";
      html += "function onPlayerStateChange(event)\n";
      html += "{\n";
      html += "if (event.data == YT.PlayerState.PLAYING && !done) {\n";
      html += "setTimeout(stopVideo, "+video_length+");\n";
      html += "done = true;\n";
      html += "}\n";
      html += "if (event.data == YT.PlayerState.ENDED) {\n";
      html += "location.reload();\n";
      html += "}\n";
      html += "}\n";
      html += "function stopVideo()\n";
      html += "{\n";
      html += "player.stopVideo();\n";
      html += "location.reload();\n";
      html += "}\n";
      html += "</script>\n";
      html += "</body>\n";
      html += "</html>\n";
      AddHtml(html);
    }

    public void AddHtml(string html)
    {
      webserverMutex.WaitOne();
      htmlQueue.Add(html);
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
      Console.WriteLine("get html "+html.Substring(10, 10));
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