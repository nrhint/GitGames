##Nathan Hinton
##This is the file for the bricks

import pygame

class Brick:
    def __init__(self, screen, pos, color):
        self.screen = screen
        self.screenWidth, self.screenHeight = self.screen.get_size()
        self.x, self.y = pos
        self.color = color
        self.delete = False
        self.rect = pygame.Rect(self.x, self.y, self.screenWidth/20, self.screenHeight/30)
    def run(self):
        if self.delete != False:
#            print("Brick deleted")
            self.color = self.delete
            self.update()
    def update(self):
        pygame.draw.rect(self.screen, self.color, self.rect)
        pygame.draw.rect(self.screen, (0, 0, 0), self.rect, 1)
