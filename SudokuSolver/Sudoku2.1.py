##Nathan Hinton
##This is for solving sudoku problems

#The file will be loaded as a long list of 81 numbers or spaces
from time import time
start = time()
file = 'easy2.puzzle'
numbers = [1, 2, 3, 4, 5, 6, 7, 8, 9]
rawData = open(file, 'r').read()

#replce all blanks with a '0':
rawData = rawData.replace('n', '0')
print(rawData)
rawData = rawData.replace('\n', '')#Remove any newlines

#Quick sanity check:
if len(rawData) != 81:
    print("!!!WARNING!!! \n This fle contains the wron number of spaces and numbers!")

#convert string to list:
data = []
for x in rawData:
    data.append(int(x))
origData = data.copy()
##########################################################
def returnRow(lst, pos):
    return lst[int(pos/9)*9 : (int(pos/9)+1)*9]
def returnCol(lst, pos):
    pos = pos%9
    newLst = []
    for x in range(0, 80, 9):newLst.append(lst[pos+x])
    return newLst
def returnBox(lst, pos):
    row = pos//3
    col = pos%9//3
    col = col*3
    rows = []
    newLst = []
    for x in range(0, 3):
        rows += returnRow(lst, (row*3)+x*9)
##    print(rows)
    for x in range(0, 3):
        newLst += rows[col:col+3]
##        print(col, rows[col:col+3])
        col +=9
    return newLst
def returnNums(lst):
    newLst = []
    for num in lst:
        if num != 0:
            newLst.append(num)
    return newLst
def checkPuzzle(lst):
    #Check the row:
    for x in range(0, 9):
        nums = returnRow(lst, x*10)
        nums = returnNums(nums)
        for num in numbers:
            if nums.count(num) > 1:
##                print("Failed @ pos: %s with number %s"%(x*10, num))
                return False
    #Check the col:
    for x in range(0, 9):
        nums = returnCol(lst, x*10)
        nums = returnNums(nums)
        for num in numbers:
            if nums.count(num) > 1:
##                print("Failed @ pos: %s with number %s"%(x*10, num))
                return False
    #Check the box:
    for x in range(0, 9):
        nums = returnBox(lst, x*10)
        nums = returnNums(nums)
        for num in numbers:
            if nums.count(num) > 1:
##                print("Failed @ pos: %s with number %s"%(x*10, num))
                return False
    return True
def tryNum(start):
    newStart = start.copy()
##    print(newStart)
    if checkPuzzle(newStart):
        i = newStart.index(0)
        for x in numbers:
            newStart[i] = x
            tryNum(newStart)
##########################################################
tryNum(data)
end = time()
print("Program took %s seconds"%(end-start))
print()
print()
