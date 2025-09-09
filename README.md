# Global Insights Dashboard

A C# WPF desktop application that provides weather, news, finance, and trivia data from various APIs.

## Features

- **Weather**: Current conditions and forecasts (OpenWeatherMap)
- **News**: Latest headlines (NewsAPI)
- **Finance**: Stock quotes and charts (Polygon.io)
- **Trivia**: Interactive quizzes (Open Trivia Database)
- **Settings**: Dark/light themes, API key management

## Setup

1. Clone the repo
2. Copy `appsettings.example.json` to `appsettings.json`
3. Add your API keys to `appsettings.json` (or use the Settings dialog)
4. Run `dotnet build` and `dotnet run`

## Requirements

- .NET 8.0 SDK
- Windows 10/11

## API Keys

Get free API keys from:
- [OpenWeatherMap](https://openweathermap.org/api)
- [NewsAPI](https://newsapi.org/)
- [Polygon.io](https://polygon.io/)

Trivia works without an API key.
