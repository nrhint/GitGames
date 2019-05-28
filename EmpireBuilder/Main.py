##Nathan Hinton

#Import:
import pygame
from time import sleep
from constants import *

class Interface():#This will be for multiplayer later
    def __init__(self):
        print("Interface Init")

class Player():#This is for the player and will handle all of the input from the user to the Screen class
    def __init__(self, color, name):
        print("Player Init")
    def button(self, width, height, x, y, text):
        pass

class Screen():#This class is for all of the drawing
    def __init__(self, width = 300, height = 200):
        print("Screen Init")
        self.display = pygame.display.set_mode((width, height))
        #print(self.display)
        #print(type(self.display))
        self.display.fill(WHITE)
        pygame.display.flip()
        self.width = width
        self.height = height
        self.images = {}#Store all of the images by image and pos
    def update(self):
        for image in self.images:
            self.display.blit(image, self.images[image])
        pygame.display.update()
    def loadImage(self, imagePath, x, y):
        try:
            image = pygame.image.load(imagePath)
        except FileNotFoundError:
            print("FILE NOT FOUND!")
            print(imagePath)
        self.images.update({image:[x, y]})

##class Button(pygame.sprite.Sprite):
##    def __init__(self, width, height, x, y, text):
##        #Convert data:
##        xy = (x, y)
##        text = str(text)
##        

imagePath = './Images/Black/TrackV1.0.png'
def TestScreen(Screen, width = 300, height = 200):#Make a test button.
    Screen.display.fill(WHITE)
    Screen.update()
    #button = Button(20, 40, 100, 100, 'nathan')
    player = Player('black', 'nathan')
    player.button(20, 40, 100, 100, 'test')
    #sleep(1)
    Screen.loadImage(imagePath, 0, 0)
    Screen.update()
    sleep(1)
    Screen.__init__(width, height)

def main():
    print("Starting Main...")
    screen = Screen()
    TestScreen(screen)
    running = True
    while running:
        screen.update()
        for event in pygame.event.get():
            print(event)
            if event.type == pygame.QUIT:
                running = False
                pygame.quit()

if __name__ == '__main__':main()
