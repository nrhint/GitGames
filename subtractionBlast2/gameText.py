##Nathan Hinton
##This file will be for the scoring and level of the game

#import pygame

class GameText:
    def __init__(self, screen, position, font, text = 0, textColor = (150, 150, 150)):
        self.screen = screen
        self.x, self.y = position
        self.font = font
        self.text = text
        self.textColor = textColor
    def tick(self, text = None):
        if text != None:
            self.text = text
    def update(self):
        text = self.font.render(str(self.text), True, self.textColor)
        self.screen.blit(text, (self.x, self.y))