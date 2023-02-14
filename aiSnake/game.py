import random
from enum import Enum
from collections import namedtuple
import numpy as np
import os

try:
    import pygame
except ModuleNotFoundError:
    print("Not importing pygame")

class Direction(Enum):
    RIGHT = 1
    LEFT = 2
    UP = 3
    DOWN = 4
    
Point = namedtuple('Point', 'x, y')

# rgb colors
WHITE = (255, 255, 255)
RED = (200,0,0)
BLACK = (0,0,0)

BLOCK_SIZE = 50
SPEED = 15

class SnakeGameAI:
    
    def __init__(self, visual, font, w=640, h=480, name = 0, food = None, display = None):
        self.BLUE1 = (0, 0, ((155*name)+255)%255)
        self.BLUE2 = (0, 100, ((155*name)+255)%255)
        self.name = name
        self.w = w
        self.h = h
        self.visual = visual
        if self.visual:
            self.font = font
            self.display = display
            self.clock = pygame.time.Clock()
            self.drawBlack = (0, 0)
        self.snake = []
        self.food = food
        self.reset()
        
    def reset(self):
        print("Start reset")
        if self.visual:
            for pos in self.snake:
                pygame.draw.rect(self.display, BLACK, pygame.Rect(pos.x, pos.y, BLOCK_SIZE, BLOCK_SIZE))
            pygame.draw.rect(self.display, BLACK, pygame.Rect(self.food.x, self.food.y, BLOCK_SIZE, BLOCK_SIZE))
        
        self.direction = Direction.RIGHT
        
        self.head = Point((random.randint(0, self.w)//BLOCK_SIZE)*BLOCK_SIZE, (random.randint(0, self.h)//BLOCK_SIZE)*BLOCK_SIZE)
        self.snake = [self.head, 
                      Point(self.head.x-BLOCK_SIZE, self.head.y),
                      Point(self.head.x-(2*BLOCK_SIZE), self.head.y)]
        
        self.score = 0
        self.frameIteration = 0
        print("End reset")
                
    def play_step(self, action, snakes):
        # 1. collect user input
        self.frameIteration += 1
        if self.visual:
            for event in pygame.event.get():
                if event.type == pygame.QUIT:
                    pygame.quit()
                    quit()
        
        # 2. move
        self._move(action) # update the head
        self.snake.insert(0, self.head)
        
        # 3. check if game over
        reward = 0
        game_over = False
        if self.is_collision(snakes = snakes) or self.frameIteration > 100*len(self.snake): 
            if self.frameIteration > 100*len(self.snake):
                print("Timed out")
            game_over = True
            reward = -10
            return reward, game_over, self.score
            
        # 4. place new food or just move
        if self.head == self.food.pos:
            self.score += 1
            reward = 20
            self.food._place_food()
        else:
            # reward = -0.001
            self.drawBlack = self.snake.pop()
        
        # 5. update ui and clock
        if self.visual:
            self._update_ui()
            self.clock.tick(SPEED)
        # 6. return game over and score
        return reward, game_over, self.score
    
    def is_collision(self, pt = None, snakes = None):
        # hits boundary
        if None == pt:
            pt = self.head
        # print(pt)
        if pt.x > self.w - BLOCK_SIZE or pt.x < 0 or pt.y > self.h - BLOCK_SIZE or pt.y < 0:
            return True
        # hits itself
        if pt in self.snake[1:]:
            return True
        if None != snakes:
            for snake in snakes:
                # print(pt in snake.snake)
                if pt in snake.snake:
                    if snake != self:
                        # print("Danger from another snake!")
                        return True
        
        return False
        
    def _update_ui(self):
        # self.display.fill(BLACK)
        pygame.draw.rect(self.display, BLACK, pygame.Rect(self.drawBlack.x, self.drawBlack.y, BLOCK_SIZE, BLOCK_SIZE))
        
        for pt in self.snake:
            pygame.draw.rect(self.display, self.BLUE1, pygame.Rect(pt.x, pt.y, BLOCK_SIZE, BLOCK_SIZE))
            pygame.draw.rect(self.display, self.BLUE2, pygame.Rect(pt.x+4, pt.y+4, BLOCK_SIZE-8, BLOCK_SIZE-8))
                    
        # text = self.font.render("Score: " + str(self.score), True, WHITE)
        # self.display.blit(text, [0, 0])
        # pygame.display.flip()
        # self.visual = False
        
    def _move(self, action):
        # [straight, right, left]

        clock_wise = [Direction.RIGHT, Direction.DOWN, Direction.LEFT, Direction.UP]
        idx = clock_wise.index(self.direction)

        if np.array_equal(action, [1, 0, 0]):
            new_dir = clock_wise[idx] # no change
        elif np.array_equal(action, [0, 1, 0]):
            next_idx = (idx + 1) % 4
            new_dir = clock_wise[next_idx] # right turn r -> d -> l -> u
        else: # [0, 0, 1]
            next_idx = (idx - 1) % 4
            new_dir = clock_wise[next_idx] # left turn r -> u -> l -> d

        self.direction = new_dir

        x = self.head.x
        y = self.head.y
        if self.direction == Direction.RIGHT:
            x += BLOCK_SIZE
        elif self.direction == Direction.LEFT:
            x -= BLOCK_SIZE
        elif self.direction == Direction.DOWN:
            y += BLOCK_SIZE
        elif self.direction == Direction.UP:
            y -= BLOCK_SIZE

        self.head = Point(x, y)

class Food:
    def __init__(self, w, h):
        self.w = w
        self.h = h
        self._place_food()
    
    def _place_food(self):
        self.x = random.randint(0, (self.w-BLOCK_SIZE )//BLOCK_SIZE )*BLOCK_SIZE 
        self.y = random.randint(0, (self.h-BLOCK_SIZE )//BLOCK_SIZE )*BLOCK_SIZE
        self.pos = Point(self.x, self.y)
        # if self.pos in snake:
        #     self._place_food()

    def _draw_food(self, display):
        pygame.draw.rect(display, RED, pygame.Rect(self.x, self.y, BLOCK_SIZE, BLOCK_SIZE))
