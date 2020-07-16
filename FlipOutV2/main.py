##Nathan Hintion
##This is a newer version of flipout that should work better.

##Setup:

(chipRow, chipHeight)= (15, 10)
size = 20
devl = True
fps = 60

##File paths:
red = './Red.png'
blue = './Blue.png'
green = './Green.png'
yellow = './Yellow.png'
white = './White.png'
clear = './Clear.png'


#do version stuff:
if devl:
    versionNum = int(open('versionNum').read())
    print(versionNum)
    open('versionNum', 'w').write(str(versionNum+1))
    caption = 'Flip Out -- version %s'%versionNum
else:
    versionNum = int(open('versionNum').read())
    print(versionNum)
    caption = 'Flip Out -- version %s'%versionNum


import pygame
from random import choice
from time import sleep, time
from chips import *

colorChoices = [red, blue, green, yellow]

pygame.init()

pygame.display.set_caption(caption)
screen = pygame.display.set_mode((chipRow*size, chipHeight*size))
background_color = (255,255,255)
screen.fill(background_color)
pygame.display.flip()

##load the chips:
chips = []
for x in range(0, chipRow):
    for y in range(0, chipHeight):
        chips.append(Chip(screen, x, y, choice(colorChoices)))
pygame.display.flip()

##Main loop:
run = True
lastClickPos = (-1, -1)
while run:
    frameEnd = time()+(1/fps)
    #do logic:
    for event in pygame.event.get():
        if event.type == pygame.MOUSEBUTTONUP:
            position = pygame.mouse.get_pos()
            for chip in chips:
                chip.click(position)

        if event.type == pygame.QUIT:
            pygame.quit()
    ##run the chips logic:
    for chip in chips:
        chip.run()

    screen.fill(background_color)
    for chip in chips:
        chip.update()
    pygame.display.flip()
    if time() > frameEnd:
        sleep(time()-frameEnd)
