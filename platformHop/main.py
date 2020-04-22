##Nathan Hinton
##This is the main file for platform hop

##Import the basics
from time import time
totalStart = time()
import pygame
from random import randint

##Import local files:
from player import *
from platforms import *
from colors import *

##vars you can change
level = 'level1.level'
fps = 200
frameStep = 1/fps
width, height = (600, 480)
backgroundColor = (0, 100, 200)

run = True
pygame.init()
screen = pygame.display.set_mode((width, height))
pygame.display.set_caption("Platform Hop")

background = pygame.Surface(screen.get_size())
background = background.convert()
background.fill(backgroundColor)
floor = pygame.Rect(0, height, width, 2)

##This is if we want to display text on the screen
#font = pygame.font.SysFont('Comis Sans MS', 30)

##Setup the player:
player = Player(screen, (0, 0))
##Setup the platforms
##Parse the level file
objects = []
try:
    file = open(level, 'r').read()
    fileLines = file.split('\n')
    for fileLine in fileLines:
        try:
            objects.append(eval(fileLine))
        except SyntaxError:
            pass
except FileNotFoundError:
    print("LEVEL NOT FOUND")
    print("Failed to open %s"%level)
    run = False
##Setup the main loop:
gameStart = time()

frameCount = 0
idle = 0 #This is used by the dynamic wait to make it do something
while run == True: #Loop while run is true
    nextStep = time() + frameStep
    #Get keypresses here
    keysPressed = pygame.key.get_pressed()
    keys = []
    if keysPressed[pygame.K_SPACE]:
        keys.append('space')
    if keysPressed[pygame.K_RIGHT]:
        keys.append('right')
    if keysPressed[pygame.K_LEFT]:
        keys.append('left')
    ##Do the thinking:
    rects = [floor]
    for obj in objects:
        rects.append(obj.rect)
        obj.run()
    touchingObject = False
    if pygame.Rect.collidelist(player.rect, rects) != -1:
        touchingObject = True
    player.run(keys, touchingObject)
    ##Update the screen
    screen.blit(background, (0, 0))
    pygame.draw.rect(screen, (0, 0, 0), floor)
    for obj in objects:
        obj.update()
    player.update()
    pygame.display.flip()#Show the updated screen
    #Wait if needed:
    while time() < nextStep:
        idle += 1
    frameCount += 1
    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            pygame.quit()
            run = False
end = time()
print("The time taken to load was %s seconds"%(gameStart-totalStart))
print("The avg fps was %sfps"%(frameCount/(end-gameStart)))
