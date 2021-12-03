using AppCrypto;
using Bittrex.Net;
using Bittrex.Net.Objects;
using CryptoExchange.Net.Authentication;
using StaticExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;


namespace OracleAlpha {
  public partial class Form1 : Form {

    #region form properties
    
    public string CallSign = "BittrexTrader";
    public string SettingsKey = "BittrexKP";
    public string[] DefMarketList = {
      "USD-BTC", 
      "USD-ADA", "BTC-ADA", 
  //    "USD-DGB", "BTC-DGB",
      "USD-LTC", "BTC-LTC",
      "USD-DOGE", "BTC-DOGE"
    };
//  "ETH-DGB", "ETH-ADA","ETH-LINK", "USD-ETH", "BTC-ETH","USD-LINK", "BTC-LINK",
  
    public CObject MarketFilter; 
    public CObject MarketFilterBittrex;

    public BittrexSocketClient BSC;
    public SecureStore Settings;

    public CMarkets Markets;
    public CPositions Positions;
    public PositionButtons PosButs;
    public CTickerQueue TickersLanding;
    
    public string LastTicSeq = "";
    public Int32 iDisplayMode = 0;
    public Int32 iOpMode = 0;
    
    public float fWidth = 0, fHeight = 0;
    public double f20Height = 0.2;
    public double f05Height = 0.065;
    public double f15Height = 0.145;
    public double f20Width = 0.2;
    public double f15Width = 0.15;
    public double f05Width = 0.05;
    public SizeF OneChar;
    public Single iCW;
    public Single iRowH;
    public decimal grLeft;
    public decimal grTop;
    public decimal grWidth;
    public decimal grHeight;
    public decimal grHeightT;
    public Int32 iCoinCount;

    public Font fCur10; Font fCur9; Font fCur8; Font fCur7; Font fCur6;
    
    string SettingsFilePath;
    string SettingsFileName;

    readonly decimal TradeFee = 0.0025m;     // trade fee as a percent 
    readonly decimal TradeFeeStopM = 1.469m;   // stop price to adjust how far down to stop release.
    readonly decimal TradeFeeExitM = 1.469m;  // min exit price as  

    String LastCur = "";            // values to simulate holding an amount of currency.
    Decimal LastAmount = 0;
    Decimal LastPrice = 0;
    Decimal StopPrice = 0;
    Decimal ExitMin = 0;
    string LastMessage = "";
    public string BuyBase = "";
    public string BuyQuote = "";

    #endregion

    public Form1() {
      InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e) {
      SettingsFilePath = DllExt.MMConLocation();

      fCur10 = new Font("Courier New", 10); fCur9 = new Font("Courier New", 9); fCur8 = new Font("Courier New", 8); 
      fCur7 = new Font("Courier New", 7); fCur6 = new Font("Courier New", 6);
      
      ServicePointManager.Expect100Continue = true;
      ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

      MarketFilter = DefMarketList.toCObject();
      
      MarketFilterBittrex = new CObject();  // markets are reversed when subscribing to feed
      foreach (string sMarket in MarketFilter.Keys) {
        MarketFilterBittrex[sMarket.ParseReverse("-", "-")] = sMarket;
      }

      Markets = new CMarkets(MarketFilter, SettingsFilePath + "\\" + CallSign + "Markets.ini");
      iCoinCount = Markets.Coins.Keys.Count;
      Positions = new CPositions(Markets, SettingsFilePath + "\\"+CallSign+"Wallet.ini");

      if (Positions.Keys.Count == 0) { 
        Positions.AddUSD(20000);  // give some credits
      }
      // PosButs = new PositionButtons();

      TickersLanding = new CTickerQueue(Markets);      
      
      if (!Directory.Exists(SettingsFilePath)) Directory.CreateDirectory(SettingsFilePath);
      SettingsFileName = SettingsFilePath + "\\" + CallSign + "Settings.ini";

      if (!File.Exists(SettingsFileName)) { // need api keys. 
        iDisplayMode = 10;
      } else {  // need password to unlock keys. 
        iDisplayMode = 20;
      }

    }

    #region Load Close Ops
    private async void Form1_FormClosing(object sender, FormClosingEventArgs e) {
      timer1.Enabled = false;
      SaveEditors();
      if ((!FirstTimeLoad) && (BSC is BittrexSocketClient)) {
          await BSC.UnsubscribeAllAsync();
        
      }
    }

    private void Form1_Shown(object sender, EventArgs e) {
      DoUpdateControlVisibility();
    }

    delegate void SetLogMessageCallback(string message);
    private void setLogMessage(string message) {
      if (this.edOut.InvokeRequired) {
        SetLogMessageCallback d = new SetLogMessageCallback(setLogMessage);
        this.Invoke(d, new object[] { message });
      } else {
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
       

#pragma warning disable CS0414 // The field 'Form1.LoadingEditors' is assigned but its value is never used
    private Boolean LoadingEditors = false;
#pragma warning restore CS0414 // The field 'Form1.LoadingEditors' is assigned but its value is never used
    private void LoadEditors() {
      LoadingEditors = true;
      if (Settings is SecureStore)
      {
          LastAmount = (Settings["LastAmount"] == "" ? 10000 : Settings["LastAmount"].toDecimal());
          LastCur = (Settings["LastCur"] == "" ? "USD" : Settings["LastCur"].toString());
          LastPrice = (Settings["LastPrice"] == "" ? 1 : Settings["LastPrice"].toDecimal());
          edLastPrice.Value = LastPrice;
          edQuantity.Value = LastAmount;
      }
      LoadingEditors = false;
    }
    private void SaveEditors()
    {
        if (Settings is SecureStore)
        {
            Settings["LastAmount"] = LastAmount.toStr8();
            Settings["LastCur"] = LastCur;
            Settings["LastPrice"] = LastPrice.toStr8();
        }

        Markets.Save();
    }

   
    private void Form1_ResizeEnd(object sender, EventArgs e) {
      Graphics g = this.CreateGraphics();
      try {

        fWidth = g.VisibleClipBounds.Width;
        fHeight = g.VisibleClipBounds.Height;

        OneChar = g.MeasureString("W00W", fCur9);
        iCW = OneChar.Width;

        f20Height = fHeight * 0.2;
        f05Height = fHeight * 0.065;
        f15Height = fHeight * 0.145;
        f20Width = fWidth * 0.2;
        f15Width = fWidth * 0.15;
        f05Width = fWidth * 0.05;

        Single iMCW = fWidth / iCW;
        Single iRow = Convert.ToSingle(fHeight * 0.12);
        Single AreaMaxHeight = Convert.ToSingle(edOutContainer.Top - iRow - 1);
        iRowH = AreaMaxHeight / 37;
        
        grLeft = Convert.ToDecimal(iCW * 14.6 + fWidth * 0.015);
        grTop = Convert.ToDecimal(iRow + (iRowH * 1.5));
        grWidth = Convert.ToDecimal(fWidth.toDecimal() * 0.995m - (grLeft));
        grHeight = Convert.ToDecimal(iRowH * 27);
        grHeightT = Convert.ToDecimal(iRowH * 5);

      } finally {
        g.Dispose();
      }
    }

    delegate void UpdateControlVisibilityCallback();
    private void DoUpdateControlVisibility() {
      if (this.InvokeRequired) {
        UpdateControlVisibilityCallback d = new UpdateControlVisibilityCallback(DoUpdateControlVisibility);
        this.Invoke(d, new object[] { });
      } else {
        if (this.Visible) {

          Form1_ResizeEnd(null, null);

          if (iDisplayMode == 30) {

            cbTrack.Left = (fWidth * 0.015).toInt32();
            cbTrack.Top = (fHeight * 0.055).toInt32();
            decimal iLeft = (fWidth.toDecimal() * 0.015m) + (iCW.toDecimal() / 2);

            edQuantity.Top = (grTop + iRowH.toDecimal() * 4).toInt32T();
            edQuantity.Left = (iLeft + iCW.toDecimal() * 13).toInt32T();

            edLastPrice.Top = (grTop + iRowH.toDecimal() * 7).toInt32T();
            edLastPrice.Left = edQuantity.Left;

            btnBuy.Top = (grTop + iRowH.toDecimal() * 14).toInt32T();
            btnBuy.Left = edQuantity.Left;
                        
            btnExit.Left = edLastPrice.Left + edLastPrice.Width + 2;
            btnExit.Top = cbTrack.Top;

            if (label1.Visible) label1.Visible = false;
            if (label2.Visible) label2.Visible = false;
            if (label3.Visible) label3.Visible = false;
            if (textBox1.Visible) textBox1.Visible = false;
            if (textBox2.Visible) textBox2.Visible = false;
            if (textBox3.Visible) textBox3.Visible = false;
            if (btnContinue.Visible) btnContinue.Visible = false;
            if (!edOutContainer.Visible) edOutContainer.Visible = true;
            if (!edOut.Visible) edOut.Visible = true;
            if (!edTradeHist.Visible) edTradeHist.Visible = true;
            if (!cbTrack.Visible) cbTrack.Visible = false;           
            // if ((cbTrack.Checked) && (Markets.Coins.CurUpCoin != LastCur)) {
            // if (!btnExit.Visible) btnExit.Visible = true;
            if (btnExit.Visible) btnExit.Visible = false;
            
            if ((iOpMode == 10) && (BuyQuote != "") && (BuyBase != "")) {
              if(!edQuantity.Visible) edQuantity.Visible = true;
              if(!edLastPrice.Visible) edLastPrice.Visible = true;
              if(!btnBuy.Visible) btnBuy.Visible = true;
              if(btnBuy.Text != "Buy") btnBuy.Text = "Buy";
            } else if ((iOpMode == 11) && (BuyQuote != "") && (BuyBase != "")) {
              if (!edQuantity.Visible) edQuantity.Visible = true;
              if (!edLastPrice.Visible) edLastPrice.Visible = true;
              if (!btnBuy.Visible) btnBuy.Visible = true;
              if (btnBuy.Text != "Sell") btnBuy.Text = "Sell";
            }
            if (iOpMode == 0) {
              if (edQuantity.Visible) edQuantity.Visible = false;
              if (edLastPrice.Visible) edLastPrice.Visible = false;
              if (btnBuy.Visible) btnBuy.Visible = false;
            }

          }

          if (iDisplayMode == 10) {
            if (!label1.Visible) label1.Visible = true;
            if (!label2.Visible) label2.Visible = true;
            if (!label3.Visible) label3.Visible = true;
            if (!textBox1.Visible) textBox1.Visible = true;
            if (!textBox2.Visible) textBox2.Visible = true;
            if (!textBox3.Visible) textBox3.Visible = true;
            if (!btnContinue.Visible) btnContinue.Visible = true;
            if (edOut.Visible) edOut.Visible = false;
            if (edTradeHist.Visible) edTradeHist.Visible = false;
            if (edOutContainer.Visible) edOutContainer.Visible = false;
            if (edQuantity.Visible) edQuantity.Visible = false;
            if (cbTrack.Visible) cbTrack.Visible = false;            
            if (edLastPrice.Visible) edLastPrice.Visible = false;
            if (btnExit.Visible) btnExit.Visible = false;
            if (btnBuy.Visible) btnBuy.Visible = false;
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
            if (edQuantity.Visible) edQuantity.Visible = false;
            if (edOutContainer.Visible) edOutContainer.Visible = false;
            if (cbTrack.Visible) cbTrack.Visible = false;           
            if (edLastPrice.Visible) edLastPrice.Visible = false;
            if (btnExit.Visible) btnExit.Visible = false;
            if (btnBuy.Visible) btnBuy.Visible = false;
          }


        }
      }
    }

    #endregion

    private void Form1_MouseDown(object sender, MouseEventArgs e) {
      try { 

        Single iLeft = Convert.ToSingle(fWidth * 0.015 + (iCW / 2));
        decimal iWM = (grLeft - iLeft.toDecimal()) / (iCoinCount - 1);
        string sFoundCur = "";


        if (iDisplayMode == 30) {

          if (e.Y >grTop - grHeightT && e.Y < grTop - iRowH.toDecimal() &&
            e.X > iLeft && e.X < iLeft.toDecimal() + iWM * (iCoinCount-1)) {
            int btnX = ((e.X.toDecimal() - iLeft.toDecimal())/iWM).toInt32T();
            var m = Markets.Coins.Keys.OrderBy(x => x);
            if (btnX < m.Count()) {
              int iX = 0;
              foreach (string sCoin in m) {
                if (btnX == iX) {
                  BuyQuote = sCoin;
                  BuyBase = Positions.BiggestBaseCoin();
                  string sQuoteMarket = BuyBase == BuyQuote ? "USD" + "-" +BuyQuote : BuyBase+"-"+BuyQuote;
                  CMarket cm = Markets.Coins[sCoin][sQuoteMarket];
                  if (!cm.isNull()&&(cm.Ask.toDecimal() != 0))
                  edLastPrice.Value = cm.Ask.toDecimal();
                  edQuantity.Value = Positions.Balance(BuyBase) / cm.Ask.toDecimal();
                }
                iX++;
              }            
            }   

            if (BuyQuote != "") {
              iOpMode = 10;            
            }

          } else if (e.X > Convert.ToInt32(iLeft + iCW * 3) && e.X < Convert.ToInt32(iLeft + iCW * 23) &&
             e.Y > Convert.ToInt32(grTop) && e.Y < Convert.ToInt32(grTop + iRowH.toDecimal() * 20) ) {  
            // in this case do nothing buypassing next else.
          } else if ( (sFoundCur = PosButs.didHitTest(e.X, e.Y)) != "" ) { 
             
              if (sFoundCur != "" && sFoundCur != "USD") { 
                BuyQuote = sFoundCur;
                BuyBase = "USD";
                string sQuoteMarket = BuyBase + '-' + BuyQuote;
                iOpMode = 11;
                decimal dBid = Markets.Coins[sFoundCur][sQuoteMarket].Bid.toDecimal();
                edLastPrice.Value = dBid;
                edQuantity.Value = Positions.Balance(BuyQuote);
            }

          
          } else {
            BuyQuote = "";
            BuyBase = "";
            iOpMode = 0;
          }



        }
      } catch (Exception ex0) {
        setLogMessage("MC "+ex0.toWalkExcTreePath() );
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
          // Int32 iW = 6;
          // 
          Single iMCW = fWidth / iCW;
          Single iRow = Convert.ToSingle(fHeight * 0.12);
          Single iRowLastCur = iRow;
          Single iLeft = Convert.ToSingle(fWidth * 0.015 + (iCW / 2));
          string sRow = "";       

          decimal PriceChangeMax = 0;
          decimal PriceChangeMin = 0;

          #region chart outline
          bg.Graphics.DrawRectangle(Pens.WhiteSmoke,
            new Rectangle(
              Convert.ToInt32(grLeft),
              Convert.ToInt32(grTop),
              Convert.ToInt32(grWidth),
              Convert.ToInt32(grHeight)
            )
          );

          bg.Graphics.DrawRectangle(Pens.WhiteSmoke,
            new Rectangle(
              Convert.ToInt32(grLeft),
              Convert.ToInt32(grTop- grHeightT), //
              Convert.ToInt32(grWidth),
              Convert.ToInt32(grHeightT)
            )
          );
          #endregion
          es = "100";
          #region draw in coins menu along top.
          Int32 iCount = 0;
          decimal iWM = (grLeft - iLeft.toDecimal())/(iCoinCount-1);
          foreach(string sCoin in Markets.Coins.Keys.OrderBy(x => x)) {
            if (sCoin != "USD") {
              Color aC = Markets.Coins[sCoin].CoinColor;
              Color aCUSD = Markets.Coins["USD"].CoinColor;
              Color aCBTC = Markets.Coins["BTC"].CoinColor;
              Pen aP = new Pen(aC, 1);
              Brush abmm = new SolidBrush(aC);
              Brush abUSD = new SolidBrush(aCUSD);
              Brush abBTC = new SolidBrush(aCBTC);

              bg.Graphics.DrawRectangle(aP,
                new Rectangle(
                  Convert.ToInt32(iLeft.toDecimal() + iWM * iCount - (iCW / 2).toDecimal()),
                  Convert.ToInt32(grTop - grHeightT), 
                  Convert.ToInt32(iWM-2),
                  Convert.ToInt32(grHeightT- iRowH.toDecimal())
                )            
              );
              Decimal dAvgChange = Markets.Coins[sCoin].AvgChange;
              Brush sbaa = (System.Math.Abs(dAvgChange) < 0.0005m ? Brushes.WhiteSmoke : ((dAvgChange < 0) ? Brushes.Red : Brushes.Chartreuse));
              sRow = "    %" + (100 * dAvgChange).toStr4();
              bg.Graphics.DrawString(sRow, fCur8, sbaa, Convert.ToSingle(iLeft.toDecimal() + iWM * iCount - (3 * iCW / 8).toDecimal()), Convert.ToSingle(iRow - (iRowH * 3.5)));
              bg.Graphics.DrawString(sCoin, fCur8, abmm, Convert.ToSingle(iLeft.toDecimal() + iWM * iCount-(3 * iCW / 8).toDecimal()), Convert.ToSingle(iRow - (iRowH * 3.5)));
                        
              string sMarket = "USD-"+sCoin;
              string sAskUSD = Markets.Coins[sCoin][sMarket].Ask.toStr4();
              bg.Graphics.DrawString(sAskUSD, fCur8, abUSD, Convert.ToSingle(iLeft.toDecimal() + iWM * iCount - (3*iCW / 8).toDecimal()), Convert.ToSingle(iRow - (iRowH * 2.5)));
            
              if (sCoin != "BTC")  {
                sMarket = "BTC-" + sCoin;
                sAskUSD = Markets.Coins[sCoin][sMarket].Ask.toStr8();
                bg.Graphics.DrawString(sAskUSD, fCur8, abBTC, Convert.ToSingle(iLeft.toDecimal() + iWM * iCount - (3*iCW / 8).toDecimal()), Convert.ToSingle(iRow - (iRowH * 1.5)));
              }

              iCount += 1;
            }
          }
          #endregion
          es = "200";
          #region draw in Markts by Avg Change 
          foreach (string sCoin in Markets.Coins.ByAvgChange()) {

            iRow = Convert.ToSingle(iRow + iRowH);
            decimal dCoin = Positions.Balance(sCoin);
            if ( dCoin > 0.0005m)  {
              decimal AvgBuyIn = Positions.AvgPrice(sCoin);
              iRowLastCur = iRow;
              Color aC = Markets.Coins[sCoin].CoinColor;
              Pen aP = new Pen(aC, 1);
              bg.Graphics.DrawRectangle(aP, new Rectangle( //  Selected Coin Rec
                (fWidth * 0.015).toInt32(),
                (iRow - (iRowH * 0.5)).toDecimal().toInt32T(),
                Convert.ToInt32(iCW * 13.5),
                Convert.ToDecimal((iRowH * (Markets.Coins[sCoin].Count + 1.5))).toInt32T() ));
              string standing = "" + dCoin.toStr8() + " " + sCoin + (sCoin != "USD" ? "  at " +  AvgBuyIn.toStr8(): "") +
                "  $" + Markets.ToUSD(sCoin, dCoin).toStr2();
              bg.Graphics.DrawString(standing, fCur8, Brushes.WhiteSmoke, Convert.ToSingle(fWidth * 0.015 + cbTrack.Width), iRowLastCur);
            }

            Decimal dAvgChange = Markets.Coins[sCoin].AvgChange;

            Brush sbaa = (System.Math.Abs(dAvgChange) < 0.0005m ? Brushes.WhiteSmoke : ((dAvgChange < 0) ? Brushes.Red : Brushes.Chartreuse));
            sRow = "    %" + (100 * dAvgChange).toStr4();
            Color aCC = Markets.Coins[sCoin].CoinColor;
            Brush abmm = new SolidBrush(aCC);

            bg.Graphics.DrawString(sRow, fCur8, sbaa, Convert.ToSingle(iLeft), Convert.ToSingle(iRow));
            bg.Graphics.DrawString(sCoin, fCur8, abmm, Convert.ToSingle(iLeft), Convert.ToSingle(iRow));
            bg.Graphics.DrawString(Markets.Coins[sCoin].UpdateCount.toInt32T().toString(),
                fCur7, Brushes.WhiteSmoke, Convert.ToSingle(iLeft - (iCW / 2)), Convert.ToSingle(iRow));

            iRow = Convert.ToSingle(iRow + iRowH);
            if (iRow > edOutContainer.Top) break;
            
            // draw Market data
            Int32 iMarketCount = 0;
            foreach (string sMarket in Markets.Coins[sCoin].KeysByPriceDelta()) {
              iMarketCount++;
              // if (iMarketCount > 4) break;
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
                sBaseM + (100 * ((dAskDelta + dBidDelta) / 2)).toStr2() + "%" + (((dAskDelta + dBidDelta) / 2) < 0 ? "↓" : "↑");
              string sAsks = " " + Markets.Coins[sCoin][sMarket].Ask.toStr8P(iP) +
                sBaseM + (100 * dAskDelta).toStr2() + "%" + (dAskDelta < 0 ? "↓" : "↑");

              string sBids = " " + Markets.Coins[sCoin][sMarket].BidAvg.toStr8P(iP) +
                sBaseM + Markets.Coins[sCoin][sMarket].AskAvg.toStr8P(iP);
              bg.Graphics.DrawString(Markets.Coins[sCoin][sMarket].UpdateCount.toInt32T().toString(),
                fCur7, abm, Convert.ToSingle(iLeft - (iCW / 2)), Convert.ToSingle(iRow));
              bg.Graphics.DrawString(sMarket, fCur7, abm, Convert.ToSingle(iLeft), Convert.ToSingle(iRow));
              bg.Graphics.DrawString(sPrice, fCur7, sba, Convert.ToSingle(iLeft + (iCW * 1.25)), Convert.ToSingle(iRow));
              bg.Graphics.DrawString(sBids, fCur7, sbb, Convert.ToSingle(iLeft + (iCW * 6.5)), Convert.ToSingle(iRow));

              iRow = Convert.ToSingle(iRow + iRowH);
              if (iRow > edOutContainer.Top) break;
            }
            if (iRow > edOutContainer.Top) break;
          }

          #endregion
          es = "300";
          #region graph code

          if (UpdateNo > 5) { 
            Pen aG = new Pen(ColorTranslator.FromHtml("#0058B8"), 1);
            Int32 iMR = 0;  // Market Ranger counts up 
            Int32 iCurCoin = 0;
            string[] ByAvgChange = Markets.Coins.ByAvgChange();
            es = "301";
            foreach (string sCoin in ByAvgChange) {
              iCurCoin += 1;
              if (iCurCoin > 3) break;
              es = "302";
              string[] KeysByPrice = Markets.Coins[sCoin].KeysByPriceDelta();
              Color aC = Markets.Coins[sCoin].CoinColor;

              es = "303";
              foreach (string sMarket in KeysByPrice) {
                iMR++;
                CAvgDecimalCache aPDC = Markets.Coins[sCoin][sMarket].PriceDeltaCache;
                CAvgDecimalCache aUCC = Markets.Coins[sCoin][sMarket].UpdateCountCache;

                if (sCoin == sMarket.ParseFirst("-")) {
                  aC = Markets.Coins[sCoin].CoinColor2;
                }
                Pen aP = new Pen(aC, 1);

                decimal dChange = 0;
                decimal dValue = 0;
                decimal dPCMa = ((PriceChangeMax * 1.05m) - (PriceChangeMin * 1.05m));
                if (dPCMa == 0) { dPCMa = 1; }
                decimal HeightRatio = grHeight / dPCMa;
                es = "331 "+sMarket;
                if (iCurCoin == 1) {  //
                  //chart middle row
                  bg.Graphics.DrawLine(aG, grLeft.toFloat(), (grTop + grHeight / 2).toFloat(), (grLeft + grWidth).toFloat(), (grTop + grHeight / 2).toFloat());
                  //minor PriceChangeMax Min lines with lables.
                  bg.Graphics.DrawLine(aG, grLeft.toFloat(), (grTop + grHeight / 2 + (PriceChangeMax * HeightRatio)).toFloat(), (grLeft + grWidth).toFloat(), (grTop + grHeight / 2 + (PriceChangeMax * HeightRatio)).toFloat());
                  bg.Graphics.DrawString(PriceChangeMax.toStr4(), fCur6, Brushes.LightGray, grLeft.toFloat(), (grTop + grHeight / 2 + (PriceChangeMax * HeightRatio)).toFloat());
                  bg.Graphics.DrawLine(aG, grLeft.toFloat(), (grTop + grHeight / 2 + (PriceChangeMin * HeightRatio)).toFloat(), (grLeft + grWidth).toFloat(), (grTop + grHeight / 2 + (PriceChangeMin * HeightRatio)).toFloat());
                  bg.Graphics.DrawString(PriceChangeMin.toStr4(), fCur6, Brushes.LightGray, grLeft.toFloat(), (grTop + grHeight / 2 + (PriceChangeMin * HeightRatio)).toFloat());
              
                }

                Int32 iSpot = 0;
                List<Point> lp = new List<Point>();
                Int32 PDCount = aPDC.Keys.Count - 1;
                decimal dHR2 = grHeightT / 40;
                foreach (Int64 key in aUCC.Keys.OrderBy(x => x)) {
                  dChange = (decimal)aUCC[key];
                  Int32 x = Convert.ToInt32(grLeft + ((grWidth / 24.25m) * (iSpot + 0.125m)) + (iMR * 2));
                  Int32 y = Convert.ToInt32(grTop - (dChange * dHR2));

                  bg.Graphics.DrawLine(aP, x, grTop.toInt32() - 1, x, y - 1);
                  bg.Graphics.DrawLine(aP, x + 1, grTop.toInt32() - 1, x + 1, y - 1);

                  iSpot += 1;
                }

                es = "332";

                iSpot = 0;
                lp.Clear();
                foreach (Int64 key in aPDC.Keys.OrderBy(x => x)) {
                  if (iSpot > 23) break;
                  es = "332a";
                  dChange = (decimal)aPDC[key];
                  dValue += dChange;
                  es = "332b";
                  Int32 x = Convert.ToInt32(grLeft + ((grWidth / 24.25m) * (iSpot + 0.125m)));
                  Int32 y = Convert.ToInt32((grTop + (grHeight / 2)) + (dValue * HeightRatio));
                  lp.Add(new Point(x, y));
                  bg.Graphics.DrawLine(Pens.WhiteSmoke, x, grTop.toInt32(), x, (grTop + 4).toInt32());

                  if ((iSpot > 2) && (iSpot == PDCount - 2)) {
                    bg.Graphics.DrawString(sMarket, fCur6, Brushes.LightGray, x, y);
                  }
                  iSpot++;
                  es = "332c";
                }
                es = "332d";
                if (lp.Count>1) bg.Graphics.DrawLines(aP, lp.ToArray());

                es = "333";

              }
            }
          }
          #endregion
          es = "400";
          bg.Graphics.DrawString(DateTime.Now.ToStrDateMM() +
            " " + UpdateNo.toString() +
            "-" + NextUpdateAvg.Value.Subtract(DateTime.Now).TotalSeconds.toInt32().ToString() + " s:" +  " "+ LastMessage , 
            fCur8, Brushes.WhiteSmoke, Convert.ToSingle(fWidth * 0.015), Convert.ToSingle(fHeight * 0.015));

          if ((iOpMode>=10)&&(BuyQuote != "")) {
            Brush abmm = new SolidBrush(ColorTranslator.FromHtml("#000B17"));
            bg.Graphics.FillRectangle(abmm, new Rectangle(
             Convert.ToInt32(iLeft + iCW * 3),
             Convert.ToInt32(grTop),
             Convert.ToInt32(iCW * 20),
             Convert.ToInt32(iRowH * 20)
             ));
            bg.Graphics.DrawRectangle(Pens.WhiteSmoke, new Rectangle(
               Convert.ToInt32(iLeft + iCW * 3),
               Convert.ToInt32(grTop),
               Convert.ToInt32(iCW * 20),
               Convert.ToInt32(iRowH * 20)
             ));
            string sOptions = Positions.GetValidBaseByQuote(BuyQuote);
            string sOut = (iOpMode==10? " Buy " + BuyQuote + " with " + BuyBase : " Sell " + BuyQuote + " for " + BuyBase )+" or "+sOptions;
            bg.Graphics.DrawString( sOut, fCur8, Brushes.WhiteSmoke, 
              Convert.ToSingle(iLeft + iCW*3.5), Convert.ToSingle(grTop + iRowH.toDecimal() * 0.5m));

            bg.Graphics.DrawString(" Quantity " + BuyQuote,
              fCur8, Brushes.WhiteSmoke, Convert.ToSingle(iLeft + iCW * 10), Convert.ToSingle(grTop + iRowH.toDecimal() * 4));

            bg.Graphics.DrawString(" Price "+BuyBase ,
              fCur8, Brushes.WhiteSmoke, Convert.ToSingle(iLeft + iCW * 10), Convert.ToSingle(grTop + iRowH.toDecimal() * 7));

            decimal dTotal =  edQuantity.Value * edLastPrice.Value;
            bg.Graphics.DrawString(" Total  "+ dTotal.toStr8() + BuyBase,
              fCur8, Brushes.WhiteSmoke, Convert.ToSingle(iLeft + iCW * 10), Convert.ToSingle(grTop + iRowH.toDecimal() * 9));

            decimal dTradeFee = dTotal * TradeFee;  
            bg.Graphics.DrawString(" Fee "+ (TradeFee*100).toStr4()+"%  " + dTradeFee.toStr8() + BuyBase,
              fCur8, Brushes.WhiteSmoke, Convert.ToSingle(iLeft + iCW * 10), Convert.ToSingle(grTop + iRowH.toDecimal() * 10));

            bg.Graphics.DrawString(" Est (Total - fee): " + (dTotal - dTradeFee).toStr8() + BuyBase,
              fCur8, Brushes.WhiteSmoke, Convert.ToSingle(iLeft + iCW * 10), Convert.ToSingle(grTop + iRowH.toDecimal() * 11));

          }
          
          es = "500";
          #region draw positions
          iCount = 0;
          iRow = edOutContainer.Top;
          iWM =  iCoinCount!=0 ?(Width - iLeft.toDecimal()- 4) / iCoinCount : 0;
          PositionButtons NewPosButs = new PositionButtons();
          foreach (string sCoin in Markets.Coins.Keys.OrderBy(x => x)) {
            decimal aBal = Positions.Balance(sCoin);
            if ( aBal > 0.0005m) {
              Color aC = Markets.Coins[sCoin].CoinColor;
              Pen aP = new Pen(aC, 1);
              Brush abmm = new SolidBrush(aC);
              NewPosButs[sCoin] = new PositionButton(
                new Rectangle(
                  Convert.ToInt32(iLeft.toDecimal() + iWM * iCount - (iCW / 2).toDecimal()),
                  Convert.ToInt32(edOutContainer.Top - grHeightT),
                  Convert.ToInt32(iWM - 2),
                  Convert.ToInt32(grHeightT - iRowH.toDecimal() / 2)
                ), sCoin);
              bg.Graphics.DrawRectangle(aP, NewPosButs[sCoin].Location );
              Decimal dPricePaid = Positions.PricePaidUSD(sCoin);
              Decimal dAvgPriceUSD = Markets.Coins[sCoin].AvgPriceUSD;
              Decimal dAvgChange = sCoin=="USD" ? 1 :(aBal * dAvgPriceUSD) - (aBal * dPricePaid); 

              Brush sbaa = (System.Math.Abs(dAvgChange) < 0.0005m ? Brushes.WhiteSmoke : (
                (dAvgChange < 0) ? Brushes.Red : Brushes.Chartreuse));
              sRow = "      " + dAvgPriceUSD.toStr4()+"  "+dAvgChange.toStr4();
              bg.Graphics.DrawString(sRow, fCur8, sbaa, 
                Convert.ToSingle(iLeft.toDecimal() + iWM * iCount - (3 * iCW / 8).toDecimal()), 
                Convert.ToSingle(iRow - (iRowH * 4)));

              bg.Graphics.DrawString(sCoin, fCur8, abmm, 
                Convert.ToSingle(iLeft.toDecimal() + iWM * iCount - (3 * iCW / 8).toDecimal()), 
                Convert.ToSingle(iRow - (iRowH * 4)));

              bg.Graphics.DrawString(aBal.toStr8() + " " + sCoin, fCur8, Brushes.WhiteSmoke, 
                Convert.ToSingle(iLeft.toDecimal() + iWM * iCount - (3 * iCW / 8).toDecimal()), 
                Convert.ToSingle(iRow - (iRowH * 3)));

              bg.Graphics.DrawString(dPricePaid.toStr4()+"  "+ (aBal*dPricePaid).toStr4() , fCur8, Brushes.WhiteSmoke, 
                Convert.ToSingle(iLeft.toDecimal() + iWM * iCount - (3 * iCW / 8).toDecimal()), 
                Convert.ToSingle(iRow - (iRowH * 2)));
              
              iCount += 1;
            }
          }
          PosButs = NewPosButs;

          #endregion

          bg.Render(g);
        } catch (Exception e) {
          e.toAppLog("Refresh " + es);
          setTradeMsg(e.toWalkExcTreePath());
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

        if (iDisplayMode == 10)
        {
            if ((textBox1.Text != "") && (textBox2.Text != "") && (textBox3.Text != ""))
            {
                try
                {
                    es = "1";
                    Settings = new SecureStore(textBox1.Text, SettingsFileName);
                    sPub = textBox2.Text;
                    sPri = textBox3.Text;
                    es = "2";
                    Settings[SettingsKey] = sPub + " " + sPri;
                    es = "3";

                    iDisplayMode = 30;

                }
                catch (Exception ea)
                {
                    throw ea.toAppLog("btnContinue " + es);
                }
            }
            else
            {
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

        if (iDisplayMode == 30)
        {
            if (!timer1.Enabled) timer1.Enabled = true;
        }

        DoUpdateControlVisibility();
    }

    void DoTickersLandingAdd(CryptoExchange.Net.Sockets.DataEvent<BittrexTick> aObj) {      
      TickersLanding.AddTic(aObj.Data);
    }

    #region Update Timer
    DateTime theNow;
    //   DateTime? NextGoTime = null;
    DateTime? NextUpdateAvg = null;
    Boolean FirstTimeLoad = true;
    Int64 UpdateNo = 0;
    private async void timer1_Tick(object sender, EventArgs e) {
      timer1.Enabled = false;

      theNow = DateTime.Now;

      if (FirstTimeLoad)  { // only first time now with tickers going.

        string kpPolo = Settings[SettingsKey];
        string sPub = kpPolo.ParseString(" ", 0);
        string sPri = kpPolo.ParseString(" ", 1);

        if (FirstTimeLoad) {
          FirstTimeLoad = false;
          BittrexSocketClientOptions socOptions = new BittrexSocketClientOptions();
          socOptions.ApiCredentials = new ApiCredentials(sPub, sPri);
          BSC = new BittrexSocketClient(socOptions);
          foreach(string sMarket in MarketFilterBittrex.Keys) {
            var aResult = await BSC.SubscribeToSymbolTickerUpdatesAsync(sMarket, data => { DoTickersLandingAdd(data);});
            if (!aResult.Success) {
              setLogMessage(sMarket + " subscribe failed: " + aResult.Error);
            }
          }
        }

      }    

      if (NextUpdateAvg.isNull() || (theNow > NextUpdateAvg)) {
        NextUpdateAvg = theNow.AddSeconds(15);
        if (!FirstTimeLoad) {
          UpdateNo += 1;
          foreach (string sMarket in MarketFilter.Keys) {
            string sBaseCur = sMarket.ParseFirst("-");
            Markets[sMarket].AdvanceAverages();
            Markets.Coins[sBaseCur][sMarket].AdvanceAverages();
          }
        }
      }

      DoRedraw();    
         
      timer1.Enabled = true;

    }

    #endregion

    private void TradeLastTo(string TargetCur, Boolean UseStops) {
      string TargetMarket = "";
      string TargetOp = "";


      if (LastCur == "ADA") {
        TargetMarket = TargetCur + "-ADA";
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
          TargetMarket = "BTC-" + TargetCur;
          TargetOp = "Buy";
        }
      } else {
        TargetMarket = "USD-" + LastCur;
        TargetOp = LastCur == "USD" ? "Sell" : "Buy";
      }

      string BaseCur = TargetMarket.ParseFirst("-");
      string QuoteCur = TargetMarket.ParseLast("-");

      if (TargetOp == "Buy") {
        decimal BaseHolding = LastAmount;
        decimal TheFee = BaseHolding * TradeFee;
        LastPrice = Markets[TargetMarket].Bid;
        decimal LastBasePriceUSD = Markets.ToUSD(BaseCur, 1);
        decimal USDLastPrice = Markets.ToUSD(BaseCur, LastPrice);
        if ((BaseCur != "USD") && (LastBasePriceUSD < ExitMin) && (LastBasePriceUSD > StopPrice)) {
          if (UseStops) {
            string sError = "Except Price " + LastBasePriceUSD.toStr8() + " in " + StopPrice.toStr8() + " " + ExitMin.toStr8();
            throw new Exception(sError);
          }
        }
        decimal QuantityToBuy = (BaseHolding - TheFee) / LastPrice;
        if (BaseCur != "USD") {
          setTradeMsg(BaseCur +
            ((LastBasePriceUSD < StopPrice) ? " Stoped at $" : " Sold at $") + LastBasePriceUSD.toStr8() + ((LastBasePriceUSD < StopPrice) ? " Stop was $" + StopPrice.toStr8() : "Exit was $" + ExitMin.toStr8())
          );
        }
        setTradeMsg(DateTime.Now.ToStrDateMM() +
          " Buy " + QuantityToBuy.toStr8() + " " + TargetCur +
          " at " + LastPrice.toStr8() + " " + BaseCur + " " + USDLastPrice.toStr8() + "USD" +
          " for " + LastAmount.toStr8() + " " + LastCur +
          " " + Markets.ToUSD(LastCur, LastAmount).toStr4());
        LastAmount = QuantityToBuy;
        StopPrice = USDLastPrice * (1 - (TradeFeeStopM * TradeFee));
        ExitMin = USDLastPrice * (1 + (TradeFeeExitM * TradeFee));
        LastPrice = USDLastPrice;

      } else {

        decimal tp = Markets.ToUSD(BaseCur, Markets[TargetMarket].Ask);
        if ((tp < ExitMin) && (tp > StopPrice)) {
          if (UseStops) {
            string sError = "Except Price " + tp.toStr8() + " in " + StopPrice.toStr8() + " " + ExitMin.toStr8();
            throw new Exception(sError);
          }
        }
        decimal QuoteHolding = LastAmount;
        LastPrice = Markets[TargetMarket].Ask;  // current asking price in BaseCur
        decimal USDLastPrice = Markets.ToUSD(BaseCur, LastPrice);  // current ask in usd
        decimal SellResult = (QuoteHolding * LastPrice); // find sell quote in BaseCur
        decimal TheFee = (SellResult * TradeFee); // Find Fee in BaseCur
        SellResult -= TheFee; // subtract fee from Result.
        setTradeMsg(BaseCur +
          ((tp < StopPrice) ? " Stoped $" : " Sold $") + tp.toStr8() + ((tp < StopPrice) ? " Stop was $" + StopPrice.toStr8() : "Exit was $" + ExitMin.toStr8())
        );

        setTradeMsg(DateTime.Now.ToStrDateMM() +
          ((tp < StopPrice) ? " Stop " : " Sell ") + LastAmount.toStr8() + " " + LastCur +
          " at " + LastPrice.toStr8() + " " + BaseCur + " $" + USDLastPrice.toStr8() + "USD" +
          " for " + SellResult.toStr8() + " " + BaseCur + " $" + Markets.ToUSD(BaseCur, SellResult).toStr4());
        LastAmount = SellResult;
        if (BaseCur != "USD") {     // BaseCur can also be sold for usd need to reset exit limits. 
          LastPrice = 1 / LastPrice;  // convert the price to be in Quote cur instead of base
          StopPrice = Markets.ToUSD(QuoteCur, LastPrice * (1 - (TradeFeeStopM * TradeFee)));
          ExitMin = Markets.ToUSD(QuoteCur, LastPrice * (1 + (TradeFeeExitM * TradeFee)));
          LastPrice = Markets.ToUSD(QuoteCur, LastPrice);
        }
      }
      LastCur = TargetCur;


    }

    private void cbTrack_CheckedChanged(object sender, EventArgs e) {

        if (cbTrack.Checked) {
            LastAmount = edQuantity.Value;
            LastPrice = edLastPrice.Value;
            StopPrice = Markets.ToUSD("USD", LastPrice * (1 - (TradeFeeStopM * TradeFee)));
            ExitMin = Markets.ToUSD("USD", LastPrice * (1 + (TradeFeeExitM * TradeFee)));
            edQuantity.Visible = false;
            edLastPrice.Visible = false;
        } else {
            edQuantity.Value = LastAmount;
            edLastPrice.Value = LastPrice;
            edQuantity.Visible = true;
            edLastPrice.Visible = true;
        }
    }


    private void btnExit_Click(object sender, EventArgs e)
    {
        if ((cbTrack.Checked) && (Markets.Coins.CurUpCoin != LastCur) && (LastCur != "USD"))
        {
            try
            {
                TradeLastTo("USD", false);
            }
            catch { }
        } else if ((cbTrack.Checked)&&(LastCur=="USD")&&(Markets.Coins.CurUpCoin!= "USD")) {
        try {
          TradeLastTo(Markets.Coins.CurUpCoin, false);
          } catch { }
        }
    }

    private void btnBuy_Click(object sender, EventArgs e) {
      try { 
        if (iOpMode == 10) { 
          Positions.BuyAsset(BuyQuote, BuyBase + '-' + BuyQuote, edQuantity.Value);
        } else if (iOpMode==11) {
          Positions.BuyAsset(BuyBase, BuyBase + '-' + BuyQuote, edQuantity.Value);
        }      
        
        BuyQuote = "";
        BuyBase = "";
        iOpMode = 0;
      } catch (Exception ex0) {
        setTradeMsg(ex0.toWalkExcTreePath());
      }
    }


    private void edLastPrice_ValueChanged(object sender, EventArgs e) {
      decimal dBB = Positions.Balance(BuyBase);
      dBB = dBB - dBB * Markets.TradeFee; 
      edQuantity.Value = dBB / edLastPrice.Value;
    }

    private void textBox1_KeyUp(object sender, KeyEventArgs e) {
      if (e.KeyCode == Keys.Enter) { btnContinue_Click(sender, null); }
    }

    private void edQuantity_ValueChanged(object sender, EventArgs e) {
   //   decimal dBB = Positions.Balance(BuyBase);      
   //   edLastPrice.Value = dBB / edQuantity.Value;
    }

   

  }

 

 





}
