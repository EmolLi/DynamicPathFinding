# DynamicPathFinding

## Overview
This project is an assignemnt for COMP 521 (Modern Computer Games) in McGill University. It simulates the shoppers behavior 
in a 2-story mall using cooperative (dynamic) path finding proposed in this [paper](https://pdfs.semanticscholar.org/ec6e/5c1a3a5729094347076fc45a503abd630eb8.pdf).

The project is written in C#, with Unity 3D. The source code is in [Scripts](https://github.com/EmolLi/DynamicPathFinding/tree/master/Assets/Script) folder.

Following is the detailed assignment requirement.

## Requirement

The overall context simulates a 2-story mall, with an upper level of 6 stores connected to a lower level of 6 stores by 15
4 stairways. Each store has an entranceway wide enough only for a single person to pass through at a time, and have
an interior size such that it can hold 9 people. The area outside the stores, should have 4 plants (small obstacles), randomly placed (ie in a different location each
time you run it).

Shoppers may traverse stairs in either direction (up/down), although the stairways are not wide enough for two people
to be side-by-side. The actual length of the stairs is measured in number of people who fit on the stairway. It is the
same for each set of stairs in a given simulation, but needs to be a parameter in creating your simulation—you will
need to generate multiple simulations with different stair lengths.

This is a 3D design, but we will treat it as planar by ensuring that the levels do not overlap; see the layout sketch
below (note that the stores do not have any ceilings). This will let you fully observe the simulation from a top-down
camera, and simplify motion-planning.

![img](https://github.com/EmolLi/DynamicPathFinding/blob/master/Screenshot%20from%202019-06-27%2014-34-39.png)

The layout is not necessarily to scale or in proper proportion; there should be ample space in front of the stores for
shoppers to move around (3–5 deep), and width-wise there should be room for at least 5 shoppers between each shop
entrance.

Populate the mall with shoppers. The 15 number of shoppers will also need to be changed, so make it a simulation parameter.
Shoppers are initially randomly located, but never overlap.

Shoppers move around the mall, shopping and doing idle behaviour. They must never overlap or pass through each
other. They all move discretely, at the same speed, with movement based on a grid. You may use Manhattan or
8-way (octile or Chebyshev) motion.

Each shopper repeatedly attempts to perform a behaviour, selecting one with equal probability:

(a) Move Randomly choose a specific, currently unoccupied destination point outside of a shop, on either level,
and go there.

(b) Shop Randomly choose one of the (12) shops as a goal, excluding the same shop as they are in if they are in
one, and go enter the shop (any spot inside will do).

This will require actual motion-planning. Use a central, coordinated approach based on Silver’s reservation-based
pathfinding for your shoppers. The size of the planning window (time-duration) should be a parameter you can easily
vary.

There are various additional factors involved in this design that will impact the ability of your shoppers to shop
effectively. Ensure that with a short, 2-step stairway length, and only a small number of shoppers (around 5–10) all
shoppers are able to get to their destinations.

The design should avoid head-to-head movement conflicts. 
