# Turn-Based Strategy Game Prototype

A turn-based strategy video game prototype developed as a **Bachelor's Thesis (TFG)** in Computer Engineering at the **Universitat Politècnica de València (UPV)**.

The project focuses on designing and implementing a tactical strategy game inspired by titles such as *XCOM 2*, *Into the Breach*, and other grid-based strategy games. The objective is to create a playable prototype that integrates different gameplay systems, including grid management, turn-based combat, probabilistic attacks, artificial intelligence, and tactical decision-making.

## Features

* **Turn-based combat system**

  * Alternating player and enemy turns.
  * Action point management system for unit actions.
  * Event-based communication between gameplay systems.

* **Grid-based map system**

  * Square tile grid for tactical movement.
  * Reachable tile calculation using the A* pathfinding algorithm.
  * Manhattan distance used for range and visibility calculations.

* **Tactical combat mechanics**

  * Probability-based attack system.
  * Accuracy influenced by:

    * Base weapon accuracy.
    * Distance.
    * Cover.
    * Height differences.
  * Different attack types:

    * Ranged attacks.
    * Melee attacks.
    * Push attacks.
    * Area-of-effect grenade attacks.

* **Cover and environment interaction**

  * Cover objects modify attack probabilities.
  * Raycasting used to detect obstacles between units.
  * Destructible objects and interactive elements.

* **Fog of War**

  * Dynamic visibility system.
  * Tiles outside unit vision are hidden using a fog overlay.
  * Visibility calculated according to unit position and vision range.

* **Enemy Artificial Intelligence**

  * AI-controlled units capable of evaluating possible actions.
  * Decision-making based on attack opportunities and tactical impact.

* **User interface**

  * Displays unit information, action points, health, and combat information.
  * Provides feedback during player interactions.

## Technologies Used

* **Game Engine:** Unity
* **Programming Language:** C#
* **Version Control:** Git / GitHub

Additional tools:

* Blender
* GIMP
* DaVinci Resolve
* Unity ProBuilder
* Unity Cinemachine

## Installation and Execution

### Requirements

* Unity version: `2022.1.0f1`
* Operating system: Windows (recommended)

### Steps

1. Clone this repository.

2. Open the project using Unity Hub.

3. Select the appropriate Unity version.

4. Open the main scene located in (Scene 1 or scene 2):

```
Assets/Scenes/<main-scene-name>
```

5. Press **Play** in Unity to run the prototype.

## Controls

| Action        | Control                 |
| ------------- | ----------------------- |
| Move camera   | WASD                    |
| Select unit   | Left mouse button       |
| Move unit     | Select destination tile |
| Rotate camera | Q or E                  |
| Zoom          | Mouse wheel             |


## Development

This project was developed as part of my Bachelor's Thesis with the goal of applying software engineering concepts to the development of a complete video game prototype.

The development process involved:

* Requirements analysis.
* Software architecture design.
* Implementation of gameplay systems.
* Testing and debugging.
* Evaluation of different approaches for AI and game mechanics.

## Author

**Jiajing Dong**

Bachelor's Degree in Computer Engineering
Universitat Politècnica de València (UPV)

## License

This project was developed for academic purposes.


> Note: This project was originally developed as a private academic project. Some parts of the code may lack comments or additional documentation, as public release was not planned initially.
