import torch
import random
import numpy as np
from collections import deque

import logging
from time import time
import sys

import os
if os.path.exists("runCount"):
    with open("runCount", 'r') as file:
        runCount = int(file.read())
else:
    runCount = 0
with open("runCount", 'w') as file:
    file.write(str(runCount+1))

logging.basicConfig(filename='logs/run%s.log'%runCount, encoding='utf-8', level=logging.INFO)
logging.info('Run started at: %s'%time())

from game import SnakeGameAI, Direction, Point, BLOCK_SIZE
from model import Linear_QNet, QTrainer
from helper import plot

MAX_MEMORY = 100_000
BATCH_SIZE = 1000
LEARNING_RATE = 0.001
GAME_COUNT_FOR_EPSILON = 100

class Agent:
    def __init__(self):
        self.n_games = 0
        self.epsilon = 0 #randomness
        self.gamma = 0.9 #Discount rate MUST BE <1
        self.memory = deque(maxlen=MAX_MEMORY)
        self.model = Linear_QNet(11, 256, 3)
        self.trainer = QTrainer(self.model, lr=LEARNING_RATE, gamma=self.gamma)

    def get_state(self, game):
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
            (dir_r and game.is_collision(point_r)) or 
            (dir_l and game.is_collision(point_l)) or
            (dir_u and game.is_collision(point_u)) or
            (dir_d and game.is_collision(point_d)),

            ##Check right for danger
            (dir_u and game.is_collision(point_r)) or 
            (dir_d and game.is_collision(point_l)) or
            (dir_l and game.is_collision(point_u)) or
            (dir_r and game.is_collision(point_d)),

            ##Check left for danger
            (dir_d and game.is_collision(point_r)) or 
            (dir_u and game.is_collision(point_l)) or
            (dir_r and game.is_collision(point_u)) or
            (dir_l and game.is_collision(point_d)),

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

def train(loadFromSave):
    plot_score = []
    plot_mean_scores = []
    total_score = 0
    record_score = 0
    agent = Agent()
    if loadFromSave:
        print("Loading from save...")
        agent.model.load()
    game = SnakeGameAI()
    while True:
        # if not agent.n_games%10:
        #     game.visual = True
        state_old = agent.get_state(game)

        final_move = agent.get_action(state_old)

        reward, done, score = game.play_step(final_move)
        new_state = agent.get_state(game)

        agent.train_short_memory(state_old, final_move, reward, new_state, done)
        agent.remember(state_old, final_move, reward, new_state, done)

        if done:
            #Train long memory and plot and reset
            game.reset()
            agent.n_games += 1
            agent.train_long_memory()
            if score > record_score:
                record_score = score
                agent.model.save(runCount, score)

            print("Game", agent.n_games, "Record", record_score, "Score", score)
            plot_score.append(score)
            total_score += score
            mean_score = total_score / agent.n_games
            plot_mean_scores.append(mean_score)
            logging.info("Game: " + str(agent.n_games) + " Score: " + str(score) + " High score: " + str(record_score) + " Time: " + str(time()))
            if agent.n_games % 50 == 0:
                plot(plot_score, plot_mean_scores)
if __name__ == "__main__":
    loadFromSave = False
    if 1 != len(sys.argv):
        print(sys.argv)
        if sys.argv[1] == "--load" or sys.argv[1] == "-l":
            loadFromSave = True
    train(loadFromSave)