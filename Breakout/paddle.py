##Nathan Hinton
##This is the player file for the paddle

import pygame

class Paddle:
    def __init__(self, screen, speed = 5):
        self.screen = screen
        self.speed = speed
        self.img = pygame.image.load('paddle-wider.png')
        self.width, self.height = self.img.get_size()
        self.gameWidth, self.gameHeight = self.screen.get_size()
        self.x, self.y = ((self.gameWidth/2)-(self.width/2), self.gameHeight - 20)#This will start the paddle in the middle and at the bottom
        self.rect = pygame.Rect(self.x, self.y, self.width, self.height)
    def run(self, keysPressed):
        if 'right' in keysPressed:
            self.x += self.speed
            if self.x > (self.gameWidth-self.width):
                self.x = self.gameWidth-self.width
        if 'left' in keysPressed:
            self.x -= self.speed
            if self.x < 0:
                self.x = 0
    def update(self):
        self.rect = pygame.Rect(self.x, self.y, self.width, self.height)
        self.screen.blit(self.img, (self.x, self.y))
