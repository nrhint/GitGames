##Nathan Hinton
##this is the main file for notebook wars
##I want the files to contain the configuration data so that they don't have to worry

#vars
fps = 30
level = 'level1.l'

import pygame
from time import sleep

from player import *
from enemy import *

#load the level:
file = open(level, 'r').read()
fileSplit = file.split('\n')
fileLines = []
for line in fileSplit:
    if line[0] == '#':
        pass
    else:
        fileLines.append(line)
lineNum = 0
width, height = fileLines[lineNum].split(', ')
width = int(width)
height = int(height)
lineNum += 1
color = fileLines[lineNum].split(', ')

pygame.init()

lineNum += 1

#main loop:
while True:
    #i = menu()
    screen = pygame.display.set_mode((width, height))
    pygame.display.set_caption('Notebook wars')
    background = pygame.Surface(screen.get_size())
    background = background.convert()
    background.fill((int(color[0]), int(color[1]), int(color[2])))
    #Setup payer:
    player = Player(screen, ship = 0, bullet = 0)
    enemies = []
    #main game loop:
    loop = True
    frameCount = 0
    while loop == True:
        frameCount += 1
        place = fileLines[lineNum].split(', ')
        if int(place[0][1:]) == frameCount:
            for x in place[1:]:
                if x[1] == '@':
                    pass
                else:
                    shipInfo = x.split(',')
                    enemies.append(Enemy(screen, int(shipInfo[2]), 50, ship = int(shipInfo[0]), bullet = int(shipInfo[1]), patern = shipInfo[3]))
                    print(shipInfo)
        #Get input:
        mousePos = pygame.mouse.get_pos()
        #Logic:

        #Run the objects:
        player.run(mousePos, frameCount)
        for e in enemies:
            e.run(frameCount)
        #Update:
        screen.blit(background, (0, 0))
        player.update()
        for e in enemies:
            e.update()
        pygame.display.flip()
        sleep(1/fps)
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                pygame.quit()
                loop = False #End the main loop
