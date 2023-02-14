##Nathan Hinton

class Player:
    def __init__(self, name):
        self.inventory = []
        self.name = name
        self.status = None
        self.health = 0
        #Equipment will be in this format:
        #Head, Chest, Right arm, Right hand, Item in right hand, Left arm, Left hand, Item in left hand, Belt?, Legs, Shoes
        self.equipment = []