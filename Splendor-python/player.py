##Nathan Hinton
##This file will hold the player information

import pygame
import myEvents

colorToIndex = {
    'white':0, 
    'green':1, 
    'blue':2, 
    'red':3, 
    'brown':4,
    'gold':5
}

class Player:
    def __init__(self, name):
        self.whiteTokens = 0
        self.greenTokens = 0
        self.blueTokens = 0
        self.redTokens = 0
        self.brownTokens = 0
        self.goldTokens = 0
        self.cards = []
        self.status = None
        self.name = name
        self.totalTokensDrawn = 0

    def buyCard(self, card):
        if "Buying" == self.status and None == card.owner: #Just a safety check that no one owns the card
            success = True #assume success
            ##Check for enough money
            realTokens = [self.whiteTokens, self.greenTokens, self.blueTokens, self.redTokens, self.brownTokens]
            cardTokens = [0, 0, 0, 0, 0]
            for card in self.cards:
                cardTokens[colorToIndex[card.cardInfo.tokenColor]] += 1
            
            #Find out how many tokens are needed and maybe use gold if needed
            tmpGold = self.goldTokens
            resultingTokens = []
            for x in range(0, 5):
                resultingTokens.append((cardTokens[x]+realTokens[x])-card.cardInfo.costs[x])
            while min(resultingTokens) < 0 and True == success:
                if tmpGold > 0:
                    resultingTokens[resultingTokens.index(min(resultingTokens))] -= 1
                    tmpGold -= 1
                else:
                    success = False
                    print("Tried to use Gold but not enough tokens :(")
            if True == success:
                cost = card.cardInfo.costs
                for x in range(0, 5):#Loop through and subtract the tokens used starting with cards
                    cost[x] -= cardTokens[x]
                    if cost[x] > 0:
                        realTokens[x] -= cost[x]
                self.whiteTokens, self.greenTokens, self.blueTokens, self.redTokens, self.brownTokens = realTokens
                self.cards.append(card)
                card.owner = self
                self.update("End turn")
                return True
            else:
                return False
        else:
            print("You are not in buying mode. Please press the buy button")
            return False

    def drawToken(self, token):
        currentTokens = [self.whiteTokens, self.greenTokens, self.blueTokens, self.redTokens, self.brownTokens, self.goldTokens]
        if 0 == token.count:
            print("Not enough tokens remaining")
        elif sum(currentTokens) < 10 and self.totalTokensDrawn < 3:
            currentTokens[colorToIndex[token.name]] += 1
            self.whiteTokens, self.greenTokens, self.blueTokens, self.redTokens, self.brownTokens, self.goldTokens = currentTokens
            self.totalTokensDrawn += 1
            token.count -= 1
            return True
        else:
            print("Too many tokens, click one you have to discard first or end turn.")
        return False

    def update(self, action):
        print("Player updating")
        if action == "End turn":
            pygame.event.post(pygame.event.Event(myEvents.END_TURN_EVENT))
        elif action == "Buy":
            self.status = "Buying"
        elif action == "Draw":
            self.status = "Drawing"
        else:
            print("Player action error. Requested action: %s"%action)
