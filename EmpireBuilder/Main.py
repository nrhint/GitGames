##Nathan Hinton

#Import:
import pygame
from constants import *

class Interface():#This will be for multiplayer later
    def __init__(self):
        print("Interface Init")

class Player():#This is for the player and will handle all of the input from the user to the Screen class
    def __init__(self, color, name):
        print("Player Init")

class Screen():#This class is for all of the drawing
    def __init__(self, width = 300, height = 200):
        print("Screen Init")
        #width, height = 300, 200
        screen = pygame.display.set_mode((width, height))
        screen.fill(WHITE)
        pygame.display.flip()
        self.width = width
        self.height = height
    def update(self):
        pass

def main():
    print("Starting Main...")
    screen = Screen()
    running = True
    while running:
        screen.update()
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                running = False
                pygame.quit()

if __name__ == '__main__':main()
