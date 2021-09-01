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
  public class TickerTranformer {
    public long Id;
    public CTickerQueue Owner;
    public BittrexTick TheUpdate;
    public CMarkets Markets;
    private BackgroundWorker Worker;
    public TickerTranformer(CTickerQueue aOwner, long aId, CMarkets aMarkets, BittrexTick aObj) {
      Owner = aOwner;
      TheUpdate = aObj;
      Markets = aMarkets;

      Worker = new BackgroundWorker();
      Worker.WorkerSupportsCancellation = false;
      Worker.DoWork += new DoWorkEventHandler(DoWorkAsync);
      Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(WorkerComplete);
      Worker.RunWorkerAsync();
    }
    private void DoWorkAsync(object sender, DoWorkEventArgs args) {
      string sMarket = TheUpdate.Symbol.ParseReverse("-", "-");
      if (Markets.Contains(sMarket)) {
        string sBaseCur = sMarket.ParseFirst("-");
        CMarket m = Markets[sMarket];
        m.UpdateAsk(TheUpdate.AskRate);
        m.UpdateBid(TheUpdate.BidRate);
        m.IncUpdateCount(1);
        CInvMarket mi = (CInvMarket)Markets.Coins[sBaseCur][sMarket];
        mi.UpdateAsk(1 / TheUpdate.BidRate);
        mi.UpdateBid(1 / TheUpdate.AskRate);
        mi.IncUpdateCount(1);
      }
    }

    private void WorkerComplete(object sender, RunWorkerCompletedEventArgs args) {
      try {
        Worker.Dispose(); // tear down the worker resources.
        Owner.Remove(Id);
      } catch (Exception e) {
        e.toAppLog(Id.toString());
      }
    }

  }

  public class CTickerQueue : CQueue {
    public string LastTicSeq = "";
    CMarkets Markets;
    public CTickerQueue(CMarkets TheMarkets) : base() {
      Markets = TheMarkets;
    }
    public void AddTic(BittrexTick aObj) {
      Nonce++;
      base[Nonce] = new TickerTranformer(this, Nonce, Markets, aObj);
      LastTicSeq = Nonce.toString();
    }
  }

}
