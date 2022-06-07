using System;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using System.Threading;
using OpenQA.Selenium.Support.UI;

namespace TwitchApp
{
  class TTS
  {
    IWebDriver driver;
    public TTS()
    {
      new DriverManager().SetUpDriver(new ChromeConfig());
      driver = new ChromeDriver();
      driver.Navigate().GoToUrl("https://azure.microsoft.com/en-us/services/cognitive-services/text-to-speech/#features");
    }
    public void Speak(string txt)
    {
      Random rand = new Random();

      driver.Navigate().Refresh();
      ///Todo change text box
      var textBox = driver.FindElement(By.Id("ttstext"));
      var script = "arguments[0].scrollIntoView(true);";
      IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
      js.ExecuteScript(script, textBox);
      while (!textBox.GetAttribute("value").Contains("SSML"))
      {
      }
      bool textChanged = false;
      while (!textChanged)
      {
        try
        {
          textBox.Clear();
          textBox.SendKeys(Keys.Tab);
          textBox.SendKeys(txt);
          textChanged = true;
        }
        catch
        {

        }
      }

      ///change voice selection
      var voice = driver.FindElement(By.Id("voiceselect"));
      js.ExecuteScript(script, voice);
      var selectElement = new SelectElement(voice);
      string[] girlname = new string[] { "Jenny", "Amber", "Ana", "Aria", "Ashley", "Cora", "Elizabeth", "Michelle", "Monica", "Sara" };
      selectElement.SelectByText(girlname[rand.Next(girlname.Length)], true);

      ///change speaking style selection
      var style = driver.FindElement(By.Id("voicestyleselect"));
      js.ExecuteScript(script, style);
      selectElement = new SelectElement(style);
      selectElement.SelectByIndex(rand.Next(selectElement.Options.Count));

      //change pitch
      var pitch = driver.FindElement(By.Id("pitch"));
      js.ExecuteScript(script, pitch);
      double amount = 0;
      for (int i = 0; i < 25; ++i)
        amount += (rand.NextDouble() - 0.5) * 4;
      bool increase = amount > 0;
      for (int i = 0; i < amount; ++i)
      {
        if (increase)
        {
          pitch.SendKeys(Keys.ArrowUp);
        }
        else
        {
          pitch.SendKeys(Keys.ArrowDown);
        }
      }
      //change speed
      var speed = driver.FindElement(By.Id("speed"));
      js.ExecuteScript(script, speed);
      amount = 0;
      for (int i = 0; i < 25; ++i)
        amount += rand.NextDouble() - 0.5;
      increase = amount > 0;
      for (int i = 0; i < amount; ++i)
      {
        if (increase)
        {
          speed.SendKeys(Keys.ArrowUp);
        }
        else
        {
          speed.SendKeys(Keys.ArrowDown);
        }
      }
      var searchButton = driver.FindElement(By.Id("playbtn"));
      js.ExecuteScript(script, searchButton);
      searchButton.SendKeys(Keys.Enter);

      //wait until the tts has stopped
      while (!searchButton.Displayed)
      {
      }

    }
  }
}
