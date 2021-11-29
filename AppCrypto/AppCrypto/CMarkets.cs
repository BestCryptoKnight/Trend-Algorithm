using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using StaticExtensions;

namespace AppCrypto {
  public class CMarket : CObject {
    public CMarkets Owner;
    public string MarketName { 
      get { return base["MarketName"].toString(); } 
      set { base["MarketName"] = value; } 
    }
    public string QuoteCur { get { return base["MarketName"].toString().ParseLast("-"); } }
    public string BaseCur { get { return base["MarketName"].toString().ParseFirst("-"); } }

    public void AdvanceAverages() {
      if (UpdateCount > 0) { 
        Ask = Ask;
        Bid = Bid;
        UpdateCount = 0;
      }
    }
    public decimal Ask {
      get {
        return ((CAvgDecimalCache)base["Ask"]).toDecimal();
      }
      set {
        if(value != 0) { 
          CAvgDecimalCache aAsk = (CAvgDecimalCache)base["Ask"];
          aAsk.Add(value);
          decimal aAvg = aAsk.toAvg();
          this.AskAvg = aAvg;
          this.AskDelta = (aAvg == 0 ? 0 :
            ((Ask / aAvg > 1) ? Ask / aAvg - 1 : 
              (1 - (Ask / aAvg)) * -1));
        }
      }
    }

    public void UpdateAsk(decimal NewAsk) {
      CAvgDecimalCache aAsk = ((CAvgDecimalCache)base["Ask"]);
      CAvgDecimalCache aAskAvg = ((CAvgDecimalCache)base["AskAvg"]);
      CAvgDecimalCache aAskDelta = ((CAvgDecimalCache)base["AskDelta"]);
      CAvgDecimalCache aAvgPrice = ((CAvgDecimalCache)base["AvgPrice"]);
      aAsk[aAsk.Nonce] = NewAsk;
      decimal aAskG = aAsk.toAvg();
      aAskAvg[aAskAvg.Nonce] = aAskG;
      aAskDelta[aAskDelta.Nonce] = (aAskG == 0 ? 0 : ((NewAsk / aAskG > 1) ? NewAsk / aAskG - 1 : (1 - (NewAsk / aAskG)) * -1));
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
      }
      set {
        CAvgDecimalCache aBid = ((CAvgDecimalCache)base["Bid"]);
        aBid.Add(value);
        decimal aAvg = aBid.toAvg();
        this.BidAvg = aAvg;
        this.BidDelta = (aAvg == 0 ? 0 : ((Bid / aAvg > 1) ? Bid / aAvg - 1 : (1 - (Bid / aAvg)) * -1));
        this.PriceDelta = (AskDelta + BidDelta) / 2;
        this.AvgPrice = (this.Bid + this.Ask) / 2;
      }
    }

    public void TrimAvgCache(int howMany) {

      CAvgDecimalCache aAsk = ((CAvgDecimalCache)base["Ask"]);
      CAvgDecimalCache aAskAvg = ((CAvgDecimalCache)base["AskAvg"]);
      CAvgDecimalCache aAskDelta = ((CAvgDecimalCache)base["AskDelta"]);

      CAvgDecimalCache aBid = ((CAvgDecimalCache)base["Bid"]);
      CAvgDecimalCache aBidAvg = ((CAvgDecimalCache)base["BidAvg"]);
      CAvgDecimalCache aBidDelta = ((CAvgDecimalCache)base["BidDelta"]);
      
      CAvgDecimalCache aAvgPrice = ((CAvgDecimalCache)base["AvgPrice"]);
      CAvgDecimalCache aPriceDelta = ((CAvgDecimalCache)base["PriceDelta"]);
      
      for(int i = 1; i<howMany; i++) {
        aAsk.Pop();
        aAskAvg.Pop();
        aAskDelta.Pop();
        aBid.Pop();
        aBidAvg.Pop();
        aBidDelta.Pop();
        aAvgPrice.Pop();
        aPriceDelta.Pop();
        ((CAvgDecimalCache)base["UpdateCount"]).Pop();
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
      aBidDelta[aBidDelta.Nonce] = (aBidA == 0 ? 0 : ((NewBid / aBidA > 1) ? NewBid / aBidA - 1 : (1 - (NewBid / aBidA)) * -1));
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

    public CAvgDecimalCache PriceDeltaCache {
      get {
        return ((CAvgDecimalCache)base["PriceDelta"]);
      }
    }


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
      get { return ((CAvgDecimalCache)base["LastAvg"]).toDecimal(); }
      set { ((CAvgDecimalCache)base["LastAvg"]).Add(value); }
    }
    public decimal UpdateCount {
      get {
        return ((CAvgDecimalCache)base["UpdateCount"]).toDecimal();
      }
      set {
        ((CAvgDecimalCache)base["UpdateCount"]).Add(value);
      }
    }

    public decimal FindQuoteAmountToBuy( decimal aBaseAmount ) {
      decimal a = Ask;       
      decimal r = (aBaseAmount - (aBaseAmount * Owner.TradeFee)) / a;
     // decimal aDiff = aBaseAmount - (r * a + r * a * Owner.TradeFee);
      return r; //+ ((aDiff > 0.00001999m) ? FindQuoteAmountToBuy(aDiff):0);
    }

    //sell aka aQuoteAmount
    public decimal FindBaseAmountToBuy(decimal aQuoteAmount) {
      decimal a = Bid;
      decimal r = a * aQuoteAmount - a* aQuoteAmount* Owner.TradeFee;      
      //decimal aDiff = aQuoteAmount - (r * (1+ Owner.TradeFee))/a;
      return r; //+ ((aDiff > 0.00001999m) ? FindBaseAmountToBuy(aDiff) : 0);
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

    public CMarket(CMarkets aOwner, string sMarket) {
      Owner = aOwner;
      MarketName = sMarket;
      int aSize = 24;
      CAvgDecimalCache adc = new CAvgDecimalCache { Size = 24 };
      base["Ask"] = adc;

      adc = new CAvgDecimalCache { Size = aSize };
      base["AskAvg"] = adc;

      adc = new CAvgDecimalCache { Size = aSize };
      base["AskDelta"] = adc;

      adc = new CAvgDecimalCache { Size = aSize };
      base["Bid"] = adc;

      adc = new CAvgDecimalCache { Size = aSize };
      base["BidAvg"] = adc;

      adc = new CAvgDecimalCache { Size = aSize };
      base["BidDelta"] = adc;

      adc = new CAvgDecimalCache { Size = aSize };
      base["Last"] = adc;

      adc = new CAvgDecimalCache { Size = aSize };
      base["LastAvg"] = adc;

      adc = new CAvgDecimalCache { Size = aSize };
      base["AvgPrice"] = adc;

      adc = new CAvgDecimalCache { Size = aSize };
      base["PriceDelta"] = adc;

      adc = new CAvgDecimalCache { Size = aSize };
      base["UpdateCount"] = adc;

    }

  }

  public class CInvMarket : CMarket {
    public CInvMarket(CMarkets aOwner, string aMarket) : base(aOwner, aMarket) {
    }
    new public decimal Ask {
      get {
        return ((CAvgDecimalCache)base["Ask"]).toDecimal();
      }
      set {
        CAvgDecimalCache aAsk = ((CAvgDecimalCache)base["Ask"]);
        aAsk.Add(1 / value);
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
        aBid.Add(1 / value);
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
        aLast.Add(1 / value);
        if (aLast.Count > 1) {
          this.LastAvg = aLast.toAvg();
        }
      }
    }
  }

  public class CMarketList : CObject {
    public CMarkets Owner;
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
      Color a;
      switch (aCoin) {
        case "ADA": a = ColorTranslator.FromHtml("#2291FF"); break;
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

    public decimal AvgPriceUSD {
      get {
        Decimal r = 0, x, c;
        if (Coin == "USD") { return 1;}
        else if (Coin == "BTC") { return Owner.Coins["BTC"]["USD-BTC"].Ask; }
        else { 
          foreach (string sMarket in base.Keys) {
            string sBaseCur = sMarket.ParseFirst("-");
            string sQuoteCur = sMarket.ParseLast("-");
            if ( sBaseCur != "USD") {             
              x = Owner.ToUSD( sBaseCur,  Owner.Coins[sQuoteCur][sMarket].Ask );  
            } else {           
              x = ((CMarket)base[sMarket]).Ask;
            }
            r += x;
          }
          return ((base.Keys.Count > 0) ? r / base.Keys.Count : 0);
        }
      }
    }
    public Decimal AvgChange {
      get {
        Decimal r = 0;
        foreach (string sMarket in base.Keys) {
          r += ((CMarket)base[sMarket]).PriceDelta;
        }
        return ((base.Keys.Count > 0) ? r / base.Keys.Count : 0);
      }
    }
    public Decimal UpdateCount {
      get {
        Decimal r = 0;
        foreach (string sMarket in base.Keys) {
          r += ((CMarket)base[sMarket]).UpdateCount;
        }
        return r;
      }
    }
    public CMarketList(CMarkets aOwner, string aCoin) : base() {
      Owner = aOwner;
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
    public CMarketCoins() : base() { }
    public new CMarketList this[string aCoin] {
      get { try { return (base[aCoin] is object ? (CMarketList)base[aCoin] : null); } catch { return null; } }
      set { base[aCoin] = value; }
    }
    public string[] ByAvgChange() {
      return base.Keys.OrderByDescending(x => ((CMarketList)base[x]).AvgChange).ToArray();
    }
    public string CurUpCoin {
      get {
        return ByAvgChange()[0];
      }
    }
  }

  public class CMarkets : CObject {
    public decimal TradeFee = 0.0025m;
    public CObject MarketFilter;
    public CMarketCoins Coins;
    public CMarkets(CObject aMarketFilter) : base() {
      MarketFilter = aMarketFilter;
      Coins = new CMarketCoins();

      foreach (string sMarket in MarketFilter.Keys) {
        CMarket aM = new CMarket(this, sMarket);
        base[sMarket] = aM;
        if (!(Coins[aM.QuoteCur] is CMarketList)) Coins[aM.QuoteCur] = new CMarketList(this, aM.QuoteCur);
        Coins[aM.QuoteCur][sMarket] = aM;
        if (!(Coins[aM.BaseCur] is CMarketList)) Coins[aM.BaseCur] = new CMarketList(this, aM.BaseCur);
        Coins[aM.BaseCur][sMarket] = new CInvMarket(this, sMarket);
      }
    }
    public new CMarket this[string aKey] {
      get { return (CMarket)base[aKey]; }
      set { base[aKey] = value; }
    }


    public Decimal BTCtoUSD(Decimal aBTCValue) {
      //+ (this["USD-BTC"].Ask - this["USD-BTC"].Bid) / 2
      return (this["USD-BTC"].Bid ) * aBTCValue;
    }
    public Decimal ETHtoUSD(Decimal aETHValue) {
      //+ (this["USD-ETH"].Ask - this["USD-ETH"].Bid) / 2)
      return (this["USD-ETH"].Bid)  * aETHValue;
    }
    public Decimal ADAtoUSD(Decimal aADAValue) {
      //+ (this["USD-ADA"].Ask - this["USD-ADA"].Bid) / 2
      return (this["USD-ADA"].Bid ) * aADAValue;
    }

    public Decimal ToUSD(string aCur, Decimal aCurValue) {
      Decimal aRet;
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
            //+ (this[sMarkAttempt].Ask - this[sMarkAttempt].Bid) / 2
            aRet = (this[sMarkAttempt].Bid ) * aCurValue;
          } else {
            sMarkAttempt = "BTC-" + aCur;
            if (this[sMarkAttempt] is CMarket) {
              //+ (this[sMarkAttempt].Ask - this[sMarkAttempt].Bid) / 2
              aRet = BTCtoUSD((this[sMarkAttempt].Bid ) * aCurValue);
            } else {
              sMarkAttempt = "ETH-" + aCur;
              if (this[sMarkAttempt] is CMarket) {
                //+ (this[sMarkAttempt].Ask - this[sMarkAttempt].Bid) / 2
                aRet = ETHtoUSD((this[sMarkAttempt].Bid ) * aCurValue);
              } else throw new Exception("Unknown Currency " + aCur);
            }
          }
          break;
      }
      return aRet;
    }

  }


}
