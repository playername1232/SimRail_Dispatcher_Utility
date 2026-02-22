# SimRail Dispatcher Utility — Roadmap

This document outlines planned features and long-term direction of the project.

---

## ✅ Iteration 1 — Manual Mode (v0.1.0)

Core functionality for managing trains manually.

- [x] Add and remove trains
- [x] Live countdown to departure
- [x] Config-driven dropdowns (posts, train types, stop types)
- [x] Automatic cleanup of outdated entries (configurable via appsettings)
- [x] Context menu actions

---

## ✅ Iteration 2 — API-Driven Stations & Routing (v0.2.0)

- [x] Load station list from SimRail API
- [x] Dynamic station switching
- [x] Extract neighboring stations from timetable data
- [x] Remove manual station configuration

---

## 🚧 Iteration 3 — User Experience Improvements (v0.3.x)

Planned:

- [ ] Native reminder notifications (replace MessageBox)
- [ ] Improved train state handling (scheduled, arriving, departed)
- [ ] Input validation & smarter defaults
- [ ] Minor UI polish & accessibility tweaks

Goal: smoother and more intuitive dispatcher workflow.

---

## 🔮 Iteration 4 — Automatic Train Import (v0.4.x)

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

The roadmap may evolve as the project grows and community feedback arrives.
