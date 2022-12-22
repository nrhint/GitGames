##Nathan Hinton
##This will be for the items that can be clicked on

class Item:
    def __init__(self, screen, rect, image = None):
        if None != image:
            self.image = image
        else:
            self.image = None
        