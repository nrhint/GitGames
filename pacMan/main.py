##Nathan Hinton
##This is the main file for the pac man project

#vars:
width, height = (600, 400)
fps = 10
wallColor = (0, 0, 255)
ghostNumber = 3

#Setup:
import pygame
from time import sleep
from random import randint

from player import *
from ghosts import *
from coins import *


pygame.init()
screen = pygame.display.set_mode((width, height))
pygame.display.set_caption('Pac man')

background = pygame.Surface(screen.get_size())
background = background.convert()
background.fill((0, 0, 0))

#Setup the objects:
objects = []
player = Player(screen)
ghosts = []
objects.append(player)
#Add the ghosts:
for ghost in range(0, ghostNumber):
    ghost = Ghost(screen)
    ghosts.append(ghost)
    objects.append(ghost)


#Make the background:
wallRects = []
coinRects = []
coins = []
for x in range(0, width, 20):
    for y in range(0, height, 20):
        wallDir = randint(0, 5)
        if wallDir == 1:
            rect = pygame.Rect((x+8, y), (4, 20))
            wallRects.append(rect)
            pygame.draw.rect(screen, wallColor, rect)
        elif wallDir == 0:
            rect = pygame.Rect((x, y+8), (20, 4))
            wallRects.append(rect)
            pygame.draw.rect(screen, wallColor, rect)            
        else:
            coin = Coin(screen, x, y)
            coins.append(coin)
            coinRects.append(coin.rect)

def updateBackground():
    for rect in wallRects:
        pygame.draw.rect(screen, wallColor, rect)

#Main Loop:
score = 0
loop = True
while loop == True:
    #Get keypresses here:
    keysPressed = pygame.key.get_pressed()
    if keysPressed[pygame.K_UP]:
        key = 'up'
    elif keysPressed[pygame.K_DOWN]:
        key = 'down'
    elif keysPressed[pygame.K_RIGHT]:
        key = 'right'
    elif keysPressed[pygame.K_LEFT]:
        key = 'left'
    else:
        key = 'NULL'#Do not do anything.
    #Logic:
    #Checking for colisions:
    if pygame.Rect.collidelist(player.rect, wallRects) != -1:
        #print("HIT WALL")
        if player.heading == 90:
            player.y += player.speed
        elif player.heading == 270:
            player.y -= player.speed
        elif player.heading == 0:
            player.x -= player.speed
        elif player.heading == 180:
            player.x += player.speed
    ghostRects = []
    for ghost in ghosts:
        ghostRects.append(ghost.rect)
    if pygame.Rect.collidelist(player.rect, ghostRects) != -1:
        loop = False
        print("Player died")
    if pygame.Rect.collidelist(player.rect, coinRects) != -1:
        i = pygame.Rect.collidelist(player.rect, coinRects)
        coins.pop(i)
        coinRects.pop(i)
        score += 1
        
    

    #Run the objects:
    
    player.run(key)
    for ghost in ghosts:
        ghost.run(player.x, player.y)
    #Blit and draw:
    screen.blit(background, (0, 0))
    updateBackground()
    for coin in coins:
        coin.update()
    for obj in objects:
        obj.update()
    pygame.display.flip()
    sleep(1/fps)
    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            pygame.quit()
            loop = False#End the main loop
    
print(score)
