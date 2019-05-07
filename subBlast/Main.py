##Nathan Hinton
##This should play subtraction blast.

print("loading...")
screen = (130, 220, 680, 500)

from pynput import mouse, keyboard
import pytesseract
from PIL import Image
import pyscreenshot as ss
import time

#Methode of playing:
#Make it search a grid for the numbers.

print("finished importing")

its = pytesseract.image_to_string
path = '/home/nathan/Pictures/Screenshot from 2019-05-06 15-20-55.png'
time.sleep(2)
im = Image.open(path)#ss.grab()
#im.show()
finished = im.crop(screen)
finished.show()

#check in a grid pattern for numbers:
for x in range(1, 11):
    for y in range(1, 
    its(finished.crop(((x-1)*50, 0, x*50, 50)))

##(132, 166)
##(681, 715)
