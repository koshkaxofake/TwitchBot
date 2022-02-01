using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using TwitchLib.Client.Events;

namespace TwitchApp
{
  class Command
  {
    public UInt32 multiplyer = 10;
    protected Bot bot;
    public Command(Bot bott)
    {
      bot = bott;
    }
    public virtual UInt32 cost()
    {
      return 99999999;
    }
    public virtual void CostExplain(string args, OnMessageReceivedArgs e)
    {
      bot.client.SendReply(e.ChatMessage.Channel, e.ChatMessage.Id, "this command cost " + cost().ToString() + " points");
    }
    public virtual void Execute(string args, OnMessageReceivedArgs e)
    {
      Console.WriteLine("error no command... "+args);
    }
    public void ExecuteCommand(string args, OnMessageReceivedArgs e)
    {
      if (SpendPoints(e.ChatMessage.Username) == false)
      {
        bot.client.SendReply(e.ChatMessage.Channel, e.ChatMessage.Id, "Not enough points, you have " + bot.chatManager.numPoints(e.ChatMessage.Username).ToString() + " you need " + cost().ToString() + " points");
        return;
      }
      Execute(args, e);
    }
    public bool SpendPoints(string username)
    {
      return bot.chatManager.SpendPoints(username, cost());
    }
  }
  class TypeCommand : Command
  {
    public TypeCommand(Bot bott) : base(bott)
    {

    }
    public override UInt32 cost()
    {
      return 120 * multiplyer;
    }
    public override void Execute(string com, OnMessageReceivedArgs e)
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
          Input.SendInputWithAPI(c.ToString());
        }
        escaped = c == '/';
      }
    }
  }
  class StopSoundCommand : Command
  {
    public StopSoundCommand(Bot bott) : base(bott)
    {

    }
    public override UInt32 cost()
    {
      return 0;
    }
    public override void Execute(string args, OnMessageReceivedArgs e)
    {
      bot.soundInterface.Clear();
    }
  }

  class InputCommand : Command
  {
    public InputCommand(Bot bott) : base(bott)
    {

    }
    public override UInt32 cost()
    {
      return 15 * multiplyer;
    }
    public override void Execute(string com, OnMessageReceivedArgs e)
    {
      string msg = com;
      if (msg.Length > 100)
        msg = msg.Substring(0, 100);
      string[] image_params = msg.Split(" ");
      foreach (string s in image_params)
        Input.SendInputWithAPIHardwareSleep(s);
    }
  }
  class MouseCommand : Command
  {
    public MouseCommand(Bot bott) : base(bott)
    {

    }
    public override UInt32 cost()
    {
      return 10* multiplyer;
    }
    public override void Execute(string com, OnMessageReceivedArgs e)
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
  }
  class WriteCommand : Command
  {
    public WriteCommand(Bot bott) : base(bott)
    {

    }
    public override UInt32 cost()
    {
      return 120* multiplyer;
    }
    public override void Execute(string com, OnMessageReceivedArgs e)
    {
      string msg = com;
      if (msg.Length > 100)
        msg = msg.Substring(0, 100);
      string image_params = msg.Substring(5);
      foreach (char c in image_params)
        Input.SendInputWithAPI(c.ToString());
      Thread.Sleep(4);
      Input.SendInputWithAPIHardware("enter");
    }
  }
  class ChatCommand : Command
  {
    public ChatCommand(Bot bott) : base(bott)
    {

    }
    public override UInt32 cost()
    {
      return 5 * multiplyer;
    }
    public override void Execute(string com, OnMessageReceivedArgs e)
    {
      Input.SendInputWithAPIHardwareHold("shift");
      Input.SendInputWithAPIHardware("enter");
      Thread.Sleep(1);
      string msg = com;
      if (msg.Length > 100)
        msg = msg.Substring(0, 100);
      string image_params = msg.Substring(4);
      foreach (char c in image_params)
        Input.SendInputWithAPI(c.ToString());
      Input.SendInputWithAPIHardwareRelease("shift");
      Input.SendInputWithAPIHardware("enter");
    }
  }
  class PlaySoundCommand : Command
  {
    public PlaySoundCommand(Bot bott) : base(bott)
    {

    }
    public override UInt32 cost()
    {
      return 600;
    }
    public override void Execute(string com, OnMessageReceivedArgs e)
    {
      //format play soundname
      string[] image_params = com.Split(" ");
      for (int i = 1; i < image_params.Length; ++i)
      {
        string soundname = image_params[i];
        bot.soundInterface.AddSound(soundname);
      }
    }
  }
  class UseQCommand : Command
  {
    public UseQCommand(Bot bott) : base(bott)
    {

    }
    public override UInt32 cost()
    {
      return 20 * multiplyer;
    }
    public override void Execute(string com, OnMessageReceivedArgs e)
    {
      Input.SendInputWithAPIHardware("q");
    }
  }
  class UseWCommand : Command
  {
    public UseWCommand(Bot bott) : base(bott)
    {

    }
    public override UInt32 cost()
    {
      return 20 * multiplyer;
    }
    public override void Execute(string com, OnMessageReceivedArgs e)
    {
      Input.SendInputWithAPIHardware("w");
    }
  }
  class UseECommand : Command
  {
    public UseECommand(Bot bott) : base(bott)
    {

    }
    public override UInt32 cost()
    {
      return 20 * multiplyer;
    }
    public override void Execute(string com, OnMessageReceivedArgs e)
    {
      Input.SendInputWithAPIHardware("e");
    }
  }
  class UseRCommand : Command
  {
    public UseRCommand(Bot bott) : base(bott)
    {

    }
    public override UInt32 cost()
    {
      return 69* multiplyer;
    }
    public override void Execute(string com, OnMessageReceivedArgs e)
    {
      Input.SendInputWithAPIHardware("r");
    }
  }
  class UseFCommand : Command
  {
    public UseFCommand(Bot bott) : base(bott)
    {

    }
    public override UInt32 cost()
    {
      return 100 * multiplyer;
    }
    public override void Execute(string com, OnMessageReceivedArgs e)
    {
      Input.SendInputWithAPIHardware("f");
    }
  }
  class MiaPingCommand : Command
  {
    public MiaPingCommand(Bot bott) : base(bott)
    {

    }
    public override UInt32 cost()
    {
      return 10 * multiplyer;
    }
    public override void Execute(string com, OnMessageReceivedArgs e)
    {
      Input.SendInputWithAPIHardware("v");
      Thread.Sleep(3);
      Input.LeftMouseHold(true);
      Thread.Sleep(3);
      Input.MouseMove(-10000, 0);
      Thread.Sleep(3);
      Input.LeftMouseHold(false);
    }
  }
  class BetCommand : Command
  {
    Bet bet = new Bet();
    public BetCommand(Bot bott) : base(bott)
    {

    }
    public override UInt32 cost()
    {
      return 0;
    }
    public override void Execute(string com, OnMessageReceivedArgs e)
    {
      //moderators make bets
      if (e.ChatMessage.IsModerator || e.ChatMessage.IsBroadcaster)
      {
        string command = com.Split(' ')[0];
        if (command == "bet")
        {
          bet = new Bet();
          bet.BetsOpen = true;
          //bet command will be structured as (bet option 1, option 2, ..)
          string[] args = com.Substring(command.Length + 1).Split(',');
          foreach (string arg in args)
          {
            bet.options.Add(arg);
          }
          bot.client.SendMessage(e.ChatMessage.Channel, "Betting is open");
        }
        else if (command == "betstop")
        {
          bot.client.SendMessage(e.ChatMessage.Channel, "Betting is closed");
          bet.BetsOpen = false;
        }
        else if (command == "win")
        {
          //deciding the winner of the last bet
          string param = com.Split(' ')[1];
          int index = 0;
          //check if the parameter is the index of the winnin option
          if (int.TryParse(param, out index) == false)
          {
            //the parameter is the string value
            for (int i = 0; i < bet.options.Count; ++i)
            {
              if (bet.options[i].ToLower() == param.ToLower())
              {
                index = i;
              }
            }
          }
          //give the winners the money
          foreach (var wagers in bet.wagers)
          {
            if (wagers.Value.Key == index)
            {
              //winner, give them money
              bot.chatManager.GivePoints(wagers.Key, wagers.Value.Value * 2);
            }
            else
            {
              //loser, to bad bro
            }
          }
          //reset bet
          bet = new Bet();
        }
      }
      //non moderators place bets
      else
      {
        //check if there is a bet
        if (bet.options.Count == 0 || bet.BetsOpen == false)
        {
          bot.client.SendReply(e.ChatMessage.Channel, e.ChatMessage.Id, "Betting is not open right now");
          return;
        }
        //format of non-mod bet command (bet option amount)
        string []args = com.Split(' ');
        if (args.Length != 3)
        {
          //todo add message
          bot.client.SendReply(e.ChatMessage.Channel, e.ChatMessage.Id, "invalid bet");
          return;
        }
        //get the option they are betting for
        UInt32 index = 0;
        if (UInt32.TryParse(args[1], out index) == false)
        {
          //the parameter is the string value
          for (UInt32 i = 0; i < bet.options.Count; ++i)
          {
            if (bet.options[(int)i].ToLower() == args[1].ToLower())
            {
              index = i;
            }
          }
        }

        //get the amount they are betting
        UInt32 amount = 0;
        if (UInt32.TryParse(args[2], out amount) == false)
        {
          //todo add error message
          bot.client.SendReply(e.ChatMessage.Channel, e.ChatMessage.Id, "invalid bet");
          return;
        }
        if (bot.chatManager.SpendPoints(e.ChatMessage.Username, amount))
        {
          bet.wagers.Add(e.ChatMessage.Username, new KeyValuePair<uint, uint>(index, amount));
        }
        else
        {
          bot.client.SendReply(e.ChatMessage.Channel, e.ChatMessage.Id, "You dont got enough points bro, " + bot.chatManager.numPoints(e.ChatMessage.Username).ToString());
        }
      }
    }
  }
  class WalletCommand : Command
  {
    public WalletCommand(Bot bott) : base(bott)
    {

    }
    public override UInt32 cost()
    {
      return 0;
    }
    public override void Execute(string com, OnMessageReceivedArgs e)
    {
      bot.client.SendReply(e.ChatMessage.Channel, e.ChatMessage.Id, "you have " + bot.chatManager.numPoints(e.ChatMessage.Username).ToString() + " points");
    }
  }
  class ImageCommand : Command
  {
    Dictionary<string, string> imageDic = new Dictionary<string, string>();
    void InitImageDic()
    {
      imageDic.Add("guy", "https://alchetron.com/cdn/petre-mshvenieradze-e19a0604-28ed-461d-9b67-76edbe6e711-resize-750.jpeg");
      imageDic.Add("horse", "https://upload.wikimedia.org/wikipedia/commons/thumb/8/85/Points_of_a_horse.jpg/330px-Points_of_a_horse.jpg");
      imageDic.Add("horse conk", "https://media.nationalgeographic.org/assets/photos/293/220/dc983ca5-23b3-496c-8cf8-40dbc33c4894.jpg");
      imageDic.Add("horse cock", "https://media.nationalgeographic.org/assets/photos/293/220/dc983ca5-23b3-496c-8cf8-40dbc33c4894.jpg");
    }
    public ImageCommand(Bot bott) : base(bott)
    {
      InitImageDic();
    }
    public override UInt32 cost()
    {
      return 500;
    }
    public override void Execute(string com, OnMessageReceivedArgs e)
    {
      string image = com.Substring(6);
      if (imageDic.ContainsKey(image))
        bot.AddHtml("<HTML><img src=\"" + imageDic[image] + "\"style=\"width:250px;height:auto;\"><meta http-equiv=\"refresh\" content=\"5\"></HTML>");
    }
  }

  class YouTubeCommand : Command
  {
    public YouTubeCommand(Bot bott) : base(bott)
    {

    }
    public override void CostExplain(string com, OnMessageReceivedArgs e)
    {
      string[] args = com.Split(' ');
      int lendth = 0;
      if (int.TryParse(args[args.Length - 1], out lendth))
      {
        string video_length = args[args.Length - 1] + "000";
        bot.client.SendReply(e.ChatMessage.Channel, e.ChatMessage.Id, "This video will cost "+ (lendth*10).ToString());

      }
      else
      {
        bot.client.SendReply(e.ChatMessage.Channel, e.ChatMessage.Id, "Youtube video will cost 10 points per second of video, be default it plays 30 seconds");
      }
    }
    public override UInt32 cost()
    {
      //as cost depending on length
      return 0;
    }
    public override void Execute(string com, OnMessageReceivedArgs e)
    {
      //video url
      string[] args = com.Split(' ');
      string video_id = "";
      string video_time = "0";
      //lenth in ms
      string video_length = "30000";
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
          video_id = args[1].Substring(args[1].IndexOf("tu.be/") + 6);
        }
        else
        {
          using (var client = new WebClient())
          {
            string result = client.DownloadString("https://www.youtube.com/results?search_query=" + com.Substring(6));
            int index1 = result.IndexOf("videoId");
            string temp = result.Substring(index1);
            video_id = temp.Split("\"")[2];
            int lendth = 0;
            if (int.TryParse(args[args.Length - 1], out lendth))
            {
              video_length = args[args.Length - 1] + "000";
            }
          }
        }
      }
      else if (args.Length > 1)
      {
        //just the id
        video_id = args[1];
      }
      UInt32 vidLength;
      if (UInt32.TryParse(video_length, out vidLength))
      {
        if (!bot.chatManager.SpendPoints(e.ChatMessage.Username, vidLength/10))
        {
          bot.client.SendReply(e.ChatMessage.Channel, e.ChatMessage.Id, "Not enough points to play that video, you have " + bot.chatManager.numPoints(e.ChatMessage.Username).ToString() + " you need " + (vidLength / 10).ToString() + " points");
          return;
        }
      }
      else
      {
        return;
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
      html += "videoId: '" + video_id + "',\n";
      html += "playerVars:\n";
      html += "{\n";
      html += "'playsinline': 1,\n";
      html += "'controls': 0,\n";
      html += "'start': " + video_time + "\n";
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
      html += "setTimeout(stopVideo, " + video_length + ");\n";
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
      bot.AddHtml(html);
    }
  }
}
