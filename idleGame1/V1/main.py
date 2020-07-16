##Nathan Hinton
##This is an idle game that I am building

from time import time, sleep

programStart = time()

import pygame
##import pickle
from money import *
from menu import *
from loadSave import *

##Vars that you can change:
fps = 60 #This will dirrectly change the hardness of the game.
step = 1/fps #Using time to delay if needed
width, height = 800, 600
##duplicateChance = 50
##maxMoneyCount = 10
##minMoneyValue = 50
##startMoneyValue = 100
##timeoutGainPercent = 0.50
##playerMoney = 0

pygame.init()
screen = pygame.display.set_mode((width, height))
pygame.display.set_caption("Idle Money!")

background = pygame.Surface(screen.get_size())
background = background.convert()
background.fill((0, 0, 255))

pygame.font.init() # you have to call this at the start, 
                   # if you want to use this module.
font = pygame.font.SysFont('Comic Sans MS', 30)

gameStart = time()

run = True
frameCount = 0
idleTime = 0
state = 'init'
events = []
while run == True:
    if frameCount%(fps*60) == 0:
        print("atempting autosave...")
        try:
            saveData([playerName, moneyList, duplicateChance, maxMoneyCount, minMoneyValue, startMoneyValue, timeoutGainPercent, playerMoney])
            print("Success!")
        except NameError:
            pass
    nextStep = time()+step
    for event in pygame.event.get():
        events.append(event)
    screen.blit(background, (0, 0))
    if state == 'play':
        ##Logic:
        clickPos = (-1, -1)
        if pygame.mouse.get_pressed()[0]:#If the first mouse button is pressed
            clickPos = pygame.mouse.get_pos()
        toRemove = []
        if len(moneyList) == 0:
            moneyList.append(Money(screen, startMoneyValue))
            print(moneyList)
        for money in moneyList:
            add = money.run(clickPos)
            if add != False:##If money expires or is clicked on
                toRemove.append(money)
                if add[1] == True and round(add[0]*0.9)> minMoneyValue:#clicked on:
                    moneyList.append(Money(screen, round(add[0]*0.9)))
                    playerMoney += add[0]
                elif add[1] == False and round(add[0]*0.9)> minMoneyValue:
                    moneyList.append(Money(screen, round(add[0]*0.9)))
                    playerMoney += int(timeoutGainPercent*add[0])
                if randint(0, 100) < duplicateChance and maxMoneyCount > len(moneyList):
                    moneyList.append(Money(screen, startMoneyValue))
                    playerMoney += int(timeoutGainPercent*add[0])
        for item in toRemove:
            moneyList.remove(item)
        for event in events:
            if activateUpgradeMenuButton.run(event):
                activateUpgradeMenuButton.changeState()
                upgradeMenu.changeState()
                state = 'getUpgrades'
                print(state)
        ##Show the money:
        showPlayerMoneyOnScreen = font.render("Money: %s"%playerMoney, False, (0, 0, 0))
        ##Update Screen:
        screen.blit(showPlayerMoneyOnScreen, (10, 10))
        if not activateUpgradeMenuButton.active:
            activateUpgradeMenuButton.changeState()
        activateUpgradeMenuButton.update(screen)
        for money in moneyList:
            money.update()
    elif state == 'getUpgrades':
        if upgradeMenu.active == False:
            upgradeMenu.changeState()
            saveData()
        if upgradeMenu.run(events):
            state = 'saveData'#Reload the data from the file. I need to have the file refreshed before the upgrades go.
        upgradeMenu.update()
    elif state == 'saveData':
        saveData(playerName)
        state = 'play'
        print('data saved. Back to playing.')
    elif state == 'init':
        startMenu = StartMenu(screen)
        activateUpgradeMenuButton = Button(width-150, 10, 140, 40, text = 'upgrades')
        state = 'startMenu'
        print(state)
    elif state == 'startMenu':
        for event in events:
            playerName = startMenu.run(event)
            if playerName != False:
                state = 'loadData'
                print(state)
        startMenu.update()
        pygame.display.flip()
    elif state == 'loadData':
        fileData = loadData(playerName)
        upgradeMenu = UpgradeMenu(screen, playerName)
        playerName = fileData[0]
        moneyList = []
        for item in fileData[1]:
            moneyList.append(Money(screen, value = item[0], minExpire = item[1], maxExpire = item[2]))
        duplicateChance = fileData[2]
        maxMoneyCount = fileData[3]
        minMoneyValue = fileData[4]
        startMoneyValue = fileData[5]
        timeoutGainPercent = fileData[6]
        playerMoney = fileData[7]
        state = 'play'
    else:
        print("Oops, something went wrong.")
        print("Current state is %s"%state)
        run = False
        pygame.quit()
    pygame.display.flip()
    frameCount += 1
    for event in events:
        if event.type == pygame.QUIT:
            pygame.quit()
            run = False #End the main loop
            print(run)
    events = []
    if time()<nextStep:
        idleTime += (nextStep-time())
        sleep(nextStep-time())
#pygame.quit()
end = time()
print("The time taken to load was %s seconds"%(gameStart-programStart))
print("The avg fps was %sfps"%(frameCount/(end-gameStart)))
print("Your game spent %s seconds idle of %s seconds"%(idleTime, end-programStart))
print("idle percent: "+str(round(idleTime/(end-programStart), 2)*100)+"%")
