##Nathan Hinton
##This is the player file

import pygame

from bullet import *

#parse the ships file:
data = open('ships', 'r').read()
d = data.split('\n')
planes = []
for line in d:
    if line[0] == '#':
        pass
    else:
        planes.append(line.split(', '))



class Player:
    def __init__(self, screen, ship = 0, bullet = 0):
        self.screen = screen
        self.x = screen.get_width()/2
        self.y = screen.get_height()/2
        #Load the ship data:
        self.planeData = planes[ship]
        print(self.planeData)
        self.img = pygame.image.load(self.planeData[0])
        self.health = int(self.planeData[1])
        self.slots = int(self.planeData[2])
        self.offset = 20
        self.width = 40
        self.height = 40
        self.rect = pygame.Rect(self.x, self.y, self.width, self.height)
        self.bullet = bullet
        self.bullets = []
    def run(self, mousePos, frameCount):
        self.x, self.y = mousePos
        self.x -= self.offset
        self.y -= self.offset
        for x in range(1, self.slots+1):
            if frameCount%int(bullets[self.bullet][2]) == 0:
                #print(frameCount)
                #print("Shoot")
                self.bullets.append(Bullet(self.screen, (self.x)+(self.width/(self.slots+1)*x), self.y, 1, bullet = self.bullet))
    def update(self):
        oldRect = self.rect
        self.rect = pygame.Rect(self.x, self.y, 40, 40)
        for bullet in self.bullets:
            if bullet.delete == True:
                i = self.bullets.index(bullet)
                self.bullets.pop(i)
            bullet.run()
            bullet.update()
        self.screen.blit(self.img, (self.x, self.y))
        #return [oldRect, self.rect]
        
