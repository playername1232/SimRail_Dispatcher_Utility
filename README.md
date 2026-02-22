# SimRail Dispatcher Utility

License: MIT

Open-source WPF companion app for SimRail dispatchers.  
Provides a live train list with time-to-departure, configurable reminders and notifications, and station-oriented workflow.

Built to eliminate constant timetable watching when handling delayed departures.

---

## 🚆 Release v0.1.0 – Core features

- Manual train management (add/remove)
- Live countdown to departure
- Reminder notifications before departure
- Train state visualization (scheduled, reminder due, departing, departed)
- Config-driven dropdowns (posts, stop types, train types)
- Automatic cleanup of outdated trains
- Context menu actions

---

## 🚆 Release v0.2.0 – API & architecture update

- Dependency Injection architecture (services & windows)
- API integration with SimRail timetable endpoint
- Configurable API endpoints and local cache paths
- Dynamic station loading from live data
- Neighbor station resolution for route building
- Service-based data flow between backend and UI

---

## 🚀 Getting Started

### Requirements
- Windows
- .NET (targeting `net10.0-windows`)

### Run locally

1. Clone the repository  
2. Copy configuration: `Settings/appsettings.example.json` → `Settings/appsettings.json`  
3. Run using Rider or Visual Studio:

```bash
dotnet run
