##Nathan Hinton
##This is the player file for platform hop:

import pygame

class Player:
    def __init__(self, display, startPos, imagePath='player.png'):
        self.x, self.y = startPos
        self.display = display
        self.img = pygame.image.load(imagePath)
        self.width, self.height = self.img.get_size()
        self.rect = pygame.Rect(self.x, self.y+self.height-2, self.width, 2)
        self.jump = False
        self.jumpWait = False
        self.jumpHeight = 20
    def run(self, keys, touchingObject):
        if touchingObject:
            pass
        else:
            self.y += 1
        if 'space' in keys and self.jumpWait == False:
            self.jump = True
        elif 'right' in keys:
            self.x += 1
        elif 'left' in keys:
            self.x -= 1
        #I am making a sort of a looping thing
        if self.jump == True:
            self.jumpWait = True
            self.jump = 'up'
            self.jumpCounter = 0
        elif self.jump == 'up':
            self.jumpCounter += 1
            self.y -= 3
            if self.jumpCounter > self.jumpHeight:
                self.jump = 'down'
        elif self.jump == 'down':
            self.jumpCounter += 1
            #No need to change the y because that is done by defualt
            if touchingObject:
                self.jump = False
                self.jumpWait = False
        if self.x < 0:
            self.x = 0
        elif self.x > self.display.get_width():
            self.x = self.display.get_width
    def update(self):
        self.rect = pygame.Rect(self.x, self.y+self.height-2, self.width, 2)
        #pygame.draw.rect(self.display, (0, 255, 0), self.rect)
        self.display.blit(self.img, (self.x, self.y))
