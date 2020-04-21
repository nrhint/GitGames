##Nathan Hinton
##This is the file for the platforms

import pygame

class Platform:
    def __init__(self, display, pos, width, height, color):
        self.display = display
        self.x, self.y = pos
        self.width = width
        self.height = height
        self.color = color
        self.rect = pygame.Rect(self.x, self.y, self.width, self.height)
    def run(self):
        pass
    def update(self):
        pygame.draw.rect(self.display, self.color, self.rect)
