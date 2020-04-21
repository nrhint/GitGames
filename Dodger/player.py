##Nathan Hinton
##This is for the player

import pygame

class Player:
    def __init__(self, display, imagePath):
        self.display = display
        self.imagePath = imagePath
        self.windowWidth, self.windowHeight = self.display.get_size()
        self.x = self.windowWidth/2
        self.y = self.windowHeight/2
        self.img = pygame.image.load(str(imagePath)) #load the players image
        self.rect = pygame.Rect((self.x+10, self.y+10), (self.x-10, self.y-10))
    def run(self):
        self.x, self.y = pygame.mouse.get_pos()
    def update(self):
        self.rect = pygame.Rect((self.x-2, self.y-2), (18, 18))
        self.display.blit(self.img, (self.x, self.y))
