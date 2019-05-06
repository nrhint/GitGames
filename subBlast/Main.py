##Nathan Hinton
##This should play subtraction blast.

print("loading...")
scren = (130, 160, 680, 710)

from pynput import mouse, keyboard
import pytesseract
from PIL import Image
import pyscreenshot as ss
import time

print("finished importing")

its = pytesseract.image_to_string

time.sleep(2)
im = ss.grab()
im.show()
im.crop(screen)

##(132, 166)
##(681, 715)
