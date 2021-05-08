using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StaticExtensions;
using AppCrypto;
using Bittrex.Net;
using Bittrex.Net.Objects;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Logging;


namespace OracleAlpha
{
  public partial class Form1 : Form {
    public string CallSign = "BittrexTrader";
    public string SettingsKey = "BittrexKP";
    public string[] DefBaseCoins = {"USDT", "USD", "BTC", "ETH" };
    public CObject BaseCoins;
  //  public string[] MarketFilter = { "USD-BTC", "USD-ADA", "BTC-ADA" };
    public string[] DefMarketList = {
      "USD-BTC", "USD-ETH", "BTC-ETH", 
      "USD-ADA", "BTC-ADA", "ETH-ADA",
 //     "USD-LINK", "BTC-LINK", "ETH-LINK",
   //   "USD-DGB", "BTC-DGB", "ETH-DGB"
     };

    public CObject MarketFilter; //= { "USD-BTC"
    
    public BittrexSocketClient BSC;
    public SecureStore Settings;

    public CMarkets Markets;
    public CBalances Balances;
    public CTickerQueue TickersLanding;
    public string LastTicSeq = "";
    public Int32 iDisplayMode = 0;
    public float fWidth = 0, fHeight = 0;
    public double f20Height = 0.2;
    public double f05Height = 0.065;
    public double f15Height = 0.145;
    public double f20Width = 0.2;
    public double f15Width = 0.15;
    public double f05Width = 0.05;

    public Font fCur10; Font fCur9; Font fCur8; Font fCur7; Font fCur6;
    public Color ColorDefBack;

    string SettingsFilePath;
    string SettingsFileName;

    decimal TradeFee = 0.0025m;     // trade fee as a percent 
    decimal TradeFeeStopM = 1.469m;   // stop price to adjust how far down to stop release.
    decimal TradeFeeExitM = 1.469m;  // min exit price as  

    String LastCur = "";            // values to simulate holding an amount of currency.
    Decimal LastAmount = 0;
    Decimal LastPrice = 0;
    Decimal StopPrice = 0;
    Decimal ExitMin = 0;

    DataSet dd;

    public Form1() {
      InitializeComponent();
    }
    
    private void Form1_Load(object sender, EventArgs e) {

      fCur10 = new Font("Courier New", 10); fCur9 = new Font("Courier New", 9); fCur8 = new Font("Courier New", 8);
      fCur7 = new Font("Courier New", 7); fCur6 = new Font("Courier New", 6);
      ColorDefBack = ColorTranslator.FromHtml("#08180F");

      ServicePointManager.Expect100Continue = true;
      ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

      BaseCoins = DefBaseCoins.toCObject();
      MarketFilter = DefMarketList.toCObject();

      Markets = new CMarkets(MarketFilter);
      Balances = new CBalances(Markets);
      TickersLanding = new CTickerQueue();

      SettingsFilePath = DllExt.MMConLocation();
      if (!Directory.Exists(SettingsFilePath)) Directory.CreateDirectory(SettingsFilePath);
      SettingsFileName = SettingsFilePath + "\\"+ CallSign + "Settings.ini";

      if (!File.Exists(SettingsFileName))
      { // need api keys. 
        iDisplayMode = 10;
      }
      else
      {  // need password to unlock keys. 
        iDisplayMode = 20;
   
      }
             

    }
    #region toHide
    private async void Form1_FormClosing(object sender, FormClosingEventArgs e) {
      SaveEditors();
      if ((!FirstTimeLoad)&&(BSC is BittrexSocketClient)) {
        await BSC.UnsubscribeAll();
      }
    }


    private void Form1_Shown(object sender, EventArgs e){

      DoUpdateControlVisibility();
    }
    
    delegate void SetLogMessageCallback(string message);
    private void setLogMessage(string message)
    {
      if (this.edOut.InvokeRequired)
      {
        SetLogMessageCallback d = new SetLogMessageCallback(setLogMessage);
        this.Invoke(d, new object[] { message });
      }
      else
      {
        this.edOut.Text = message + Environment.NewLine + this.edOut.Text;
      }
    }

    private void setTradeMsg(string message) {      
      if (this.edTradeHist.InvokeRequired) {
        SetLogMessageCallback d = new SetLogMessageCallback(setLogMessage);
        this.Invoke(d, new object[] { message });
      } else {
        this.edTradeHist.Text = message + Environment.NewLine + this.edTradeHist.Text;
      }
      
    }

    delegate void UpdateControlVisibilityCallback();

    private Boolean LoadingEditors = false;
    private void LoadEditors() {
      LoadingEditors = true;
      if (Settings is SecureStore) {
        LastAmount = ( Settings["LastAmount"]=="" ? 10000 : Settings["LastAmount"].toDecimal());
        LastCur = (Settings["LastCur"] == "" ? "USD" : Settings["LastCur"].toString());
        LastPrice = (Settings["LastPrice"] == "" ? 1 : Settings["LastPrice"].toDecimal());
        edLastPrice.Value = LastPrice;
        cbStartCur.SelectedItem = LastCur;
        edStarting.Value = LastAmount;        
      }
      LoadingEditors = false;
    }
    private void SaveEditors() {
      if (Settings is SecureStore) {
        Settings["LastAmount"] = LastAmount.toStr8();
        Settings["LastCur"] = LastCur;
        Settings["LastPrice"] = LastPrice.toStr8();
      }
    }
    #endregion
    private void DoUpdateControlVisibility() {
      if (this.InvokeRequired) {
        UpdateControlVisibilityCallback d = new UpdateControlVisibilityCallback(DoUpdateControlVisibility);
        this.Invoke(d, new object[] { });
      } else {
        if (this.Visible) {

          Form1_ResizeEnd(null, null);

          if ((iDisplayMode == 0) || (iDisplayMode == 86) || (iDisplayMode == 30)) {

            cbTrack.Left = (fWidth * 0.015).toInt32();
            cbTrack.Top = (fHeight * 0.055).toInt32();
            edStarting.Top = cbTrack.Top;
            cbStartCur.Top = cbTrack.Top;
            edLastPrice.Top = cbTrack.Top;
            edStarting.Left = cbTrack.Width + cbTrack.Left + 2;
            cbStartCur.Left = edStarting.Left + edStarting.Width + 2;
            edLastPrice.Left = cbStartCur.Left + cbStartCur.Width + 2;
            btnExit.Left  = edLastPrice.Left + edLastPrice.Width + 2;
            btnExit.Top = cbTrack.Top;
            if (label1.Visible) label1.Visible = false;
            if (label2.Visible) label2.Visible = false;
            if (label3.Visible) label3.Visible = false;
            if (textBox1.Visible) textBox1.Visible = false;
            if (textBox2.Visible) textBox2.Visible = false;
            if (textBox3.Visible) textBox3.Visible = false;
            if (btnContinue.Visible) btnContinue.Visible = false;
           // if (!edOut.Visible) edOut.Visible = true;
            if (!edTradeHist.Visible) edTradeHist.Visible = true;
            if (!cbTrack.Visible) cbTrack.Visible = true;
            if (cbTrack.Checked) {
              edStarting.Visible = false;
              cbStartCur.Visible = false;
              edLastPrice.Visible = false;
            } else {
              edStarting.Visible = true;
              cbStartCur.Visible = true;
              edLastPrice.Visible = true;
            }
            if ((cbTrack.Checked) && (Markets.Coins.CurUpCoin != LastCur)) {
              if (!btnExit.Visible) btnExit.Visible = true;
            } else {
              if (btnExit.Visible) btnExit.Visible = false;
            }
          }

          if (iDisplayMode == 10){
            if (!label1.Visible) label1.Visible = true;
            if (!label2.Visible) label2.Visible = true;
            if (!label3.Visible) label3.Visible = true;
            if (!textBox1.Visible) textBox1.Visible = true;
            if (!textBox2.Visible) textBox2.Visible = true;
            if (!textBox3.Visible) textBox3.Visible = true;
            if (!btnContinue.Visible) btnContinue.Visible = true;
            if (edOut.Visible) edOut.Visible = false;
            if (edTradeHist.Visible) edTradeHist.Visible = false;
            if (edStarting.Visible) edStarting.Visible = false;
            if (cbTrack.Visible) cbTrack.Visible = false;
            if (cbStartCur.Visible) cbStartCur.Visible = false;
            if (edLastPrice.Visible) edLastPrice.Visible = false;
            if (btnExit.Visible) btnExit.Visible = false;
          }

          if (iDisplayMode == 20) {
            if (!label1.Visible) label1.Visible = true;
            if (label2.Visible) label2.Visible = false;
            if (label3.Visible) label3.Visible = false;
            if (!textBox1.Visible) textBox1.Visible = true;
            if (textBox2.Visible) textBox2.Visible = false;
            if (textBox3.Visible) textBox3.Visible = false;
            if (!btnContinue.Visible) btnContinue.Visible = true;
            if (edOut.Visible) edOut.Visible = false;
            if (edTradeHist.Visible) edTradeHist.Visible = false;
            if (edStarting.Visible) edStarting.Visible = false;
            if (cbTrack.Visible) cbTrack.Visible = false;
            if (cbStartCur.Visible) cbStartCur.Visible = false;
            if (edLastPrice.Visible) edLastPrice.Visible = false;
            if (btnExit.Visible) btnExit.Visible = false;
          }


        }
      }
    }
       
    private void DoRedraw() { 
      
      DoUpdateControlVisibility();
     
      Graphics g = this.CreateGraphics();
      try {
        string es = "0";
        BufferedGraphics bg = BufferedGraphicsManager.Current.Allocate(g, this.DisplayRectangle);
        try {
          Int32 iP = 14;
          Int32 iW = 6;
          SizeF OneChar = bg.Graphics.MeasureString("W00W", fCur9);
          Single iCW = OneChar.Width;
          Single iMCW = fWidth / iCW;
        
          Single iRow = Convert.ToSingle(fHeight * 0.12);
          Single iRowLastCur = iRow;
          Single iLeft = Convert.ToSingle(fWidth * 0.05);
          Single AreaMaxHeight = Convert.ToSingle(edTradeHist.Top - iRow-1);
          Single iRowH = AreaMaxHeight / 30;
          
          string sRow = "";
          string ThisUp = "";
          decimal ThisAvgPriceUSD = 0;
          decimal ThisAvgCount = 0;
          decimal grLeft = Convert.ToDecimal( iCW * 13.6 + fWidth * 0.015);
          decimal grTop = Convert.ToDecimal( iRow + (iRowH * 1.5));
          decimal grWidth = Convert.ToDecimal(fWidth.toDecimal() * 0.995m - (grLeft));
          decimal grHeight = Convert.ToDecimal(iRowH * 18.25);

          decimal grTopT = Convert.ToDecimal(iRow + (iRowH * 0.5));
          decimal grHeightT = Convert.ToDecimal(iRowH);

          decimal PriceChangeMax = 0;
          decimal PriceChangeMin = 0;

          // chart outline
          bg.Graphics.DrawRectangle(Pens.WhiteSmoke,
            new Rectangle(
              Convert.ToInt32( grLeft),
              Convert.ToInt32( grTop ),
              Convert.ToInt32( grWidth ),
              Convert.ToInt32( grHeight )
            )
          );

          bg.Graphics.DrawRectangle(Pens.WhiteSmoke,
            new Rectangle(
              Convert.ToInt32(grLeft),
              Convert.ToInt32(grTopT),
              Convert.ToInt32(grWidth),
              Convert.ToInt32(grHeightT)
            )
          );

          foreach (string sCoin in Markets.Coins.ByAvgChange()) {
            
            iRow = Convert.ToSingle(iRow + iRowH);
            if (LastCur == sCoin) {
              iRowLastCur = iRow;
              var coincount = Markets.Coins.Keys.Count;
              Pen aG = new Pen(Markets.Coins[sCoin].CoinColor, 1);
              bg.Graphics.DrawRectangle(aG,    //  Selected Coin Rec
                new Rectangle(
                  (fWidth * 0.015).toInt32(), 
                  (iRow - (iRowH * 0.5)).toDecimal().toInt32(),
                  Convert.ToInt32(iCW * 13.5),
                  (iRowH * (coincount+2)).toInt32()
                )
              );
            }
            Decimal dAvgChange = Markets.Coins[sCoin].AvgChange;
            
            Brush sbaa = (System.Math.Abs(dAvgChange) < 0.0005m ? Brushes.WhiteSmoke : ((dAvgChange < 0) ? Brushes.Red : Brushes.Chartreuse));
            sRow = "    %" + (100 * dAvgChange).toStr4();
            Color aCC = Markets.Coins[sCoin].CoinColor;           
            Brush abmm = new SolidBrush(aCC);

            // draw Coin info
            bg.Graphics.DrawString(sRow, fCur8, sbaa, Convert.ToSingle(iLeft), Convert.ToSingle(iRow));
            bg.Graphics.DrawString(sCoin, fCur8, abmm, Convert.ToSingle(iLeft), Convert.ToSingle(iRow));
            bg.Graphics.DrawString(Markets.Coins[sCoin].UpdateCount.toInt32T().toString(),
                fCur7, Brushes.WhiteSmoke, Convert.ToSingle(iLeft - (iCW / 2)), Convert.ToSingle(iRow));

            iRow = Convert.ToSingle(iRow + iRowH);
            if (iRow > edTradeHist.Top) break;

            // draw Market data limit top 4
            Int32 iMarketCount = 0;
            foreach (string sMarket in Markets.Coins[sCoin].KeysByPriceDelta()) {
              iMarketCount++;
              if (iMarketCount > 4) break;
              string sBaseCur = sMarket.ParseFirst("-");
              string sBaseM = " " + (sCoin == sBaseCur ? sMarket.ParseLast("-") : sBaseCur) + " ";
              decimal dAskDelta = Markets.Coins[sCoin][sMarket].AskDelta;
              decimal dBidDelta = Markets.Coins[sCoin][sMarket].BidDelta;
              CAvgDecimalCache dPC = Markets.Coins[sCoin][sMarket].PriceDeltaCache;
              decimal tv = dPC.SumMax;
              if (tv > PriceChangeMax) { PriceChangeMax = tv; }
              tv = dPC.SumMin;
              if (tv < PriceChangeMin) { PriceChangeMin = tv; }
              Color aC = Markets.Coins[sCoin].CoinColor;
              if (sCoin == sMarket.ParseFirst("-")) {
                aC = Markets.Coins[sCoin].CoinColor2;
              }
              Brush abm = new SolidBrush(aC);
              Brush sba = (System.Math.Abs(dAskDelta) < 0.0005m ? Brushes.WhiteSmoke : ((dAskDelta < 0) ? Brushes.Red : Brushes.Chartreuse));
              Brush sbb = (System.Math.Abs(dBidDelta) < 0.0005m ? Brushes.WhiteSmoke : ((dBidDelta < 0) ? Brushes.Red : Brushes.Chartreuse));
              sRow = sMarket;

              string sPrice = " " + Markets.Coins[sCoin][sMarket].AvgPrice.toStr8P(iP) +
                sBaseM + (100 * ((dAskDelta + dBidDelta)/2)).toStr2() + "%" + (((dAskDelta + dBidDelta) / 2) < 0 ? "↓" : "↑");
              string sAsks =  " " + Markets.Coins[sCoin][sMarket].Ask.toStr8P(iP) + 
                sBaseM + (100 * dAskDelta).toStr2() + "%" +(dAskDelta < 0 ? "↓" : "↑"); 
              string sBids =  " " + Markets.Coins[sCoin][sMarket].Bid.toStr8P(iP) + 
                sBaseM + (100 * dBidDelta).toStr2() + "%" + (dBidDelta < 0 ? "↓" : "↑");
              bg.Graphics.DrawString( Markets.Coins[sCoin][sMarket].UpdateCount.toInt32T().toString(), 
                fCur7, abm, Convert.ToSingle(iLeft - (iCW/2)), Convert.ToSingle(iRow));
              bg.Graphics.DrawString(sMarket, fCur7, abm, Convert.ToSingle(iLeft), Convert.ToSingle(iRow));
              bg.Graphics.DrawString(sPrice, fCur7, sba, Convert.ToSingle(iLeft+(iCW * 1.25)), Convert.ToSingle(iRow));
              //bg.Graphics.DrawString(sBids, fCur7, sbb, Convert.ToSingle(iLeft+(iCW * 6.5)), Convert.ToSingle(iRow));

              iRow = Convert.ToSingle(iRow + iRowH);
              if (iRow > edTradeHist.Top) break;
              if (sCoin == LastCur) {
                ThisAvgPriceUSD = ThisAvgPriceUSD + Markets.ToUSD((sCoin == sBaseCur ? sMarket.ParseLast("-") : sMarket.ParseFirst("-")), Markets.Coins[sCoin][sMarket].Ask);
                ThisAvgCount++;
              }
            }
            if (iRow > edTradeHist.Top) break;
          }
      //    ThisAvgPriceUSD = ThisAvgPriceUSD / ThisAvgCount;
          Int32 iMR = 0;
          // draw the graph
          #region graph code
          /*
          Int32 iCurCoin = 0;
          Int32 iCoinCount = Markets.Coins.Keys.Count;
          foreach (string sCoin in Markets.Coins.ByAvgChange()) {
            iCurCoin =+ 1;
            if (iCurCoin > 3) break;
            Int32 iMarketMax = 0;
            foreach (string sMarket in Markets.Coins[sCoin].KeysByPriceDelta()) {
              iMR++;
              iMarketMax++; if (iMarketMax > 4) break;
              CAvgDecimalCache aPDC = Markets.Coins[sCoin][sMarket].PriceDeltaCache;
              CAvgDecimalCache aUCC = Markets.Coins[sCoin][sMarket].UpdateCountCache;
              Color aC = Markets.Coins[sCoin].CoinColor;
              if (sCoin == sMarket.ParseFirst("-")) {
                aC = Markets.Coins[sCoin].CoinColor2;
              }
              Pen aP = new Pen(aC, 1);
              Pen aG = new Pen(ColorTranslator.FromHtml("#101010"), 1);
              decimal dChange = 0;
              decimal dValue = 0;
              
              decimal PriceChangeRange = (PriceChangeMax * 1.05m) - (PriceChangeMin * 1.05m);
              decimal HeightRatio =  grHeight/ PriceChangeRange;
              if (iCurCoin == 1) { 
                bg.Graphics.DrawLine(aG, grLeft.toFloat(), (grTop+ grHeight/2).toFloat(), (grLeft+grWidth).toFloat(), (grTop + grHeight / 2).toFloat());
                bg.Graphics.DrawLine(aG, grLeft.toFloat(), (grTop + grHeight / 2 + (PriceChangeMax * HeightRatio)).toFloat(), (grLeft + grWidth).toFloat(), (grTop + grHeight / 2 + (PriceChangeMax * HeightRatio)).toFloat());
                bg.Graphics.DrawLine(aG, grLeft.toFloat(), (grTop + grHeight / 2 + (PriceChangeMin * HeightRatio)).toFloat(), (grLeft + grWidth).toFloat(), (grTop + grHeight / 2 + (PriceChangeMin * HeightRatio)).toFloat());
                bg.Graphics.DrawString(PriceChangeMax.toStr4(), fCur6, Brushes.LightGray, grLeft.toFloat(), (grTop + grHeight / 2 + (PriceChangeMax * HeightRatio)).toFloat());
                bg.Graphics.DrawString(PriceChangeMin.toStr4(), fCur6, Brushes.LightGray, grLeft.toFloat(), (grTop + grHeight / 2 + (PriceChangeMin * HeightRatio)).toFloat());
              }
              Int32 iSpot = 0;
              List<Point> lp = new List<Point>();
              Int32 PDCount = aPDC.Keys.Count-1;
              decimal dHR2 = grHeightT / 40;
              foreach(Int64 key in aUCC.Keys.OrderBy(x => x)) {
                dChange = (decimal)aUCC[key];
                Int32 x = Convert.ToInt32(grLeft + ((grWidth / 24.25m) * (iSpot + 0.125m))+(iMR*2));
                Int32 y = Convert.ToInt32(grTop - (dChange * dHR2));

                bg.Graphics.DrawLine(aP, x, grTop.toInt32()-1, x, y-1);
                bg.Graphics.DrawLine(aP, x+1, grTop.toInt32()-1, x+1, y-1);

                iSpot += 1;
              }

              iSpot = 0;
              foreach (Int64 key in aPDC.Keys.OrderBy(x => x)) {
                if (iSpot > 23) break;
                
                dChange = (decimal)aPDC[key];
                dValue += dChange;
                
                Int32 x = Convert.ToInt32( grLeft + ((grWidth / 24.25m) * (iSpot+0.125m)));
                Int32 y = Convert.ToInt32( (grTop + (grHeight/2)) + ( dValue * HeightRatio ));
                lp.Add(new Point(x, y));
                bg.Graphics.DrawLine(Pens.WhiteSmoke, x, grTop.toInt32(), x, (grTop+4).toInt32());

                if ((iSpot>2)&&(iSpot == PDCount-2)) {
                  bg.Graphics.DrawString(sMarket, fCur6, Brushes.LightGray, x, y);
                }
                iSpot++;
              }
              bg.Graphics.DrawLines(aP, lp.ToArray());

             
              
            }
          }
          */
          #endregion

          bg.Graphics.DrawString(DateTime.Now.ToStrDateMM() + 
      //     " T-" + NextGoTime.Value.Subtract(DateTime.Now).TotalSeconds.toInt32().ToString() +
           " T-" + NextUpdateAvg.Value.Subtract(DateTime.Now).TotalSeconds.toInt32().ToString() +
           " s:" +
           LastTicSeq +" "+
            ThisUp + " ", fCur8, Brushes.WhiteSmoke, Convert.ToSingle(fWidth * 0.015), Convert.ToSingle(fHeight * 0.015));

        //  if ((LastCur != "") &&(cbTrack.Checked)) {
        //    string standing = "" + LastAmount.toStr8() + " " + LastCur + ( LastCur !="USD"? "  at " + LastPrice.toStr8() :"") +
        //      "  $" + Markets.ToUSD(LastCur, LastAmount).toStr2();
        //    bg.Graphics.DrawString(standing, fCur8, Brushes.WhiteSmoke, Convert.ToSingle(fWidth * 0.015 + cbTrack.Width), Convert.ToSingle(fHeight * 0.06));
        //    if(LastCur != "USD") { 
        //      standing = "               Avg $" + ThisAvgPriceUSD.toStr4() + "  Stop $" + StopPrice.toStr4() + "  Exit $" + ExitMin.toStr4();
        //      bg.Graphics.DrawString(standing, fCur8, Brushes.WhiteSmoke, Convert.ToSingle(iLeft), iRowLastCur);
        //    }
        //  }

          bg.Render(g);
        } catch (Exception e) {          
          e.toAppLog("Refresh " + es);
        } finally {
          bg.Dispose();
        }
      } finally {
        g.Dispose();
      }
      
    }
    private void btnContinue_Click(object sender, EventArgs e)
    {
      string es = "0";
      string sPub, sPri;

      if (iDisplayMode == 10)  {
        if ((textBox1.Text != "") && (textBox2.Text != "") && (textBox3.Text != ""))  {
          try  {
            es = "1";
            Settings = new SecureStore(textBox1.Text, SettingsFileName);
            sPub = textBox2.Text;
            sPri = textBox3.Text;
            es = "2";
            Settings[SettingsKey] = sPub + " " + sPri;
            es = "3";

            iDisplayMode = 30;

          } catch (Exception ea) {
            throw ea.toAppLog("btnContinue " + es);
          }
        } else {
          MessageBox.Show("Editors cannot be empty.");
        }
      }

      if (iDisplayMode == 20)
      {
        if (textBox1.Text != "")
        {
          try
          {
            es = "5";
            Settings = new SecureStore(textBox1.Text, SettingsFileName);
            es = "6";
            string kpPolo = Settings[SettingsKey];
            es = "7";
            sPub = kpPolo.ParseString(" ", 0);
            sPri = kpPolo.ParseString(" ", 1);
        
            iDisplayMode = 30;
            LoadEditors();

          }
          catch (Exception ee)
          {
            throw ee.toAppLog("btnContinue " + es);
          }
        }
        else
        {
          MessageBox.Show("Editors cannot be empty.");
        }
      }

      if (iDisplayMode == 30){
        if (!timer1.Enabled) timer1.Enabled = true;
      }

      DoUpdateControlVisibility();
    }
       

    DateTime theNow;
 //   DateTime? NextGoTime = null;
    DateTime? NextUpdateAvg = null;
    Boolean FirstTimeLoad = true;
    Int32 RedrawToggle = 0;
    private async void timer1_Tick(object sender, EventArgs e) {
      timer1.Enabled = false;

      theNow = DateTime.Now;

      if (FirstTimeLoad) {  // only first time now with tickers going. 

       // if (NextGoTime.isNull()||(theNow > NextGoTime)) {
       // NextGoTime = theNow.AddMinutes(2);
       
        string kpPolo = Settings[SettingsKey];
        string sPub = kpPolo.ParseString(" ", 0);
        string sPri = kpPolo.ParseString(" ", 1);

        #region test 
         
        using (var client = new BittrexClient()) {          
          client.SetApiCredentials(sPub, sPri);
          
          var Tickers = client.GetSymbolSummaries();                 
          if (Tickers.Success) {
            //CObject Coins = new CObject();
            //foreach (var tic in Tickers.Data) {
            //  string sB = tic.Symbol.ParseFirst("-");
            //  string sQ = tic.Symbol.ParseLast("-");
            //  if ( ((sB=="USD")&&(tic.BaseVolume > 1900000)) ||
            //    ((sB == "USDT") && (tic.BaseVolume > 1900000)) ||
            //    ((sB == "BTC") && (tic.BaseVolume > 32)) ||
            //    ((sB == "ETH") && (tic.BaseVolume > 200)) 
            //    ) { 
            //    Coins[sB] += "\"" + tic.Symbol + "\",";
            //    Coins[sQ] += "\"" + tic.Symbol + "\",";                  
            //  }              
            //}
           // CObject sML = new CObject();
           // foreach (var tic in Tickers.Data) {
           //   string sBaseCur = tic.Symbol.ParseFirst("-");
           //   string sQuoteCur = tic.Symbol.ParseLast("-");
           //   if (BaseCoins.Contains(sBaseCur) && Coins.Contains(sQuoteCur)) {
           //     sML[tic.Symbol] = tic.Symbol;
           //   }
           // }
     //       MarketFilter = sML.Keys.ToArray().toCObject();
     //       Markets = new CMarkets(MarketFilter);
            foreach (var tic in Tickers.Data) {
                            BittrexSymbolSummary b;
              string sBaseCur = tic.Symbol.ParseFirst("-");            
              if (MarketFilter.Contains(tic.Symbol)){            
                string aMarket = tic.Symbol;
        //        DateTime aCr =tic.Created;
        //        DateTime aTS =tic.TimeStamp;

        //        decimal? aAsk = tic.Ask;
        //        decimal? aBid = tic.Bid; 
                
        //       decimal? aLast = tic.Last;
                decimal? aHigh = tic.High ;
                decimal? aLow = tic.Low ;
                decimal? aVolume = tic.Volume;            
        //        decimal? aBaseVol = tic.BaseVolume;
        //        decimal? aPrevDay = tic.PrevDay;
        //        int? aOBO = tic.OpenBuyOrders ;
        //        int? aOSO = tic.OpenSellOrders ;

        //        Markets[aMarket].Ask = aAsk.toDecimal();
        //        Markets[aMarket].Bid = aBid.toDecimal();
        //        Markets[aMarket].Last = aLast.toDecimal();
                Markets[aMarket].UpdateCount = 1;

        //        ((CInvMarket)Markets.Coins[sBaseCur][aMarket]).Ask = aBid.toDecimal();
        //        ((CInvMarket)Markets.Coins[sBaseCur][aMarket]).Bid = aAsk.toDecimal();
         //       ((CInvMarket)Markets.Coins[sBaseCur][aMarket]).Last = aLast.toDecimal();
                ((CInvMarket)Markets.Coins[sBaseCur][aMarket]).UpdateCount = 1;
              }
            }
          } else { 
            throw new Exception("Initial Request failed.");
          }
          #region HideBal 
          /*
          var ResultBal = client.GetBalances();
          if (ResultBal.Success) {
            foreach (BittrexBalance b in ResultBal.Data) {
              Balances.AddUpdate(b.Currency, (decimal)b.Balance, (decimal)b.Available);
            }
            decimal USDTotal = 0;
            string sMsg = "";
            foreach (string sCur in Balances.Keys) {
              if (Markets.Coins.Keys.Contains(sCur)) { 
                var aEst = Markets.ToUSD(sCur, Balances[sCur].Balance);
                USDTotal = USDTotal + aEst;
                sMsg = (sCur == "USD" ? " $" : " ") + Balances[sCur].Balance.toStr8()+ " " + sCur + (sCur=="USD"? "":" $" + aEst.toStr2()) + Environment.NewLine + sMsg; 
              } 
            }
            setTradeMsg(" $"+USDTotal.toStr4()+ Environment.NewLine+sMsg);          
          }
          */
          #endregion
          setLogMessage(theNow.ToShortTimeString() + " completed ");// + NextGoTime.toString());      
        } 
        #endregion

        if (FirstTimeLoad) {
          FirstTimeLoad = false;

          BittrexSocketClientOptions socOptions = new BittrexSocketClientOptions();
          socOptions.ApiCredentials = new ApiCredentials( sPub,  sPri);            
          BSC = new BittrexSocketClient(socOptions);
          var aResult = await BSC.SubscribeToSymbolTickerUpdatesAsync( data => { TickersLanding.Add(data); } );   
          if (!aResult.Success) {
            setLogMessage( "subscribe failed: "+ aResult.Error);
          }
        //  var aRes = await BSC.SubscribeToBalanceUpdatesAsync(data => { 
        //    Bittrex.Net.Objects.V3.BittrexBalanceV3 x = data.Delta; 
        //    Balances.AddUpdate(x.Currency, x.Total, x.Available); 
        //  }); 
         }
      }

      if (RedrawToggle >= 5) {
        RedrawToggle = 0;
        DoRedraw();
      } else {
        RedrawToggle += 1;
      }

      if (TickersLanding.Count > 0) {
        Int32 iMaxWork = 5;
        Int32 iWork = 0;
        while ((TickersLanding.Count > 0)&&(iWork<iMaxWork)) {
          iWork++;
          Bittrex.Net.Sockets.BittrexTickersUpdate T = TickersLanding.Pop();
          LastTicSeq = T.Sequence.toString();
          foreach(Bittrex.Net.Objects.BittrexTick x in T.Deltas.Where(x=> MarketFilter.Contains(x.Symbol.ParseReverse("-","-")))) {
            string sMarket = x.Symbol.ParseReverse("-", "-");
            //string sMarket = x.Symbol.ParseLast("-")+"-"+x.Symbol.ParseFirst("-");
            if (MarketFilter.Contains(sMarket)) {
              string sBaseCur = sMarket.ParseFirst("-");
              CMarket m = Markets[sMarket];
              m.UpdateAsk(x.AskRate);
              m.UpdateBid(x.BidRate);
              m.IncUpdateCount(1);
              CInvMarket mi = (CInvMarket)Markets.Coins[sBaseCur][sMarket];
              mi.UpdateAsk(1/x.BidRate);
              mi.UpdateBid(1/x.AskRate);
              mi.IncUpdateCount(1);
            }
          }
        }
      }

      if (NextUpdateAvg.isNull() || (theNow > NextUpdateAvg)) {
        NextUpdateAvg = theNow.AddSeconds(15);
        if (!FirstTimeLoad) {
          foreach (string sMarket in MarketFilter.Keys) {
            string sBaseCur = sMarket.ParseFirst("-");
            Markets[sMarket].AdvanceAverages();
            Markets.Coins[sBaseCur][sMarket].AdvanceAverages();
          }
        }
        
      } 
      
   //   if ((cbTrack.Checked) && (Markets.Coins.CurUpCoin != LastCur)) {
   //     try {
   //       TradeLastTo(Markets.Coins.CurUpCoin, true);
   //     } catch (Exception ev) { }
   //   }

     

      
      /* ← ↑ → ↓ */
      timer1.Enabled = true;
      
    }

    private void TradeLastTo(string TargetCur, Boolean UseStops) {
      string TargetMarket = "";
      string TargetOp = "";
      

      if (LastCur == "ADA") {
        TargetMarket = TargetCur+"-ADA";
        TargetOp = "Sell";
        // sell to top bid
      } else if (LastCur == "ETH") {
        if (TargetCur == "ADA") {
          TargetMarket = "ETH-ADA";
          TargetOp = "Buy";
        } else {
          TargetMarket = TargetCur + "-ETH";
          TargetOp = "Sell";
        }
      } else if (LastCur == "BTC") {
        if (TargetCur == "USD") {
          TargetMarket = "USD-BTC";
          TargetOp = "Sell";
        } else {
          TargetMarket = "BTC-"+TargetCur;
          TargetOp = "Buy";
        }
      } else {
        TargetMarket = "USD-"+TargetCur;
        TargetOp = "Buy";
      }

      string BaseCur = TargetMarket.ParseFirst("-");
      string QuoteCur = TargetMarket.ParseLast("-");

      if (TargetOp == "Buy"){        
        decimal BaseHolding = LastAmount;
        decimal TheFee = BaseHolding * TradeFee;
        LastPrice = Markets[TargetMarket].Bid;
        decimal LastBasePriceUSD = Markets.ToUSD(BaseCur, 1);
        decimal USDLastPrice = Markets.ToUSD(BaseCur, LastPrice);
        if ((BaseCur != "USD")&& (LastBasePriceUSD < ExitMin) && (LastBasePriceUSD > StopPrice)) {
          if (UseStops) { 
            string sError = "Except Price " + LastBasePriceUSD.toStr8() + " in " + StopPrice.toStr8() + " " + ExitMin.toStr8();
            throw new Exception(sError);
          }
        }
        decimal QuantityToBuy = (BaseHolding - TheFee) / LastPrice; 
        if (BaseCur != "USD") {          
          setTradeMsg(BaseCur + 
            ((LastBasePriceUSD < StopPrice) ? " Stoped at $"  : " Sold at $") + LastBasePriceUSD.toStr8() + ((LastBasePriceUSD < StopPrice) ? " Stop was $" + StopPrice.toStr8() : "Exit was $" + ExitMin.toStr8())  
          );
        }
        setTradeMsg(DateTime.Now.ToStrDateMM()+ 
          " Buy " + QuantityToBuy.toStr8() + " " + TargetCur+ 
          " at "+LastPrice.toStr8()+" "+ BaseCur + " "+ USDLastPrice.toStr8() + "USD"+
          " for "+LastAmount.toStr8() + " " + LastCur+
          " "+ Markets.ToUSD(LastCur, LastAmount).toStr4());
        LastAmount = QuantityToBuy;
        StopPrice = USDLastPrice * (1 - (TradeFeeStopM * TradeFee));
        ExitMin =  USDLastPrice * (1 + (TradeFeeExitM * TradeFee));        
        LastPrice = USDLastPrice;
      } else {
        decimal tp = Markets.ToUSD(BaseCur, Markets[TargetMarket].Ask);
        if ((tp < ExitMin) && (tp > StopPrice)) {
          if (UseStops) { 
            string sError = "Except Price "+tp.toStr8()+" in "+StopPrice.toStr8()+" "+ ExitMin.toStr8();
            throw new Exception(sError);
          }
        } 
        decimal QuoteHolding = LastAmount;        
        LastPrice = Markets[TargetMarket].Ask;  // current asking price in BaseCur
        decimal USDLastPrice = Markets.ToUSD(BaseCur, LastPrice);  // current ask in usd
        decimal SellResult = (QuoteHolding * LastPrice); // find sell quote in BaseCur
        decimal TheFee = (SellResult * TradeFee); // Find Fee in BaseCur
        SellResult =  SellResult - TheFee; // subtract fee from Result.
        setTradeMsg(BaseCur +
          ((tp < StopPrice) ? " Stoped $" : " Sold $") + tp.toStr8() + ((tp < StopPrice) ? " Stop was $" + StopPrice.toStr8() : "Exit was $" + ExitMin.toStr8())
        );

        setTradeMsg(DateTime.Now.ToStrDateMM() + 
          ((tp < StopPrice) ? " Stop " : " Sell ") + LastAmount.toStr8() + " " + LastCur + 
          " at "+ LastPrice.toStr8()+" " + BaseCur + " $" + USDLastPrice.toStr8() + "USD" +
          " for " +SellResult.toStr8()+" "+BaseCur + " $" + Markets.ToUSD(BaseCur, SellResult).toStr4());
        LastAmount = SellResult;
        if (BaseCur != "USD") {     // BaseCur can also be sold for usd need to reset exit limits. 
          LastPrice = 1/LastPrice;  // convert the price to be in Quote cur instead of base
          StopPrice = Markets.ToUSD(QuoteCur, LastPrice * (1- (TradeFeeStopM * TradeFee)));
          ExitMin = Markets.ToUSD(QuoteCur, LastPrice * (1+(TradeFeeExitM * TradeFee)));
          LastPrice = Markets.ToUSD(QuoteCur, LastPrice);
        }
      }
      LastCur = TargetCur;


    }

    private void cbTrack_CheckedChanged(object sender, EventArgs e) {
      if (cbTrack.Checked) {
        LastAmount = edStarting.Value;
        LastCur = cbStartCur.Text;
        LastPrice = edLastPrice.Value;
        StopPrice = Markets.ToUSD("USD", LastPrice * (1 - (TradeFeeStopM * TradeFee)));
        ExitMin = Markets.ToUSD("USD", LastPrice * (1 + (TradeFeeExitM * TradeFee)));
        edStarting.Visible = false;
        cbStartCur.Visible = false;
        edLastPrice.Visible = false;
      } else {
        edStarting.Value = LastAmount;
        cbStartCur.Text= LastCur;
        edLastPrice.Value= LastPrice;
        edStarting.Visible = true;
        edStarting.Visible = true;
        cbStartCur.Visible = true;
        edLastPrice.Visible = true;
      }
    }

    private void edStarting_ValueChanged(object sender, EventArgs e) {
      if (!LoadingEditors) { 
        LastPrice = Markets.ToUSD(cbStartCur.Text, 1);
        edLastPrice.Value = LastPrice;
      }
    }

    private void btnExit_Click(object sender, EventArgs e) {
      if ((cbTrack.Checked) && (Markets.Coins.CurUpCoin != LastCur)) {
        try {
          TradeLastTo(Markets.Coins.CurUpCoin, false);
        } catch (Exception ev) {
        }
      }
    }

    private void Form1_ResizeEnd(object sender, EventArgs e)
    {
      Graphics g = this.CreateGraphics();
      try
      {
        fWidth = g.VisibleClipBounds.Width;
        fHeight = g.VisibleClipBounds.Height;
        f20Height = fHeight * 0.2;
        f05Height = fHeight * 0.065;
        f15Height = fHeight * 0.145;
        f20Width = fWidth * 0.2;
        f15Width = fWidth * 0.15;
        f05Width = fWidth * 0.05;
      }
      finally
      {
        g.Dispose();
      }
    }

  }

 public class CTickerQueue : CQueue { 
    public CTickerQueue() : base() { 
    }
    public Bittrex.Net.Sockets.BittrexTickersUpdate Add(Bittrex.Net.Sockets.BittrexTickersUpdate aObj) {
      Nonce++;
      base.TryAdd(Nonce, aObj );
      base[Nonce] = aObj;
      return aObj;      
    }
    public new Bittrex.Net.Sockets.BittrexTickersUpdate Pop() {
      Object aR = null;
      if (Keys.Count > 0) {
        base.TryRemove(base.Keys.First(), out aR);
      }
      return (Bittrex.Net.Sockets.BittrexTickersUpdate)aR;
    }
  }
  
  public class CMarket : CObject {
    public string MarketName { get { return base["MarketName"].toString(); } set { base["MarketName"] = value; } }
    public string QuoteCur { get { return base["MarketName"].toString().ParseLast("-"); } }
    public string BaseCur { get { return base["MarketName"].toString().ParseFirst("-"); } }

    public void AdvanceAverages() {
      Ask = Ask;
      Bid = Bid;
      UpdateCount = 0;
    }
    public decimal Ask { 
      get { 
        return ((CAvgDecimalCache) base["Ask"]).toDecimal(); 
      } set {
        CAvgDecimalCache aAsk = ((CAvgDecimalCache)base["Ask"]);
        aAsk.Add(value);
        this.AskAvg = aAsk.toAvg();
        this.AskDelta = (this.AskAvg==0?0: (( Ask/AskAvg>1)?  Ask / AskAvg-1 : (1-(Ask / AskAvg))*-1));                       
      } 
    }

    public void UpdateAsk(decimal NewAsk) {
      CAvgDecimalCache aAsk = ((CAvgDecimalCache)base["Ask"]);
      CAvgDecimalCache aAskAvg = ((CAvgDecimalCache)base["AskAvg"]);
      CAvgDecimalCache aBidDelta = ((CAvgDecimalCache)base["BidDelta"]);
      CAvgDecimalCache aAskDelta = ((CAvgDecimalCache)base["AskDelta"]);
      CAvgDecimalCache aAvgPrice = ((CAvgDecimalCache)base["AvgPrice"]);      
      CAvgDecimalCache aPriceDelta = ((CAvgDecimalCache)base["PriceDelta"]);
      aAsk[ aAsk.Nonce] = NewAsk;
      decimal aAskG = aAsk.toAvg();
      aAskAvg[aAskAvg.Nonce] = aAskG;
      aAskDelta[aAskDelta.Nonce] = (aAskG==0?0:((NewAsk / aAskG > 1) ? NewAsk / aAskG - 1 : (1 - (NewAsk / aAskG)) * -1));
      //  aPriceDelta[aPriceDelta.Nonce] = (aAskDelta[aAskDelta.Nonce] + aBidDelta[aBidDelta.Nonce]) / 2;
      aAvgPrice[aAvgPrice.Nonce] = (this.Bid + this.Ask) / 2;
    }

    public decimal AskAvg {
      get {
        return ((CAvgDecimalCache)base["AskAvg"]).toDecimal();
      }
      set {
        CAvgDecimalCache aAskAvg = ((CAvgDecimalCache)base["AskAvg"]);
        aAskAvg.Add(value);
      }
    }

    public decimal AskDelta { 
      get {
        return ((CAvgDecimalCache)base["AskDelta"]).toDecimal();
      }
      set {
        CAvgDecimalCache aAskDelta = ((CAvgDecimalCache)base["AskDelta"]);
        aAskDelta.Add(value);
      }
    }

    public decimal Bid { 
      get { 
        return ((CAvgDecimalCache)base["Bid"]).toDecimal();
      } set { 
        CAvgDecimalCache aBid = ((CAvgDecimalCache)base["Bid"]);
        aBid.Add(value);
        this.BidAvg = aBid.toAvg();
        this.BidDelta = (this.BidAvg==0 ? 0 : ((Bid / BidAvg>1)? Bid / BidAvg-1: (1-(Bid / BidAvg))*-1));
        this.PriceDelta = (AskDelta + BidDelta) /2;
        this.AvgPrice = (this.Bid + this.Ask) / 2;
      } 
    }

    public void UpdateBid(decimal NewBid) {
      CAvgDecimalCache aBid = ((CAvgDecimalCache)base["Bid"]);
      CAvgDecimalCache aBidAvg = ((CAvgDecimalCache)base["BidAvg"]);
      CAvgDecimalCache aBidDelta = ((CAvgDecimalCache)base["BidDelta"]);
      CAvgDecimalCache aAskDelta = ((CAvgDecimalCache)base["AskDelta"]);
      CAvgDecimalCache aPriceDelta = ((CAvgDecimalCache)base["PriceDelta"]);
      CAvgDecimalCache aAvgPrice = ((CAvgDecimalCache)base["AvgPrice"]);
      aBid[aBid.Nonce] = NewBid;
      decimal aBidA = aBid.toAvg();
      aBidAvg[aBidAvg.Nonce] = aBidA;
      aBidDelta[aBidDelta.Nonce] = (aBidA==0?0:((NewBid / aBidA > 1) ? NewBid / aBidA - 1 : (1 - (NewBid / aBidA)) * -1));
      aPriceDelta[aPriceDelta.Nonce] = (aAskDelta[aAskDelta.Nonce] + aBidDelta[aBidDelta.Nonce]) / 2;
      aAvgPrice[aAvgPrice.Nonce] = (this.Bid + this.Ask) / 2;
    }

    public decimal BidAvg {
      get {
        return ((CAvgDecimalCache)base["BidAvg"]).toDecimal();
      }
      set {
        ((CAvgDecimalCache)base["BidAvg"]).Add(value);
      }
    }

    public decimal BidDelta {
      get {
        return ((CAvgDecimalCache)base["BidDelta"]).toDecimal();
      }
      set {
        ((CAvgDecimalCache)base["BidDelta"]).Add(value);
      }
    }

    public decimal AvgPrice {
      get {
        return ((CAvgDecimalCache)base["AvgPrice"]).toDecimal();
      }
      set {
        ((CAvgDecimalCache)base["AvgPrice"]).Add(value);
      }
    }

    public decimal PriceDelta {
      get {
        return ((CAvgDecimalCache)base["PriceDelta"]).toDecimal();
      }
      set {
        ((CAvgDecimalCache)base["PriceDelta"]).Add(value);
      }
    }

    public CAvgDecimalCache PriceDeltaCache { get { 
      return ((CAvgDecimalCache)base["PriceDelta"]);
    } }


    public decimal Last {
      get {
        return ((CAvgDecimalCache)base["Last"]).toDecimal();
      }
      set {
        ((CAvgDecimalCache)base["Last"]).Add(value);
        CAvgDecimalCache aLast = ((CAvgDecimalCache)base["Last"]);
        aLast.Add(value);
        if (aLast.Count > 1) {
          this.LastAvg = aLast.toAvg();
        }
      }
    }

    public decimal LastAvg { 
      get { 
        return ((CAvgDecimalCache)base["LastAvg"]).toDecimal();
      }
      set {
        ((CAvgDecimalCache)base["LastAvg"]).Add(value);
      } 
    }
    public decimal UpdateCount {
      get {
        return ((CAvgDecimalCache)base["UpdateCount"]).toDecimal();
      }
      set {
        ((CAvgDecimalCache)base["UpdateCount"]).Add(value);
      }
    }

    public CAvgDecimalCache UpdateCountCache {
      get {
        return ((CAvgDecimalCache)base["UpdateCount"]);
      }
    }

    public void IncUpdateCount(Int32 HowMany) {
      CAvgDecimalCache aUC = ((CAvgDecimalCache)base["UpdateCount"]);
      aUC[aUC.Nonce] = aUC[aUC.Nonce] + HowMany;
    }

    public CMarket(string sMarket) {
      MarketName = sMarket;
      CAvgDecimalCache adc = new CAvgDecimalCache();
      adc.Size = 24;
      base["Ask"] = adc;

      adc = new CAvgDecimalCache();
      adc.Size = 24;
      base["AskAvg"] = adc;

      adc = new CAvgDecimalCache();
      adc.Size = 24;
      base["AskDelta"] = adc;

      adc = new CAvgDecimalCache();
      adc.Size = 24;
      base["Bid"] = adc;

      adc = new CAvgDecimalCache();
      adc.Size = 24;
      base["BidAvg"] = adc;

      adc = new CAvgDecimalCache();
      adc.Size = 24;
      base["BidDelta"] = adc;

      adc = new CAvgDecimalCache();
      adc.Size = 24;
      base["Last"] = adc;

      adc = new CAvgDecimalCache();
      adc.Size = 24;
      base["LastAvg"] = adc;

      adc = new CAvgDecimalCache();
      adc.Size = 24;
      base["AvgPrice"] = adc;

      adc = new CAvgDecimalCache();
      adc.Size = 24;
      base["PriceDelta"] = adc;

      adc = new CAvgDecimalCache();
      adc.Size = 24;
      base["UpdateCount"] = adc;


    }

  }

  public class CInvMarket : CMarket {
    public CInvMarket(string aMarket): base(aMarket) {    
      }
    new public decimal Ask {
      get {
        return ((CAvgDecimalCache)base["Ask"]).toDecimal();
      }
      set {
        CAvgDecimalCache aAsk = ((CAvgDecimalCache)base["Ask"]);
        aAsk.Add(1/value);
        this.AskAvg = aAsk.toAvg();
        this.AskDelta = ((Ask / AskAvg > 1) ? Ask / AskAvg - 1 : (1 - (Ask / AskAvg)) * -1); 
      }
    }
    new public decimal Bid {
      get {
        return ((CAvgDecimalCache)base["Bid"]).toDecimal();
      }
      set {
        CAvgDecimalCache aBid = ((CAvgDecimalCache)base["Bid"]);
        aBid.Add(1/value);
        this.BidAvg = aBid.toAvg();
        this.BidDelta = ((Bid / BidAvg > 1) ? Bid / BidAvg - 1 : (1 - (Bid / BidAvg)) * -1);
        this.PriceDelta = (AskDelta + BidDelta) / 2;
      }
    }

    new public decimal Last {
      get {
        return ((CAvgDecimalCache)base["Last"]).toDecimal();
      }
      set {       
        CAvgDecimalCache aLast = ((CAvgDecimalCache)base["Last"]);
        aLast.Add(1/value);
        if (aLast.Count > 1) {
          this.LastAvg = aLast.toAvg();
        }
      }
    }
  }

  public class CMarketList : CObject {
    public string Coin;
    public Int32 ColorIndex = 0;
    public string[] SomeColors = {
    "#E5D82D","#DFCB3C","#D9BE4B","#D3B15A","#CDA469", "#C79778","#C18A87","#BB7D96","#B570A5","#AF63B4", 
    "#A956C3","#A349D2","#9D3CE1","#972FF0","#9727F0", "#9D2CE1","#A331D2","#A936C3","#AF3BB4","#B540A5",
    "#BB4596","#C14A87","#C74F78","#CD5469","#D3595A", "#D95E4B","#DF633C","#E5682D","#EB6D1E","#F1720F",
    "#F1720F","#FF8000","#804000","#81320F","#82311E", "#83302D","#842F3C","#852E4B","#862D5A","#872C69",
    "#882B78","#892A87","#8A2996","#8B28A5","#8C27B4", "#8D26C3","#8E25D2","#8F24E1","#9023F0","#6666FF",
    "#7162FF","#735EFF","#755AFF","#7756FF","#7952FF", "#7B4EFF","#7D4AFF","#7F46FF","#8142FF","#833EFF",
    "#853AFF"};
    public Color GetCoinColor(string aCoin) {
      Color a = ColorTranslator.FromHtml("#42E2B8");
      switch (aCoin) {
        case "USD": a = ColorTranslator.FromHtml("#42E2B8"); break;
        case "USDT": a = ColorTranslator.FromHtml("#2C9790"); break;
        case "BTC": a = ColorTranslator.FromHtml("#F28123"); break;
        case "ETH": a = ColorTranslator.FromHtml("#FF74D4"); break;
        default:
          Int32 h = Encoding.ASCII.GetBytes(aCoin).toSum();
          a = ColorTranslator.FromHtml(SomeColors[h % 61]); break;
      }
      return a;
    }
    public Color CoinColor { get { return GetCoinColor(Coin); } }
    public Color CoinColor2 {
      get {
        Color b = ColorTranslator.FromHtml("#000040");
        Color[] C = DllExt.GetColors(CoinColor, b, 2);  // darken it by 3rd.       
        return C[1];
      }
    }
    public Color MarketColor( string aMarket) {

      return Color.Wheat;
    }
    public Decimal AvgChange { get { 
        Decimal r = 0;  
        foreach(string sMarket in base.Keys) { 
          r = r + ((CMarket)base[sMarket]).PriceDelta;
        }
        return ((base.Keys.Count>0)? r/base.Keys.Count:0);
    } }
    public Decimal UpdateCount { get{
        Decimal r = 0;
        foreach (string sMarket in base.Keys) {
          r = r + ((CMarket)base[sMarket]).UpdateCount;
        }
        return r;
      } }
    public CMarketList(string aCoin) : base (){
      Coin = aCoin;
      Int32 h = Encoding.ASCII.GetBytes(Coin).toSum();
      ColorIndex = (h % 61);

    }
    public new CMarket this[string aMarket] {
      get { try { return (base[aMarket] is object ? (CMarket)base[aMarket] : null); } catch { return null; } }
      set { base[aMarket] = value; }
    }
    public string[] KeysByPriceDelta() {
      return base.Keys.OrderByDescending(x => ((CMarket)base[x]).PriceDelta).ToArray();
    }
  }

  public class CMarketCoins : CObject { 
    public CMarketCoins () : base() { }
    public new CMarketList this[string aCoin] { 
      get { try { return (base[aCoin] is object ? (CMarketList)base[aCoin] : null); } catch { return null; } } 
      set { base[aCoin] = value; } 
    }
    public string[] ByAvgChange() {
      return base.Keys.OrderByDescending(x => ((CMarketList)base[x]).AvgChange ).ToArray();
    }
    public string CurUpCoin { get { 
      return ByAvgChange()[0];
    } }
  }

  public class CMarkets : CObject {
    public CObject MarketFilter;
    public CMarketCoins Coins;
    public CMarkets(CObject aMarketFilter) : base() {
      MarketFilter = aMarketFilter;
      Coins = new CMarketCoins();

      foreach (string sMarket in MarketFilter.Keys) {
        CMarket aM = new CMarket(sMarket);
        base[sMarket] = aM;
        if (!(Coins[aM.QuoteCur] is CMarketList)) Coins[aM.QuoteCur] = new CMarketList(aM.QuoteCur);
        Coins[aM.QuoteCur][sMarket] = aM;
        if (!(Coins[aM.BaseCur] is CMarketList)) Coins[aM.BaseCur] = new CMarketList(aM.BaseCur);
        Coins[aM.BaseCur][sMarket] = new CInvMarket(sMarket);
      }
    }    
    public new CMarket this[string aKey] { 
      get { return (CMarket)base[aKey]; } 
      set { base[aKey] = value; } 
    }
  

    public Decimal BTCtoUSD(Decimal aBTCValue) {
      return (this["USD-BTC"].Bid + (this["USD-BTC"].Ask - this["USD-BTC"].Bid) / 2 ) * aBTCValue;
    }
    public Decimal ETHtoUSD(Decimal aETHValue) {
      return (this["USD-ETH"].Bid + (this["USD-ETH"].Ask - this["USD-ETH"].Bid) / 2) * aETHValue;
    }
    public Decimal ADAtoUSD(Decimal aADAValue) {
      return (this["USD-ADA"].Bid + (this["USD-ADA"].Ask-this["USD-ADA"].Bid) /2) * aADAValue;
    }

    public Decimal ToUSD(string aCur, Decimal aCurValue) {
      Decimal aRet = 0;
      switch (aCur) { 
        case "USD":
          aRet = aCurValue;
          break;
        case "BTC":
          aRet = BTCtoUSD(aCurValue);
          break;
        case "ETH":
          aRet = ETHtoUSD(aCurValue);
          break;
        default:
          string sMarkAttempt = "USD-" + aCur;
          if (this[sMarkAttempt] is CMarket) {
            aRet = (this[sMarkAttempt].Bid + (this[sMarkAttempt].Ask - this[sMarkAttempt].Bid) / 2) * aCurValue;
          } else {
            sMarkAttempt = "BTC-" + aCur;
            if (this[sMarkAttempt] is CMarket) {
              aRet = BTCtoUSD((this[sMarkAttempt].Bid + (this[sMarkAttempt].Ask - this[sMarkAttempt].Bid) / 2) * aCurValue);
            } else {
              sMarkAttempt = "ETH-" + aCur;
              if (this[sMarkAttempt] is CMarket) {
                aRet = ETHtoUSD((this[sMarkAttempt].Bid + (this[sMarkAttempt].Ask - this[sMarkAttempt].Bid) / 2) * aCurValue);
              } else throw new Exception("Unknown Currency "+aCur);
            }
          }
          break;          
      }
      return aRet;
    }

  }

  //b.Currency, b.Balance, b.Available
  public class CBalances : CObject {

    public class CBalance : CObject {
      public CBalances Owner;
      public string Currency { get { return base["Currency"].toString(); } set { base["Currency"] = value; } }
      public decimal Available { get { return base["Available"].toDecimal(); } set { base["Available"] = value; } }
      public decimal Balance { get { return base["Balance"].toDecimal(); } set { base["Balance"] = value; } }
      public CBalance(CBalances aOwner, string aCurrency, decimal aBalance, decimal aAvailable) : base() {
        Owner = aOwner;
        Currency = aCurrency;
        Available = aAvailable;
        Balance = aBalance;
      }      
    }

    public CMarkets Markets;
    public CBalances(CMarkets aMarkets) : base() {
      Markets = aMarkets;
    }
    public new CBalance this[string aCur] { get { return (CBalance)base[aCur]; } set { base[aCur] = value; } }
    public void AddUpdate(string sCurrency, decimal aBalance, decimal aAvail ) {
      if (!Contains(sCurrency)) {
        this[sCurrency] = new CBalances.CBalance(this, sCurrency, aBalance, aAvail);
      } else {
        CBalances.CBalance wb = (CBalances.CBalance)this[sCurrency];
        wb.Available = aAvail;
        wb.Balance = aBalance;
      }
    }

  }

  


}
