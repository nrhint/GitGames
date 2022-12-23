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