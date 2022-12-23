##Nathan Hinton
##This will be for the items that can be clicked on

from pygame.image import load
from pygame import sprite
from random import choice
import pygame

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

colorToIndex = {
    'white':0, 
    'green':1, 
    'blue':2, 
    'red':3, 
    'brown':4,
    'gold':5
}

##This clas will controll the cards
class Card(sprite.Sprite): 
    def __init__(self, screen, font, level, remainingCards, group, rect, facedown = False):
        super().__init__()
        super().add(group)
        self.screen = screen
        self.level = level
        self.remainingCards = remainingCards
        self.facedown = facedown
        self.level = self.level
        self.rect = rect
        self.font = font
        self.surfaceRect = pygame.Rect((0, 0), self.rect.size)
        self.colors = [(0, 255, 0), (255, 0, 0), (0, 0, 255)]
        self.cardColors = [(255, 255, 255), (22, 181, 0), (30, 34, 207), (208, 10, 0), (94, 50, 17), (255, 215, 0)]

        ##Pick card:
        try:
            self.cardInfo = choice(self.remainingCards[self.level])
            self.remainingCards[self.level].remove(self.cardInfo)
        except IndexError:
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


    def update(self, screen, font):
        screen.blit(self.backgroundSurface, self.rect.topleft)


class Token(sprite.Sprite):
    def __init__(self, screen, imageName, group):
        super().__init__()
        super().add(group)
        self.imageName = imageName
        self.image = load("images/gems/%s" %imageName)
        self.screen = screen
        self.image = pygame.transform.scale(self.image, (80, 80))
    def draw(self, screen):
        screen.blit(self.image, self.rect.topleft)

