**Title:** Final Keep - Arena-Survival Roguelite

## Mechanics  
- Move player character (WAD) (or on android up/down/left/right based on where they click the screen)
- Rebuild walls/doors when holding down movement, on them.
- Enemies spawn to left and right side of player, they can break doors.
- Castle staff the player finds can be hired to man walls/doors to destroy enemies.  
- Player loses on consectuive attack from enemies. Can survive one hit, every placeholder 5 seconds.
 Day/night cycle to signal the enemie attackers. Can only be viewed through the windows in the throne room. Allowing players to get surprised if they wander too far off.
- Unit unlocks are saved across playthroughs, alongside unit upgrade score boosters.

## Aesthetics 
- Medieval pixel castle being attacked by hordes of enemies
- Player is besieged and must revigorate the keeps defenders to keep the enemies out.
- Day/night cycle to signal the enemie attackers. Can only be viewed through the windows in the throne room. 

## Dynamics 
- Units and enemies interact in real time independent of players view
- Player balances upgrading vs. expanding    
- Difficulty scales with wave progression, additional difficulties modifiers unlocked each run to enable score boosters 

## Scope Lock  (CA1–CA3 Targets)
**CA1:** Splash screen + main menu + first working build.  
**CA2:** Playable vertical slice (single room, one tower/door each side (player invests their score in upgrading the base stats of one unit instead of multiple units for this demo), single enemy spawn type, scoring).  
**CA3:** Polished slice with proper main menu, further one-two rooms in either left/right direction, and 1-2 additional unit types.

## Performance Budget (Target / Minimum)
| Metric | Target | Minimum |
Frame-time | 60 FPS | 30 FPS |
Memory | 300 MB | 512 MB |
APK size | 150 MB | 400 MB |
Load time | 5 s | < 20 s |

## Cut List  (What Drops First)
1. Cap the total waves to a limited amount to prevent extreme scaling pushing demands
2. Reduce enemy variety (keep 1–2 types).  
3. Reduce unit variety (keep 1–2 types)
4. Visual day/night cycle for the enemy spawn phases. Change to a base timer on screen. (Maybe make this a base upgrade)
5. additional difficulties modifiers
6. Simplify visuals.  
7. Additional rooms outside the base spawn room

## Risks
Gameplay will relatively repetitive/predictable in all splices versions upon repeated play
Multiple units may be ambitious, but is hopefully mitigated by the time saved on AI, as enemies will simply have to walk to the left or right on spawning with little other mechanical depth besides stopping to hit walls.