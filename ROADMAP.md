
---

# 🗺 ROADMAP.md

```md
# SimRail Dispatcher Utility — Roadmap

This document outlines planned features and long-term direction of the project.

---

## ✅ Iteration 1 — Manual Mode (Current)

Core functionality for managing trains manually.

- [x] Add and remove trains
- [x] Live countdown to departure
- [] Reminder notifications (Currently implemented via windows built-in MessageBox)
- [] Train state handling
- [x] Config-driven dropdowns (Posts, Train and Stop types)
- [x] Automatic cleanup of outdated entries (Configurable via appsettings)
- [x] Context menu actions

---

## 🚧 Iteration 2 — API-Driven Stations & Routing

Automate station handling using SimRail open API.

Planned:

- [ ] Load station list from API
- [ ] Extract neighboring stations from timetable data
- [ ] Dynamic station switching
- [ ] Remove manual station config

Goal: eliminate static configuration and reflect real network topology.

---

## 🔮 Iteration 3 — Automatic Train Import

Populate train list directly from SimRail API.

Planned:

- [ ] Fetch trains for selected station
- [ ] Time-window filtering (e.g. next 1–2 hours only)
- [ ] Smart refresh & deduplication
- [ ] Optional tray notifications & sound alerts

Goal: zero manual input during gameplay.

---

## 🌍 Future Ideas

Potential enhancements:

- [ ] Localization (JSON-based language packs)
- [ ] Dark / light themes
- [ ] Export / import sessions
- [ ] Advanced filtering & sorting
- [ ] Performance optimizations
- [ ] Plugin-like extensions

---

## 📌 Notes

Roadmap may evolve as the project grows and community feedback arrives.
