using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StaticExtensions;

namespace AppCrypto {

  public enum PositionStatus { open, closed }
  public class CPosition {
    public CPositions Owner;
    public PositionStatus Status;
    public string Asset;
    public decimal PriceUSDEst;
    public decimal PriceBaseCur;
    public decimal FeeBaseCur;
    public decimal FeeUSDEst;
    public decimal Quantity;
    public decimal TotalUSD { get { return PriceUSDEst * Quantity + FeeUSDEst;} }
    public decimal TotalBaseCur {get { return PriceBaseCur * Quantity + FeeBaseCur;} }

    public CPositions SourcePositions;
    public CPositions Exits;
    public CPosition(CPositions aOwner, string aAsset, decimal aPriceUSDEst, decimal aPriceBaseCur,
      decimal aFeeBaseCur, decimal aFeeUSDEst, decimal aQuantity ) {
      Owner = aOwner;
      Status = PositionStatus.open;
      Asset = aAsset;
      PriceUSDEst = aPriceUSDEst;
      PriceBaseCur = aPriceBaseCur;
      FeeBaseCur = aFeeBaseCur;
      FeeUSDEst = aFeeUSDEst;
      Quantity = aQuantity;
      SourcePositions = new CPositions(Owner.Markets);
      Exits = new CPositions(Owner.Markets);
    }
  }
  public class CPositions : CQueue {
    public CMarkets Markets;
    public CPositions(CMarkets aMarkets) : base() {
      Markets = aMarkets;
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
        (CPosition)base[x]).Asset == aCurrency && ((CPosition)base[x]).Status == PositionStatus.open
      ).ToArray().Sum(x => ((CPosition)base[x]).Quantity);
    }

    public decimal AvgPrice(string aCurrency) {
      decimal r = 0;
      long[] aPos = base.Keys.Where(x => (
        (CPosition)base[x]).Asset == aCurrency && ((CPosition)base[x]).Status == PositionStatus.open
      ).ToArray();
      CPosition wp;
      decimal aQuantitySum = 0;
      decimal aCostSum = 0;
      foreach (Int64 key in aPos) {
        wp = ((CPosition)base[key]);
        aQuantitySum += wp.Quantity;
        aCostSum += wp.Quantity * wp.PriceBaseCur + wp.FeeBaseCur;
      }
      r = aCostSum / aQuantitySum;
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

    public void BuyAsset(String aAsset, string aMarket, decimal aQuantity) {
      string BaseCur = aMarket.ParseFirst("-");
      string QuoteCur = aMarket.ParseLast("-");
      string PayWithCur = aAsset == BaseCur ? QuoteCur : BaseCur;
      decimal PayWithCurBal = Balance(PayWithCur);
      string TradeOp = PayWithCur == BaseCur ? "Buy" : "Sell";
        

      if (TradeOp == "Buy") { 
        decimal aLastPrice = Markets[aMarket].Ask;
        decimal aTotalBaseCur = aLastPrice * aQuantity;
        decimal aFeeBaseCur = aTotalBaseCur * Markets.TradeFee;
        aTotalBaseCur += aFeeBaseCur;
        if (aTotalBaseCur > PayWithCurBal) throw new Exception("insuffcent base cur to make buy.");
          decimal aFeeUSDEst = Markets.ToUSD(BaseCur, aFeeBaseCur);
          decimal aPriceUSDEst = Markets.ToUSD(BaseCur, aLastPrice);

          CPosition NewPosition = (CPosition)Add(new CPosition(this, aAsset, aPriceUSDEst, aLastPrice, aFeeBaseCur, aFeeUSDEst, aQuantity));

          long[] PosToClose = base.Keys.Where(x => (
           (CPosition)base[x]).Asset == PayWithCur && ((CPosition)base[x]).Status == PositionStatus.open
          ).ToArray();
          decimal AmountToFind = aTotalBaseCur;
          CPosition wp;
          foreach(Int64 key in PosToClose) {
            wp = ((CPosition)base[key]);
            if (AmountToFind <= wp.Quantity){
              if (AmountToFind > 0) {
              // break up wp with remainder
                if (wp.Quantity - AmountToFind > 0) { 
                  CPosition Remainder = (CPosition)Add(new CPosition(this, wp.Asset, wp.PriceUSDEst, wp.PriceBaseCur, 0, 0, wp.Quantity - AmountToFind));
                  Remainder.SourcePositions.Add(wp);
                  wp.Exits.Add(Remainder);
                }  
                NewPosition.SourcePositions.Add(wp);
                wp.Exits.Add(NewPosition);
                AmountToFind = 0 ;
                wp.Status = PositionStatus.closed;
                
              } else break;
            } else { // AmountToFind > shard 
                // mark consumed.
                AmountToFind -= wp.Quantity;
                NewPosition.SourcePositions.Add(wp);
                wp.Status = PositionStatus.closed;
                wp.Exits.Add(NewPosition);
            }            
          }

      } else {  // is a Sell

        decimal aLastPrice = Markets[aMarket].Bid;
        decimal ToSellQuantity = aQuantity * (1/aLastPrice);
        decimal aFeeBaseCur = ToSellQuantity* aLastPrice * Markets.TradeFee;
        decimal aFeeUSDEst = Markets.ToUSD(BaseCur, aFeeBaseCur);
        decimal aPriceUSDEst = Markets.ToUSD(BaseCur, aLastPrice);
        CPosition NewPosition = (CPosition)Add(new CPosition(this, aAsset, aPriceUSDEst, aLastPrice, aFeeBaseCur, aFeeUSDEst, aQuantity));

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
                Remainder.SourcePositions.Add(wp);
                wp.Exits.Add(Remainder);
              }
              NewPosition.SourcePositions.Add(wp);
              wp.Exits.Add(NewPosition);
              AmountToFind = 0;
              wp.Status = PositionStatus.closed;

            } else break;
          } else { // AmountToFind > shard 
                   // mark consumed.
            AmountToFind -= wp.Quantity;
            NewPosition.SourcePositions.Add(wp);
            wp.Status = PositionStatus.closed;
            wp.Exits.Add(NewPosition);
          }
        }


      }


    }
  }



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
    

  }


}
