##Nathan Hinton
##This is the bullet file

#Vars you can change
speed = 15
distance = 30

#Setup
import pygame
from math import cos, sin

def radians(x):
    return x*3.14/180

class Bullet:
    def __init__(self, display, x, y, heading):
        self.display = display
        self.x = x
        self.y = y
        self.heading = heading
        self.img = pygame.image.load('bullet.png')
        self.speed = speed
        self.dist = 0
        self.delete = False
        self.size = 2
        self.rect = pygame.Rect((self.x+9, self.y+9), (2, 2))
    def run(self):
        if self.dist > distance:
            self.delete = True
        else:
            self.dist += 1
            self.y+= -(cos(radians(self.heading)))* self.speed
            self.x+= -(sin(radians(self.heading)))* self.speed
    def update(self):
        self.rect = pygame.Rect((self.x%self.display.get_width()+9, self.y%self.display.get_height()+9), (self.size, self.size))
        self.display.blit(self.img, (self.x%self.display.get_width(), self.y%self.display.get_height()))
#        pygame.draw.rect(self.display, (0, 0, 255), self.rect)
