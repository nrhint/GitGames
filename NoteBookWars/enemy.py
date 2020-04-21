##Nathan Hinton
##This is the file for the enemy

import pygame
from random import randint

from bullet import *

data = open('enemy', 'r').read()
d = data.split('\n')
planes = []
for line in d:
    if line[0] == '#':
        pass
    else:
        planes.append(line.split(', '))

class Enemy:
    def __init__(self, screen, x, y, ship = 0, bullet = 0, patern = 'h100'):
        self.wait = randint(5, 10)
        self.maxWait = self.wait*2
        self.waitCounter = 0
        print(self.wait, self.maxWait)
        self.screen = screen
        self.x = x
        self.y = y
        self.ship = ship
        self.bullet = bullet
        self.planeData = planes[ship]
        self.img = pygame.image.load(self.planeData[0])
        self.health = int(self.planeData[1])
        self.slots = int(self.planeData[2])
        self.width = 40
        self.height = 40
        self.rect = pygame.Rect(self.x, self.y, self.width, self.height)
        self.bullet = bullet
        self.bullets = []
    def run(self, frameCount):
        if self.waitCounter == self.maxWait:
            self.waitCounter = 0
        self.waitCounter += 1
        if self.waitCounter < self.wait:
        #if frameCount//self.wait%2 == 1:
            for x in range(1, self.slots+1):
                if frameCount%int(bullets[self.bullet][2]) == 0:
        ##                print(frameCount)
        ##                print("Shoot")
                    self.bullets.append(Bullet(self.screen, (self.x)+(self.width/(self.slots+1)*x), self.y, -1, bullet = self.bullet))
    def update(self):
        self.rect = pygame.Rect(self.x, self.y, self.width, self.height)
        for bullet in self.bullets:
            if bullet.delete == True:
                i = self.bullets.index(bullet)
                self.bullets.pop(i)
            bullet.run()
            bullet.update()
        self.screen.blit(pygame.transform.rotate(self.img, 180), (self.x, self.y))
