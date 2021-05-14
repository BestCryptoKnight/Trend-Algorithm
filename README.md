# CryptoRockets
  - Attempt at simulated trading with goal of profitable trader algo
    
## Chapter 1. Components and Settings

  - Bittrex.Net api to access my target exchange. https://github.com/JKorf/Bittrex.Net
  - CryptoExchange.Net cause Bittrex.Net needs it https://github.com/JKorf/CryptoExchange.Net
    - see https://docs.microsoft.com/en-us/dotnet/standard/net-standard for deployment chart.  found I'm not quite 2.1 and since that was giving me error, I removed the deployment.

  - AppCrypto
    - INI file support objects for file IO. thanks https://www.codeproject.com/Articles/20120/INI-Files  by Jacek Gajek 
    - CObjects  Concurrent dictionary as the basic object.  
    - CQueue
    - CCache
    - CAvgDecimalCache
    - CFileDictionary
    - SecureStore

  - StaticExtensions library of helper extensions I use. https://github.com/mmeents/StaticExtensions
        
## Chapter 2. Market Viewer
  - Viewer App
    - Secure Settings
    - Market Data Structure
    - Subscription Management
    - Feeds Landing
    - Feeds Processing
    - Signal Production
    - Trade Simulating



