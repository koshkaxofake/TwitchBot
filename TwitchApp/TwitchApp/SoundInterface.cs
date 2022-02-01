using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace TwitchApp
{
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
    public void AddSoundFront(string sound)
    {
      mut.WaitOne();
      sound_queue.Insert(0,sound);
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
      while (true)
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
}
