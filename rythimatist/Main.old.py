##Nathan Hinton

#This game is basd on the boot the Rythamist. It is about drawing lines and shapes

#Here are the imports:
from tkinter import *
import threading
from time import time

#tk = Tk()

#vars that will need to be changed by the program later:
drawingWidth = 500
drawingHeight = 500

#Here are the classes for handeling errors:
class gameError(Exception):
    pass

class CreateDrawingArea(threading.Thread):
    def __init__(self, canvas_width, canvas_height):
        print("Initing...")
        super(CreateDrawingArea, self).__init__()
        self.size = 1
        self.ticks = 0
        self.b1 = None
        self.xold = None
        self.yold = None
        root = Tk()
        drawing_area = Canvas(root,width=canvas_width,height=canvas_height,bg="white")
        root.title("DrawingWindow")
        drawing_area.bind("<Motion>", self.motion)
        drawing_area.bind("<ButtonPress-1>", self.b1down)
        drawing_area.bind("<ButtonRelease-1>", self.b1up)
        drawing_area.pack(side=RIGHT)
        root.mainloop()
    def run(self):
        self.ticks += 1

    def b1down(self, event):
        self.b1
        x1, y1 = ( event.x - self.size ), ( event.y - self.size )
        x2, y2 = ( event.x + self.size ), ( event.y + self.size )
        event.widget.create_oval( x1, y1, x2, y2, fill = "black" )
        self.xold = event.x
        self.yold = event.y
        self.b1 = "down"

    def b1up(self, event):
        self.b1 = "up"
        self.xold = None
        self.yold = None

    def motion(self, event):
        if self.b1 == "down":
            x1, y1 = ( event.x - self.size ), ( event.y - self.size )
            x2, y2 = ( event.x + self.size ), ( event.y + self.size )
            event.widget.create_oval( x1, y1, x2, y2, fill = "black" )
            if self.xold is not None and self.yold is not None:
                x1, y1 = ( event.x - self.size ), ( event.y - self.size )
                x2, y2 = ( event.x + self.size ), ( event.y + self.size )
                event.widget.create_oval( x1, y1, x2, y2, fill = "black" )
                event.widget.create_line(self.xold,self.yold,event.x,event.y,smooth=TRUE,width=self.size*2+1)
            self.xold = event.x
            self.yold = event.y

class Game:
    #This class should handle the game management.
    def __init__(self):
        self.windowWidth = 500
        self.windowHeight = 500
        self.player = Player()
        self.drawingWidth = 500
        self.drawingHeight = 500

class Drawing:
    #This is a parent class for all of the drawings that will be animated
    def __init__(self):
        pass
    def update(self):
        pass
    def draw(self):
        for drawing in self.drawings:
            drawing.update()

class Player(Game):
    #This will handle all of the player input and actions and then pass them to the correct place
    def __init__(self):
        self.drawings = []
        if str(input("Load from file (y, N): ")) == 'y':
            self.openFile()            
    def openFile(self):
        self.filename = str(input("file name: "))
        try :
            open(self.fileneme, 'r')
        except FileNotFoundError:
            print("File not found. try again.")
            self.openFile


game = Game()
#CreateDrawingArea(100, 100)
#CreateDrawingArea(200, 200)
#CreateDrawingArea(300, 300)
#CreateDrawingArea(400, 400)
start = time()
drawing = CreateDrawingArea(500, 500)
print("Can I do things after starthing the drawing?")
#drawing.start()


end = time()
