using System;
using System.IO;
using System.Security.Cryptography;
using StaticExtentions;

namespace AppCrypto {

  public class AppKey {
    private readonly string KeyA;
    private readonly string KeyB;
    public AppKey(string sPassword) {     
      PasswordDeriveBytes aPDB = new PasswordDeriveBytes(sPassword, null);
      KeyA = aPDB.GetBytes(32).toHexStr();
      KeyB = aPDB.GetBytes(16).toHexStr();
    }    
    public byte[] getKey { get { return KeyA.toByteArray(); } }
    public byte[] getIV { get { return KeyB.toByteArray(); } }
    public string toAESCipher(string sText) {
      string sResult = "";
      try {
        AesCryptoServiceProvider aASP = new AesCryptoServiceProvider();
        AesManaged aes = new AesManaged();
        aes.Key = KeyA.toByteArray();
        aes.IV = KeyB.toByteArray();
        MemoryStream ms = new MemoryStream();
        CryptoStream encStream = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
        StreamWriter sw = new StreamWriter(encStream);
        sw.WriteLine(sText.toBase64EncryptStr());
        sw.Close();
        encStream.Close();
        byte[] buffer = ms.ToArray();
        ms.Close();
        sResult = buffer.toHexStr();
      } catch (Exception e) {
        throw e;
      }
      return sResult;
    }
    public string toDecryptAES(string sAESCipherText) {
      string val = "";
      try {
        AesCryptoServiceProvider aASP = new AesCryptoServiceProvider();
        AesManaged aes = new AesManaged();
        aes.Key = KeyA.toByteArray(); 
        aes.IV = KeyB.toByteArray();
        MemoryStream ms = new MemoryStream(sAESCipherText.toByteArray());
        CryptoStream encStream = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
        StreamReader sr = new StreamReader(encStream);
        val = sr.ReadToEnd();
        val = val.toBase64DecryptStr();
        sr.Close();
        encStream.Close();
        ms.Close();
      } catch (Exception e) {
        throw e;
      }
      return val;
    }
  }
  public class SecureStore {
    public CFileDictionary fvMain;
    public AppKey kpMain;
    public string StorageFileName;
    public SecureStore(string aMasterPwd, string aFileName) {
      if (aMasterPwd == "") throw new Exception("Password not present");
      kpMain = new AppKey(aMasterPwd);

      string sDir = Path.GetDirectoryName(aFileName);
      if (!Directory.Exists(sDir)) throw new Exception("Path not found.");
      fvMain = new CFileDictionary(aFileName);
      if (File.Exists(aFileName)) {
        fvMain.LoadValues();
      }
    }

    public string this[string sCredentialName] {
      get { return fvMain["c" + sCredentialName] == null ? "" : kpMain.toDecryptAES(fvMain["c" + sCredentialName]); }
      set { fvMain["c" + sCredentialName] = kpMain.toAESCipher(value); }
    }
    public void RemoveCredential(string sCredentialName) {
      fvMain.RemoveVar("c" + sCredentialName);
    }

  }


}
