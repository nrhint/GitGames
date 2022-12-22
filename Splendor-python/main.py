##Nathan Hinton

import pygame
from time import time, sleep

##Vars that you can change:
fps = 60 #This will dirrectly change the hardness of the game.
step = 1/fps #Using time to delay if needed
width, height = 600, 600

pygame.init()
screen = pygame.display.set_mode((width, height))
pygame.display.set_caption("Splendor")

background = pygame.Surface(screen.get_size())
background = background.convert()
background.fill((0, 0, 255))

pygame.font.init() # you have to call this at the start, 
                   # if you want to use this module.
myfont = pygame.font.SysFont('Comic Sans MS', 30)

gameStart = time()

sprites = []
items = []

run = True
frameCount = 0
while run == True:
    nextStep = time()+step
    ##Update Screen:
    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            pygame.quit()
            run = False #End the main loop
        if event.type == pygame.MOUSEBUTTONDOWN:
            if pygame.mouse.get_pressed() == (True, False, False):
                clickPos = pygame.mouse.get_pos()
                for sprite in sprites:
                    if sprite.rect.collidepoint(clickPos):
                        pass

    ##Update the screen:
    screen.blit(background, (0, 0))
    pygame.display.flip()
    ##Sleep if needed
    if nextStep - time() > 0:
        sleep(nextStep - time())
    frameCount += 1
