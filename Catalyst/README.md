# Catalyst

## TODO
- [X] Basic world generation
- [X] Basic Player Entity
- [X] Working Collision System
- [X] Solve Graphical Problems
- [X] Sprite2D
- [X] Tiles System
- [X] Enable "Nullable" in project for better type safety
- [X] Decouple game world logic from LoadContent()
  - ~~Use a main Spritesheet and make sprites only point to them~~
  - Or alternatively load all sprites - and keep them in a dictionary where Sprites look them up
- [X] Good resolution and automatic scaling
  - Reverse engineer how many tiles fit in the Terraria's screen
- [X] Basic Input Handling System (Keyboard)
  - [X] Core, Keyboard
  - [X] Mouse
- [ ] Chunking (ongoing)
- [ ] World Generation System
- [ ] Tiles from XML (data-driven approach)
- [ ] Autotiling/Atlas
- [ ] Lighting
- [ ] Entity System
- [ ] Player Interaction with the world
  - [ ] Actions (no need to generalize for Entity right now)
- [ ] Tile State (Manager) 
- [ ] UI
- [ ] Make Input Action registering data-driven (like in godot)