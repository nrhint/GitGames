##Nathan Hinton
##This is simon

import pygame
from random import randint

class Simon:
    def __init__(self, display, speed = 60):
        self.screen = display
        self.speed = speed
        self.screenWidth, self.screenHeight = self.screen.get_size()
        self.square1 = pygame.Rect((self.screenWidth/10)*0.5, (self.screenHeight/10)*0.5, (self.screenWidth/10)*4, (self.screenHeight/10)*4)
        self.square2 = pygame.Rect((self.screenWidth/10)*5.5, (self.screenHeight/10)*0.5, (self.screenWidth/10)*4, (self.screenHeight/10)*4)
        self.square3 = pygame.Rect((self.screenWidth/10)*0.5, (self.screenHeight/10)*5.5, (self.screenWidth/10)*4, (self.screenHeight/10)*4)
        self.square4 = pygame.Rect((self.screenWidth/10)*5.5, (self.screenHeight/10)*5.5, (self.screenWidth/10)*4, (self.screenHeight/10)*4)
        self.square1Color = [0, 200, 100]
        self.square2Color = [200, 100, 0]
        self.square3Color = [100, 0, 200]
        self.square4Color = [200, 0, 100]
        self.rects = [[self.square1, self.square1Color], [self.square2, self.square2Color], [self.square3, self.square3Color], [self.square4, self.square4Color]]
        self.clickyRects = [self.square1, self.square2, self.square3, self.square4]
        self.history = [randint(0, 3), randint(0, 3), randint(0, 3), randint(0, 3)]
        self.mode = 'show'
        self.counter = 0
        self.index = -1
    def run(self, mouseClickPos):
        run = True
        if self.mode == 'show':
            if self.counter%self.speed == 0:
                self.index += 1
                self.rects = [[self.square1, self.square1Color], [self.square2, self.square2Color], [self.square3, self.square3Color], [self.square4, self.square4Color]]
            elif self.counter%self.speed == self.speed/10:
                try:#Set the current color
                    self.rects[self.history[self.index]][1] = self.addToColor(self.rects[self.history[self.index]][1], 50)
                except IndexError:
                    self.mode = 'waitInit'
                #print("colors changes, index %s"%self.index)
            self.counter += 1
        elif self.mode == 'waitInit':
            self.index = 0
            self.move = False
            self.mode = 'input'
        elif self.mode == 'add':#Add a part to the history
            self.history.append(randint(0, 3))
            self.mode = 'show'
            self.index = -1
        else:
            if self.index == len(self.history):
                self.mode = 'add'
            if self.move == True and mouseClickPos == (0, 0):
                self.index += 1
                self.rects = [[self.square1, self.square1Color], [self.square2, self.square2Color], [self.square3, self.square3Color], [self.square4, self.square4Color]]
                self.move = False
            for rect in self.rects:#This is just the rect valuse in the squares
                if rect[0].collidepoint(mouseClickPos):
                    clickedIndex = self.rects.index(rect)
                    if clickedIndex == self.history[self.index] and self.move == False:
                        self.move = True
                        self.rects[self.history[self.index]][1] = self.addToColor(self.rects[self.history[self.index]][1], 50)
                    elif clickedIndex == self.history[self.index]:
                        pass
                    else:
                        print("Nope")
                        run = False #This is a loose line.
        return run
    def addToColor(self, color, value):
        result = []
        for place in color:
            result.append(place + value)
        return result
    def update(self):
        pygame.draw.rect(self.screen, self.rects[0][1], self.rects[0][0])
        pygame.draw.rect(self.screen, self.rects[1][1], self.rects[1][0])
        pygame.draw.rect(self.screen, self.rects[2][1], self.rects[2][0])
        pygame.draw.rect(self.screen, self.rects[3][1], self.rects[3][0])
