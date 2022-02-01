using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchApp
{
  class Bet
  {
    public bool BetsOpen = false;
    public List<String> options = new List<string>();
    //key is username of better, value is a pair of first option second the amount wagered
    public Dictionary<string, KeyValuePair<uint, uint>> wagers = new System.Collections.Generic.Dictionary<string, KeyValuePair<uint, uint>>();
  }
}
