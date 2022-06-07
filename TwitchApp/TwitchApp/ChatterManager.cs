using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TwitchLib.Client.Events;
using System.Threading;

namespace TwitchApp
{
  class ChatterManager
  {

    public delegate void OnFollow(string username);
    public ChatterManager(string file)
    {
      saveFile = file;
      if (File.Exists(saveFile))
      {
        Load();
      }
    }
    ~ChatterManager()
    {
      foreach (var user in activeUsers)
      {
        ChatterLeft(user.Key);
      }
      Save();
    }
    public OnFollow followerEvent;
    public OnFollow unfollowerEvent;
    string saveFile;
    Dictionary<string, Chatter> chatMap = new Dictionary<string, Chatter>();
    Dictionary<string, Chatter> activeUsers = new Dictionary<string, Chatter>();
    Mutex mutex = new Mutex();
    public UInt32 numPoints(string username)
    {
      Chatter chatman;
      if (chatMap.ContainsKey(username) == false)
      {
        return 0;
      }
      else
      {
        chatman = chatMap[username];
      }

      return chatman.points;
    }
    //gives all active users points
    public void GivePoints(UInt32 points)
    {
            mutex.WaitOne();
      foreach (var user in activeUsers)
      {
        user.Value.GetPoints(points);
      }
            mutex.ReleaseMutex();
    }
    //give a specific user points
    public void GivePoints(string username, UInt32 points)
    {
      if (chatMap.ContainsKey(username) == false)
        return;
      chatMap[username].GetPoints(points);
    }
    public bool SpendPoints(string username, UInt32 points)
    {
      Chatter chatman;
      if (chatMap.ContainsKey(username) == false)
      {
        return false;
      }
      else
      {
        chatman = chatMap[username];
      }

      return chatman.SpendPoints(points);
    }
    public void ChatterJoined(string username)
    {
      Chatter chatman;
      if (chatMap.ContainsKey(username) == false)
      {
        chatman = new Chatter();
        chatman.username = username;
        //get the chatters id
        List<string> names = new List<string>();
        names.Add(username);
        var res = Program.api.Helix.Users.GetUsersAsync(logins: names);
        res.Wait();
        chatman.userId = res.Result.Users[0].Id;

        mutex.WaitOne();
        chatMap.Add(username, chatman);
        mutex.ReleaseMutex();
        //give new chatters some free starting points
        chatman.GetPoints(10000);
      }
      else
      {
        chatman = chatMap[username];
      }
      if (activeUsers.ContainsKey(username))
      {
        return;
      }
      mutex.WaitOne();
      activeUsers.Add(chatman.username, chatman);
      mutex.ReleaseMutex();

      //set joined time
      chatman.timeJoined = DateTime.UtcNow;
    }
    public void ChatterLeft(string username)
    {
      Chatter chatman;
      if (chatMap.ContainsKey(username) == false)
      {
        return;
      }
      else
      {
        chatman = chatMap[username];
      }

      //remove from active users
      mutex.WaitOne();
      activeUsers.Remove(username);
      mutex.ReleaseMutex();

      //update time spent
      chatman.timeChatting += (UInt32)(DateTime.UtcNow - chatman.timeJoined).TotalSeconds;
    }
    public void OnMessage(string username, MessageData e)
    {
      Chatter chatman;
      if (chatMap.ContainsKey(username) == false)
      {
        return;
      }
      else
      {
        chatman = chatMap[username];
      }
      chatman.messageNum += 1;
      chatman.userId = e.UserId;
    }
    //does web request dont call very often
    public void CheckForFollowers()
    {
      mutex.WaitOne();
      foreach (var chatpair in chatMap)
      {
        var res = Program.api.Helix.Users.GetUsersFollowsAsync(fromId: chatpair.Value.userId);
        res.Wait();
        bool doesFollow = false;
        foreach (var follow in res.Result.Follows)
        {
          if (follow.ToUserName == "KoshkaXOFake")
          {
            doesFollow = true;
            break;
          }
        }
        if ((chatpair.Value.isFollower==1) != doesFollow)
        {
          if (doesFollow)
          {
            followerEvent(chatpair.Key);
          }
          else
          {
            unfollowerEvent(chatpair.Key);
          }
          chatpair.Value.UpdateFollow(doesFollow);
        }
      }
      mutex.ReleaseMutex();
    }
    public void Load()
    {
      using (StreamReader reader = new StreamReader(saveFile))
      {
        while (reader.EndOfStream == false)
        {
          Chatter chatman = new Chatter(reader.ReadLine());
          chatMap.Add(chatman.username, chatman);
        }
      }
    }
    public void Save()
    {
      using (StreamWriter writer = new StreamWriter(saveFile))
      {
        foreach (var user in chatMap)
        {
          writer.WriteLine(user.Value.save());
        }
      }
    }
  }
}
