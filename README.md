#  TB-Browser

A lightweight, privacy-focused Windows browser built with **WinUI 3**, **WebView2**, and **.NET 10**. Designed for speed, minimal footprint, and user control.

---

## 📦 Installation & Requirements

This build is **stripped of all .NET and WinAppSDK runtime files** to minimize distribution size (~2–4 MB). End users must install the runtimes manually once.

### 🔹 Prerequisites
1. **.NET 10 Desktop Runtime**  
   🔗 https://dotnet.microsoft.com/download/dotnet/10.0
2. **Windows App Runtime 1.6+**  
    https://aka.ms/windowsappsdk/latest

###  Quick Start
1. Download `TB-Browser-Minimal.zip` from [Releases](../../releases) or [Actions](../../actions)
2. Extract to any folder (e.g., `C:\Apps\TB-Browser`)
3. Run `TB-Browser.exe`

> ✅ No installer required. Portable by design. Data & logs auto-create in `%LOCALAPPDATA%\TB-Browser\` on first run.

---

## 🚀 Features

| Category | Implementation |
|----------|----------------|
| **UI/UX** | Draggable custom title bar (`TabView`), instant theme toggle (Light/Dark/System), standard browser hotkeys |
| **Navigation** | `AutoSuggestBox` with fuzzy autocomplete (`Levenshtein ≤2`), auto HTTPS upgrade, F12 DevTools per tab |
| **Data Layer** | SQLite + Dapper, queue-based bookmark/history flushing, 30-day auto-purge, session restore on launch/crash |
| **Performance** | Tab suspension on inactivity/low RAM, on-demand favicon caching (`MemoryCache`), minimal memory footprint |
| **Privacy** | Third-party cookie blocking, `DNT: 1` header enforcement, silent date-rotated logging (`logs/`) |
| **Updates** | Manual "Check for Updates" → in-app HTTP fetch → graceful installer prompt |
| **Build** | Framework-dependent, runtime-stripped via CI, ~2–4 MB portable zip |

---

## 🏗️ Architecture
