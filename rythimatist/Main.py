##Nathan Hinton

#This game is basd on the boot the Rythamist. It is about drawing lines and shapes

#Here are the imports:
import tkinter
import pynput

class Game:
    #This class should handle the game management.
    def __init__(self):
        pass

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
    #This will handel all of the player input ad actions and then pass them to the correct place
    def __init__(self):
        pass
