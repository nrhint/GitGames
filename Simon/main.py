##Nathan Hinton
##This is the main file for simon

from time import time, sleep

programStart = time()

import pygame
from simon import *

##Vars that you can change:
fps = 60 #This will dirrectly change the hardness of the game.
step = 1/fps #Using time to delay if needed
width, height = 600, 600

pygame.init()
screen = pygame.display.set_mode((width, height))
pygame.display.set_caption("Simon")

background = pygame.Surface(screen.get_size())
background = background.convert()
background.fill((0, 0, 255))

pygame.font.init() # you have to call this at the start, 
                   # if you want to use this module.
myfont = pygame.font.SysFont('Comic Sans MS', 30)

gameStart = time()

simon = Simon(screen)
##simon.update()
##pygame.display.flip()
##sleep(2)

run = True
frameCount = 0
idle = 0
sleep(1)
while run == True:
    nextStep = time()+step
    ##Logic:
    clickPos = (0, 0)
    if pygame.mouse.get_pressed()[0]:#If the first mouse button is pressed
        clickPos = pygame.mouse.get_pos()
    run = simon.run(clickPos)
    ##Update Screen:
    screen.blit(background, (0, 0))
    simon.update()
    pygame.display.flip()
    ##Sleep if needed
    if nextStep - time() > 0:
        sleep(nextStep - time())
##    while time()<nextStep:
##        idle += 1
    frameCount += 1
    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            pygame.quit()
            run = False #End the main loop
#pygame.quit()
score = len(simon.history)
##Show your score and the high score:
try:
    scoreData = int(open('scores', 'r').read())
except FileNotFoundError:
    scoreData = 0
if score > scoreData:
    file = open('scores', 'w')
    file.write(str(score))
    file.close()
    screen.blit(background, (0, 0))
    scoreData = score

myfont = pygame.font.SysFont('Comic Sans MS', 60)
scoreSurface = myfont.render('High score: %s'%scoreData, False, (0, 0, 0))
levelSurface = myfont.render('Your score: %s'%score, False, (0, 0, 0))
screen.blit(background, (0, 0))
screen.blit(scoreSurface,(10,height/3))
screen.blit(levelSurface,(10,(height/3)*2))
pygame.display.flip()

end = time()
print("The time taken to load was %s seconds"%(gameStart-programStart))
print("The avg fps was %sfps"%(frameCount/(end-gameStart)))
showScore = True
while showScore:
    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            pygame.quit()
            showScore = False #End the main loop
pygame.quit()
