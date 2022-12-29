import torch
import random
import numpy as np
from collections import deque
import os
import logging
from time import time
import sys
from threading import Thread

import os
if os.path.exists("runCount"):
    with open("runCount", 'r') as file:
        runCount = int(file.read())
else:
    runCount = 0
with open("runCount", 'w') as file:
    file.write(str(runCount+1))

os.makedirs("logs", exist_ok=True)
logging.basicConfig(filename='logs/run%s.log'%runCount, encoding='utf-8', level=logging.INFO)
logging.info('Run started at: %s'%time())

from game import SnakeGameAI, Food, Direction, Point, BLOCK_SIZE
from model import Linear_QNet, QTrainer
from helper import plot

MAX_MEMORY = 100_000
BATCH_SIZE = 1000
LEARNING_RATE = 0.01
GAME_COUNT_FOR_EPSILON = 200
(WIDTH, HEIGHT) = (1000, 600)
DEBUG = False

class Agent:
    def __init__(self):
        self.n_games = 0
        self.epsilon = 0 #randomness
        self.gamma = 0.9 #Discount rate MUST BE <1
        self.memory = deque(maxlen=MAX_MEMORY)
        self.model = Linear_QNet(18, 256, 3)
        self.trainer = QTrainer(self.model, lr=LEARNING_RATE, gamma=self.gamma)

    def get_state(self, game, snakes):
        head = game.snake[0]
        point_l = Point(head.x - BLOCK_SIZE, head.y)
        point_r = Point(head.x + BLOCK_SIZE, head.y)
        point_u = Point(head.x, head.y - BLOCK_SIZE)
        point_d = Point(head.x, head.y + BLOCK_SIZE)

        dir_l = game.direction == Direction.LEFT
        dir_r = game.direction == Direction.RIGHT
        dir_u = game.direction == Direction.UP
        dir_d = game.direction == Direction.DOWN

        state = [
            ##Check in front for danger
            (dir_r and game.is_collision(point_r, snakes)) or 
            (dir_l and game.is_collision(point_l, snakes)) or
            (dir_u and game.is_collision(point_u, snakes)) or
            (dir_d and game.is_collision(point_d, snakes)),

            ##Check right for danger
            (dir_u and game.is_collision(point_r, snakes)) or 
            (dir_d and game.is_collision(point_l, snakes)) or
            (dir_l and game.is_collision(point_u, snakes)) or
            (dir_r and game.is_collision(point_d, snakes)),

            ##Check left for danger
            (dir_d and game.is_collision(point_r, snakes)) or 
            (dir_u and game.is_collision(point_l, snakes)) or
            (dir_r and game.is_collision(point_u, snakes)) or
            (dir_l and game.is_collision(point_d, snakes)),

            ##Check corners for danger clockwise starting from top left
            game.is_collision(Point(head.x - BLOCK_SIZE, head.y - BLOCK_SIZE), snakes),
            game.is_collision(Point(head.x + BLOCK_SIZE, head.y - BLOCK_SIZE), snakes),
            game.is_collision(Point(head.x + BLOCK_SIZE, head.y + BLOCK_SIZE), snakes),
            game.is_collision(Point(head.x - BLOCK_SIZE, head.y + BLOCK_SIZE), snakes),

            ##Check two  front, right, left
            game.is_collision(Point(head.x, head.y + (BLOCK_SIZE*2)), snakes),
            game.is_collision(Point(head.x - (BLOCK_SIZE * 2), head.y), snakes),
            game.is_collision(Point(head.x + (BLOCK_SIZE * 2), head.y), snakes),

            ##Move direction
            dir_r, 
            dir_l, 
            dir_u, 
            dir_d, 

            ##Food direction
            game.food.x < game.head.x,
            game.food.x > game.head.x,
            game.food.y < game.head.y, 
            game.food.y > game.head.x
        ]
        return np.array(state, dtype=int)

    def remember(self, state, action, reward, next_state, game_over):
        self.memory.append((state, action, reward, next_state, game_over))

    def train_long_memory(self):
        if len(self.memory) > BATCH_SIZE:
            mini_sample = random.sample(self.memory, BATCH_SIZE)
        else:
            mini_sample = self.memory
        
        states, actions, rewards, next_states, game_overs = zip(*mini_sample)
        self.trainer.train_step(states, actions, rewards, next_states, game_overs)

    def train_short_memory(self, state, action, reward, next_state, game_over):
        self.trainer.train_step(state, action, reward, next_state, game_over)

    def get_action(self, state):
        self.epsilon = GAME_COUNT_FOR_EPSILON-self.n_games
        final_move = [0, 0, 0]
        if random.randint(0, int(GAME_COUNT_FOR_EPSILON*2.5)) < self.epsilon:
            move = random.randint(0, 2)
            final_move[move] = 1
        else:
            state0 = torch.tensor(state, dtype=torch.float)
            predication = self.model(state0)
            move = torch.argmax(predication).item()
            final_move[move] = 1

        return final_move

def train(loadFromSave, visual, count):
    plot_score = []
    plot_mean_scores = []
    total_score = 0
    record_score = 0
    agents = []
    snakes = []
    food = Food(w = WIDTH, h = HEIGHT)
    for x in range(0, count):
        snakes.append(SnakeGameAI(visual, w = WIDTH, h = HEIGHT, name = x, food = food, display = display, font = font))
        agents.append([Agent(), snakes[-1]])
    if loadFromSave == 1:
        if DEBUG:print("Loading from save...")
        for agent, game in agents:
            agent.trainer.load()
    if loadFromSave == 2:
        if DEBUG:print("Loading from save...")
        runNumber = input("Enter the run you want to resume: ")
        snakeNumber = 0
        files = os.listdir("./model/")
        files.sort()
        if DEBUG:print(files)
        for agent, game in agents:
            tmpFile = None
            for file in files:
                if "model-run%s-snake%s-"%(runNumber, snakeNumber) in file:
                    tmpFile = file 
            if None != tmpFile:
                agent.trainer.load(filename = "./model/%s"%tmpFile)
            else:
                print("Unable to find load file for snake! Creating with empty brain")
            snakeNumber += 1
    if loadFromSave == 3:
        runNumber = input("Enter the run number: ")
        snakeNumber = input("Enter the snake number: ")
        highScore = input("Enter the high score of the run: ")
        if runNumber != "":
            filename = "./model/model-run%s-snake%s-%s.pth"%(runNumber, snakeNumber, highScore)
        for agent, game in agents:
            agent.trainer.load(filename)


    while True:
        # if not agent.n_games%10:
        #     game.visual = True
        for agent, game in agents:
            if visual:
                food._draw_food(display)
                pygame.display.flip()
            state_old = agent.get_state(game, snakes)

            final_move = agent.get_action(state_old)

            reward, done, score = game.play_step(final_move, snakes)
            new_state = agent.get_state(game, snakes)

            agent.train_short_memory(state_old, final_move, reward, new_state, done)
            agent.remember(state_old, final_move, reward, new_state, done)

            if done:
                #Train long memory and plot and reset
                game.reset()
                if DEBUG:print("Agent reset")
                agent.n_games += 1
                th = Thread(target=agent.train_long_memory())
                if DEBUG:print("Agent thread started")
                # agent.train_long_memory()
                if score > record_score:
                    record_score = score
                    gameName = game.name
                    agent.trainer.save(runCount, score, gameName)
                    
                    print("Reloading all models with new record")
                    # while os.path.exists()
                    for agent, game in agents:
                        agent.trainer.load("./model/model-run%s-snake%s-%s.pth"%(runCount, gameName, record_score))

                print("Snake", game.name, "Game", agent.n_games, "Record", record_score, "Score", score)
                plot_score.append(score)
                total_score += score
                mean_score = total_score / agent.n_games
                plot_mean_scores.append(mean_score)
                logging.info("Game: " + str(agent.n_games) + " Score: " + str(score) + " High score: " + str(record_score) + " Time: " + str(time()))
                if DEBUG:print("Agent reset done")
                # if agent.n_games%50 == 0:
                #     plot(plot_score, plot_mean_scores)
if __name__ == "__main__":
    loadFromSave = False
    display, visual, font = False, False, False
    count = 1
    if 1 != len(sys.argv):
        if DEBUG:print(sys.argv)
        if "--load" in sys.argv or "-l" in sys.argv:
            loadFromSave = True
            GAME_COUNT_FOR_EPSILON = 0
        if "--resume" in sys.argv or "-r" in sys.argv:
            loadFromSave = 2
            GAME_COUNT_FOR_EPSILON = 0
        if "--trainFromModel" in sys.argv:
            loadFromSave = 3
        if "--visual"  in sys.argv or "-vi" in sys.argv:
            visual = True
            import pygame
            pygame.init()
            font = pygame.font.Font('arial.ttf', 25)
            # init display
            display = pygame.display.set_mode((WIDTH, HEIGHT))
            pygame.display.set_caption('Snake')

        if "--count" in sys.argv:
            count = int(sys.argv[sys.argv.index("--count")+1])
    train(loadFromSave, visual, count)