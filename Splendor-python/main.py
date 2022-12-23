##Nathan Hinton

print("Loading...")
import pygame
from time import time, sleep

from loadData import *
from items import Card

##Vars that you can change:
fps = 60 #This will dirrectly change the hardness of the game.
step = 1/fps #Using time to delay if needed
width, height = 800, 600

pygame.init()
screen = pygame.display.set_mode((width, height))
pygame.display.set_caption("Splendor")

background = pygame.Surface(screen.get_size())
background = background.convert()
background.fill((100, 100, 100))

pygame.font.init() # you have to call this at the start,  if you want to use this module.
myfont = pygame.font.SysFont('Comic Sans MS', 30)

gameStart = time()

sprites = []
cards = pygame.sprite.Group()

print("Starting game!")
run = True
frameCount = 0
##Setup the screen:
"""Game layout:
Nobles, 1x5 grid
Cards, 3x5 grid with one col being face down cards
Tokens, 1x5
---------- player area ----------
|nobles|5 slots for cards/tokens|
---------------------------------
"""
##Place the nobles:

##Place the cards:
for i in range(1, 4):
    print("Adding level %s cards to layout" % i)
    ##Add the draw card:
    remainingCards = Card(i, remainingCards, cards, True)
    for j in range(0, 5):
        remainingCards = Card(i, remainingCards, cards)

##Place the tokend:

##Place the player layout:


while run == True:
    nextStep = time()+step
    ##Update the screen:
    screen.blit(background, (0, 0))
    pygame.display.flip()

    ##Prepare for next frame
    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            print("Exiting...")
            pygame.quit()
            run = False #End the main loop
        if event.type == pygame.MOUSEBUTTONDOWN:
            if pygame.mouse.get_pressed() == (True, False, False):
                clickPos = pygame.mouse.get_pos()
                print(clickPos)
                for sprite in sprites:
                    if sprite.rect.collidepoint(clickPos):
                        pass

    ##Sleep if needed
    if nextStep - time() > 0:
        sleep(nextStep - time())
    frameCount += 1

print("Thanks for playing!")