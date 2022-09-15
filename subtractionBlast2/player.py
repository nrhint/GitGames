##Nathan Hinton
##This file will have the data for the player object

import pygame
from math import sin, cos, atan2

class Player:
    def __init__(self, screen, position, width, height):
        self.x, self.y = position
        self.width = width
        self.height = height
        self.rect = pygame.rect.Rect(self.x, self.y, self.width, self.height)
        self.screen = screen
        self.screenWidth, self.screenHeight = self.screen.get_size()
        self.color = (255, 255, 255)
        self.pointPos = (self.x+(self.width/2), self.y-100)
        self.turretLength = 75
        self.angle = 0
    def tick(self, mousePosition):
        self.angle = atan2(mousePosition[0]-(self.screenWidth/2), self.screenHeight-mousePosition[1])
        #print(self.angle)
        self.pointPos = (
            self.x+(self.width/2) + self.turretLength*sin(self.angle),
            self.y+ (self.height/2) - self.turretLength*cos(self.angle)
            #self.x+(self.width/2) + ((mousePosition[0]/(self.screenWidth/2))-1) * 400,
            #self.y-100 +((self.height/2) + ((mousePosition[1]/(self.screenHeight))-1) * 400)
        )
        #pass
    def update(self): ##This will update the sprite every frame
        pygame.draw.line(self.screen, self.color, (self.x+(self.width/2), self.y+(self.height/2)), self.pointPos, width = 5)
        #pygame.transform.rotate(line, self.angle)
        pygame.draw.arc(self.screen, (255, 255, 255), self.rect, 0.0, 3.14, width = 50)