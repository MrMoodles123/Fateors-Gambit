
# Fateor's Gambit

<img src="https://github.com/IbrahAbd/Flappy-Bird-in-PyGame/blob/main/flappy-bird-assets-master/finalFG.png" width="1000" height = "500"/>

<div align="center">
A high-stakes 1v1 FPS Arena Card Battler where every round is unpredictable.  

Step into the fight across three distinct battlegrounds (Gambit Grounds, Gambit Runner 3099, and Fateor’s Foyer) each featuring unique, theme-driven weaponry that changes the way you play.  Strategize, adapt, and outgun your opponent using a card-based system that ensures no two matches ever feel the same.

 ⚔️ Will you leave it to fate or take control with a gambit? ⚔️
 
 [Source files](https://drive.google.com/drive/folders/1PlO6uSis0Wpo8oFeZmvemRLxcDg413wU?usp=drive_link)
</div>

## How To Run

- The executable files can be found within the Fateor's Gambit folder. 
    - The Windows executable is located in the FateorsGambitWin folder and called "FateorsGambitWin.exe"
       - Launch it to play the game. 
- Press "Matchmake" to play. 
    - When trying to connect with a second player, make sure to let Player 1 load in completely before Player 2 begins matchmaking. Failure to do this will result in 2 different lobbies being created.
- A tutorial is available where you can learn the basics of the game. Press the "Tutorial" button to load into it.

## Template Used

The game was built off the free asset template: [Projectiles Advanced Multiplayer - Photon Fusion](https://assetstore.unity.com/packages/templates/systems/projectiles-advanced-multiplayer-photon-fusion-286072). The template uses Photon and Fusion 2 Multiplayer SDKs to setup basic multiplayer connections and logic for Unity. This template was editied and expanded to include our own round and game logic. 

The ranged weapons were provided by the template, with their damage numbers, ranges and attack rates being modified. 

All the melee weapons were taken from free assets provided on the unity asset store with their functionality being a modified implementation of the "WeaponBeam.cs" script. 
Like the ranged weapons, their damage numbers, ranges and attack rates are changed to provide variety between different weapons.

## Scripts 📜

- In the Scripts folder, the following sub-folders were provided by the template and left largely unchanged :
    - Extensions
    - Utilities
    - UI
    - Health
    - Projectiles
    - GameplayObjects 

- In the Game sub-folder: 
    - the GameManager and Spawnpoint scripts were unchanged from the template. 
    - The BeanBounce script was added. 
    - The Gameplay script was heavily changed, with basically no methods the same as the template.

- In the Scene sub-folder, All scripts were provided by the template. 
    - Only the SceneContext script was changed, with all the methods and variables added by us, except the first 6 variables.
      
- All scripts in the Audio, Cards, and Tutorial sub-folders were created by us.

- In the Tutorial sub-folder, all scripts are original for many of the wall triggers within the tutorial scene.
  
- Some Fusion scripts were built upon for the GUI:
    - FusionBootstrapDebugGUI was adjusted for the more styled main menu.
    - FusionBootstrapDebugGUIS was created for the settings menu.
    - FusionBootstrap was modified to restart after a game is completed.

## Assets used for maps 🗺️ , player models 🕹️, weapons 🗡️ and music 🎵
- https://assetstore.unity.com/packages/3d/props/3d-low-poly-tools-weapons-containers-274127
- https://assetstore.unity.com/packages/p/character-effects-shaders-304307
- https://assetstore.unity.com/packages/3d/props/weapons/dagger-69460
- https://assetstore.unity.com/packages/3d/props/weapons/free-rpg-weapons-199738
- https://assetstore.unity.com/packages/3d/environments/fantasy/halloween-pack-cemetery-snap-235573
- https://assetstore.unity.com/packages/3d/characters/little-ghost-lowpoly-free-271926
- https://assetstore.unity.com/packages/3d/props/weapons/low-poly-modular-swords-162070
- https://assetstore.unity.com/packages/3d/vegetation/trees/low-poly-trees-free-nature-pack-300824
- https://assetstore.unity.com/packages/3d/environments/landscapes/low-poly-woods-232818
- https://assetstore.unity.com/packages/3d/environments/dungeons/lowpoly-mysterious-dungeon-281809
- https://assetstore.unity.com/packages/3d/environments/dungeons/n-gonforge-dungeon-low-poly-303819
- https://assetstore.unity.com/packages/p/pbr-corinthian-helmets-230928
- https://assetstore.unity.com/packages/3d/environments/sci-fi/real-stars-skybox-lite-116333
- https://assetstore.unity.com/packages/vfx/shaders/retrowave-skies-lite-dynamic-synthwave-skybox-asset-pack-282063
- https://assetstore.unity.com/packages/3d/environments/sci-fi/sci-fi-futuristic-environment-pack-v2-0-246983
- https://assetstore.unity.com/packages/3d/environments/simplepoly-city-low-poly-assets-58899
- https://assetstore.unity.com/packages/p/versatile-building-kit-15-medium-poly-models-for-game-developmen-303398
- https://sketchfab.com/3d-models/neon-signs-307e887d740649f88fbc77b061f3a742
- https://sketchfab.com/3d-models/urban-enviroment-2e5cb092ff2948bc9c500b73aa5c7829
- https://syntystore.com/products/polygon-particle-fx-pack (Purchased from Humble Bundle for $1)
- https://freesound.org/people/SoundBiterSFX/sounds/730466/
- https://pixabay.com/music/video-games-gravedigger-spooky-graveyard-dance-halloween-whistling-electronic-148256/
- https://pixabay.com/music/beats-hype-trailer-background-advertising-showreel-289872/
- https://pixabay.com/music/upbeat-cyberpunk-music-277931/
- https://pixabay.com/music/upbeat-forward-312979/
  
## Known Bugs 🐞
- There are a number of UI bugs due to the nature of the game
  - After completing the tutorial, the player's client freezes once they return to main menu.
  - Occasionally the main menu UI will duplicate after you win or lose a game.
    
These issues were resolved by making the game restart itself after finishing the tutorial or the round.
    
## Developers 🛠️

- [Mahir Moodaley](https://github.com/MrMoodles123)
- [Ibrahim Abdou](https://github.com/IbrahAbd)
- [Kai Connock](https://github.com/kcurious)

