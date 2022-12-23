##Nathan Hinton

print("Loading...")
import pygame
from time import time, sleep

from loadData import *
from items import Card, Token
from player import Player
import myEvents

##Vars that you can change:
nobleCount = 3
playerCount = 2
fps = 60 #This will dirrectly change the hardness of the game.
step = 1/fps #Using time to delay if needed
width, height = 700, 700

pygame.init()
screen = pygame.display.set_mode((width, height))
pygame.display.set_caption("Splendor")

background = pygame.Surface(screen.get_size())
background = background.convert()
background.fill((100, 100, 100))

pygame.font.init() # you have to call this at the start,  if you want to use this module.
myFont = pygame.font.SysFont('Comic Sans MS', 30)

gameStart = time()

cards = pygame.sprite.Group()
tokens = pygame.sprite.Group()
playerItemsGroup = pygame.sprite.Group()

##Setup the screen:
"""Game layout:
Nobles, 1x5 grid | Tokens
Cards, 3x5 grid  | Tokens
---------- player area ----------
|nobles|5 slots for cards/tokens| end turn |
---------------------------------
"""
##Place the nobles:

##Place the cards:
for i in range(0, 3):
    print("Adding level %s cards to layout" %(i+1))
    ##Add the draw card:
    tmp = Card(screen, myFont, i, remainingCards, cards, pygame.Rect(10, (i*150)+80, 80, 120), True)
    remainingCards = tmp.remainingCards
    for j in range(0, 4):
        tmp = Card(screen, myFont, i, remainingCards, cards, pygame.Rect((j*100)+110, (i*150)+80, 80, 120))
        remainingCards = tmp.remainingCards

##Place the tokens:
yOffset = 17
tmp = Token(screen, "blueGem.png", tokens)
tmp.rect = pygame.Rect(500, 80+yOffset, 80, 80)
tmp = Token(screen, "brownGem.png", tokens)
tmp.rect = pygame.Rect(500, 230+yOffset, 80, 80)
tmp = Token(screen, "goldGem.png", tokens)
tmp.rect = pygame.Rect(500, 380+yOffset, 80, 80)
tmp = Token(screen, "greenGem.png", tokens)
tmp.rect = pygame.Rect(600, 80+yOffset, 80, 80)
tmp = Token(screen, "redGem.png", tokens)
tmp.rect = pygame.Rect(600, 230+yOffset, 80, 80)
tmp = Token(screen, "whiteGem.png", tokens)
tmp.rect = pygame.Rect(600, 380+yOffset, 80, 80)

##Place the player layout:
tmp = pygame.sprite.Sprite(playerItemsGroup)
tmp.image = pygame.image.load("images/player/endTurn.png")
tmp.rect = pygame.Rect(625, 650, 50, 25)
tmp.action = "End turn"

##Create the players:
players = []
for x in range(0, playerCount):
    players.append(Player())

##Last minuite setup:
print("Starting game!")
run = True
frameCount = 0
turnCounter = 0
currentPlayer = players[0]
while run == True:
    nextStep = time()+step

    ##Apply updates to the screen:
    screen.blit(background, (0, 0))
    cards.update(screen, myFont)
    tokens.draw(screen)
    playerItemsGroup.draw(screen)
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
                for playerObject in playerItemsGroup:
                    if True == playerObject.rect.collidepoint(clickPos):
                        playerObject.update()
                for card in cards:
                    if True == card.rect.collidepoint(clickPos):
                        print("Clicked on a card")
        if event.type == myEvents.END_TURN_EVENT:
            turnCounter += 1
            currentPlayer = players[turnCounter%playerCount]
            print("Changed turns to %s"%currentPlayer)
    ##Sleep if needed
    if nextStep - time() > 0:
        try:
            sleep(nextStep - time())
        except ValueError:
            pass
    frameCount += 1

print("Thanks for playing!")