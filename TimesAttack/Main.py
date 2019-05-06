##Nathan Hinton
##Times attack like game.

from random import randint as rint
from time import sleep

class Player:
    def __init__(self, saveFile):
        self.file = saveFile
        self.limit = 2
        self.load()
    def end(self):
        pass
    def load(self):
        try:
            self.f = open(self.file, 'r').read()
        except FileNotFoundError:
            open(self.file, 'w')
            self.f = open(self.file, 'r').read()
        if self.f == '':
            pass
        else:
            pass
        

class Game:
    def __init__(self):
        self.player = None
    def loadPlayer(self, playerFile):
        pass
    def killPlayer(self):
        pass
    def genProblem(self):
        a = rint(1, self.player.limit)
        b = rint(1, self.player.limit)
        return a, b
    def runProblem(self):
        a, b = self.genProblem()
        print('%s * %s' %(a, b))
        ans = int(input())
        if ans == a*b:
            print('Correct!')
        else:
            print('Incorect.')
        print()
        print()
        
#####LOAD TEST DATA#####

nathan = Player('null')
game = Game()

game.player = nathan
game.runProblem()
