# CryptoRockets
  - Attempt at simulated trading with the goal of a profitable trading algo
    
## Chapter 1. Main Components and Settings

  - Bittrex.Net api to access the exchange https://github.com/JKorf/Bittrex.Net
  - CryptoExchange.Net from Bittrex.Net dependency https://github.com/JKorf/CryptoExchange.Net
    - Note: Microsoft.Extensions.Logging.Abstractions needed to be included in project.
        
  - OracleDelta is the app
    - TickerTranformer is technology built to catch ticker events, house the data and return execution flow asap asycronously.
    
  - AppCrypto is a helper data module library. 
    - CObjects Concurrent dictionary as the basic object.  
    - CQueue  CObject decendant representing a Queue built with concurrency in mind.
    - CCache  similar object representing a Cache built with concurrency in mind. 
    - CAvgDecimalCache saves a set amount of decimals.  
    - INI file support objects for IO. [by Jacek Gajek](https://www.codeproject.com/Articles/20120/INI-Files)
    - CFileDictionary is a Persistant Dictionary object 
    - SecureStore password derived AES encrypted settings file dictionary
    - CMarkets virtual model dictionary used to track the feed data. 
    - CPositions persistant virtual balances to simulate holding positions

  - [StaticExtensions](https://github.com/mmeents/StaticExtensions) library of helper extensions. 
        
## Chapter 2. Models and Viewers
  - Viewer App a C# winforms application
    - Secure Settings
      - Password derived key unlocks the AES encrypted settings files. 
        - App prompts user to enter password which generates keys that unlock the settings. 
      - All Settings and NoSQL Stores are saved in the ProgramData\MMCommon folder on the deployment computer.   
      - Exchange API keys are stored as AES encrypted values in ini files.    
      - Market Last Rates and Positions are persisted via NOSQL files in ProgranData\MMCommon folder  
    - Feeds Processing
      - Subscription Management
        - From DefMarketList we open tic socket subscriptions for each market.      
        - When the Form is closed we close all sockets. 
      - Sockets report events to the DoTickersLandingAdd function. 
        - TickersLanding is a [queue of tickers](https://github.com/mmeents/CryptoRockets/blob/7534a7bd72c11bf05cfe97c3e83e202c54ef8284/FeedTech.cs) and code to transform the details into the Markets data structure and then remove themselves from the queue.  
    - The Timer is the display refresh 16fps.  
      - The ReDraw is double buffer, draw on a image and then overlay image over forms canvas.  
      - UpdateControlVisibilityCallback is main resize and position function that moves form controls around based on the forms state.  
      - The data drawn is the Markets and Positions structures.        
    - Market Data Structure
      - Markets to track are identified in code on main form as string[] DefMarketList 
      - Markets data structure is a dictionary of Coins each of which is a list of Dictionary Markets each of which are either a Market or the inverse market type. Inverse meaning a special class of market where all feed data is saved as 1/value.     
      - Markets can persist the last Ask and Bid values.  This makes working in the unit testing able to load last market data and test the position structures.
    - Trade Simulating
      - Trade virtual positions at current market rates via user interface.
        - Rate buttons (top left) bring up buy dialog.
        - Position buttons (bottom) bring up sell dialogs. 
      - Position Structure to persist positions with dates and USD est. 
      - ToDo add Order Placing and Feeds processing when the order is crossed does simulated trade. 
    - Signal Production
      - Currently attempting to put together different signals within the app.
      - Feeds Processing updates the Markets structure in real time.
      - Each statistic is recalculated every 15 sec 
      - ToDo profitable trading algo...
        
## Chapter 3. TBD... hope you like




