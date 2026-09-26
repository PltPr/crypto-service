<h1>
  <img src="./crypto-wh.svg" width=32 height="32" alt="">
  CryptoTrader
</h1>

A .NET service that collects cryptocurrency trading signals from TradingView and processes them into automated trading decisions for Bybit.

## Flow
```text
TradingView
     ↓
Playwright
     ↓
Technical Signal
     ↓
Buy / Sell / Hold
     ↓
Bybit API
     ↓
Trade & Balance Data
     ↓
Database
```

## Implementation

Built with ASP.NET Core, Playwright, Bybit API, Entity Framework Core, BackgroundService and HTTP Client for automated signal processing, account management and trade tracking.
