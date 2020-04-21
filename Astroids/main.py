##Nathan Hinton
##This is the main file for astroids

#Vars:
width, height = 600, 600
fps = 15
startAstroids = 4
maxAstroids = 20
spawnChance = 1#percent

##Setup window:
import pygame
from time import sleep, time
from random import randint

from player import *
from bullet import *
from astroid import *

pygame.init()
screen = pygame.display.set_mode((width, height))
pygame.display.set_caption('Astro')

background = pygame.Surface(screen.get_size())
background = background.convert()
background.fill((0, 0, 0))


#Setup objects:
score = 0
player = Player(screen)
astroids = []
bullets = []

allObjects = []

for x in range(startAstroids):
    astroids.append(Astroid(screen))
    allObjects.append(astroids[-1])

#benchmarking:
start = time()
##Main Loop:
loop = True
count = 0#This is for making sure that the player has been hit for 2 frames
frameCount = 0
while loop == True:
    pygame.display.flip()
    screen.blit(background, (0, 0))
    #Get keypresses here
    keysPressed = pygame.key.get_pressed()
    keys = []
    if keysPressed[pygame.K_w]:
        keys.append('up')
    if keysPressed[pygame.K_d]:
        keys.append('right')
    if keysPressed[pygame.K_a]:
        keys.append('left')
    if keysPressed[pygame.K_SPACE]:
        bullets.append(Bullet(screen, player.x, player.y, player.heading))
        allObjects.append(bullets[-1])
            
    ##Logic here:
    ##Checking for colisions:
    astRects = []
    for obj in astroids:
        astRects.append(obj.rect)
    #print(pygame.Rect.collidelist(player.rect, astRects))
    if pygame.Rect.collidelist(player.rect, astRects) != -1:
        count += 1
        if count > 3:
            loop = False
            print("player died")
            print("score:", score)
    player.run(keys)
    for bullet in bullets:
        if pygame.Rect.collidelist(bullet.rect, astRects) != -1:
            i = pygame.Rect.collidelist(bullet.rect, astRects)
            astroids[i].hit()
            bullet.delete = True
        bullet.run()
        if bullet.delete == True:
            del bullet
            bullets.pop(0)
    for astroid in astroids:
        astroid.run()
        if astroid.delete == True:
            l = astroid.level
            score += l
            x = astroid.x
            y = astroid.y
            if l != 3:
                astroids.append(Astroid(screen, level = l+1, x = x, y = y))
                astroids.append(Astroid(screen, level = l+1, x = x, y = y))
            else:
                spawnChance += 1
            astroids.pop(astroids.index(astroid))
            del astroid
    if randint(0, 100) < spawnChance:
        if len(astroids) < maxAstroids:
            astroids.append(Astroid(screen))

    ##Update Graphics:
    player.update()
    for bullet in bullets:
        bullet.update()
    for astroid in astroids:
        astroid.update()
    frameCount += 1
    sleep(1/fps)
    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            pygame.quit()
            loop = False #End the main loop

end = time()
print("Avg fps was: %s"%(frameCount/(end-start)))
