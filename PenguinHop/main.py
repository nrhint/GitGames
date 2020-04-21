##Setup: scroll down twice. site: https://www.arcademics.com/games/penguin-hop

import pyscreenshot# as ImageGrab
#import os
from pynput.mouse import Button, Controller
##from pynput.keyboard import Listener
import time
import PIL

xPad = 200
yPad = 160
mouse = Controller()

def screenGrab(delay = 0, name = 'test'):
    time.sleep(delay)
    box = ()
    im = pyscreenshot.grab((xPad, yPad, xPad+950, yPad+540))
    im.save(name+'.jpg')
    return im

def leftClick():
    mouse.click(Button.left)

def moveMouse(x, y):
    tx, ty = mouse.position
    mouse.move((x-tx)+xPad, (y-ty)+yPad)

def clicker(pos):
    if pos == 1:
        base = (100, 175)
        for x in range(0, 3):
            for y in range(0, 3):
                moveMouse(base[0]+(x*10), base[1]+(y*10))
                leftClick()#time.sleep(.5)
    elif pos == 2:
        base = (350, 175)
        for x in range(0, 3):
            for y in range(0, 3):
                moveMouse(base[0]+(x*10), base[1]+(y*10))
                leftClick()#time.sleep(.5)
    elif pos == 3:
        base = (600, 175)
        for x in range(0, 3):
            for y in range(0, 3):
                moveMouse(base[0]+(x*10), base[1]+(y*10))
                leftClick()#time.sleep(.5)
    elif pos == 4:
        base = (825, 200)
        for x in range(0, 7):
            for y in range(0, 7):
                moveMouse(base[0]+(x*10), base[1]+(y*10))
                #time.sleep(.5)
                leftClick()

######Key clicker:
def kbd():
    import pynput
    def on_press(key):
        if key == pynput.keyboard.KeyCode.from_char('1'):
            print(key)
            clicker(1)
        elif key == pynput.keyboard.KeyCode.from_char('2'):
            print(key)
            clicker(2)
        elif key == pynput.keyboard.KeyCode.from_char('3'):
            print(key)
            clicker(3)
        elif key == pynput.keyboard.KeyCode.from_char('4'):
            print(key)
            clicker(4)
    l = pynput.keyboard.Listener(on_press = on_press)
    l.start()



from PIL import Image

def compute_average_image_color(img):
    width, height = img.size

    r_total = 0
    g_total = 0
    b_total = 0

    count = 0
    for x in range(0, width):
        for y in range(0, height):
            r, g, b = img.getpixel((x,y))
            r_total += r
            g_total += g
            b_total += b
            count += 1

    return (r_total/count, g_total/count, b_total/count)

#screenGrab(delay = 3)

##Begin the game logic:
#Start the game:
capPos = (310, 478, 641, 533)
def loadImages():
    i = []
    for x in range(0, 1000):
        try:
            i.append(PIL.Image.open(str(x)+'.jpg'))
        except FileNotFoundError:
            pass
    return i
time.sleep(1.5)
print("starting game...")
moveMouse(650, 35)
leftClick()
######Start game loop:
#time.sleep(5)#Wait for timer and opening animation
#kbd()
images = loadImages()
while True:
    #print("Taking screen shot..")
    test = screenGrab()
    test = test.crop(capPos)
    test.save('tmp.jpg')
    test = Image.open('tmp.jpg')
    c = compute_average_image_color(test)
    if test in images:
        pass
    elif c[1] > 70:
        pass
    else:
        print("NewPicture")
        test.save(str(len(images)+1)+'.jpg')
        images.append(PIL.Image.open(str(len(images)+1)+'.jpg'))
        images = loadImages()
    
