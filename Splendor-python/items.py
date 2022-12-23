##Nathan Hinton
##This will be for the items that can be clicked on

from pygame.image import load
from pygame import sprite
from random import choice
import pygame

class Card(sprite.Sprite): 
    def __init__(self, screen, level, remainingCards, group, facedown = False):
        super().__init__()
        super().add(group)
        self.screen = screen
        self.level = level
        self.remainingCards = remainingCards
        self.facedown = facedown
        ##Generate the card
        if False == self.facedown:
            try:
                self.cardInfo = choice(self.remainingCards[self.level])
                self.remainingCards[self.level].remove(self.cardInfo)
            except IndexError:
                print("Invalid card level %s" %self.level)
                raise ValueError
        else: #The card is the draw pile:
            self.image = load("images/cards/facedownCardLevel%s.png" %(self.level+1))
        self.level = self.level

    def update(self):
        if False == self.facedown:
            pygame.draw.rect(self.screen, (0, 255, 0), self.rect, 2)
        else:
            self.screen.blit(self.image)