##Nathan Hinton
##This is the mze generator for the maze escape game.

#import numpy as np
from random import randint, choice

def generate_maze(height, width): ##This will generate a 0 for a blank space and a 1 for a full space
    maze = []
    for x in range(0, width+2):#Adding 2 for the outside perimeter
        y_tmp = []
        for y in range(0, height+2):
            y_tmp.append(1)
        maze.append(y_tmp)
    ##Set the borders
##    for x in range(0, width+2):
##        maze[0][x] = 1
##        maze[-1][x] = 1
##    for y in range(0, height+2):
##        maze[y][0] = 1
##        maze[y][-1] = 1
    ##Make the maze begining
    start_position = randint(1, len(maze[0])-2)
    maze[0][start_position] = 0
    maze[1][start_position] = 0
    ##Make the rest of the maze:
    history = [[0, start_position]]
    position = [1, start_position]
    counter = 0
    maze_done = False
    while maze_done == False:
        maze[position[0]][position[1]] = 0
        options = ['up', 'right', 'down', 'left']
        ##Eliminate the dirrections available:
        try:
            if maze[position[0-1]][position[1]] == 0 or position[0-1] < height:
                options.remove('down')
        except IndexError:
            options.remove('down')
        try:
            if maze[position[0+1]][position[1]] == 0 or position[0+1] > height:
                options.remove('up')
        except IndexError:
            options.remove('up')
        try:
            if maze[position[0]][position[1-1]] == 0 or position[1-1] > width:
                options.remove('left')
        except IndexError:
            options.remove('left')
        try:
            if maze[position[0]][position[1+1]] == 0 or position[1+1] < width:
                options.remove('right')
        except IndexError:
            options.remove('right')
        if len(options) == 0:
            ##Move back a step;
            position = history[-1]
        else:
            c = choice(options)
            print(c)
            print(options)
            if c == 'up':
                position = [position[0]+1, position[1]]
            elif c == 'down':
                position = [position[0]-1, position[1]]
            elif c == 'left':
                position = [position[0], position[1]-1]
            elif c == 'right':
                position = [position[0], position[1]+1]
        counter += 1
        if counter > 10:
            maze_done = True
        input()
        for line in maze:
            print(line)
        
    ##Return the data to the program...
    return (start_position, maze)


##Here is the self test section
print()
print()
print()
print()
print()
m = generate_maze(10, 10)
for line in m[1]:
    print(line)
