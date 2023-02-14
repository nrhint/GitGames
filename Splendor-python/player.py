##Nathan Hinton
##This file will hold the player information

import pygame

import myEvents
from items import Token
from dataManipulation import colorToIndex

##Load player assets
playerItemsGroup = pygame.sprite.Group()

##Place the player layout:
tmp = pygame.sprite.Sprite(playerItemsGroup)
tmp.image = pygame.image.load("images/player/drawTokens.png")
tmp.rect = pygame.Rect(600, 550, 80, 40)
tmp.action = "Draw"
tmp = pygame.sprite.Sprite(playerItemsGroup)
tmp.image = pygame.image.load("images/player/buyCard.png")
tmp.rect = pygame.Rect(600, 600, 80, 40)
tmp.action = "Buy"
tmp = pygame.sprite.Sprite(playerItemsGroup)
tmp.image = pygame.image.load("images/player/endTurn.png")
tmp.rect = pygame.Rect(600, 650, 80, 40)
tmp.action = "End turn"

tmp = Token("green", playerItemsGroup)
tmp.rect = pygame.Rect(110, 525, 80, 80)
tmp = Token("red", playerItemsGroup)
tmp.rect = pygame.Rect(210, 525, 80, 80)
tmp = Token("white", playerItemsGroup)
tmp.rect = pygame.Rect(310, 525, 80, 80)
tmp = Token("blue", playerItemsGroup)
tmp.rect = pygame.Rect(110, 612, 80, 80)
tmp = Token("brown", playerItemsGroup)
tmp.rect = pygame.Rect(210, 612, 80, 80)
tmp = Token("gold", playerItemsGroup)
tmp.rect = pygame.Rect(310, 612, 80, 80)


class Player:
    def __init__(self, name):
        self.whiteTokens = 0
        self.greenTokens = 0
        self.blueTokens = 0
        self.redTokens = 0
        self.brownTokens = 0
        self.goldTokens = 0
        self.currentTokens = [self.whiteTokens, self.greenTokens, self.blueTokens, self.redTokens, self.brownTokens, self.goldTokens]
        self.cards = []
        self.status = None
        self.name = name
        self.points = 0
        self.totalTokensDrawn = 0

    def buyCard(self, cardToPurchase, tokens):
        if "Buying" == self.status and None == cardToPurchase.owner and self.totalTokensDrawn == 0: #Just a safety check that no one owns the card
            success = True #assume success
            ##Check for enough money
            cardTokens = [0, 0, 0, 0, 0]
            for card in self.cards:
                cardTokens[colorToIndex[card.cardInfo.tokenColor]] += 1
            
            #Find out how many tokens are needed and maybe use gold if needed
            tmpGold = self.currentTokens[-1]
            resultingTokens = []
            for x in range(0, 5):
                resultingTokens.append((cardTokens[x]+self.currentTokens[x])-cardToPurchase.cardInfo.costs[x])
            while min(resultingTokens) < 0 and True == success:
                if tmpGold > 0:
                    resultingTokens[resultingTokens.index(min(resultingTokens))] += 1
                    tmpGold -= 1
                else:
                    success = False
                    print("Tried to use Gold but not enough tokens :(")
            if True == success:
                self.currentTokens[-1] = tmpGold
                cost = cardToPurchase.cardInfo.costs
                for token in tokens:
                    token.count += self.currentTokens[colorToIndex[token.name]]
                for x in range(0, 5):#Loop through and subtract the tokens used starting with cards
                    cost[x] -= cardTokens[x]
                    if cost[x] > 0:
                       self.currentTokens[x] = max(self.currentTokens[x]-cost[x], 0)
                self.whiteTokens, self.greenTokens, self.blueTokens, self.redTokens, self.brownTokens, self.goldTokens = self.currentTokens
                self.cards.append(cardToPurchase)
                cardToPurchase.owner = self
                self.points += cardToPurchase.cardInfo.pointValue
                self.update("End turn")
                return True
            else:
                return False
        else:
            if self.totalTokensDrawn != 0:
                print("Are you trying to cheat? You already drew tokens this turn")
                self.update("End turn")
            print("You are not in buying mode. Please press the buy button")
            return False

    def drawNewToken(self, token):
        if 0 == token.count:
            print("Not enough tokens remaining")
        elif sum(self.currentTokens) < 10 and self.totalTokensDrawn < 3:
            if 'gold' == token.name:
                if 0 != self.totalTokensDrawn:
                    print("You can only draw a gold in place of 3 tiles...")
                    return False
                else:
                    self.totalTokensDrawn = 1

            self.currentTokens[colorToIndex[token.name]] += 1
            self.whiteTokens, self.greenTokens, self.blueTokens, self.redTokens, self.brownTokens, self.goldTokens = self.currentTokens
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
