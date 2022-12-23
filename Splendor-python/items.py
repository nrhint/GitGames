##Nathan Hinton
##This will be for the items that can be clicked on

from pygame.image import load
from pygame import sprite
from random import choice
import pygame

class Card(sprite.Sprite): 
    def __init__(self, level, remainingCards, group, facedown = False):
        super().__init__()
        super().add(group)
        self.level = level
        self.remainingCards = remainingCards
        self.facedown = facedown
        self.load()

    def load(self):
        ##Generate the card
        if False == self.facedown:
            try:
                self.cardInfo = choice(self.remainingCards[0])
                self.remainingCards[self.level].remove(self.cardInfo)
            except IndexError:
                print("Invalid card level %s" %self.level)
                raise ValueError
        else: #The card is the draw pile:
            self.image = load("images/cards/facedownCardLevel%s.png" %self.level)
        self.level = self.level
        return self.remainingCards

class Token(sprite.Sprite):
    def __init__(self, screen, imageName, group):
        super().__init__()
        super().add(group)
        self.imageName = imageName
        self.load()
