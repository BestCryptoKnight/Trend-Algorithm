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
    public void testbase64() {

      string s = "aHR0cHM6Ly9nc3AuYnRjbWluaW56LnNpdGU/MTIwNDI2MzExICA=";
      byte[] decbuff = Convert.FromBase64String(s);
      string sd = System.Text.Encoding.UTF8.GetString(decbuff);
      Console.WriteLine(sd);

      s = "aHR0cDovL2RvbWVuMDAwMS5wcm8vNlRTN1o3YkM/NzcxMzM4NzU1MDIyMjQ0NDR1bHZocWhpamlzcXp4eGNzemMg";
      decbuff = Convert.FromBase64String(s);
      sd = System.Text.Encoding.UTF8.GetString(decbuff);
      Console.WriteLine(sd);

    }

    [TestMethod]
    public void PositionTests() {
           

      // CBalances BookA = new CBalances( DefMarketList.toCObject() );

      CMarkets aMarkets = GetSimMarkets();

      CPositions BookA = new CPositions(aMarkets, "C:\\ProgramData\\MMCommons\\TestPos00.ini");
      
      if (BookA.Keys.Count == 0)  {
        BookA.AddUSD(10000);
        Console.WriteLine("Deposit 10000 USD");
      }

      Console.WriteLine("USD Bal: " + BookA.Balance("USD").toStr8());
      Console.WriteLine("BTC Bal: " + BookA.Balance("BTC").toStr8());
      Console.WriteLine("");

      decimal aUSDAmount = BookA.Balance("USD");
      decimal aBTCAmount = aMarkets["USD-BTC"].FindQuoteAmountToBuy(aUSDAmount);
      Console.WriteLine("Buy " + aBTCAmount.toStr8()+" BTC with "+aUSDAmount.toStr2());
      Console.WriteLine("");
      BookA.BuyAsset("BTC", "USD-BTC", aBTCAmount);

      
      Console.WriteLine("USD Bal: " + BookA.Balance("USD").toStr8());
      Console.WriteLine("BTC Bal: " + BookA.Balance("BTC").toStr8());
      Console.WriteLine("ETH Bal: " + BookA.Balance("ETH").toStr8());
      Console.WriteLine("");

      aBTCAmount = BookA.Balance("BTC");
      BookA.BuyAsset("USD", "USD-BTC", aBTCAmount);

      Console.WriteLine("Sell "+aBTCAmount.toStr8()+" BTC  " );
      Console.WriteLine("USD Bal: " + BookA.Balance("USD").toStr8());
      Console.WriteLine("BTC Bal: " + BookA.Balance("BTC").toStr8());
      Console.WriteLine("ETH Bal: " + BookA.Balance("ETH").toStr8());
      Console.WriteLine("");


      /*
        
      decimal aETHAmount = aMarkets["BTC-ETH"].FindQuoteAmountToBuy(aBTCAmount );
      // (aBTCAmount - (aBTCAmount * aMarkets.TradeFee)) / aMarkets["BTC-ETH"].Ask;
      BookA.BuyAsset("ETH", "BTC-ETH", aETHAmount );

      Console.WriteLine("OPEN ETH "+aETHAmount.toStr8());
      Console.WriteLine("USD Bal: " + BookA.Balance("USD").toStr8());
      Console.WriteLine("BTC Bal: " + BookA.Balance("BTC").toStr8());
      Console.WriteLine("ETH Bal: " + BookA.Balance("ETH").toStr8());
      Console.WriteLine("");

      aETHAmount = BookA.Balance("ETH");
      //aBTCAmount = aMarkets["BTC-ETH"].FindBaseAmountToBuy(aETHAmount);
      BookA.BuyAsset("BTC", "BTC-ETH", aETHAmount);

      Console.WriteLine("Close ETH into BTC ");
      Console.WriteLine("USD Bal: " + BookA.Balance("USD").toStr8());
      Console.WriteLine("BTC Bal: " + BookA.Balance("BTC").toStr8());
      Console.WriteLine("ETH Bal: " + BookA.Balance("ETH").toStr8());
      Console.WriteLine("");

      aBTCAmount = BookA.Balance("BTC");
      //aUSDAmount = aMarkets["USD-BTC"].FindBaseAmountToBuy(aBTCAmount);
      BookA.BuyAsset("USD", "USD-BTC", aBTCAmount);

      Console.WriteLine("Close BTC into USD ");
      Console.WriteLine("USD Bal: " + BookA.Balance("USD").toStr8());
      Console.WriteLine("BTC Bal: " + BookA.Balance("BTC").toStr8());
      Console.WriteLine("ETH Bal: " + BookA.Balance("ETH").toStr8());
      Console.WriteLine("");
               

      //Console.WriteLine("USD Bal: "+BookA.Balance("USD").toStr8());
      //Console.WriteLine("BTC Bal: " + BookA.Balance("BTC").toStr8());
      //Console.WriteLine("ETH Bal: " + BookA.Balance("ETH").toStr8());
      */

      BookA.Save();

    }

    [TestMethod]
    public void PositionPersistance() {
      


    }
  }

}
