<h1>SmartExchange Trading Bot</h1>
<h2>Patience is a virtue</h2>
The SmartExchange Trading Bot is a cryptocurrency trading bot designed to automate trading actions.</br>
The goal is to increase the quantity of cryptocurrencies. Each transaction made by this bot is designed to achieve that with a high degree of certainty. 
However, if the market price drops, that is beyond the bot's control.</br>
</br>
<strong >
    WARNING: Use this bot at your own risk. The author holds no responsibility for any losses incurred.
  
<ul>
    <li><strong>Market Volatility</strong>: Cryptocurrency prices can be highly volatile, and sudden changes can result in unexpected losses.</li>
    <li><strong>No Guaranteed Profits</strong>: While the bot is designed to maximize asset quantity, profit is not guaranteed.</li>
    <li><strong>Bot Limitations</strong>: The bot cannot account for all market conditions or predict extreme price movements.</li>
    <li><strong>User Responsibility</strong>: Users are solely responsible for monitoring their investments.</li>
    <li><strong>Test Before Full Use</strong>: It’s recommended to test the bot with small amounts or in a demo environment.</li>
    <li><strong>Backup Funds</strong>: Only use funds you can afford to lose, as the bot cannot prevent losses during price drops.</li>
    <li><strong>System Dependencies</strong>: The bot relies on stable internet, exchange APIs, and server uptime, which may affect performance.</li>
</ul>
</strong>
<h2>Overview</h2> 
<p>This trading bot is designed to make trades based on potential increases in asset quantity. It begins by reading your portfolio balance, identifying the asset with the highest USDT value, and calculating the target amount based on the current quantity and a predefined threshold. The bot continuously monitors prices for relevant trading pairs, comparing each asset with others and tracking price changes on a scheduled basis.
When a buy action is triggered based on set thresholds, the bot waits for the price to decrease to a specified level before executing the purchase. Similarly, for a sell action, the bot completes the transaction once the price reaches the desired target.

If the price moves against the desired outcome and there are no favorable trade opportunities, the bot remains idle until the price aligns with the target. <b>Each transaction aims to increase the quantity of assets held</b>; however, profit in USDT or EUR is not guaranteed due to potential price fluctuations. Patience is key.

For example, if you hold BTC and buy ETH, the quantity of ETH may increase. However, if the ETH/EUR price drops more significantly than the BTC/EUR price, your total balance in EUR may decrease.</p>
<p><strong>The bot calculates the estimated trading fees</strong>, and the threshold is based on how the current quantity, with fees considered, exceeds the desired amount. Given the rapid nature of transactions, it cannot guarantee execution at the exact target price. To address this, three proactive measures are in place:</p>
<ul>
    <li><strong>Rounds Check</strong>: The bot waits for prices to stabilize before executing a trade, improving trade accuracy.</li>
    <li><strong>Profit Threshold</strong>: A target profit margin is set, with a buffer to account for minor fluctuations, ensuring profitable trades.</li>
    <li><strong>Account Quantity Tracking</strong>: The bot monitors and stores the quantity of each cryptocurrency in the account. This approach maintains accuracy over time, so even if a single trade does not meet the target, subsequent trades will aim to exceed the highest previously achieved quantity.</li>
    <li><strong>Action Execution Delay</strong>: The bot will not execute an action immediately; it will wait. If the price moves favorably, it will hold off until it receives a signal in the opposite direction.</li>
</ul>

<h3>Key Features</h3>
<ul>
    <li>Automated Buy/Sell Execution: Executes buy and sell actions based on configurable thresholds, current market prices, and asset quantities.</li>
    <li>Threshold-based Actions: Configurable ThresholdBuy and ThresholdSell values define when a trade action is considered "worthwhile."</li>
    <li>Account and Asset Tracking: Retrieves and logs current asset quantities, tracking account performance and history.</li>
</ul>
<h2>Restrictions</h2>
<ul>
    <li>All configurable pairs must include both symbols with a USDT pair, as this is used to estimate profit at the current USDT price.</li>
    <li>Avoid making manual changes to your crypto portfolio while the bot is running. For example, if using Binance, be cautious when using the BNB trading fee discount, as it may affect the bot’s calculations.</li>
   <li>Although the bot is built to support multiple exchange providers, currently only Binance is supported.</li>

</ul>
<h2>Prerequisites</h2>
<p>Before running the bot, ensure you have the following tools and credentials set up:</p>
<ul>
    <li><strong>.NET Core SDK</strong>: Required to build and run the application.</li>
    <li><strong>Binance API Key and Secret</strong>: Necessary for accessing your account and executing trades via the Binance API.</li>
    <li><strong>MS SQL Server</strong>: Used for database storage and tracking trade history.</li>
    <li><strong>Node.js and Angular</strong>: Required if you plan to use the frontend component for monitoring or configuring the bot.</li>
    <li><strong>Visual Studio and Visual Studio Code</strong>: Recommended development tools for running, debugging, and managing the code.</li>
</ul>

<h2>Configuration</h2>
<p>The bot requires configuration in the AppSettings file. Each asset pair is configured with the following parameters:</p>
<ul>
    <li><strong>Name</strong>: The asset pair (e.g., BTCUSDT).</li>
    <li><strong>ThresholdBuy</strong>: The minimum threshold to trigger a buy action.</li>
    <li><strong>ThresholdSell</strong>: The minimum threshold to trigger a sell action.</li>
</ul>
<p>Example configuration:</p>
<pre>
<code>
"Assets": [
    {
      "Name": "BTCUSDT",
      "ThresholdBuy": 0.02,
      "ThresholdSell": 0.0001
    },
    ...
]
</code>

    
</pre>
<pre><code>
    "Interval": 10000,
    "TradingProvider": {
      "Name": "Binance",
      "ApiKey": "",
      "Secret": ""
    },
    "StopOnError": true,
    "RoundsCheck": 5,
</code></pre>
<h2>Technologies Used</h2>
<ul>
    <li>.NET</li>
    <li>SignalR</li>
    <li>Angular</li>
    <li>Entity Framework (EF)</li>
</ul>
<h2>Installation</h2>
<p>To install the bot, follow these steps:</p>
<ol>
    <li>Clone the repository:</li>
    <pre><code>git clone https://github.com/odysseusboras/SmartExchange</code></pre>
    <li>Install the required NuGet packages if they are not included.</li>
    <li>Configure your <strong>AppSettings</strong> file to include the following:</li>
    <ul>
        <li>API credentials for your trading provider</li>
        <li>Assets with trading pairs and thresholds</li>
        <li><strong>Database connection string</strong> - create and execute the migration using Entity Framework commands:</li>
        <pre><code>dotnet ef migrations add InitialCreate</code></pre>
        <pre><code>dotnet ef database update</code></pre>
    </ul>
    <li>
        A script named <strong>install-service.ps1</strong> is provided, which will create and run the bot as a Windows service. 
        <br>
        To use this script, follow these steps:
        <ul>
            <li>Build the project in <strong>Release</strong> mode.</li>
            <li>Open the Command Prompt and navigate to the project directory.</li>
            <li>Run the script by executing <code>.\install-service.ps1</code>.</li>
        </ul>
    </li>
<li>
    Launch the Angular UI project. Optionally, you may build the project and host the UI in Internet Information Services (IIS) for enhanced monitoring capabilities.
</li>

</ol>
<h2>Contributing</h2>
<p>We welcome contributions! If you would like to contribute to the project, please submit a pull request for any feature additions, bug fixes, or improvements.</p>
<p>Future Enhancements:</p>
<ul>
    <li>Implement functionality to split amounts into different pairs and enable multiple monitoring.</li>
    <li>Integrate AI or additional indicators that can pause a transaction based on predicted significant changes.</li>
</ul>
<h2>License</h2>
<p>This software is provided for personal use only.</p>
<p>By using this software, you acknowledge that the author does not assume any responsibility for any losses incurred as a result of its use.</p>
<p>This project is not licensed under any formal license, and users are granted permission to use it at their own risk.</p>




