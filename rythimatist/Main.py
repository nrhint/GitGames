##Nathan Hinton

#This game is basd on the boot the Rythamist. It is about drawing lines and shapes

#Here are the imports:
from tkinter import *


#Here are the classes for handeling errors:
class gameError(Exception):
    pass

class Game:
    #This class should handle the game management.
    def __init__(self):
        self.tk = Tk()
        self.windowWidth = 500
        self.windowHeight = 500
        self.drawingWidth = None
        self.drawingHeight = None
    def createDrawingArea(self, x1, y1, x2, y2):
        if x1 > self.windowWidth or x1 < 0:
            raise GameError("x1 outside of the screen!")
        elif x2 > self.windowWidth or x2 < 0:
            raise GameError("x2 outside of the screen!")
        self.drawingArea = Canvas(self.tk, width = self.drawingWidth, height = self.drawingHeight, bg = "white")

class Drawing:
    #This is a parent class for all of the drawings that will be animated
    def __init__(self):
        self.drawings = []
    def update(self):
        pass
    def draw(self):
        for drawing in self.drawings:
            drawing.update()

class Player:
    #This will handle all of the player input ad actions and then pass them to the correct place
    def __init__(self):
        pass
