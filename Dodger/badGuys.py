##Nathan Hinton
##This is the bad guy file:

import pygame
from random import randint

class BadGuy:
    def __init__(self, display, imagePath, speed = 1):
        self.display = display
        self.widthMax, self.heightMax = self.display.get_size()
        self.speed = speed
        self.x = randint(0, self.widthMax)
        self.y = randint(-50, 0)
        self.img = pygame.image.load(str(imagePath)) #Load the image
        self.scale = randint(50, 200) #This will be used to scale the image to a percent to make them more interisting
        self.img = pygame.transform.scale(self.img, (round(20*(self.scale/100)), round(20*(self.scale/100))))
        self.width = round(20*(self.scale/100))
        self.height = round(20*(self.scale/100))
        self.rect = pygame.Rect(self.x+(self.width/2), self.y+(self.width/2), self.width, self.height)
    def run(self):
        self.y += self.speed
        #print(self.x, self.y)
    def update(self):
        self.rect = pygame.Rect(self.x, self.y, self.width, self.height)
        self.display.blit(self.img, (self.x, self.y))
        #pygame.draw.rect(self.display, (100, 0, 200), self.rect)
