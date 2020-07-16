##Nathan Hinton
##This is the chips file tha will controll all of them

import pygame

class Chip:
    def __init__(self, display, startX, startY, color):
        self.x = startX
        self.y = startY
        self.display = display
        self.color = color
        self.image = pygame.image.load(self.color)
        self.size = self.image.get_width()
        self.display.blit(self.image, (self.x*self.size, self.y*self.size))
    def click(self, eventPosition):
        if eventPosition[0]//self.size and eventPosition[1]//self.size:
            print('I was clicked on!')
    def run(self):
        pass
    def update(self):
        self.display.blit(self.image, (self.x*self.size, self.y*self.size))
