using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AppCrypto{

  /// <summary> ConcurrentDictionary with String as key is CObject</summary>
  /// <remarks> C is for Concurrent. Most common type is string lookup version so lets call them CObjects. </remarks>
  public class CObject : ConcurrentDictionary<string, object> {
    public CObject() : base() { }
    public Boolean Contains(string aKey) {
      try { return (base[aKey] is Object); } catch { return false; }
    }
    public new object this[string aKey] {
      get { try { return Contains(aKey) ? base[aKey] : null; } catch { return null; } }
      set { base[aKey] = value; }
    }
    public void Remove(string aKey) {
      if (Contains(aKey)) {
        base.TryRemove(aKey, out _);
      }
    }
    public void Merge(CObject aObject, Boolean OnDupOverwiteExisting) {
      if (aObject != null) {
        if (OnDupOverwiteExisting) {
          foreach (string sKey in aObject.Keys) {
            base[sKey] = aObject[sKey];
          }
        } else {
          foreach (string sKey in aObject.Keys) {
            if (!Contains(sKey)) {
              base[sKey] = aObject[sKey];
            }
          }
        }
      }
    }
  }

  /// <summary>ConcurrentDictionary with Int64 key ordered from Min to Max, adds largest, pops smallest is a CQueue64 lifetime total not to exceed 18,446,744,073,709,551,616 items</summary>  
  public class CQueue : ConcurrentDictionary<Int64, object> {
    public Int64 Nonce = Int64.MinValue;
    public CQueue() : base() { }
    public Boolean Contains(Int64 aKey) {
      try { return (base[aKey] is object); } catch { return false; }
    }
    public object Add(object aObj) {
      Nonce++;
      base[Nonce] = aObj;
      return aObj;
    }
    public object Pop() {
      Object aR = null;
      if (Keys.Count > 0) {
        base.TryRemove(base.Keys.OrderBy(x => x).First(), out aR);
      }
      return aR;
    }
    public void Remove(Int64 aKey) {
      if (Contains(aKey)) {
        base.TryRemove(aKey, out _);
      }
    }
  }

  /// <summary>ConcurrentDictionary with Int64 key ordered from Max to Min, add smallest, pop largest last </summary>  
  public class CCache : ConcurrentDictionary<Int64, object> {
    public Int64 Nonce = Int64.MaxValue;
    public Int64 Height { get { return Int64.MaxValue - Nonce; } }
    public Boolean Contains(Int64 aKey) {
      try { return (base[aKey] is Object); } catch { return false; }
    }
    public Int32 Size = 200;
    public CCache() : base() {
    }
    public object Add(object aObj) {
      Nonce--;
      base[Nonce] = aObj;
      if (base.Keys.Count > Size) {
        Pop();
      }
      return aObj;
    }
    public object Pop() {
      Object aR = null;
      if (Keys.Count > 0) {
        base.TryRemove(base.Keys.OrderBy(x => x).Last(), out aR);
      }
      return aR;
    }
    public void Remove(Int64 aKey) {
      if (Contains(aKey)) {
        base.TryRemove(aKey, out _);
      }
    }
  }

  public class CBook : ConcurrentDictionary<decimal, object> {
    public CBook() : base() {
    }
    public Boolean Contains(decimal aKey) { try { return base[aKey] is object; } catch { return false; }}
    public new object this[decimal aKey] {
      get { return (Contains(aKey) ? base[aKey] : null); }
      set { base[aKey] = value; }
    }
    public void Remove(decimal aKey) {
      if (Contains(aKey)) {
        object outcast;
        base.TryRemove(aKey, out outcast);
      }
    }
    public decimal ElementKeyAt(Int32 iIndex) {
      IEnumerable<decimal> lQS = base.Keys.OrderByDescending(x => (x));
      return lQS.ElementAt(iIndex);
    }
  }

  public static class CObjExt
    {
        public static CObject toCObject(this string[] KeyIsValueArray)
        {
            CObject r = new CObject();
            foreach (string s in KeyIsValueArray) r[s] = s;
            return r;
        }
    }

  public class CAvgDecimalCache : CCache {
    public decimal SumMax {
      get {
        decimal dSum = 0;
        decimal dMax = Int64.MinValue;
        foreach (Int64 key in base.Keys.OrderBy(x => x)) {
          dSum += (decimal)base[key];
          if (dSum > dMax) { dMax = dSum; }
        }
        return dMax;
      }
    }
    public decimal SumMin {
      get {
        decimal dSum = 0;
        decimal dMin = Int64.MaxValue;
        foreach (Int64 key in base.Keys.OrderBy(x => x)) {
          dSum += (decimal)base[key];
          if (dSum < dMin) { dMin = dSum; }
        }
        return dMin;
      }
    }
    public CAvgDecimalCache() : base() { }
    public object Add(decimal aObj) {
      Nonce--;
      base[Nonce] = aObj;
      if (base.Keys.Count > Size) {
        Pop();
      }
      return aObj;
    }
    public new decimal Pop() {
      object aR = 0;
      if (Keys.Count > 0) {
        base.TryRemove(base.Keys.OrderBy(x => x).Last(), out aR);
      }
      return (decimal)aR;
    }
    public new decimal this[Int64 aKey] {
      get { return (Contains(aKey) ? (decimal)base[aKey] : 0); }
      set { base[aKey] = value; }
    }
    public decimal toSum() {
      decimal aSum = 0;
      if (base.Keys.Count > 0)
        foreach (Int64 iKey in base.Keys) {
          aSum += (decimal)base[iKey];
        }
      return aSum;
    }
    public decimal toAvg() {
      return (base.Keys.Count > 0) ? toSum() / base.Keys.Count : 0;
    }
    public decimal toDecimal() {
      return this[Nonce];
    }
  }


}
