##Nathan Hinon
##This is the player file. It will do the drawing and recognise the shapes

import pygame

##These are the tuning settings for the guessing of what shape

lineForgiveness = 50
circleForgiveness = 50

class Player:
    def __init__(self, display):
        self.display = display
        self.x = False
        self.y = False
        self.positions = []
        self.oldPositions = []
        self.size = 75
        self.color = (0, 0, 0)
        self.oldColor = (200, 200, 200)
        self.rect = ('rect', pygame.Rect(0, 0, 0, 0))
    def run(self):
        self.x, self.y = pygame.mouse.get_pos()
        if pygame.mouse.get_pressed() == (1, 0, 0):
            self.positions.append((self.x, self.y))
        elif pygame.mouse.get_pressed() == (0, 0, 0):
            if self.positions != []:
                self.rect = guessShape(self.positions)
                self.oldPositions += guessShape(self.positions)
                self.positions = []
    def update(self):
        for item in self.oldPositions:
            if item[0] == 'rect':
                pygame.draw.rect(self.display, (0, 255, 0), item[1])
            elif item[0] == 'circle':
                pygame.draw.ellipse(self.display, (125, 0, 125), item[1])
        for position in self.positions:
            pygame.draw.ellipse(self.display, self.color, pygame.Rect(position[0], position[1], self.size, self.size))


def guessShape(positions):
    shapeMax = max(positions)
    shapeMin = min(positions)
    shapeMid = mode(positions)
    print(shapeMax, shapeMid, shapeMin)
    if diff(shapeMax[0], shapeMid[0])+diff(shapeMid[0], shapeMin[0]) < lineForgiveness:
        print("Vertical line")
        return ('rect', pygame.Rect(positions[0], (diff(positions[0][0], positions[-1][0]), diff(positions[0][1], positions[-1][1]))))
    elif diff(shapeMax[1], shapeMid[1])+diff(shapeMid[1], shapeMin[1]) < lineForgiveness:
        print("Horizontal line")
        return ('rect', pygame.Rect(positions[0], (diff(positions[0][0], positions[-1][0]), diff(positions[0][1], positions[-1][1]))))
    elif diff(positions[0][0], positions[-1][0]) < circleForgiveness and diff(positions[0][1], positions[-1][1]) < circleForgiveness:
        print("circle")
        print(positions[0], (diff(positions[0][0], positions[-1][0]), diff(positions[0][1], positions[-1][1])))
        return ('circle', pygame.Rect(positions[0], (diff(positions[0][0], positions[-1][0]), diff(positions[0][1], positions[-1][1]))))
    else:
        print("Failed to reccognise...")
    return ('rect', pygame.Rect(0, 0, 0, 0))

def mode(data):
    return data[round(len(data)/2)]
def diff(x, y):
    return abs(x-y)
