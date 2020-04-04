##Nathan Hinton
##This is the main file for the population game

####Features:
##Auto save
##Output information to a file to allow continuation
##Maybe create graphs

####Vars:
##maxFoodSupply
##baseDeathRate
##
##
##
##
##
##

from time import sleep #This will be used to limit the output speed of the lines
from random import randint
import matplotlib.pyplot as plt

class World:
    def __init__(self, population = 400, maxPopulation = 1000000,
                 deathRate = 0.01, minDeathRate = 0.5,
                 populationGrowthRate = 2, diseaseChance = 0, food = 800,
                 maxFood = 1000000, foodConsumptionRate = 1,
                 foodGrowthRate = 3):
        
        self.population = population #This is the starting population
        self.maxPopulation = maxPopulation
        self.deathRate = deathRate #This is the death rate
        self.minDeathRate = minDeathRate #The death rate will never go below this
        self.populationGrowthRate = populationGrowthRate #This is the population growth rate
        self.maxPopulationGrowthRate = populationGrowthRate
        self.diseaseChance = diseaseChance #This is a number between 0 and 1000 where 0 is never and 1000 is alwyas

        self.food = food #This is the starting amount of food
        self.maxFood = maxFood
        self.foodConsumptionRate = foodConsumptionRate #This is the ratio at which the people eat the food
        self.foodGrowthRate = foodGrowthRate #This is the base food growth rate
        self.loop = True

    def showPopPlot(self):
        plt.plot(self.generations, self.populationHistory)
        plt.xlabel('time')
        plt.ylabel('population')
        plt.show()

    def plotAll(self):
        plt.plot(self.generations, self.populationHistory)
        plt.plot(self.generations, self.foodHistory)
        #plt.plot(self.generations, self.deathHistory)
        plt.xlabel('time')
        plt.ylabel('population, food and deaths')
        plt.show()

    def updatePopulation(self):
        self.population = self.populationGrowthRate*self.population
        self.deaths = self.population*self.deathRate
        self.population = self.population-self.deaths

    def updateFood(self):
        self.consumption = self.population*self.foodConsumptionRate
        self.food = self.food*self.foodGrowthRate+(self.population/1)
        self.food = self.food-self.consumption
        if self.food > self.maxFood:
            self.food = self.maxFood/randint(1, 5)
        elif self.food <= 0:
            self.food = 0
        if self.food < self.population:
            self.deathRate = self.deathRate*4
            self.populationGrowthRate = self.populationGrowthRate/2
        else:
            if self.deathRate > self.minDeathRate:
                self.deathRate/8
                self.populationGrowthRate = self.maxPopulationGrowthRate

    def run(self):
        ##Here is the main program for the sim
        self.generation = 0
        self.event = ''
        self.populationHistory = []
        self.foodHistory = []
        self.deathRateHistory = []
        self.deathHistory = []
        self.generations = []
        while self.loop == True:
            self.generation += 1
            ##Use a random number to decide events:
            self.randNum = randint(1, 1000)
            #Is there a desease?
            if self.randNum <= self.diseaseChance:
                self.event += 'disease'
                self.eathRate = self.deathRate*32
                print("THERE IS A DISEASE!!! THE DEATH RATE IS NOW: %s"%self.deathRate)

            self.updateFood()
            self.updatePopulation()
            ##Set limits:
            if self.population < 1:
                print("Everybody died...")
                self.loop = False
            if self.event != '':
                print(self.generation)
                print(self.event)
                self.event = ''
            self.populationHistory.append(self.population)
            self.foodHistory.append(self.food)
            self.deathRateHistory.append(self.deathRate)
            self.deathHistory.append(self.deaths)
            self.generations.append(self.generation)
            if self.generation == 500:
                self.loop = False

        self.plotAll()

world = World()
world.run()
