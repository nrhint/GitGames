##Nathan Hinton
##This file will hold the player information

import pygame
import myEvents

class Player:
    def __init__(self):
        self.whiteTokens = 0
        self.greenTokens = 0
        self.blueTokens = 0
        self.redTokens = 0
        self.brownTokens = 0
        self.goldTokens = 0
        self.cards = None
        self.action = "Error"

    def update(self, activePlayer, card = None):
        if self == activePlayer:
            if self.action == "End turn":
                pygame.event.post(myEvents.END_TURN_EVENT)
            elif self.action == "Buy" and card != None:
                pass
            elif self.action == "Draw":
                pass
            else:
                print("Player action error. Requested action: %s"%self.action)
        else:
            pass ##This action is not for me