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
    'Montgomery':'Alabama', 
    'Juneau':'Alaska', 
    'Phoenix':'Arizona', 
    'Little Rock':'Arkansas', 
    'Sacramento':'California', 
    'Denver':'Colorado', 
    'Hartford':'Connecticut', 
    'Dover':'Delaware', 
    'Tallahassee':'Florida', 
    'Atlanta':'Georgia', 
    'Honolulu':'Hawaii', 
    'Boise':'Idaho', 
    'Springfield':'Illinois', 
    'Indianapolis':'Indiana', 
    'Des Moines':'Iowa', 
    'Topeka':'Kansas', 
    'Frankfort':'Kentucky', 
    'Baton Rouge':'Louisiana', 
    'Augusta':'Maine', 
    'Annapolis':'Maryland', 
    'Boston':'Massachusetts', 
    'Lansing':'Michigan', 
    'Saint Paul':'Minnesota', 
    'Jackson':'Mississippi', 
    'Jefferson City':'Missouri', 
    'Helena':'Montana', 
    'Lincoln':'Nebraska', 
    'Carson City':'Nevada', 
    'Concord':'New Hampshire', 
    'Trenton':'New Jersey', 
    'Santa Fe':'New Mexico', 
    'Albany':'New York', 
    'Raleigh':'North Carolina', 
    'Bismarck':'North Dakota', 
    'Columbus':'Ohio', 
    'Oklahoma City':'Oklahoma', 
    'Salem':'Oregon', 
    'Harrisburg':'Pennsylvania', 
    'Providence':'Rhode Island', 
    'Columbia':'South Carolina', 
    'Pierre':'South Dakota', 
    'Nashville':'Tennessee', 
    'Austin':'Texas', 
    'Salt Lake City':'Utah', 
    'Montpelier':'Vermont', 
    'Richmond':'Virginia', 
    'Olympia':'Washington', 
    'Charleston':'West Virginia', 
    'Madison':'Wisconsin', 
    'Cheyenne':'Wyoming'}

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

###Start the image stuff:
import pytesseract
from PIL import Image

testPath = '/home/nathan/Pictures/Screenshot from 2019-02-16 13-11-49.png'
print(pytesseract.image_to_string(Image.open(testPath)))

with Listener(on_press=on_press) as listener:
    listener.join()
