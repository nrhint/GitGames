##Nathan Hinton
##This is the main file for astroids

##Imporoving fps old was 450 avg with varriances in playing
##New is:

#Vars:
width, height = 600, 600
fps = 60
startAstroids = 4
maxAstroids = 4
spawnChance = 100#percent

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
bg = pygame.Rect((0, 0), screen.get_size())


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
times = []
##Main Loop:
loop = True
count = 0#This is for making sure that the player has been hit for 2 frames
frameCount = 0
while loop == True:
    nextStep = time()#+(1/fps)#Dissable fps tracking
    dirtyRects = [bg]
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
            #loop = False # dissable death
            #print("player died")
            #print("score:", score)
            pass
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
    screen.blit(background, (0, 0))
    k=player.update()
    for bullet in bullets:
        dirtyRects.append(bullet.update())
    for astroid in astroids:
        dirtyRects.append(astroid.update())
    pygame.display.update([k, player.oldRect])
    frameCount += 1
    times.append(nextStep-time())
    try:
        if nextStep > time():
            sleep(nextStep-time())
    except ValueError:
        pass
    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            loop = False #End the main loop

pygame.quit()
end = time()
print("Avg fps was: %s"%(frameCount/(end-start)))
