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
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Users;
using System.Speech.Synthesis;
using System.Net.Sockets;

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
    public static TwitchAPI api;
    static readonly HttpClient client = new HttpClient();
    public static string Get(string uri)
    {
      HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
      request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

      using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
      using (Stream stream = response.GetResponseStream())
      using (StreamReader reader = new StreamReader(stream))
      {
        return reader.ReadToEnd();
      }
    }
    public static string Post(string uri, string data, string contentType, string method = "POST")
    {
      byte[] dataBytes = Encoding.UTF8.GetBytes(data);

      HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
      request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
      request.ContentLength = dataBytes.Length;
      request.ContentType = contentType;
      request.Method = method;

      using (Stream requestBody = request.GetRequestStream())
      {
        requestBody.Write(dataBytes, 0, dataBytes.Length);
      }

      using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
      using (Stream stream = response.GetResponseStream())
      using (StreamReader reader = new StreamReader(stream))
      {
        return reader.ReadToEnd();
      }
    }
    static void Main(string[] args)                                                    
    {
      //get token for helix
      string scopes = "analytics:read:extensions analytics:read:games bits:read channel:edit:commercial channel:manage:broadcast channel:manage:extensions channel:manage:polls channel:manage:predictions channel:manage:redemptions channel:manage:schedule channel:manage:videos channel:read:editors channel:read:goals channel:read:hype_train channel:read:polls channel:read:predictions channel:read:redemptions channel:read:stream_key channel:read:subscriptions clips:edit moderation:read moderator:manage:banned_users moderator:read:blocked_terms moderator:manage:blocked_terms moderator:manage:automod moderator:read:automod_settings moderator:manage:automod_settings moderator:read:chat_settings moderator:manage:chat_settings user:edit user:edit:follows user:manage:blocked_users user:read:blocked_users user:read:broadcast user:read:email user:read:follows user:read:subscriptions";
      string requestUri = "https://id.twitch.tv/oauth2/token?client_id=" + SecretDontOpenOnStreamFile.ClientId() + "&client_secret=" + SecretDontOpenOnStreamFile.ClientSecret() + "&scope=" + scopes + "&grant_type=client_credentials";
      
      string response = Post(requestUri, "{}", "text/html");
      string[] parsed = response.Split("\"");
      string token = parsed[3];
      for (int i = 2; i < parsed.Length; ++i)
      {
        if (parsed[i - 2] == "access_token")
          token = parsed[i];
      }
      //SecretDontOpenOnStreamFile.SetAccessToken(token);

      api = new TwitchAPI();
      api.Settings.ClientId = SecretDontOpenOnStreamFile.ClientId();
      api.Settings.AccessToken = token; // App Secret is not an Accesstoken

      List<string> channelnames = new List<string>();
      channelnames.Add("koshkaxofake");

      //gets the user id from username
      var res = api.Helix.Users.GetUsersAsync(logins: channelnames);
      res.Wait();

      //gets channel id from user id
      var res2 = api.Helix.Channels.GetChannelInformationAsync(res.Result.Users[0].Id);
      res2.Wait();

      api.Helix.Users.GetUsersFollowsAsync(fromId: res.Result.Users[0].Id);

      SoundInterface sounds = new SoundInterface();
      Bot bot = new Bot(sounds);

      //do sound thing
      Thread thread1 = new Thread(sounds.SoundLoop);
      thread1.Start();

      thread1.Join();
    }
  }
}