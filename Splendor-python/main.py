##Nathan Hinton

print("Loading...")
import pygame
from time import time, sleep

from loadData import *
from items import Card, Token
from player import Player, playerItemsGroup
import myEvents

##Vars that you can change:
nobleCount = 3
playerCount = 3
fps = 60 #This will dirrectly change the hardness of the game.
step = 1/fps #Using time to delay if needed
width, height = 700, 700

pygame.init()
screen = pygame.display.set_mode((width, height))
pygame.display.set_caption("Splendor")

background = pygame.Surface(screen.get_size())
background = background.convert()
background.fill((128, 100, 100))

pygame.font.init() # you have to call this at the start,  if you want to use this module.
smallFont = pygame.font.SysFont('Comic Sans MS', 30)
mediumFont = pygame.font.SysFont('Comic Sans MS', 60)
largeFont = pygame.font.SysFont('Comic Sans MS', 90)

gameStart = time()

cards = pygame.sprite.Group()
tokens = pygame.sprite.Group()

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
    tmp = Card(smallFont, i, remainingCards, cards, pygame.Rect(10, (i*150)+80, 80, 120), True)
    remainingCards = tmp.remainingCards
    for j in range(0, 4):
        tmp = Card(smallFont, i, remainingCards, cards, pygame.Rect((j*100)+110, (i*150)+80, 80, 120))
        remainingCards = tmp.remainingCards

##Place the tokens:
yOffset = 17
tmp = Token("blue", tokens)
tmp.rect = pygame.Rect(500, 80+yOffset, 80, 80)
tmp = Token("brown", tokens)
tmp.rect = pygame.Rect(500, 230+yOffset, 80, 80)
tmp = Token("gold", tokens)
tmp.rect = pygame.Rect(500, 380+yOffset, 80, 80)
tmp = Token("green", tokens)
tmp.rect = pygame.Rect(600, 80+yOffset, 80, 80)
tmp = Token("red", tokens)
tmp.rect = pygame.Rect(600, 230+yOffset, 80, 80)
tmp = Token("white", tokens)
tmp.rect = pygame.Rect(600, 380+yOffset, 80, 80)

##Create the players:
players = []
for x in range(0, playerCount):
    pName = input("Enter player %s's name: "%x)
    players.append(Player("%s"%pName))
    players[-1].currentTokens = [900, 900, 900, 900, 900, 5]

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
    screen.blit(smallFont.render("%s's turn"%currentPlayer.name, False, (255, 255, 255)), (500, 525))
    screen.blit(smallFont.render("%s points"%currentPlayer.points, False, (255, 255, 255)), (450, 650))
    cards.update(screen, smallFont)
    tokens.update(screen, largeFont)
    playerItemsGroup.draw(screen)
    for playerObject in playerItemsGroup:
        if type(playerObject) == Token:
            playerObject.update(screen, mediumFont, currentPlayer)
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
                    if True == playerObject.rect.collidepoint(clickPos) and type(playerObject) != Token:
                        print("Clicked player object")
                        currentPlayer.update(playerObject.action)
                for card in cards:
                    if True == card.rect.collidepoint(clickPos) and currentPlayer.status == "Buying":
                        print("Clicked on a card to try to buy")
                        result = currentPlayer.buyCard(card, tokens)
                        cards.remove(card)
                        if True == result:
                            remainingCards = card.update(screen, smallFont, True, remainingCards, cards)
                for token in tokens:
                    if True == token.rect.collidepoint(clickPos) and currentPlayer.status == "Drawing":
                        print("Trying to draw a token...")
                        result = currentPlayer.drawNewToken(token)
                        if True == result:
                            print("Success on buying token!")
                        
        if event.type == myEvents.END_TURN_EVENT:
            turnCounter += 1
            currentPlayer = players[turnCounter%playerCount]
            currentPlayer.action = None
            currentPlayer.totalTokensDrawn = 0
            print("Changed turns to %s"%currentPlayer)
    ##Sleep if needed
    if nextStep - time() > 0:
        try:
            sleep(nextStep - time())
        except ValueError:
            print("Missed frame timing")
    frameCount += 1
gameEnd = time()
print("FPS: %s"%(frameCount/(gameEnd-gameStart)))
print("Thanks for playing!")