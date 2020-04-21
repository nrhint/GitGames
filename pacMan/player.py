##Nathan Hinton
##This is the player file

#Vars:
speed = 5

import pygame

#Setup the class:
class Player:
    def __init__(self, display):
        self.speed = speed
        self.display = display
        self.img = pygame.image.load('player.png')
        self.x = self.display.get_width()/2
        self.y = self.display.get_height()/2
        self.display.blit(self.img, (self.x, self.y))
        self.rect = pygame.Rect((self.x+2, self.y+2), (16, 16))
        self.heading = 0
    def run(self, key):
        if key == 'up':
            self.y -= speed
            self.heading = 90
        elif key == 'left':
            self.x -= speed
            self.heading = 180
        elif key == 'right':
            self.x += speed
            self.heading = 0
        elif key == 'down':
            self.y += speed
            self.heading = 270
        if self.x< 0:
            self.x = 0
        elif self.x > self.display.get_width()-20:
            self.x = self.display.get_width()-20
        if self.y < 0:
            self.y = 0
        elif self.y > self.display.get_height()-20:
            self.y = self.display.get_height()-20
    def update(self):
        self.rect = pygame.Rect((self.x+2, self.y+2), (16, 16))
        self.display.blit(pygame.transform.rotate(self.img, self.heading), (self.x, self.y))
