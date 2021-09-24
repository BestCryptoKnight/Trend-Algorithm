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
      - Password derived key user knows unlocks the AES encrypted settings files.  (where exchange API keys are stored.)
      - Exchange API keys are stored as AES encrypted values in Ini files within the ProgramData folders on the deployment computer.    
    - Market Data Structure
      - Markets to track are identified in code on main form as string[] DefMarketList 
    - Subscription Management
      - From DefMarketList we open tic socket subscriptions for each.  
    - Feeds Processing
      - Configuring the sockets to report events to the DoTickersLandingAdd 
        - TickersLanding is a queue of tickers and code to transform the details into the Markets data structure.  We don't want to wait so the transformer utilizing a background worker allows the quick return of the socket communication.  
      - Processing the data. 
        - The Timer is the heart beat that runs the display refresh code.
        - The data is the Markets and Positions structures.    
    - Trade Simulating
    - Signal Production



