##Nathan Hinton
##THis will be the threading for the server

from random import choice

class PlayerThread:
    def __init__(self, connection, eventQueue):
        self.eventQueue = eventQueue
        self.connection = connection
        self.playerName = self.eventQueue.get()
        self.targetPlayerCount = self.eventQueue.get()

    def mainloop(self):
        pass

    def shutdown(self):
        return 1

class GameThread:
    def __init__(self, targetPlayerCount, eventQueue):
        self.targetPlayerCount = targetPlayerCount
        self.eventQueue = eventQueue
        self.players = []
        self.currentTurn = 0
        if not self.waitForPlayers():
            print("Failed to get the right number of players")
            quit(0)
        else:
            self.changeTurn()

    def waitForPlayers(self):
        playerCount = 1 #Start with self as a player
        while playerCount < self.targetPlayerCount:
            if not self.eventQueue.empty():
                data = self.eventQueue.get()
                if "playerCount" in data:
                    playerCount = int(data[11:])
                else:
                    print("Recieved garbage data:")
                    print(data)
        print("Game ready!")
        return 1

    def changeTurn(self):
        self.currentTurn += 1
        self.players[self.currentTurn%len(self.players)]
        self.eventQueue.put("playerTurn%s"%len(self.players))