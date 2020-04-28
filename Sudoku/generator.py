##Nathan Hinton
##This program will generate sudoku puzzles

from random import choice

NUMBERS = [1, 2, 3, 4, 5, 6, 7, 8, 9]

puzzle = []

##Generate the puzzle:
a, b, c, d, e, f, g, h, i = [], [], [], [], [], [], [], [], []#These are the lists of the vertical used numbers

for y in range(0, 9):
    print(y)
    aChoices = []
    for number in NUMBERS:
        if number not in a:
            aChoices.append(number)
    a.append(choice(aChoices))
