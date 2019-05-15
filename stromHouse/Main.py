##Nathan Hinton
##Storm the house

##TODO:
#Fix the screen size so it only looks at the first 100 pixels and looks for black spots.
#Make the reload function more reliable.

screen = (20, 125, 620, 525)#(375, 225, 775, 800)
height = 400
width = 600

import pyscreenshot as ss
from time import sleep
from pynput import mouse, keyboard
import PIL


#Setup for the mouse:
m = mouse.Controller()
button = mouse.Button.left

def click(pos):
    mouse.Controller.position.fset(m, pos)
    mouse.Controller.click(m, button)
    mouse.Controller.click(m, button)

#Setup for the keyboard:
k = keyboard.Controller()
space = keyboard.Key.space
def reload():
    #sleep(1)
    keyboard.Controller.press(k, space)
    keyboard.Controller.release(k, space)
    keyboard.Controller.type(k, ' ')

sleep(3)
while True:
    dat = ss.grab(screen)
    dat.show()
    for y in range(0, height):
        if dat.getpixel((0, y)) == (0, 0, 0):
            click((0, y))
    reload()
    sleep(2)
