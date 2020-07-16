##Nathan Hinton
##This is the dollars place

import pygame
from random import randint
from time import time

##Measured in seconds
##minExpire = 1
##maxExpire = 10

pygame.font.init()
font = pygame.font.SysFont('Comic Sans MS', 30)

class Money:
    def __init__(self, screen, value = 100, minExpire = 1, maxExpire = 10):
        self.screen = screen
        self.minExpire = minExpire
        self.maxExpire = maxExpire
        self.timeout = randint(self.minExpire, self.maxExpire)
        self.start = time()
        self.value = value
        self.width = 75
        self.height = 25
        self.x, self.y = randint(0, self.screen.get_width()-self.width), randint(0, self.screen.get_height()-self.height)
        self.rect = pygame.rect.Rect(self.x, self.y, self.width, self.height)
        self.text = font.render(str(self.value), False, (0, 0, 0))
    def run(self, clickPos):
        if self.start+self.timeout <= time():##Check for the timeout:
            print("Money added!")
            return (self.value, False)
        elif self.rect.collidepoint(clickPos):
            print("Money clicked on!")
            return (self.value, True)
        else:
            return False
        
    def update(self):
        pygame.draw.rect(self.screen, (0, 255, 0), self.rect)
        self.screen.blit(self.text, (self.x, self.y))
        
