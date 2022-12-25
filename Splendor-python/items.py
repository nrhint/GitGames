##Nathan Hinton
##This will be for the items that can be clicked on

from pygame.image import load
from pygame import sprite
from random import choice
import pygame

from dataManipulation import colorToIndex, invertedColorToRGB

##Load the images to be used
## [white, green, blue, red, brown]
brownTokenImage = load("images/gems/brownGem.png")
redTokenImage = load("images/gems/redGem.png")
whiteTokenImage = load("images/gems/whiteGem.png")
greenTokenImage = load("images/gems/greenGem.png")
blueTokenImage = load("images/gems/blueGem.png")
goldTokenImage = load("images/gems/goldGem.png")
tokenImages = [
    whiteTokenImage, 
    greenTokenImage, 
    blueTokenImage, 
    redTokenImage, 
    brownTokenImage
]
smallTokenImages = []
for tokenImage in tokenImages:
    smallTokenImages.append(pygame.transform.scale(tokenImage, (17, 17)))

##This clas will controll the cards
class Card(sprite.Sprite): 
    def __init__(self, font, level, remainingCards, group, rect, facedown = False):
        super().__init__()
        super().add(group)
        self.level = level
        self.remainingCards = remainingCards
        self.facedown = facedown
        self.level = self.level
        self.rect = rect
        self.font = font
        self.owner = None
        self.surfaceRect = pygame.Rect((0, 0), self.rect.size)
        self.colors = [(0, 255, 0), (255, 0, 0), (0, 0, 255)]
        self.cardColors = [(255, 255, 255), (22, 181, 0), (30, 34, 207), (208, 10, 0), (94, 50, 17), (255, 215, 0)]

        ##Pick card:
        try:
            self.cardInfo = choice(self.remainingCards[self.level])
            self.remainingCards[self.level].remove(self.cardInfo)
        except IndexError:
            if [] == self.remainingCards[self.level]:
                print("Out of cards of that color :(")
            else:
                print("Invalid card level %s" %self.level)
                raise ValueError

        ##Generate the card
        self.backgroundSurface = pygame.Surface(self.rect.size)
        self.alphaSurface = pygame.Surface(self.rect.size)
        if False == self.facedown:
            self.alphaSurface.set_alpha(50)
            self.alphaSurface.fill(self.cardColors[colorToIndex[self.cardInfo.tokenColor]])
            # pygame.draw.rect(self.backgroundSurface, self.colors[self.level], pygame.Rect((0, 0), self.rect.size), 2, -5)
            self.backgroundSurface.fill((100, 100, 100))
            self.backgroundSurface.blit(self.alphaSurface, (0, 0))
            pygame.draw.rect(self.backgroundSurface, self.colors[self.level], pygame.Rect((0, 0), (self.backgroundSurface.get_size())), 2)
            imageWidthPadded = (smallTokenImages[0].get_width()/4)+smallTokenImages[0].get_width()
            ##Blit card token image to surface
            pos = (self.backgroundSurface.get_width()-(tokenImages[0].get_width())-5, +5)
            self.backgroundSurface.blit(tokenImages[colorToIndex[self.cardInfo.tokenColor]], pos)
            ##Blit the card point value to surface
            if 0 != self.cardInfo.pointValue:
                self.textSurface = self.font.render('%s'%self.cardInfo.pointValue, False, (255, 255, 255))
                self.backgroundSurface.blit(self.textSurface, (5, 5))

            #blit cost information to surface
            counter = 0
            for x in range(0, 5):
                if self.cardInfo.costs[x] != 0:
                    pos = (5, self.backgroundSurface.get_height()-((imageWidthPadded)*counter)-imageWidthPadded)
                    print(pos)
                    self.backgroundSurface.blit(smallTokenImages[x], pos)
                    self.textSurface = self.font.render('%s'%self.cardInfo.costs[x], False, (255, 255, 255))
                    self.backgroundSurface.blit(self.textSurface, (imageWidthPadded, pos[1]))
                    counter += 1
        else: #The card is the draw pile:
            self.image = load("images/cards/facedownCardLevel%s.png" %(self.level+1))
            self.backgroundSurface.blit(self.image, (0, 0))

    def update(self, screen, font, replace = False, remainingCards = None, cardsGroup = None):
        if True == replace:
            tmp = Card(self.font, self.level, remainingCards, cardsGroup, self.rect)
            return tmp.remainingCards
        else:
            screen.blit(self.backgroundSurface, self.rect.topleft)



class Token(sprite.Sprite):
    def __init__(self, name, group):
        super().__init__()
        super().add(group)
        self.name = name
        self.image = load("images/gems/%sGem.png" %name)
        self.image = pygame.transform.scale(self.image, (80, 80))
        self.count = 5
    
    def update(self, screen, font, player = False):
        screen.blit(self.image, self.rect.topleft)
        if 0 != self.count and False == player:
            pos = (self.rect.topleft[0]+20, self.rect.topleft[1]+10)
            screen.blit(font.render("%s"%self.count, False, invertedColorToRGB[self.name]), pos)
        elif 0 != self.count:
            pos = (self.rect.topleft[0]+10, self.rect.topleft[1]+20)
            cardTokens = [0, 0, 0, 0, 0, 0]
            for card in player.cards:
                cardTokens[colorToIndex[card.cardInfo.tokenColor]] += 1
            screen.blit(font.render("%s(%s)"%(player.currentTokens[colorToIndex[self.name]], cardTokens[colorToIndex[self.name]]), False, invertedColorToRGB[self.name]), pos)