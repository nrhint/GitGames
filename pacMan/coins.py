##Nathan Hinton
##This is where the coins are done

#vars

import pygame

#Setup the class
class Coin:
    def __init__(self, display, x, y):
        self.rect = pygame.Rect((x+5, y+5), (10, 10))
        self.display = display
        self.x = x
        self.y = y
        self.img = pygame.image.load('coin.png')
    def update(self):
        self.display.blit(self.img, (self.x, self.y))
        #pygame.draw.rect(self.display, (0, 255, 255), self.rect)
