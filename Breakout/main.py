##Nathan Hinton
##This is The main file for the reakout game

from time import time

programStart = time()

import pygame
from time import sleep
from paddle import *
from ball import *
from brick import *
from colors import *


##Vars that you can change:
fps = 60 #This will dirrectly change the hardness of the game.
step = 1/fps #Using time to delay if needed
width, height = 800, 600
spaceBetweenbads = fps
bgColor = (0, 0, 200)
score = 0

pygame.init()
screen = pygame.display.set_mode((width, height))
pygame.display.set_caption("Breakout")

background = pygame.Surface(screen.get_size())
background = background.convert()
background.fill(bgColor)

##pygame.font.init() # you have to call this at the start, 
##                   # if you want to use this module.
##myfont = pygame.font.SysFont('Comic Sans MS', 30)

gameStart = time()

paddle = Paddle(screen)
ball = Ball(screen)
bricks = []
for x in range(0, 20):
    for y in range(5, 15):
        bricks.append(Brick(screen, (x*(width/20), y*(width/40)), RED))

run = True
frameCount = 0
idle = 0
while run == True:
    nextStep = time()+step
    keysPressed = pygame.key.get_pressed()
    keys = []
    if keysPressed[pygame.K_RIGHT]:
        keys.append('right')
    if keysPressed[pygame.K_LEFT]:
        keys.append('left')
    ##Logic:
    paddle.run(keys)
    hit = pygame.Rect.collidelist(paddle.rect, [ball.rect])#Check if the ball hit the paddle
    for brick in bricks:
        if pygame.Rect.collidelist(brick.rect, [ball.rect]) != -1:#Making the ball rect a one item list is a cruse way to do this. there is progogaly a function meant for this
            brick.delete = bgColor#Set it to the beckground color. it removes the brick and gives it the color
            bricks.remove(brick)
            score += 1
            hit = -2
        brick.run()
    run = ball.run(hit, paddle.x, paddle.width)
    ##Update Screen:
    screen.blit(background, (0, 0))
    paddle.update()
    ball.update()
    for brick in bricks:
        brick.update()
    pygame.display.flip()
    try:
        sleep(nextStep-time())
    except ValueError:
        pass
    frameCount += 1
    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            pygame.quit()
            run = False #End the main loop
pygame.quit()
end = time()
print("The time taken to load was %s seconds"%(gameStart-programStart))
print("The avg fps was %sfps"%(frameCount/(end-gameStart)))
