##Nathan Hinton
##This is the bullet file

import pygame

class Bullet:
    def __init__(self, display, imgPath, position, speed):
        self.display = display
        self.imagePath = imgPath
        self.x, self.y = position
        self.speed = speed
        self.img = pygame.image.load(str(self.imagePath)) #load the players image
        self.rect = pygame.Rect((self.x+10, self.y+10), (self.x-10, self.y-10))
    def run(self):
            self.y -= self.speed
    def update(self):
        self.rect = pygame.Rect((self.x, self.y), (12, 3))
        pygame.draw.rect(self.display, (255, 0, 0), self.rect)
        self.display.blit(self.img, (self.x, self.y))
