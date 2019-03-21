##Nathan Hinton
##Interface for penguin hop

#Using pynput:
from time import sleep
import threading
from pynput.mouse import Button, Controller
from pynput.keyboard import Listener, KeyCode

mouse = Controller()

#Define the keys used:
exitKey = KeyCode(char='q')
a = KeyCode(char='1')
b = KeyCode(char='2')
c = KeyCode(char='3')
d = KeyCode(char='4')

y = 315

stateAndCapsList ={
    'Montgomery':'al', 
    'Juneau':'ak', 
    'Phoenix':'az', 
    'Little Rock':'ar', 
    'Sacramento':'ca', 
    'Denver':'co', 
    'Hartford':'ct', 
    'Dover':'de', 
    'Tallahassee':'fl', 
    'Atlanta':'ga', 
    'Honolulu':'hi', 
    'Boise':'id', 
    'Springfield':'il', 
    'Indianapolis':'in', 
    'Des Moines':'ia', 
    'Topeka':'ks', 
    'Frankfort':'ky', 
    'Baton Rouge':'la', 
    'Augusta':'me', 
    'Annapolis':'md', 
    'Boston':'ma', 
    'Lansing':'mi', 
    'Saint Paul':'mn', 
    'Jackson':'ms', 
    'Jefferson City':'mo', 
    'Helena':'mt', 
    'Lincoln':'ne', 
    'Carson City':'nv', 
    'Concord':'nh', 
    'Trenton':'nj', 
    'Santa Fe':'nm', 
    'Albany':'ny', 
    'Raleigh':'nc', 
    'Bismarck':'nd', 
    'Columbus':'oh', 
    'Oklahoma City':'ok', 
    'Salem':'or', 
    'Harrisburg':'pa', 
    'Providence':'ri', 
    'Columbia':'sc', 
    'Pierre':'sd', 
    'Nashville':'tn', 
    'Austin':'tx', 
    'Salt Lake City':'ut', 
    'Montpelier':'vt', 
    'Richmond':'va', 
    'Olympia':'wa', 
    'Charleston':'wv', 
    'Madison':'wi', 
    'Cheyenne':'wy'}

def resetMouse():
    mouse.move(-2000, -2000)

def click(x):
    resetMouse()
    mouse.move(x, y)
    for a in range(-25, 35, 10):
        #resetMouse()
        mouse.move(a, 0)
        #print(x+a, y)
        for z in range(-25, 35, 10):
            #resetMouse()
            mouse.move(0, z)
            mouse.click(Button.left)
            #sleep(3)
            #print(x+a, y+z)
    #print("Clicked!")

class kybd(threading.Thread):#Used for getting the kyboard input.
    def __init__(self):
        super(kybd, self).__init__()
        self.x = 0

test = kybd()#ClickMouse(delay, button)
test.start()


def on_press(key):
    #Logic for the keys:
    if key == a:
        click(350)
    elif key == b:
        click(600)
    elif key == c:
        click(775)
    elif key == d:
        click(1050)
    elif key == exitKey:
        listener.stop()

#Start the image stuff:
import pytesseract
from PIL import Image

testPath = '/home/nathan/Pictures/screen.png'
print(pytesseract.image_to_string(Image.open(testPath)))

##with Listener(on_press=on_press) as listener:
##    listener.join()
