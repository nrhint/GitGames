##Nathan Hinton
#This is a clone of flip out.
##Goals:
# 1) Larger game board

#Setup Vars:

(widths, heights) = (20, 20)
size = 20
BLACK = (0, 0, 0)

#File paths:
red = './Red.png'
blue = './Blue.png'
green = './Green.png'
yellow = './Yellow.png'

#Round the game size to be nice:
(width, height) = (widths*17, heights*17)

import pygame
from random import choice
from time import sleep

choices = ['red', 'blue', 'green', 'yellow']

class Chip:
    def __init__(self, surface, posx, posy, color):
        self.x = posx
        self.y = posy
        if color == 'red':
            self.color = red
        elif color == 'blue':
            self.color = blue
        elif color == 'green':
            self.color = green
        elif color == 'yellow':
            self.color = yellow
        else:
            print("!INVALID COLOR OPTION!")
        self.image = pygame.image.load(self.color).convert_alpha()
        surface.blit(self.image, (self.x, self.y))
    def drop(self):
        pass#if a peice is not under it then fall down until peice is under
    def score(self):
        pass

#Square size = size
def drawBG():
    for x in range(int(width/size)):
        pygame.draw.line(screen, BLACK, (x*size, 0), (x*size, height), 3)
        for y in range(int(height/size)):
            pygame.draw.line(screen, BLACK, (0, y*size), (width, y*size), 3)

##Setup the game:
pygame.init()
pygame.display.set_caption('FlipOut 2.0')
screen = pygame.display.set_mode((width, height))
background_color = (255,255,255)
screen.fill(background_color)
pygame.display.flip()
##drawBG()
##pygame.display.flip()

chips = []
for posx in range(widths):
    for posy in range(heights):
        chips.append(Chip(screen, posx*size, posy*size, choice(choices)))

pygame.display.flip()    

#Main Loop:
running = True
##running = False
while running:
    pygame.display.flip()
    for event in pygame.event.get():
        if event.type == pygame.MOUSEBUTTONUP:
            pos = pygame.mouse.get_pos()
            print(pos)
            print(int(pos[0]/size), int(pos[1]/size))
        if event.type == pygame.QUIT:
            running = False
            pygame.quit()
