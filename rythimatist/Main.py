##Nathan Hinton

#This game is basd on the book the Rythamist. It is about drawing lines and shapes

from time import time, sleep

programStart = time()

import pygame
#import local files here:
from player import *

##Vars that you can change:
fps = 60 #This will dirrectly change the hardness of the game.
step = 1/fps #Using time to delay if needed
width, height = 800, 600

pygame.init()
screen = pygame.display.set_mode((width, height))
pygame.display.set_caption("Pythamist")

background = pygame.Surface(screen.get_size())
background = background.convert()
background.fill((255, 255, 255))

##pygame.font.init() # you have to call this at the start, 
##                   # if you want to use this module.
##myfont = pygame.font.SysFont('Comic Sans MS', 30)

gameStart = time()

player = Player(screen)

run = True
frameCount = 0
idle = 0
while run == True:
    nextStep = time()+step
    ##Logic:
    player.run()
    ##Update Screen:
    screen.blit(background, (0, 0)) 
    player.update()
    pygame.display.flip()
    if time()<nextStep:
        sleep(nextStep-time())
    frameCount += 1
    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            pygame.quit()
            run = False #End the main loop
#pygame.quit()
end = time()
print("The time taken to load was %s seconds"%(gameStart-programStart))
print("The avg fps was %sfps"%(frameCount/(end-gameStart)))

