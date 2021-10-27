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

  public class PositionButton {
    public Rectangle Location;
    string CurrencyStr;
    public PositionButton (Rectangle r, string aCurrencyStr) { 
      Location = r;
      CurrencyStr = aCurrencyStr;
    }    
  }

  public class PositionButtons : CObject { 
    public PositionButtons() : base() {
    
    }

    public new PositionButton this[string aCurStr] {
      get { return (Contains(aCurStr) ? (PositionButton)base[aCurStr] : null); }
      set { base[aCurStr] = value; }
    }

    public string didHitTest(Int32 X, Int32 Y) { 
      string tr = "";
      foreach (string sCur in Keys) {
        if (X > this[sCur].Location.Left &&
          X < this[sCur].Location.Right &&
          Y > this[sCur].Location.Top &&
          Y < this[sCur].Location.Top + this[sCur].Location.Height) {
          tr = sCur;
          break;
        }
      }
      return tr;
            
      }



  }



 

}
