##Nathan Hinton
##This is the file for the ball

import pygame
from random import randint

class Ball:
    def __init__(self, screen, speed = 10):
        self.screen = screen
        self.speed = speed
        self.gameWidth, self.gameHeight = self.screen.get_size()
        self.x, self.y = (self.gameWidth/2, (self.gameHeight/5)*3)#Put the ball in the middle and 3/5 of the way down
        self.img = pygame.image.load('ball.png')
        self.width, self.height = self.img.get_size()
        self.dirrection = randint(-45, 45)
    def run(self):
        
    def update(self):
        self.rect = pygame.Rect(self.x, self.y, self.width, self.height)
        self.screen.blit(self.img, (self.x, self.y))
