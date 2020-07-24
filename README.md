# 2x2 and 3x3 Rubik's cube solver for Computer Science A-Level NEA

## Finished minimum viable product
1. The user can input the state of their cube via the command line
	1. A letter corresponds to a colour (e.g. White -> W, Yellow -> Y, Green -> G, Blue -> B, Red -> R, Orange -> O)
		1. The actual letters used does not matter, as long as each colour has a unique letter
	1. The stickers are input in the following order (holding a fixed orientation)
	```
				    0  1  2
				    3  4  5
				    6  7  8

		 9 10 11  18 19 20  27 28 29  36 37 38
		12 13 14  21 22 23  30 31 32  39 40 41
		15 16 17  24 25 26  33 34 35  42 43 44

				    45 46 47
				    48 49 50
				    51 52 53
	```		
1. The program must detect the colour scheme of the cube based on the input
	1. The centre pieces (4, 13, 22, 31, 40, 49) are used as reference for the colour scheme as they are fixed relative to each other (they cannot be moved by only turning the outer faces of the cube)
1. The program can find the result of performing a move (turn of one of the outer faces) on the cube
1. The user must be able to input a move for the program to execute
1. The program must be able to do pattern matching to check if the cube is in a solved state

## Finished stage 2 - full terminal
1. The program checks that the cube is in a valid state
	1. There are 9 stickers of each colour
	1. Each piece of the cube occurs exactly once
	1. The sum of the orientations of the edge pieces is a multiple of 2 (oriented = 0 and misoriented = 1)
	1. The sum of the orientations of the corner pieces is a multiple of 3 (oriented = 0, twisted clockwise = 1, twisted counter-clockwise = 2)
1. The program can tell the user how to solve their cube using WCA notation
    1. The program will continue looking for more efficient solutions until the user stops it
1. The program should be able to generate a valid random cube state (with all possible cube states being equally likely)