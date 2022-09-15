##Nathan Hinton
##This is the main file for the subtraction blast game

width = 800
height = 600
fps = 30
maxFalling = 10
level = 5
score = 0

from time import sleep, time
import pygame

from player import Player
from shot import Shooter
from falling import Falling
from utils import doesThisColide
from gameText import GameText
from random import randint

pygame.init()
screen = pygame.display.set_mode((width, height))
pygame.display.set_caption('Subtraction Blast')

font = pygame.font.Font('freesansbold.ttf', 32)

background = pygame.Surface(screen.get_size())
background = background.convert()
background.fill((0, 0, 0))

##Setup players:
playerPos = (width/2-50, height-35)
player = Player(screen, playerPos, 100, 75)

##Set up item lists
sprites = []
scoreText = GameText(screen, (0, 0), font, "Score: 0")
levelText = GameText(screen, (0, 50), font, "Level: 0")

##Main loop:
lastFrameTime = time()
run = True
numberFalling = 0
while run == True:
    level = (score//10)+5
    ##Do stuff:
    if numberFalling == 0:
        sprites.append(Falling(screen, level, font))
        numberFalling += 1
    elif numberFalling<maxFalling:
        if level/4 > randint(0, 100):
            sprites.append(Falling(screen, level, font))
            numberFalling += 1

    player.tick(pygame.mouse.get_pos())
    for item in sprites:
        item.tick()
        if type(item) == Shooter:
            for object in sprites:
                if type(object) == Falling:
                    status = doesThisColide(item.pts, object.pts)
                    if status == True:
                        #print("Colided!")
                        object.value -= item.value
                        if object.value <= 0:
                            score += object.startValue
                            scoreText.tick("Score: %s"%score)
                            levelText.tick("Level: %s"%level)
                            sprites.remove(object)
                            numberFalling -= 1
                        sprites.remove(item)
                        break
        if (item.y < -100 or item.y > height+100) or (item.y < -100 or item.x > width + 100):
            if type(item) == Falling:
                numberFalling -= 1
            try:
                sprites.remove(item)
            except ValueError:
                pass
    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            run = False #End the main loop
        if event.type == pygame.KEYDOWN:
            if event.key == pygame.K_1:
                sprites.append(Shooter(screen, (playerPos[0]+(player.width/2), height), player.angle, font, value = 1))
            elif event.key == pygame.K_2:
                sprites.append(Shooter(screen, (playerPos[0]+(player.width/2), height), player.angle, font, value = 2))
            elif event.key == pygame.K_3:
                sprites.append(Shooter(screen, (playerPos[0]+(player.width/2), height), player.angle, font, value = 3))
            elif event.key == pygame.K_4:
                sprites.append(Shooter(screen, (playerPos[0]+(player.width/2), height), player.angle, font, value = 4))
            elif event.key == pygame.K_5:
                sprites.append(Shooter(screen, (playerPos[0]+(player.width/2), height), player.angle, font, value = 5))
            elif event.key == pygame.K_6:
                sprites.append(Shooter(screen, (playerPos[0]+(player.width/2), height), player.angle, font, value = 6))
            elif event.key == pygame.K_7:
                sprites.append(Shooter(screen, (playerPos[0]+(player.width/2), height), player.angle, font, value = 7))
            elif event.key == pygame.K_8:
                sprites.append(Shooter(screen, (playerPos[0]+(player.width/2), height), player.angle, font, value = 8))
            elif event.key == pygame.K_9:
                sprites.append(Shooter(screen, (playerPos[0]+(player.width/2), height), player.angle, font, value = 9))

    try:
        sleep((lastFrameTime-time())+(1/fps))
    except ValueError:
        pass
    lastFrameTime = time()
    screen.blit(background, (0, 0))
    for item in sprites:
        item.update()
    player.update()
    scoreText.update()
    levelText.update()
    pygame.display.flip()

pygame.quit()