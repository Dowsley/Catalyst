# Catalyst
This project uses [FastNoiseLite](https://auburn.github.io/FastNoiseLite/).

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
- [X] Basic Input System (Keyboard)
  - [X] Core, Keyboard
  - [X] Mouse
- [X] Only render what's visible
- [X] Basic World Generation
  - [X] Passes
  - [X] Layers
  - [X] Change spawn (not working with caves)
- [ ] Add map/debug mode to help see whole map
  - [ ] Input Handling?
- [ ] Complex World Generation
- [ ] Walls
- [ ] Tile Types from XML (data-driven approach)
- [ ] Autotiling/Atlas
- [ ] Lighting
- [ ] Entity System
- [ ] Tile State (Manager)
- [ ] Player Interaction with the world
  - [ ] Actions (no need to generalize for Entity right now)
- [ ] UI
- [ ] Make Input Action registering data-driven (like in godot)
- [ ] Saving
- [ ] Chunking