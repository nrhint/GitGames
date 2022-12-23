##Nathan Hinton

print("Loading...")
import pygame
from time import time, sleep

from loadData import *
from items import Card, Token

##Vars that you can change:
nobleCount = 3
fps = 60 #This will dirrectly change the hardness of the game.
step = 1/fps #Using time to delay if needed
width, height = 800, 800

pygame.init()
screen = pygame.display.set_mode((width, height))
pygame.display.set_caption("Splendor")

background = pygame.Surface(screen.get_size())
background = background.convert()
background.fill((100, 100, 100))

pygame.font.init() # you have to call this at the start,  if you want to use this module.
myfont = pygame.font.SysFont('Comic Sans MS', 30)

gameStart = time()

cards = pygame.sprite.Group()
tokens = pygame.sprite.Group()

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
for i in range(0, 3):
    print("Adding level %s cards to layout" %(i+1))
    ##Add the draw card:
    tmp = Card(screen, i, remainingCards, cards, True)
    tmp.rect = pygame.Rect(40, ((i+1)*150)+15, 80, 120)
    remainingCards = tmp.remainingCards
    for j in range(0, 5):
        tmp = Card(screen, i, remainingCards, cards)
        tmp.rect = pygame.Rect((j*160)+40, ((i+1)*150)+15, 80, 120)
        remainingCards = tmp.remainingCards

##Place the tokens:
tmp = Token(screen, "blueGem.png", tokens)
tmp.rect = pygame.Rect(300, 100, 50, 50)
tmp = Token(screen, "brownGem.png", tokens)
tmp.rect = pygame.Rect(30, 30, 50, 50)
tmp = Token(screen, "goldGem.png", tokens)
tmp.rect = pygame.Rect(30, 30, 50, 50)
tmp = Token(screen, "greenGem.png", tokens)
tmp.rect = pygame.Rect(30, 30, 50, 50)
tmp = Token(screen, "redGem.png", tokens)
tmp.rect = pygame.Rect(30, 30, 50, 50)
tmp = Token(screen, "whiteGem.png", tokens)
tmp.rect = pygame.Rect(30, 30, 50, 50)

##Place the player layout:


while run == True:
    nextStep = time()+step

    ##Apply updates to the screen:
    screen.blit(background, (0, 0))
    cards.update(screen)
    tokens.draw(screen)
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
                for card in cards:
                    if True == card.rect.collidepoint(clickPos):
                        print("Clicked on a card")
                        card.wasClicked()
                

    ##Sleep if needed
    if nextStep - time() > 0:
        try:
            sleep(nextStep - time())
        except ValueError:
            pass
    frameCount += 1

print("Thanks for playing!")