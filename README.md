# CryptoRockets
  - Attempt at simulated trading with goal of profitable trader algo
    
## Chapter 1. Components and Settings

  - Bittrex.Net api to access my target exchange. https://github.com/JKorf/Bittrex.Net
  - CryptoExchange.Net cause Bittrex.Net needs it https://github.com/JKorf/CryptoExchange.Net
    - see https://docs.microsoft.com/en-us/dotnet/standard/net-standard for deployment chart.  found I'm not quite 2.1 and since that was giving me error, I removed the deployment.
    
  - OracleDelta is the application domain.
    - TickerTranformer is technology built to catch ticker events, house the data and return execution flow asap asycronously.
    - Mainform  

  - AppCrypto
    - CObjects Concurrent dictionary as the basic object.  
    - CQueue  CObject decendant representing a Queue built with concurrency in mind.
    - CCache  similar object representing a Cache built with concurrency in mind. 
    - CAvgDecimalCache saves a set amount of decimals.  
    - INI file support objects for IO. [by Jacek Gajek](https://www.codeproject.com/Articles/20120/INI-Files)
    - CFileDictionary is a Persistant Dictionary object 
    - SecureStore password derived AES encrypted settings file dictionary
    - CMarkets virtual model dictionary used to track the feed data. 
    - CPositions persistant virtual balances to simulate holding positions

  - [StaticExtensions](https://github.com/mmeents/StaticExtensions) library of helper extensions I use. 
        
## Chapter 2. Market Viewer
  - Viewer App a C# winforms application
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
      - Trade virtual balances.   
      - needs to add Order Placing and Feeds processing when the order is crossed does simulated trade. 
           
    - Signal Production
      - needs to add Trade Signal design and development platform. 



