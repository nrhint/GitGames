##Nathan Hinton
##This is the player file

##Vars you can change
acceleration = 1
maxSpeed = 10
turnRate = 5

#Setup:
import pygame
from math import cos, sin

def radians(x):
    return x*3.14/180

#Player class
class Player:
    def __init__(self, display):
        self.display = display
        self.x, self.y = self.display.get_size()
        self.x = self.x/2#Center the player
        self.y = self.y/2#Center the player
        self.img = pygame.image.load('player.png')#load player image
        self.speed = 0
        self.heading = 0
        self.bullets = []
        self.rect = pygame.Rect((self.x+10, self.y+10), (self.x-10, self.y-10))
    def run(self, keys):
        if 'up' in keys:
            self.speed += acceleration
        else:#Decel
            self.speed -= acceleration
            if self.speed < 0:
                self.speed = 0
        if self.speed > maxSpeed:
            self.speed = maxSpeed
        if 'right' in keys:
            self.heading -= turnRate
        if 'left' in keys:
            self.heading += turnRate
        #print(radians(cos(self.heading)), radians(sin(self.heading)))
        self.y+= -(cos(radians(self.heading)))* self.speed
        self.x+= -(sin(radians(self.heading)))* self.speed
    def update(self):
        self.oldRect = self.rect
        self.rect = pygame.Rect((self.x%self.display.get_width()-2, self.y%self.display.get_height()-2), (18, 18))
        return self.rect
        #self.display.blit(pygame.transform.rotate(self.img, self.heading), (self.x%self.display.get_width(), self.y%self.display.get_height()))
#        pygame.draw.rect(self.display, (0, 255, 0), self.rect)
