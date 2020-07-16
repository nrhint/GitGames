##Nathan Hinton
##This is the node file

import pygame

##Information:
#### The screen is the display for pygame
#### The team defines who controlls it
#### position is obvious...
#### value is the inital value the node starts at
#### power is how fast it grows, smaller is faster
####

class Node:
    def __init__(self, screen, team, position, value = 10, power = 100):
        self.screen = screen
        self.team = team
        self.position = position
        self.value = value
        self.diameter = self.value
        self.power = power
        self.timer = 0
        self.rect = pygame.Rect(self.position[0], self.position[1], self.position[0]+self.value, self.position[1]+self.value)
        if self.team == 'human':
            self.color = (0, 0, 0)
        elif self.team == 'evil1':
            self.color = (255, 255, 255)
        else:#Team not valid
            print("TEAM %S NOT VALID!"%self.team)
            raise SyntaxError
    def run(self):
        self.timer += 1
        if self.timer % self.power == 0:
            self.value += 1
            self.diameter = self.value
    def update(self):
        pygame.draw.circle(self.screen, self.color, self.position, self.diameter)
        self.rect = pygame.Rect(self.position[0], self.position[1], self.position[0]+self.diameter, self.position[1]+self.diameter)
