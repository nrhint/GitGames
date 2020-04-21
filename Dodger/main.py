##Nathan Hinton
##This is for the dodger game

import pygame

from time import sleep, time
from player import *
from badGuys import *
from random import randint

totalStart = time()

##Vars that you can change:
fps = 60 #This will dirrectly change the hardness of the game.
step = 1/fps #Using time to delay if needed
width, height = 600, 480
spaceBetweenbads = fps

pygame.init()
screen = pygame.display.set_mode((width, height))
pygame.display.set_caption("dodger")

background = pygame.Surface(screen.get_size())
background = background.convert()
background.fill((0, 0, 255))

##Setup the player
player = Player(screen, 'player.png')

##Add the bad guys:
badGuys = []
level = 1
maxBads = level*5
speed = level

gameStart = time()

##Main loop:
run = True
frameCount = 0
score = 0
idle = 0 #Give the program sometthing to do while waiting
pygame.mouse.set_visible(False)
timeSinceLastAdd = 0
while run == True:
    nextStep = time()+step
    ##Level up if needed:
    if score > (level*2)**2:
        level += 1
        maxBads = level*5
        speed = level
    ##Add badGuys when needed:
    timeSinceLastAdd += 1
    if len(badGuys) < maxBads and timeSinceLastAdd> (spaceBetweenbads/level) :
        badGuys.append(BadGuy(screen, 'badGuy.png', speed = speed))
        timeSinceLastAdd = 0
    ##Do all the thinking:
    badGuyRects = []
    for obj in badGuys:
        obj.run()
        badGuyRects.append(obj.rect)
        if obj.y > height:
            badGuys.pop(badGuys.index(obj))
            score += 1
    if pygame.Rect.collidelist(player.rect, badGuyRects) != -1:
        print("You loose!")
        print("You scorred %s points"%score)
        run = False
    player.run()
    ##Draw all of the stuff
    screen.blit(background, (0, 0))
    for obj in badGuys:
        obj.update()
    player.update()
    pygame.display.flip()
    #delay if needed:
    while time()<nextStep:
        idle += 1
    frameCount += 1
    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            pygame.quit()
            run = False #End the main loop
end = time()
print("The time taken to load was %s seconds"%(gameStart-totalStart))
print("The avg fps was %sfps"%(frameCount/(end-gameStart)))
