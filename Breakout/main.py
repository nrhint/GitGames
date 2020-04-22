##Nathan Hinton
##This is the main file for breakout

from time import time

programStart = time()

import pygame
from paddle import *
from ball import *

##Vars that you can change:
fps = 60 #This will dirrectly change the hardness of the game.
step = 1/fps #Using time to delay if needed
width, height = 800, 600
spaceBetweenbads = fps

pygame.init()
screen = pygame.display.set_mode((width, height))
pygame.display.set_caption("Breakout")

background = pygame.Surface(screen.get_size())
background = background.convert()
background.fill((0, 0, 255))

pygame.font.init() # you have to call this at the start, 
                   # if you want to use this module.
myfont = pygame.font.SysFont('Comic Sans MS', 30)

gameStart = time()

paddle = Paddle(screen)
ball = Ball(screen)

run = True
frameCount = 0
idle = 0
while run == True:
    nextStep = time()+step
    ##Get keyresses here:
    keysPressed = pygame.key.get_pressed()
    keys = []
    if keysPressed[pygame.K_RIGHT]:
        keys.append('right')
    if keysPressed[pygame.K_LEFT]:
        keys.append('left')
    ##Logic:
    paddle.run(keys)
    ball.run()
    ##Update Screen:
    screen.blit(background, (0, 0))
    paddle.update()
    ball.update()
    pygame.display.flip()
    while time()<nextStep:
        idle += 1
    frameCount += 1
    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            pygame.quit()
            run = False #End the main loop
#pygame.quit()
end = time()
print("The time taken to load was %s seconds"%(gameStart-programStart))
print("The avg fps was %sfps"%(frameCount/(end-gameStart)))
