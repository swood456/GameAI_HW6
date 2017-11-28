Running code:
	To place a start point, press S and a start point will be placed where your mouse is in the scene
	To plaec an end point, press E and an end point will be placed where your mouse is in the scene
	To start pathfinding, simply press the Find Path button

	To move the camera around, use the arrow keys
	To zoom the camera in or out, use the - and = keys, respectively.
other GUI:
	To change the weight of the heuristic, use the labeled slider. The default value is 2, and can move from 0 to 10
	To pick the Heuristic mode, use the dropdown menu

	To swap from A* to Waypoint version and vice versa, use the button that will state the opposite of the current version
		note that for the grid versions this could take upwards of 10 seconds for the map to be built up from the input file

	The red dots represent tiles or waypoints that were visited but not in the final path. The green represent the ones that were
		in the final path.

	There is also a small triangle (hard to see in the tile versions) that will follow the path from start to end and avoid walls