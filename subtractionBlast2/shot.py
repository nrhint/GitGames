##Nathan Hinton
##This will be the file for the numbers that will be shot

from math import pi, cos, sin
import pygame

class Shooter:
    def __init__(self, screen, startPosition, dirrection, font, value = 0, speed = 5):
        self.screen = screen
        self.font = font
        self.x, self.y = startPosition
        self.dirrection = -dirrection - 3.14
        self.value = value
        self.speed = speed
        self.color = (255, 0, 255)
        self.textColor = (0, 255, 255)
        self.tick()
    def tick(self):
        self.x += sin(self.dirrection)*self.speed
        self.y += cos(self.dirrection)*self.speed
        self.pts = []
        for i in range(max(3, self.value)):
            x = self.x + self.value*3 * cos(pi * 2 * i / self.value)
            y = self.y + self.value*3 * sin(pi * 2 * i / self.value)
            self.pts.append([int(x), int(y)])
        #print(self.pts)
    def update(self):
        pygame.draw.polygon(self.screen, self.color, self.pts)        
        text = self.font.render(str(self.value), True, self.textColor)
        self.screen.blit(text, (self.x, self.y))
