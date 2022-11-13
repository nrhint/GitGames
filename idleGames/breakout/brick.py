##Nathan Hinton

from pygame import Rect

class Brick:
    def __init__(self, x, y, width, height, value):
        self.x = x
        self.y = y
        self.width = width
        self.height = height
        self.rect = Rect(x, y, width, height)
        self.value = value
    
    def draw(self):
        pass

    def damage(self, value):
        self.value -= value
        if self.value <= 0:
            return 0
        else:
            return 1