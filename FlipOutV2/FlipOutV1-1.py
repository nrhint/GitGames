##Nathan Hinton
#This is a clone of flip out.
##Goals:
# 1) Larger game board

#Setup Vars:

(widths, heights) = (3, 3)
size = 20
BLACK = (0, 0, 0)

#File paths:
red = './Red.png'
blue = './Blue.png'
green = './Green.png'
yellow = './Yellow.png'
white = './White.png'
clear = './Clear.png'

#Round the game size to be nice:
(width, height) = (widths*size, heights*size)

import pygame
from random import choice
from time import sleep

choices = ['red', 'blue', 'green', 'yellow']

class Chip:
    def __init__(self, surface, posx, posy, color):
        self.x = posx
        self.y = posy
        self.surface = surface
        if color == 'red':
            self.color = red
        elif color == 'blue':
            self.color = blue
        elif color == 'green':
            self.color = green
        elif color == 'yellow':
            self.color = yellow
        elif color == 'white':
            self.color = white
        else:
            print("!INVALID COLOR OPTION!")
        self.image = pygame.image.load(self.color)#.convert_alpha()
        self.surface.blit(self.image, (self.x*size, self.y*size))
    def draw(self, chips):#The chips are needed to know if something is under the chip.
        if self.y == heights -1:
            self.update()
        elif self.y != heights -1:
            for chip in chips:
                if chip.y == self.y+1:
                    self.update()
        else:
            print("Falling")
    def update(self):
        self.surface.blit(self.image, (self.x*size, self.y*size))
    def delete(self):
        self.color = white
        self.image = pygame.image.load(self.color)#.convert_alpha()
        self.surface.blit(self.image, (self.x*size, self.y*size))

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

allChips = []
for posx in range(widths):
    for posy in range(heights):
        allChips.append(Chip(screen, posx, posy, choice(choices)))

pygame.display.flip()    

def reduce(lst):#This will eliminate duplicate entries in lists.
    return list(dict.fromkeys(lst))

def getNextTo(chips, pos):
    x, y = pos
    closeChips = []
    for chip in allChips:
        if (chip.x, chip.y) == (x+1, y):
            closeChips.append(chip)
        elif (chip.x, chip.y) == (x-1, y):
            closeChips.append(chip)
        elif (chip.x, chip.y) == (x, y+1):
            closeChips.append(chip)
        elif (chip.x, chip.y) == (x, y-1):
            closeChips.append(chip)
        else:
            pass
    return closeChips

#Main Loop:
running = True
while running:
    screen.fill(background_color)
    for chip in allChips:
        chip.draw(allChips)
    for event in pygame.event.get():
        if event.type == pygame.MOUSEBUTTONUP:
            #Start the function for when chip is clisked:
            chipsToDissapear = []
            pos = pygame.mouse.get_pos()
            pos = int(pos[0]/size), int(pos[1]/size)
            for chip in allChips:#Figure out which chip clicked and where
                if (chip.x, chip.y) == pos:
                    #print(pos)
                    clickedChip = chip
                    color = chip.color
                    chipsToDissapear.append(clickedChip)
                    break
            #Figure out if the ones next to it are the same color.
            check = True
            checked = []
            closeChips = getNextTo(allChips, pos)
            nextIteration = []
            for chip in closeChips:
                if chip in chipsToDissapear:
                    pass
                elif chip.color == color:
                    chipsToDissapear.append(chip)
                    nextIteration.append(chip)
            tempNextItration = []
            while check == True:
                if nextIteration == []:
                    check = False
                    #print(len(chipsToDissapear))
                    print(len(reduce(chipsToDissapear)))
                for chip in nextIteration:
                    chipsToDissapear.append(chip)
                    closeChips = getNextTo(allChips, (chip.x, chip.y))
                    for cChip in closeChips:
                        if cChip in chipsToDissapear:
                            pass
                        elif cChip.color == color:
                            
                            tempNextItration.append(cChip)
                nextIteration = tempNextItration
                tempNextItration = []
            chipsToDissapear = reduce(chipsToDissapear)
            for chip in chipsToDissapear:
                chip.delete()
                allChips.remove(chip)
        if event.type == pygame.QUIT:
            running = False
            pygame.quit()
    sleep(1)
    pygame.display.flip()
