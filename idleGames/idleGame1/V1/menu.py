##Nathan Hinton
##This is the menu area It is responsible for loading games from files.

####Here is an example of the file formatting:
## moneyList
## duplicateChance
## maxMoneyCount
## minMoneyValue
## StartMoneyValue
## timeoutGainPercent
## playerMoney
## 
## 
## 
## 
## 
## 
## 
## 

import pygame
from loadSave import *

COLOR_INACTIVE = pygame.Color('white')
COLOR_ACTIVE = pygame.Color('black')
font = pygame.font.Font(None, 32)


class InputBox:

    def __init__(self, x, y, w, h, text=''):
        self.rect = pygame.Rect(x, y, w, h)
        self.color = COLOR_INACTIVE
        self.text = text
        self.txt_surface = font.render(text, True, self.color)
        self.active = False

    def handleEvent(self, event):
        if event.type == pygame.MOUSEBUTTONDOWN:
            # If the user clicked on the input_box rect.
            if self.rect.collidepoint(event.pos):
                # Toggle the active variable.
                self.active = not self.active
            else:
                self.active = False
            # Change the current color of the input box.
            self.color = COLOR_ACTIVE if self.active else COLOR_INACTIVE
        if event.type == pygame.KEYDOWN:
            if self.active:
                if event.key == pygame.K_RETURN:
                    print(self.text)
                    return self.text
                    self.text = ''
                elif event.key == pygame.K_BACKSPACE:
                    self.text = self.text[:-1]
                else:
                    self.text += event.unicode
                # Re-render the text.
                self.txt_surface = font.render(self.text, True, self.color)
        return False

    def update(self):
        # Resize the box if the text is too long.
        width = max(200, self.txt_surface.get_width()+10)
        self.rect.w = width

    def draw(self, screen):
        # Blit the text.
        screen.blit(self.txt_surface, (self.rect.x+5, self.rect.y+5))
        # Blit the rect.
        pygame.draw.rect(screen, self.color, self.rect, 2)

class StartMenu:
    def __init__(self, screen):
        self.screen = screen
        self.playerName = InputBox(100, 100, 200, 40, 'Player name: ')
    def run(self, event):
        data = self.playerName.handleEvent(event)
        if data != False:
            return data[13:]
        else:
            return False
    def update(self):
        self.playerName.update()
        self.playerName.draw(self.screen)

class UpgradeMenu:
    def __init__(self, screen, playerName):
        self.screen = screen
        self.active = False
        self.moreMoney = Button(25, 25, 375, 40, text = 'Increase money spawn amount', active = self.active)
        self.close = Button(screen.get_width()-100, 10, 80, 40, text = 'close', active = self.active, color = (200, 0, 0))
        self.playerName = playerName
        self.data = loadData(self.playerName)
    def run(self, events):
        for event in events:
            if self.moreMoney.run(event):
                if self.data[-1] < 10*self.data[5]:
                    print("Upgrade bought! increased money starting amount.")
                    self.data[5] += 10
                    self.data[7] -= 10*self.data[5]
                    saveData(self.data)
                    self.reloadData()
                else:
                    print("NOT ENOUGHT MONEY! you need %s"%(10*self.data[5]))
            elif self.close.run(event):
                self.changeState()
                return True
        return False
    def update(self):
        if self.active:
            self.moreMoney.update(self.screen)
            self.close.update(self.screen)
    def changeState(self):
        self.active = not self.active
        self.moreMoney.active = self.active
        self.close.active = self.active
    def reloadData(self):
        self.data = loadData(self.playerName)
        
    
class Button:
    def __init__(self, x, y, width, height, text = '', active = True, color = (100, 50, 200)):
        self.rect = pygame.Rect(x, y, width, height)
        self.active = active
        self.color = color
        self.textRender = font.render(text, True, (0, 0, 0))
    def run(self, event):
        if self.active:
            if event.type == pygame.MOUSEBUTTONDOWN:
                if self.rect.collidepoint(event.pos):
                    return True
                else:
                    return False
    def update(self, screen):
        if self.active:
            pygame.draw.rect(screen, self.color, self.rect)
            screen.blit(self.textRender, (self.rect.x+5, self.rect.y+5))
    def changeState(self):
        self.active = not self.active
