##Nathan Hinton
##This is for the ghosts

#Vars
ghostSpeed = 2.5

import pygame
from random import randint

#Setup the ghost class
class Ghost:
    def __init__(self, display, color = 'red'):
        self.speed = ghostSpeed
        self.display = display
        self.img = pygame.image.load('%sGhost.png'%color)
        self.x = randint(0, self.display.get_width()/20)*20
        self.y = randint(0, self.display.get_height()/20)*20
        self.display.blit(self.img, (self.x, self.y))
        self.rect = pygame.Rect((self.x+2, self.y+2), (16, 16))
        self.heading = 90
    def run(self, playerx, playery):
        lastx = self.x
        lasty = self.y
        #AI portion:
        if self.x - playerx > 0:
            self.heading = 180
        elif self.x - playerx < 0:
            self.heading = 0
        else:
            if self.y - playery < 0:
                self.heading = 90
            else:
                self.heading = 270
        if self.heading == 0:
            self.x += ghostSpeed
        elif self.heading == 90:
            self.y += ghostSpeed
        elif self.heading == 180:
            self.x -= ghostSpeed
        elif self.heading == 270:
            self.y -= ghostSpeed
        else:
            print("Ghost heading problem")
    def update(self):
        self.rect = pygame.Rect((self.x+2, self.y+2), (16, 16))
        self.display.blit(self.img, (self.x, self.y))
