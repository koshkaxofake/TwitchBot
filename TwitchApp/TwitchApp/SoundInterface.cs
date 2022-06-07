using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Speech.Synthesis;

namespace TwitchApp
{
  class SoundInterface
  {
    Mutex mut = new Mutex();
    List<string> sound_queue = new List<string>();
    List<string> tts_queue = new List<string>();
    TTS tts = new TTS();
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
    public void AddTTS(string text)
    {
      mut.WaitOne();
      tts_queue.Add(text);
      mut.ReleaseMutex();
    }
    public void AddTTSFront(string text)
    {
      mut.WaitOne();
      tts_queue.Insert(0, text);
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
    string GetNextTTS()
    {
      string next = "";
      mut.WaitOne();
      if (tts_queue.Count > 0)
      {
        next = tts_queue[0];
        tts_queue.Remove(next);
      }
      mut.ReleaseMutex();
      return next;
    }
    void PlayText(string text)
    {
      tts.Speak(text);
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
        else if ((sound = GetNextTTS()) != "")
        {
          PlayText(sound);
        }
        else
        {
          Thread.Sleep(100);
        }
      }
    }
  }
}
