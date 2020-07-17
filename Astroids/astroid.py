##Nathan Hinton
##This is the astroid file

#Vars you can change:
speed = 5

#setup
import pygame
from math import cos, sin
from random import randint

def radians(x):
    return x*3.14/180

class Astroid:
    def __init__(self, display, level = 1, x = False, y = False):
        self.display = display
        self.level = level
        if x == False or y == False:
            self.x = randint(self.display.get_width()-20, self.display.get_width()+20)
            self.y = randint(self.display.get_height()-20, self.display.get_height()+20)
        else:
            self.x = x
            self.y = y
        self.heading = randint(0, 359)
        self.img = pygame.image.load('astroid%s.png'%self.level)
        self.speed = speed+ (self.level)
        self.size = 80/(2**(self.level-1))
        #print(self.size)
        self.rect = pygame.Rect((self.x, self.y), (self.size, self.size))
        self.delete = False
    def run(self):
        self.y+= -(cos(radians(self.heading)))* self.speed
        self.x+= -(sin(radians(self.heading)))* self.speed
    def update(self):
        self.rect = pygame.Rect((self.x%self.display.get_width()-3, self.y%self.display.get_height()-3), (self.size-3, self.size-3))
        self.display.blit(self.img, (self.x%self.display.get_width(), self.y%self.display.get_height()))
#        pygame.draw.rect(self.display, (255, 0, 0), self.rect)
    def hit(self):
        self.delete = True
