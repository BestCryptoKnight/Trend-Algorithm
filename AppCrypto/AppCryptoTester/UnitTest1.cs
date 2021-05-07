using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;
using System.IO;
using AppCrypto;
using StaticExtentions;

namespace AppCryptoTester {
  [TestClass]
  public class TestSecureStore {
    public string CallSign = "BittrexTrader";
    public string SettingsKey = "BittrexKP";
    [TestMethod]
    public void LoadTest() {
      string TestPsw = "r2d2";

      string SettingsFilePath = DllExt.MMConLocation();
      if (!Directory.Exists(SettingsFilePath)) Directory.CreateDirectory(SettingsFilePath);
      string SettingsFileName = SettingsFilePath + "\\" + CallSign + "Settings.ini";

      Console.WriteLine(SettingsFileName);

      if (File.Exists(SettingsFileName)) {
        SecureStore Settings = new SecureStore(TestPsw, SettingsFileName);
        string kpSettings;
        try { 
          kpSettings = Settings[SettingsKey];
          string sPub = kpSettings.ParseString(" ", 0);
          string sPri = kpSettings.ParseString(" ", 1);
        } catch (Exception ee) {
          throw new Exception("Password Failed");          
        }
      }
    }

    [TestMethod]
    public void TestColors() {
      Color a = Color.Red;
      Color b = Color.Blue;
             
      Color[] aOut = DllExt.GetColors(a,b, 1);
      foreach(Color c in aOut) { 
        Console.WriteLine( c.ToString());
      }
      
    }

  }

}
