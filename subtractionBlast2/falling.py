##Nathan Hinton
##This file will be for the falling objects

import pygame
from math import pi, cos, sin
from random import randrange

class Falling:
    def __init__(self, screen, level, font, speed = 1):
        self.screen = screen
        self.font = font
        self.x, self.y = (randrange(0, screen.get_width()), -50)
        self.dirrection = 0
        self.value = randrange(4, level+1, step = max(int((level-4)/10), 1))
        self.startValue = self.value
        self.speed = speed
        self.color = (255, 255, 0)
        self.textColor = (0, 255, 255)
        self.tick()
    def tick(self):
        self.x += sin(self.dirrection)*self.speed
        self.y += cos(self.dirrection)*self.speed
        self.pts = []
        for i in range(max(3, self.value//5)):
            x = self.x + self.value*3 * cos(0.1 + (pi * 2 * i / max(self.value/5, 3)))
            y = self.y + self.value*3 * sin(0.1 + (pi * 2 * i / max(self.value/5, 3)))
            self.pts.append([int(x), int(y)])
        #print(self.pts)
    def update(self):
        pygame.draw.polygon(self.screen, self.color, self.pts)
        text = self.font.render(str(self.value), True, self.textColor)
        self.screen.blit(text, (self.x, self.y))