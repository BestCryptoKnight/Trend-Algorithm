using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;
using System.IO;
using AppCrypto;
using StaticExtensions;

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
      //  SecureStore Settings = new SecureStore(TestPsw, SettingsFileName);
      //  string kpSettings;
      //  try { 
      //    kpSettings = Settings[SettingsKey];
      //    string sPub = kpSettings.ParseString(" ", 0);
      //    string sPri = kpSettings.ParseString(" ", 1);
      //  } catch (Exception ee) {
      //    throw new Exception("Password Failed");          
      //  }
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

    private CMarkets GetSimMarkets() {
      string[] DefMarketList = {
        "USD-BTC", "USD-ETH", "BTC-ETH"
//        "USD-ADA", "BTC-ADA", "ETH-ADA",
//        "USD-LINK", "BTC-LINK", "ETH-LINK"
       };      
      CMarkets r = new CMarkets(DefMarketList.toCObject());
      r["USD-BTC"].Ask = 47000;
      r["USD-BTC"].Bid = 46969;
      r["USD-ETH"].Ask = 3533; 
      r["USD-ETH"].Bid = 3532;
      r["BTC-ETH"].Ask = 0.0752m; 
      r["BTC-ETH"].Bid = 0.0744m;
     
      return r;
    }

    [TestMethod]
    public void PositionTests() {

      

      // CBalances BookA = new CBalances( DefMarketList.toCObject() );

      CMarkets aMarkets = GetSimMarkets();

      CPositions BookA = new CPositions(aMarkets);
      decimal aUSDAmount = 10000;
      BookA.AddUSD(aUSDAmount);
      Console.WriteLine("Deposit 10000 USD");
      Console.WriteLine("USD Bal: " + BookA.Balance("USD").toStr8());
      Console.WriteLine("BTC Bal: " + BookA.Balance("BTC").toStr8());
      Console.WriteLine("ETH Bal: " + BookA.Balance("ETH").toStr8());
      Console.WriteLine("");

      decimal aBTCAmount = aMarkets["USD-BTC"].FindQuoteAmountToBuy(10000);
//      Console.WriteLine("Get Max BTC Buy:" + aBTCAmount);
//      Console.WriteLine("");


      BookA.BuyAsset("BTC", "USD-BTC", aBTCAmount);
      Console.WriteLine("OPEN BTC position");
      Console.WriteLine("USD Bal: " + BookA.Balance("USD").toStr8());
      Console.WriteLine("BTC Bal: " + BookA.Balance("BTC").toStr8());
      Console.WriteLine("ETH Bal: " + BookA.Balance("ETH").toStr8());
      Console.WriteLine("");

      decimal aETHAmount = aMarkets["BTC-ETH"].FindQuoteAmountToBuy(aBTCAmount );
      // (aBTCAmount - (aBTCAmount * aMarkets.TradeFee)) / aMarkets["BTC-ETH"].Ask;
      BookA.BuyAsset("ETH", "BTC-ETH", aETHAmount );

      Console.WriteLine("OPEN ETH ");
      Console.WriteLine("USD Bal: " + BookA.Balance("USD").toStr8());
      Console.WriteLine("BTC Bal: " + BookA.Balance("BTC").toStr8());
      Console.WriteLine("ETH Bal: " + BookA.Balance("ETH").toStr8());
      Console.WriteLine("");

      aETHAmount = BookA.Balance("ETH");
      aBTCAmount = aMarkets["BTC-ETH"].FindBaseAmountToBuy(aETHAmount);
      BookA.BuyAsset("BTC", "BTC-ETH", aBTCAmount);

      Console.WriteLine("Close ETH into BTC ");
      Console.WriteLine("USD Bal: " + BookA.Balance("USD").toStr8());
      Console.WriteLine("BTC Bal: " + BookA.Balance("BTC").toStr8());
      Console.WriteLine("ETH Bal: " + BookA.Balance("ETH").toStr8());
      Console.WriteLine("");

      aBTCAmount = BookA.Balance("BTC");
      aUSDAmount = aMarkets["USD-BTC"].FindBaseAmountToBuy(aBTCAmount);
      BookA.BuyAsset("USD", "USD-BTC", aUSDAmount);

      Console.WriteLine("Close BTC into USD ");
      Console.WriteLine("USD Bal: " + BookA.Balance("USD").toStr8());
      Console.WriteLine("BTC Bal: " + BookA.Balance("BTC").toStr8());
      Console.WriteLine("ETH Bal: " + BookA.Balance("ETH").toStr8());
      Console.WriteLine("");

          

      //Console.WriteLine("USD Bal: "+BookA.Balance("USD").toStr8());
      //Console.WriteLine("BTC Bal: " + BookA.Balance("BTC").toStr8());
      //Console.WriteLine("ETH Bal: " + BookA.Balance("ETH").toStr8());
      



    }
  }

}
