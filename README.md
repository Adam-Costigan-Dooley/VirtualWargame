# Virtual Wargaming - Final Year Project

**Student:** Adam Costigan Dooley (20101954)  

## Project Overview

Virtual Wargaming is a digital adaptation of community-run Discord wargames, transforming manual, slow-paced turn-based strategy gameplay into an automated, efficient multiplayer experience.

### The Problem
Discord-based wargames currently suffer from:
- **5-7 day turn resolution times** due to manual processing
- **Human error** in adjudication and calculations
- **Volunteer GM burnout** from manual work

### The Solution
An automated Unity-based game that:
- Reduces turn resolution from days to **minutes**
- Enables **asynchronous multiplayer** via cloud services
- Features **fog of war** and **intelligence gathering** systems

---

## Project Goals

1. **Validate Technical Feasibility** - Prove core systems work at scale
2. **Automate Turn Resolution** - Eliminate manual game master workload
3. **Enable Cloud Multiplayer** - Support 2-4 players asynchronously
4. **Build Modular Architecture** - Support multiple game scenarios/themes

---

## Current Status
**Sprint 2 (Feburary 16-27, 2026):** Completed
**Sprint 3 (March 2-13, 2026):** Starting

### Completed Features

#### Sprint 1 (Feb 2-13) - Authentication & UI Foundation
- **Login Screen** with Unity Gaming Services integration
- **Username/Password Authentication** - Secure account creation and login
- **Guest Mode** - Anonymous authentication for quick testing
- **Main Menu** - Scene navigation with Play, Settings, Quit buttons
- **Responsive Layout** - Scales to different screen sizes

#### Sprint 2 (Feb 16-27) - Game Map & Combat System
- **Interactive Tile Map** - 12 clickable territories with adjacency system
- **Unit Assignment UI** - Scrollable panel for selecting and assigning units
- **Faction Control** - Visual indicators showing territory ownership (Red/Blue)
- **Camera Panning** - Click and drag to navigate the map
- **Turn-Based Combat** - Simultaneous resolution with attacker vs defender logic
- **Combat Calculations** - Strength-based combat with proper defender inclusion
- **Resource Generation** - Factions earn resources from controlled territories
- **Turn Processing** - Complete turn cycle with conflict resolution


Completed Features as of 
### Upcoming Features
- **Sprint 3 (Mar 2-15):** Action assignment UI [Partly Implemented in Sprint 2], Turn submission system [Partly Implemented in Sprint 2], "Game/Match Creation"
- **Sprint 4 (Mar 16-29):** Combat resolution [Partly Implemented in Sprint 2], Movement system, Improve code for map tiles
- **Sprint 5 (Mar 30-Apr 12):** Multiplayer synchronization, Intelligence system
