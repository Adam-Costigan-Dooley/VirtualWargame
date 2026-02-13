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

**Sprint 1 (Feb 2-15, 2026):** Frontend Prototype - IN PROGRESS

### Completed Features

#### Authentication System
- **Login Screen** with Unity Gaming Services integration
- **Username/Password Authentication** - Secure account creation and login
- **Guest Mode** - Anonymous authentication for quick testing
- **Validation** - Username (3+ chars), Password (6+ chars)
- **Error Handling** - Clear user feedback on authentication failures
- **Scene Transitions** - Smooth flow from Login → Main Menu

#### User Interface
- **Main Menu** - Play, Settings, Quit buttons
- **Responsive Layout** - Scales to different screen sizes
- **TextMeshPro Integration** - Clean, readable text rendering

### In Progress
- Tile map rendering system
- Unit GameObject foundation
- Camera controls for map navigation

### Upcoming Features
- **Sprint 2 (Feb 16-Mar 1):** Map rendering, Unit display, Camera controls
- **Sprint 3 (Mar 2-15):** Action assignment UI, Turn submission system
- **Sprint 4 (Mar 16-29):** Combat resolution, Movement system
- **Sprint 5 (Mar 30-Apr 12):** Multiplayer synchronization, Intelligence system
