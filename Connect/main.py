##Nathan Hinton
##This is for the connect game

from time import time, sleep

programStart = time()

import pygame
from node import *

##Vars that you can change:
fps = 60 #This will dirrectly change the hardness of the game.
step = 1/fps #Using time to delay if needed
width, height = 800, 600
spaceBetweenbads = fps

pygame.init()
screen = pygame.display.set_mode((width, height))
pygame.display.set_caption("CONNECT")

background = pygame.Surface(screen.get_size())
background = background.convert()
background.fill((0, 0, 255))

##pygame.font.init() # you have to call this at the start, 
##                   # if you want to use this module.
##myfont = pygame.font.SysFont('Comic Sans MS', 30)

gameStart = time()

##For testing:
nodes = [Node(screen, 'human', (int(width/2), int(height/2)))]

run = True
frameCount = 0
idle = 0
while run == True:
    nextStep = time()+step
    ##Logic:
    for node in nodes:
        node.run()
    ##Update Screen:
    screen.blit(background, (0, 0))
    for node in nodes:
        node.update()
    pygame.display.flip()
    frameCount += 1
    if time()<nextStep:
        sleep(nextStep-time())
    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            pygame.quit()
            run = False #End the main loop
#pygame.quit()
end = time()
print("The time taken to load was %s seconds"%(gameStart-programStart))
print("The avg fps was %sfps"%(frameCount/(end-gameStart)))
