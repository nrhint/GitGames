##Nathan hinton
##This is the file for the paddle

import pygame

class Paddle:
    def __init__(self, screen, speed = 5):
        self.screen = screen
        self.speed = speed
        self.img = 'paddle.png'
        self.img = pygame.image.load(self.img)
        self.width, self.height = self.img.get_size()
        self.gameWidth, self.gameHeight = self.screen.get_size()
        self.x, self.y = ((self.gameWidth/2), self.gameHeight - 20)
    def run(self, keysPressed):
        if 'right' in keysPressed:
            self.x += self.speed
        elif 'left' in keysPressed:
            self.x -= self.speed
    def update(self):
        self.rect = pygame.Rect(self.x, self.y, self.width, self.height)
        self.screen.blit(self.img, (self.x, self.y))
