##Nathan Hinton
##This is the bullet file

import pygame

#Parse the bullet file:
data = open('bullet', 'r').read()
d = data.split('\n')
bullets = []
for line in d:
    if line[0] == '#':
        pass
    else:
        bullets.append(line.split(', '))

##Coppied from the astroid bullet
class Bullet:
    def __init__(self, screen, x, y, direction, bullet = 0):
        self.screen = screen
        self.x = x-5
        self.y = y
        self.bulletData = bullets[bullet]
        self.img = pygame.image.load(self.bulletData[0])
        self.damage = int(self.bulletData[1])
        self.rate = int(self.bulletData[2])
        self.speed = int(self.bulletData[3])
        self.delete = False
        self.direction = direction
        self.size = 6
        self.rect = pygame.Rect((self.x+2, self.y+2), (self.size, self.size))
    def run(self):
        if self.y < 0:
            self.delete = True
        self.y -= self.speed*self.direction
    def update(self):
        self.rect = pygame.Rect((self.x+2, self.y+2), (self.size, self.size))
        self.screen.blit(self.img, (self.x, self.y))
#        pygame.draw.rect(self.screen, (0, 0, 255), self.rect)
