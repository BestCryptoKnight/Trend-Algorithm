using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StaticExtensions;

namespace AppCrypto {

  public enum PositionStatus { open, closed }
  public class CPosition {
    public Int64 SelfID = Int64.MaxValue; 
    public CPositions Owner;
    public PositionStatus Status;
    public DateTime? Opened = null;
    private DateTime? fClosed = null;
    public DateTime? Closed { 
      get { return fClosed; }
      set { 
        fClosed = value; 
      } 
    }
    public string Asset;
    public decimal PriceUSDEst;
    public decimal PriceBaseCur;
    public decimal FeeBaseCur;
    public decimal FeeUSDEst;
    public decimal Quantity;
    public decimal TotalUSD { get { return PriceUSDEst * Quantity + FeeUSDEst;} }
    public decimal TotalBaseCur {get { return PriceBaseCur * Quantity + FeeBaseCur;} }
    public string sSPTemp = "";
    public CPositions SourcePositions;
    public string sETemp = "";
    public CPositions Exits;
    public CPosition(CPositions aOwner, string aAsset, decimal aPriceUSDEst, decimal aPriceBaseCur,
      decimal aFeeBaseCur, decimal aFeeUSDEst, decimal aQuantity ) {
      Owner = aOwner;
      Status = PositionStatus.open;
      DateTime Opened = DateTime.Now;
      Asset = aAsset;
      PriceUSDEst = aPriceUSDEst;
      PriceBaseCur = aPriceBaseCur;
      FeeBaseCur = aFeeBaseCur;
      FeeUSDEst = aFeeUSDEst;
      Quantity = aQuantity;

      SourcePositions = new CPositions(Owner.Markets, "");
      Exits = new CPositions(Owner.Markets,"");
    }

    public static CPosition Load(CPositions aOwner, String sPack) {
      string sSelfID = sPack.ParseString(" ", 0);
      string sStatus = sPack.ParseString(" ", 1);
      string sAsset = sPack.ParseString(" ", 2);
      string sQuantity = sPack.ParseString(" ", 3);
      string sPBC = sPack.ParseString(" ", 4);
      string sPUS = sPack.ParseString(" ", 5);
      string sFBC = sPack.ParseString(" ", 6);
      string sFUS = sPack.ParseString(" ", 7);
      string sOpened = sPack.ParseString(" ", 8);
      string sClosed = sPack.ParseString(" ", 9);
      string sSource = sPack.ParseString(" ", 10).toBase64DecryptStr();
      string sExits = sPack.ParseString(" ", 11).toBase64DecryptStr();
      CPosition r = new CPosition(aOwner, sAsset, sPUS.toDecimal(), sPBC.toDecimal(), sFBC.toDecimal(), sFUS.toDecimal(), sQuantity.toDecimal());
      r.SelfID = sSelfID.toInt64();
      r.Status = (sStatus=="open"?PositionStatus.open:PositionStatus.closed);
      r.sSPTemp = sSource=="NULL"?"":sSource;
      r.sETemp = sExits=="NULL"?"": sExits;
      if (sOpened == "NULL") {
        r.Opened = null;
      } else {
        r.Opened = sOpened.toDateTime();
      }
      if (sClosed == "NULL") { 
        r.Closed = null;
      } else {
        r.Closed = sClosed.toDateTime();
      }
      return r; 
     }

    public string Pack() {
      string n = "NULL".toBase64EncryptStr();
      string r = SelfID.toString() + " " +
        (Status == PositionStatus.open ? "open ":"closed " )+
        Asset + " " +
        Quantity.toStr8() + " " +
        PriceBaseCur.toStr8()+ " "+ PriceUSDEst.toStr8() + " " + 
        FeeBaseCur.toStr8()+ " "+ FeeUSDEst.toStr8()+" "+
        (Opened.isNull() ? n : Opened.toString().toBase64EncryptStr())+" "+
        (Closed.isNull() ? n : Closed.toString().toBase64EncryptStr());
      string Sources = "";
      string sExits = "";
      foreach (long x in SourcePositions.Keys) {
        Sources = Sources +" "+ SourcePositions[x].SelfID.toString();
      }
      foreach (long x in Exits.Keys) {
        sExits = sExits + " " + Exits[x].SelfID.toString();
      }     
      
      r = r + " "+ (Sources==""? n : Sources.toBase64EncryptStr())+ 
              " "+ (sExits==""? n : sExits.toBase64EncryptStr());
      return r;
    }


  }
  public class CPositions : CQueue {

    public CMarkets Markets;
    public string PositionFileName;
    public CPositions(CMarkets aMarkets, string aFileName) : base() {
      Markets = aMarkets;
      PositionFileName = aFileName;
      if (File.Exists(PositionFileName)) {        
        Load();
      }
    }

    public void Save() {
      if (Directory.Exists( Path.GetDirectoryName(PositionFileName))) {        
        IniFile f = IniFile.FromFile(PositionFileName);
        foreach (long k in base.Keys) {
          f["Positions"]["A"+k.toString()] =  ((CPosition)base[k]).Pack(); 
        }
        f.Save(PositionFileName);        
      }
    }

    public void Load() {
      if (File.Exists(PositionFileName)) {
        IniFile f = IniFile.FromFile(PositionFileName);       
        foreach (string s in  f["Positions"].GetKeys() ) {
          Int64 x = s.ParseFirst("A").toInt64();          
          string l = f["Positions"][s];
          this[x] = CPosition.Load(this, l);
          if (x > Nonce) Nonce = x;
        }
        foreach (long x in base.Keys) {          
          string s = ((CPosition)this[x]).sSPTemp;
          Int32 iC = s.ParseCount(" ");
          if (iC > 0) { 
            for(var i =0; i<iC; i++) {
              long xx = s.ParseString(" ", i).toInt64();
               this[x].SourcePositions[xx]=this[xx];
            } 
          }
          s = ((CPosition)this[x]).sETemp;
          iC = s.ParseCount(" ");
          if (iC > 0) {
            for (var i = 0; i < iC; i++) {
            long xx = s.ParseString(" ", i).toInt64();
            this[x].Exits[xx]=this[xx];
            }
          }
        }
      }
    }

   // public System.Collections.ObjectModel.ReadOnlyCollection<string> getVarNames() {
   //   IniFile f = IniFile.FromFile(PositionFileName); 
   //   return f["Positions"].GetKeys();
   // }

    public CPosition Add(CPosition aObj) {
      Nonce++;
      base[Nonce] = aObj;
      if(aObj.SelfID == Int64.MaxValue) { 
        aObj.SelfID = Nonce;
      }
      return aObj;
    }

    public new CPosition this[Int64 IndexKey] {
      get { try { return (CPosition)base[IndexKey]; } catch { return null; } }
      set { base[IndexKey] = value; }
    }

    public CPosition AddUSD(decimal DepositAmount) {
      return  (CPosition)Add( new CPosition(this, "USD", 1, 1, 0, 0, DepositAmount));      
    }

    public decimal Balance(string aCurrency) {
      return base.Keys.Where(x => (
        (CPosition)base[x]).Asset == aCurrency && 
        ((CPosition)base[x]).Status == PositionStatus.open
      ).ToArray().Sum(x => ((CPosition)base[x]).Quantity);
    }

    public decimal PricePaidUSD(string aCurrency) { 
      decimal TotalQuantity = this.Balance(aCurrency);
      decimal SumTotal = base.Keys.Where(y => (
        (CPosition)base[y]).Asset == aCurrency &&
        ((CPosition)base[y]).Status == PositionStatus.open         
      ).ToArray().Sum(y => ((CPosition)base[y]).TotalUSD );
      return TotalQuantity > 0 ?  SumTotal / TotalQuantity : 0;
    }

    public decimal AvgPrice(string aCurrency) {
      decimal r = 0;
      long[] aPos = base.Keys.Where(x => (
        (CPosition)base[x]).Asset == aCurrency && 
        ((CPosition)base[x]).Status == PositionStatus.open
      ).ToArray();
      CPosition wp;
      decimal aQuantitySum = 0;
      decimal aCostSum = 0;
      foreach (Int64 key in aPos) {
        wp = ((CPosition)base[key]);
        aQuantitySum += wp.Quantity;
        aCostSum += wp.Quantity * wp.PriceBaseCur + wp.FeeBaseCur;
      }
      r = aQuantitySum != 0 ? aCostSum / aQuantitySum : 0;
      return r;
    }

    public string BiggestBaseCoin() {
      string r = "";
      decimal bUSD = Balance("USD");
      decimal bBTC = Markets.ToUSD("BTC", Balance("BTC"));      
      return bUSD > bBTC ? "USD" : "BTC";
    }
    public decimal BalanceUSD { get {
      Int64[] tempList = base.Keys.Where(x => ((CPosition)base[x]).Asset=="USD" && ((CPosition)base[x]).Status == PositionStatus.open).ToArray();
      return tempList.Sum(x => ((CPosition)base[x]).Quantity);  
    } }

    public void BuyAsset(String aAsset, string aMarket, decimal aAssetQuantity) {
      string BaseCur = aMarket.ParseFirst("-");
      string QuoteCur = aMarket.ParseLast("-");
      string PayWithCur = aAsset == BaseCur ? QuoteCur : BaseCur;
      decimal PayWithCurBal = Balance(PayWithCur);
      string TradeOp = PayWithCur == BaseCur ? "Buy" : "Sell";
      decimal aLastPrice = TradeOp == "Buy" ? Markets[aMarket].Ask : Markets[aMarket].Bid;
     
      if (TradeOp == "Buy") {
        decimal aFeeBaseCur = aAssetQuantity * aLastPrice * Markets.TradeFee;
        decimal aAssetCost = aAssetQuantity * aLastPrice + aFeeBaseCur;
        
        //decimal aFeeAdjQuantity = (PayWithCurBal - (PayWithCurBal * Markets.TradeFee)) / aLastPrice;
                
        if (aAssetCost > PayWithCurBal) throw new Exception("insuffcent "+BaseCur+" to make buy.");
          decimal aFeeUSDEst = Markets.ToUSD(BaseCur, aFeeBaseCur);
          decimal aPriceUSDEst = Markets.ToUSD(BaseCur, aLastPrice);

          CPosition NewPosition = (CPosition)Add(new CPosition(this, aAsset, aPriceUSDEst, aLastPrice, aFeeBaseCur, aFeeUSDEst, aAssetQuantity));

          long[] PosToClose = base.Keys.Where(x => (
           (CPosition)base[x]).Asset == PayWithCur && ((CPosition)base[x]).Status == PositionStatus.open
          ).ToArray();
          decimal AmountToFind = aAssetCost;
          CPosition wp;
          foreach(Int64 key in PosToClose) {
            wp = ((CPosition)base[key]);
            if (AmountToFind <= wp.Quantity){
              if (AmountToFind > 0) {
              // break up wp with remainder
                if (wp.Quantity - AmountToFind > 0) { 
                  CPosition Remainder = (CPosition)Add(new CPosition(this, wp.Asset, wp.PriceUSDEst, wp.PriceBaseCur, 0, 0, wp.Quantity - AmountToFind));
                  Remainder.SourcePositions[wp.SelfID]=(wp);
                  wp.Exits[Remainder.SelfID]=(Remainder);
                }  
                NewPosition.SourcePositions[wp.SelfID]=(wp);
                wp.Exits[NewPosition.SelfID]=(NewPosition);
                AmountToFind = 0 ;
                wp.Status = PositionStatus.closed;
                wp.Closed = DateTime.Now;
                
              } else break;
            } else { // AmountToFind > shard 
              // mark consumed.
              AmountToFind -= wp.Quantity;
              NewPosition.SourcePositions[wp.SelfID]=(wp);
              wp.Status = PositionStatus.closed;
              wp.Closed = DateTime.Now;
              wp.Exits[NewPosition.SelfID]=(NewPosition);
            }            
          }

      } else {  // is a Sell

        decimal ToSellQuantity = aAssetQuantity; //* (1/aLastPrice);
        decimal aFeeBaseCur = ToSellQuantity* aLastPrice * Markets.TradeFee;
        decimal ToSellProceeds = aAssetQuantity * aLastPrice - aFeeBaseCur;
        
        decimal aFeeUSDEst = Markets.ToUSD(BaseCur, aFeeBaseCur);
        decimal aPriceUSDEst = Markets.ToUSD(BaseCur, aLastPrice);
        CPosition NewPosition = Add(new CPosition(this, aAsset, aPriceUSDEst, aLastPrice, aFeeBaseCur, aFeeUSDEst, ToSellProceeds));

        long[] PosToClose = base.Keys.Where(x => (
           (CPosition)base[x]).Asset == PayWithCur && ((CPosition)base[x]).Status == PositionStatus.open
          ).ToArray();
        decimal AmountToFind = ToSellQuantity;
        CPosition wp;
        foreach (Int64 key in PosToClose) {
          wp = ((CPosition)base[key]);
          if (AmountToFind <= wp.Quantity) {
            if (AmountToFind > 0) {
              // break up wp with remainder
              if (wp.Quantity - AmountToFind > 0) {
                CPosition Remainder = (CPosition)Add(new CPosition(this, wp.Asset, wp.PriceUSDEst, wp.PriceBaseCur, 0, 0, wp.Quantity - AmountToFind));
                Remainder.SourcePositions[wp.SelfID]=(wp);
                wp.Exits[Remainder.SelfID]=(Remainder);
              }
              NewPosition.SourcePositions[wp.SelfID]=(wp);
              wp.Exits[NewPosition.SelfID]=NewPosition;
              AmountToFind = 0;
              wp.Status = PositionStatus.closed;
              wp.Closed = DateTime.Now;
            } else break;
          } else { // AmountToFind > shard 
                   // mark consumed.
            AmountToFind -= wp.Quantity;
            NewPosition.SourcePositions[wp.SelfID]=(wp);
            wp.Status = PositionStatus.closed;
            wp.Closed = DateTime.Now;
            wp.Exits[NewPosition.SelfID]=(NewPosition);
          }
        }


      }
      if (PositionFileName != "") { 
        Save();
      }
    }
  }


/*
  public class CCurrency : CObject {
    public CBalances Owner;
    public string Currency { get { return base["Currency"].toString(); } set { base["Currency"] = value; } }
    public decimal Available { get { return base["Available"].toDecimal(); } set { base["Available"] = value; } }
    public decimal Balance { get { return base["Balance"].toDecimal(); } set { base["Balance"] = value; } }

    public CPositions Positions;

    public CCurrency(CBalances aOwner, string aCurrency, decimal aBalance, decimal aAvailable) : base() {
      Owner = aOwner;
      Positions = new CPositions(Owner.Markets);
      Currency = aCurrency;
      Available = aAvailable;
      Balance = aBalance;
    }
  }
  public class CBalances : CObject {

    public CMarkets Markets;
    public CBalances(CMarkets aMarkets) : base() {
      Markets = aMarkets;
    }
    public new CCurrency this[string aCur] {
      get {
        try {
          return (CCurrency)base[aCur];
        } catch {
          base[aCur] = new CCurrency(this, aCur, 0, 0);
          return (CCurrency)base[aCur];
        }
      }
      set { base[aCur] = value; }
    }
    

  }  */

}
