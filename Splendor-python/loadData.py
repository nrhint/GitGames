##Nathan Hinton
##This will load data from files and prepare it for usage by the program

import yaml

class CardInfo:
    def __init__(self, pointValue, tokenColor, whiteCost, greenCost, blueCost, redCost, brownCost):
        self.pointValue = pointValue
        self.tokenColor = tokenColor
        self.whiteCost = whiteCost
        self.greenCost = greenCost
        self.blueCost = blueCost
        self.redCost = redCost
        self.brownCost = brownCost
        self.costs = [whiteCost, greenCost, blueCost, redCost, brownCost]

##Parse the card file and load the data into a list
with open("cards.yaml", 'r') as yamlFile:
    try:
        data = yaml.safe_load(yamlFile)
        # print(data)
        remainingCards = []
        for level in data:
            remainingCards.append([])
            for card in level:
                newCard = CardInfo(card[0], card[1], card[2], card[3], card[4], card[5], card[6])
                remainingCards[-1].append(newCard)

    except yaml.YAMLError as exception:
        print("YAML ERROR WHILE LOADING FILE:")
        print(exception)
        raise Exception

##Load nobles:
class NobleInfo:
    def __init__(self, pointValue, whiteCost, greenCost, blueCost, redCost, brownCost):
        self.pointValue = pointValue
        self.whiteCost = whiteCost
        self.greenCost = greenCost
        self.blueCost = blueCost
        self.redCost = redCost
        self.brownCost = brownCost

with open("nobles.yaml", "r") as yamlFile:
    try:
        data = yaml.safe_load(yamlFile)
        nobles = []
        for noble in data:
            nobles.append(NobleInfo(noble[0], noble[1], noble[2], noble[3], noble[4], noble[5]))
    except yaml.YAMLError as exception:
        print("YAML ERROR WHILE LOADING FILE:")
        print(exception)
        raise Exception