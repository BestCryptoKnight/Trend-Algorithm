using StaticExtensions;
using System;

namespace AppCrypto
{

    public class CFileDictionary : CObject {
        string FileName;
        IniFile f;

        public CFileDictionary(string sFileName) : base() {
            FileName = sFileName;
            f = IniFile.FromFile(FileName);
        }

        public void LoadValues()
        {
            foreach (string s in this.getVarNames())
            {
                string ss = this[s];
            }
        }

        private void SetVarValue(string VarName, string VarValue)
        {
            try
            {
                f["Variables"][VarName] = VarValue;
                f.Save(FileName);
                base[VarName] = VarValue;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private string GetVarValue(string VarName)
        {
            string result = "";
            try
            {
                if (base.ContainsKey(VarName))
                {
                    result = base[VarName].toString();
                }
                else
                {
                    result = f["Variables"][VarName];
                    base[VarName] = result;
                }
            }
            catch { }
            return result;
        }

        public void RemoveVar(string VarName)
        {
            f["Values"].DeleteKey(VarName);
            f.Save(FileName);
            if (this.ContainsKey(VarName))
            {
                this.TryRemove(VarName, out object value);
            }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<string> getVarNames()
        {
            return f["Variables"].GetKeys();
        }

        public new string this[string VarName]
        {
            get { return GetVarValue(VarName); }
            set { SetVarValue(VarName, value); }
        }
    }

}
