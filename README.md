# Steel Colony — Extended Mechs

A complete mechanoid colony experience for RimWorld. Command a fully self-sufficient army of specialized mechanoids covering every colony need—mining, medicine, crafting, research, defense, and beyond. Focus your strategy around bandwidth limits, not colonist count, to build a self-sustaining mechanical empire.

---

## Key Features

### 1. Expanded Mechanoid Roster

#### Tier 1 — Basic Labor (Basic Subcore, Small Gestator)
*   **Mining Drone** (`SC_Mech_MiningDrone`): Compact drill-arm chassis built for deep rock extraction and hauling.
*   **Cook Unit** (`SC_Mech_CookUnit`): Multi-limbed culinary chassis with calibrated sensors. Consistent meal quality with near-zero food poison chance.
*   **Firefighter Unit** (`SC_Mech_FirefighterUnit`): Rapid suppressive foam unit that prioritizes fire extinguishing above all else.
*   **Warden Unit** (`SC_Mech_WardenUnit`): Prisoner management unit. Feeds and monitors detainees with cold, mechanical efficiency (cannot recruit).
*   **Repair Drone** (`SC_Mech_RepairDrone`): Maintenance chassis equipped with custom AI that prioritizes structural repairs over new construction.
*   **Hauler Mk2** (`SC_Mech_HaulerMkII`): Heavy-frame logistics unit. Slower than a standard Lifter but carries double the capacity (150).
*   **Refinery Refueler** (`SC_Mech_Refueler`): Speeds up bulk production (cutting stone, refining chemfuel, smelting) and manages fuel levels.

#### Tier 2 — Specialist Labor (Standard Subcore, Large Gestator)
*   **Animal Handler** (`SC_Mech_AnimalHandler`): Zoological management unit. Feeds, trains, and tames wild animals utilizing pacifying pheromone emitters.

#### Tier 3 — Advanced Systems (High Subcore, Large Gestator)
*   **Research Unit** (`SC_Mech_ResearchUnit`): Cognitive processor dedicated to systematic knowledge acquisition. Researches tirelessly at research benches.
*   **Sentinel** (`SC_Mech_Sentinel`): Heavy armored combat chassis. Anchors to a player-designated post, engages all threats within range, and automatically returns to post when idle.

---

### 2. Infrastructure & Bandwidth Systems

*   **Advanced Band Node** (`SC_AdvancedBandNode`): High-gain signal amplifier providing +5 bandwidth to its tuned mechanitor.
*   **Control Relay Tower** (`SC_MechControlRelay`): Expands bandwidth by +10. When powered, it extends the mechanitor's command range map-wide (suppressing the command radius indicator).
*   **Subcore Buffer Rack** (`SC_SubcoreBuffer`): High-tech server rack that integrates active subcores directly into the mechnet, generating bandwidth (+1 for Basic, +2 for Regular, +4 for High subcores stored).
*   **Central Control Hub** (`SC_MechCentralControl`): Endgame 4x4 console array (+15 bandwidth). Once active and powered, it broadcasts control synchronization protocols, allowing your mechs to operate fully autonomously across the map without requiring a human overseer (feral-proof, map-wide operation).

---

### 3. Machined Subcores (Solo-Run Mechanics)
To support the "zero-human" endgame progression, standard Biotech brain scanners are bypassable:
*   **Machined Regular Subcore**: Assemble regular subcores at a machining table using advanced materials (250 Steel, 50 Plasteel, 6 Components). Requires `Specialist Mechs` research.

---

## Technical Information & C# Systems

The mod contains custom C# AI logic and Harmony patches targeting **RimWorld 1.6**:
*   **Repair Prioritizer**: Intercepts the repair drone to search and prioritize structural patching before executing general construction work tasks.
*   **Research NRE Fix**: Dynamically instantiates a skills tracker for the Research Unit to prevent game-breaking NullReferenceExceptions when learning/researching.
*   **Stat Overrides**: Injects base taming and training success chances for the Animal Handler's animal interactions.
*   **Autonomous Operation**: Overrides mech connection loss checks, feral timers, and forces mechs to default to `Work` mode when the Central Control Hub is powered.

---

## Installation & Requirements

1.  Requires **RimWorld (Version 1.6)**.
2.  Requires the **Biotech DLC**.
3.  Load Order:
    *   Ludeon Core
    *   Biotech (DLC)
    *   **Steel Colony — Extended Mechs** (This mod)
