##Nathan Hinton
##This is a new version of snake

import pygame
from random import randint

class Game():
    def __init__(self, width, height):
        self.width = width
        self.height = height
        self.fps = 10
        self.play = True

        pygame.init()
        self.screen = pygame.display.set_mode((self.width, self.height))#Width and height
        pygame.display.set_caption('Snake2.0')

        self.background = pygame.Surface(self.screen.get_size())
        self.background = self.background.convert()
        self.background.fill((0, 0, 0))

    
    def run(self):
        while True == self.play:
            self.screen.blit(self.background, (0, 0))
            for event in pygame.event.get():
                #print(event)
                if event.type == pygame.QUIT:
                    pygame.quit()
                    play = False
                if event.type == pygame.KEYDOWN:
        ######################FOR THE nathan SNAKE########################
                    if event.key == pygame.K_UP:
                        direction = 'up'
                    elif event.key == pygame.K_DOWN:
                        direction = 'down'
                    elif event.key == pygame.K_RIGHT:
                        direction = 'right'
                    elif event.key == pygame.K_LEFT:
                        direction = 'left'

    
class Apple():
    def __init__(self, maxX, maxY, size, value):
        self.value = value
        self.x = randint(0, maxX)
        self.y = randint(0, maxY)
        self.rect = pygame.Rect
    
    def draw(self):

    
class PlayerSnake():
    def __init__(self):
        pass

    def takeInput(self, input):
        pass

class AISnake():
    def __init__(self):
        pass

    def look(self):
        pass

    def changeDirection(self):
        pass