##Nathan Hinton

import pygame
from levels import levels
from brick import Brick

##Game vars:
currentLevel = 1
resetLevel = True
verticalBrickCount = 20
horizontalBrickCount = 15
bricks = []

pygame.init()
screen = pygame.display.set_mode((800, 640))
pygame.display.set_caption('Idle Breakout')

background = pygame.Surface(screen.get_size())
background = background.convert()
background.fill((0, 0, 0))

running = True
while running:
    if resetLevel:
        level = levels[currentLevel]
        for aa in range(0, 8):
            for bb in range(0, 8):
                if level[aa][bb] == 1:
                    for xx in range(0, int(horizontalBrickCount/8)):
                        for yy in range(0, int(verticalBrickCount/8)):
                            bricks.append(Brick((horizontalBrickCount/8*aa)+(horizontalBrickCount/8+xx), 
                            (verticalBrickCount/8*bb)+(verticalBrickCount/8*yy), int(horizontalBrickCount/8), int(verticalBrickCount/8), currentLevel))
        resetLevel = False
    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            running = False #End the main loop
    ##Update screen:
    screen.blit(background, (0, 0))
    for brick in bricks:
        brick.draw(screen)
    pygame.display.flip()

pygame.quit()

print("Finished!")