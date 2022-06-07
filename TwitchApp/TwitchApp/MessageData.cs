using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Http.Headers;
using System.Media;
using NAudio;
using System.Threading;
using NAudio.Wave;
using System.Windows.Forms;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Users;
using System.Speech.Synthesis;
using System.Net.Sockets;


namespace TwitchApp
{
  class MessageData
  {
    public string Username;
    public bool IsBroadcaster;
    public bool IsModerator;
    public string UserId;
    public string Channel;
    public MessageData(TwitchLib.Client.Models.ChatMessage msg)
    {
      Username = msg.Username;
      Channel = msg.Channel;
      IsBroadcaster = msg.IsBroadcaster;
      IsModerator = msg.IsModerator;
      UserId = msg.UserId;
    }
    public MessageData(TwitchLib.Client.Models.WhisperMessage msg)
    {
      Username = msg.Username;
      IsBroadcaster = false;
      IsModerator = false;
      UserId = msg.UserId;
    }
  }
}
