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
    tmp = Card(myFont, i, remainingCards, cards, pygame.Rect(10, (i*150)+80, 80, 120), True)
    remainingCards = tmp.remainingCards
    for j in range(0, 4):
        tmp = Card(myFont, i, remainingCards, cards, pygame.Rect((j*100)+110, (i*150)+80, 80, 120))
        remainingCards = tmp.remainingCards

##Place the tokens:
yOffset = 17
tmp = Token(screen, "blue", tokens)
tmp.rect = pygame.Rect(500, 80+yOffset, 80, 80)
tmp = Token(screen, "brown", tokens)
tmp.rect = pygame.Rect(500, 230+yOffset, 80, 80)
tmp = Token(screen, "gold", tokens)
tmp.rect = pygame.Rect(500, 380+yOffset, 80, 80)
tmp = Token(screen, "green", tokens)
tmp.rect = pygame.Rect(600, 80+yOffset, 80, 80)
tmp = Token(screen, "red", tokens)
tmp.rect = pygame.Rect(600, 230+yOffset, 80, 80)
tmp = Token(screen, "white", tokens)
tmp.rect = pygame.Rect(600, 380+yOffset, 80, 80)

##Place the player layout:
tmp = pygame.sprite.Sprite(playerItemsGroup)
tmp.image = pygame.image.load("images/player/drawTokens.png")
tmp.rect = pygame.Rect(600, 550, 80, 40)
tmp.action = "Draw"
tmp = pygame.sprite.Sprite(playerItemsGroup)
tmp.image = pygame.image.load("images/player/buyCard.png")
tmp.rect = pygame.Rect(600, 600, 80, 40)
tmp.action = "Buy"
tmp = pygame.sprite.Sprite(playerItemsGroup)
tmp.image = pygame.image.load("images/player/endTurn.png")
tmp.rect = pygame.Rect(600, 650, 80, 40)
tmp.action = "End turn"

##Create the players:
players = []
for x in range(0, playerCount):
    players.append(Player("Player %s"%x))
    # players[-1].whiteTokens = 9
    # players[-1].redTokens = 9
    # players[-1].blueTokens = 9
    # players[-1].greenTokens = 9

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
    screen.blit(myFont.render("%s's turn"%currentPlayer.name, False, (255, 255, 255)), (500, 520))
    cards.update(screen, myFont)
    tokens.update(screen, myFont)
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
                        print("Clicked player object")
                        currentPlayer.update(playerObject.action)
                for card in cards:
                    if True == card.rect.collidepoint(clickPos) and currentPlayer.status == "Buying":
                        print("Clicked on a card to try to buy")
                        result = currentPlayer.buyCard(card)
                        if True == result:
                            remainingCards = card.update(screen, myFont, True, remainingCards, cards)
                for token in tokens:
                    if True == token.rect.collidepoint(clickPos) and currentPlayer.status == "Drawing":
                        print("Trying to draw a token...")
                        result = currentPlayer.drawToken(token)
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