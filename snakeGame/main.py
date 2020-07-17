##Nathan Hinton

#This is a snake game for practice

import pygame
from time import sleep
from random import randint as randNum

gameType = 'dynamic'

class Snake():
    def __init__(self, x=10, y=10, startLength=4, color='green'):
        self.color = color
        self.die = False
        self.img = pygame.image.load('%s.png'%color)
        self.positions = []
        for z in range(startLength):
            self.positions.append([x, y+z])
        self.direction = 'up'
        self.draw()
    def run(self, direction):
        if direction != self.direction:
            #print("direction CHANGE")
            self.direction = direction
        if direction == 'up':#Change y to be smaller
            self.positions.insert(0, [self.positions[0][0], self.positions[0][1]-1])
        elif direction == 'left':#change x to be smaller
            self.positions.insert(0, [self.positions[0][0]-1, self.positions[0][1]])
        elif direction == 'right':#change the x to be larger
            self.positions.insert(0, [self.positions[0][0]+1, self.positions[0][1]])
        else:#direction is down, y gets larger
            self.positions.insert(0, [self.positions[0][0], self.positions[0][1]+1])

        if self.positions[0] != apple.position:
            self.positions.pop()
        else:
            apple.move()
        self.draw()
        self.x, self.y = self.positions[0]
        #print(self.x, self.y)
        if 0>self.x or self.x>(screen.get_width()/20)-1:
            print("%s OUT OF SCREEN X"%self.color)
            self.die = True
            return False#End the main loop
        if 0>self.y or self.y>(screen.get_height()/20)-1:
            print("%s OUT OF SCREEN Y"%self.color)
            self.die = True
            return False#End the main loop
        if self.positions[0] in self.positions[1::]:
            print("%s HIT ITSELF"%self.color)
            self.die = True
            return False#End the main loop
        return True
    def draw(self):
        for pos in self.positions:
            screen.blit(self.img, (pos[0]*20, pos[1]*20))
        #print(self.positions)
            
        ##Take in keys
        ##apple eaten?
        ##Move the snake

class AISnake(Snake):
    def __init__(self, x=10, y=10, startLength=4, color='green'):
        self.color = color
        self.die = False
        self.img = pygame.image.load('%s.png'%color)
        self.positions = []
        for z in range(startLength):
            self.positions.append([x, y+z])
        self.direction = 'up'
        self.screenWidth = 20
        self.screenHeight = 20
        self.target = [0, 0]
        self.draw()

    def logic(self):
        self.pos = self.positions[0]
        #print(self.pos == self.target)
        if self.pos == self.target:#move to the next position
            if self.target == [19, 18]:
                self.target = [19, 19]
            elif self.pos == [19, 19]:
                self.target = [0, 19]
            elif self.pos == [0, 19]:
                self.target = [0, 0]
            else:
                self.target = [self.target[0]+1, (self.target[1]+18)%36]
        #Get the left/right:
        if self.pos[0]-self.target[0] > 0:
            self.direction = 'left'
        elif self.pos[0]-self.target[0] < 0:
            self.direction = 'right'
        else:
            if self.pos[1]-self.target[1] > 0:
                self.direction = 'up'
            else:
                self.direction = 'down'
        #print(self.target)
        return self.direction

class Apple():
    def __init__(self):
        self.img = pygame.image.load('apple.png')
        self.position = [randNum(0, screen.get_width()/20-1), randNum(0, screen.get_height()/20-1)]
        self.draw()
    def run(self):
        self.draw()
        #self.position = (randNum(0, 20), randNum(0, 20))
    def move(self):
        self.position = [randNum(0, screen.get_width()/20-1), randNum(0, screen.get_height()/20-1)]
        print(self.position)
    def draw(self):
        screen.blit(self.img, (self.position[0]*20, self.position[1]*20))
        

##Initalize pygame

pygame.init()
screen = pygame.display.set_mode((800, 600))#Width and height
pygame.display.set_caption('Snake')

background = pygame.Surface(screen.get_size())
background = background.convert()
background.fill((0, 0, 0))
##This is the reset point:
loop = True
fps = 10
while loop == True:
    pygame.event.clear()
    screen.blit(background, (0, 0))
    #pygame.display.flip()
    if gameType != 'dynamic':
        sleep(1)
    ##Init the player and apple
    
    eric = Snake(startLength = 4, color='eric')
    nathan = Snake(5, 10, startLength = 4, color = 'green')
    players = [eric, nathan]
    AI = AISnake(startLength = 4, color = 'AI')
    x = 0
    total = 0
    apple = Apple()

    pygame.display.flip()
    ##Begin the game loop:
    play = []
    direction = 'up'
    direction2 = 'up'
    while False not in play:
        screen.blit(background, (0, 0))#Update the screen first so that things are not covered
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
    ##########################FOR THE ERIC SNAKE#################
                if event.key == pygame.K_w:
                    direction2 = 'up'
                elif event.key == pygame.K_s:
                    direction2 = 'down'
                elif event.key == pygame.K_d:
                    direction2 = 'right'
                elif event.key == pygame.K_a:
                    direction2 = 'left'

        apple.run()
        if gameType != 'dynamic':
            pass
            play.append(eric.run(direction2))#Returns True of not out of screen or not hitting self
            play.append(nathan.run(direction))
        else:#The game type is dynamic
            AI.run(AI.logic())
            if AI.die == True:
                if x > 99:
                    play = [False]
                    loop = False
                else:
                    print(AI.pos)
                    #sleep(1)
                    x += 1
                    total += len(AI.positions)
                    print(len(AI.positions))
                    avg = total/x
                    AI.__init__(color = AI.color)

            eric.run(direction)
            nathan.run(direction2)
            if eric.die == True:
                eric.__init__(color = eric.color)
#                print("%s scored %s"%(eric.color, len(eric.positions)))
            if nathan.die == True:
                nathan.__init__(color = nathan.color)
#                print("%s scored %s"%(nathan.color, len(nathan.positions)))
        pygame.display.flip()
        sleep(1/fps)
    for player in players:
        pass
#        print("%s scored %s"%(player.color, len(player.positions)))
