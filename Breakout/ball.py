##Nathan Hinton
##This is the file for the ball

import pygame
from random import randint#For the start
from math import sin, cos
#from time import sleep#This is for slowing down the program

def radians(x):
    return x*3.14/180

class Ball:
    def __init__(self, screen, speed = 3):
        self.screen = screen
        self.speed = speed
        self.img = pygame.image.load('ball.png')
        self.width, self.height = self.img.get_size()
        self.screenWidth, self.screenHeight = self.screen.get_size()
        self.x, self.y = self.screen.get_size()
        self.x = (self.x/2)-(self.width/2)
        self.y = (self.y/5)*3 #Put the ball 3/5 down the screen
        self.dirrection = 180#randint(135, 225)
        self.rect = pygame.Rect(self.x, self.y, self.width, self.height)
        self.keepRunning = True
    def run(self, hitPaddle, paddleX, paddleWidth):
        self.speed += 0.0001
        if self.x < 0:
            self.dirrection = 360 - self.dirrection
            self.x = 1
        elif self.x > self.screenWidth-self.width:
            self.dirrection = 360 - self.dirrection
            self.x = self.screenWidth-(self.width+1)
        if self.y < 0:
            self.dirrection = 180 - self.dirrection
            self.y = 1
        if hitPaddle == -2:
            ##360 will to the left and right while 180 tould do up and down.
            ##I need both.
            self.dirrection = 540 - self.dirrection
        if hitPaddle != -1 and hitPaddle != -2:
            self.collidePoint = (self.x-paddleX+(self.width/2))-(paddleWidth/2)
            #print((45/(paddleWidth/2))*self.collidePoint)
            self.dirrection = 180 - self.dirrection
            self.dirrection -= (45/(paddleWidth/2))*self.collidePoint
            self.y = self.screenHeight-(21+self.height)
        if self.y > self.screenHeight:
            #print("You lost a life")
            self.keepRunning = False
        self.y+= -(cos(radians(self.dirrection)))* self.speed
        self.x+= -(sin(radians(self.dirrection)))* self.speed
        return self.keepRunning
    def update(self):
        self.rect = pygame.Rect(self.x, self.y, self.width, self.height)
        #pygame.draw.rect(self.screen, (0, 200, 0), self.rect)
        self.screen.blit(self.img, (self.x, self.y))
