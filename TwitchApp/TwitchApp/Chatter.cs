using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchApp
{
  class Chatter
  {
    //chatters username
    public string username;
    //the user id
    public string userId;
    //all the points they ever got
    public UInt32 totalPoints;
    //currency in chat
    public UInt32 points;
    //time spent whatching stream in seconds
    public UInt32 timeChatting;
    //number of messages sent
    public UInt32 messageNum;
    //does the chatter follow my channel
    public UInt32 isFollower;

    // Temp data

    //time chatter joined recent session
    public DateTime timeJoined;
    public Chatter()
    {

    }
    public void GetPoints(UInt32 new_points)
    {
      totalPoints += new_points;
      points += new_points;
    }
    public bool SpendPoints(UInt32 new_points)
    {
      if (points >= new_points)
      {
        points -= new_points;
        return true;
      }
      return false;
    }
    public void UpdateFollow(bool doesFollow)
    {
      if (doesFollow)
      {
        isFollower = 1;
      }
      else
      {
        isFollower = 0;
      }
    }
    public Chatter(string data)
    {
      load(data);
    }
    void load(string data)
    {
      string[] args = data.Split(",");
      int index = 0;
      username = args[index++];
      if (args.Length < 7)
        return;
      userId= args[index++];
      if (!UInt32.TryParse(args[index++], out totalPoints))
      {
        //error
        Console.WriteLine("oh no");
      }
      if (!UInt32.TryParse(args[index++], out points))
      {
        //error
        Console.WriteLine("oh no");
      }
      if (!UInt32.TryParse(args[index++], out timeChatting))
      {
        //error
        Console.WriteLine("oh no");
      }
      if (!UInt32.TryParse(args[index++], out messageNum))
      {
        //error
        Console.WriteLine("oh no");
      }
      if (!UInt32.TryParse(args[index++], out isFollower))
      {
        //error
        Console.WriteLine("oh no");
      }

      //in case there is no user id?
      if (userId == "")
      {
        //get the chatters id
        List<string> names = new List<string>();
        names.Add(username);
        var res = Program.api.Helix.Users.GetUsersAsync(logins: names);
        res.Wait();
        userId = res.Result.Users[0].Id;
      }
    }
    public string save()
    {
      return username + "," + userId + "," + totalPoints.ToString() + "," + points.ToString() + "," + timeChatting.ToString() + "," + messageNum.ToString() + "," + isFollower.ToString();
    }
  }
}
