##Nathan Hinton
##This is for solving sudoku problems

#The file will be loaded as a long list of 81 numbers or spaces
from time import time
start = time()
file = 'medium1.puzzle'
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
def row(pos, puzzle):
    return puzzle[int(pos/9)*9 : (int(pos/9)+1)*9]
def col(pos, puzzle):
    dat = []
    pos = pos%9
    for x in range(0, 9):
        dat.append(puzzle[pos])
        pos += 9
    return dat
def box(pos, puzzle):
    dat = []
    rowNum = pos//27
    col = pos%9//3
    rows = row(rowNum*3*9, puzzle)+row((rowNum*3+1)*9, puzzle)+row((rowNum*3+2)*9, puzzle)
    for x in range(len(rows)):
        if x%9//3 == col:
            dat.append(rows[x])
    return dat
def returnNums(data):
    dat = []
    for x in data:
        if x != 0:
            dat.append(x)
    return dat
def checkPuzzle(puzzle):
    pos=0
    for x in puzzle:
        curValue = x
        if x == 0:
            pass
        else:
            if row(pos, puzzle).count(x) > 1:
                return False
            if col(pos, puzzle).count(x) > 1:
                return False
##            if box(pos, puzzle).count(x) > 1:
##                return False
        pos +=1
    return True
def checkPuzzle2(puzzle):
    pos=0
    for x in puzzle:
        curValue = x
        if x == 0:
            pass
        else:
            if row(pos, puzzle).count(x) > 1:
                return False
            if col(pos, puzzle).count(x) > 1:
                return False
            if box(pos, puzzle).count(x) > 1:
                return False
        pos +=1
    return True
def pretyPrint(puzzle):
    print(row(0, puzzle))
    print(row(9, puzzle))
    print(row(18, puzzle))
    print('---------------------------')
    print(row(27, puzzle))
    print(row(36, puzzle))
    print(row(45, puzzle))
    print('---------------------------')
    print(row(54, puzzle))
    print(row(63, puzzle))
    print(row(73, puzzle))
    #print('---------------------------')
solved = []
def solve(board, sln):
    try:
        pos = sln.index(0)
    except ValueError:
        print(sln)
        solved.append(sln.copy())
        return True
    for x in numbers:
        sln[pos] = x
        if checkPuzzle2(sln):
            #print("Trying %s in %s"%(x, pos))
            solve(board, sln)
##            print(sln)
        else:
            #print("Failed pos %s with number %s"%(pos, x))
##            print(sln)
            sln[pos] = 0
solve(data, data)
finalData = []
for l in solved:
    if checkPuzzle2(l):
        finalData.append(l)
pretyPrint(data)
end = time()
print("Program took %s seconds"%(end-start))
print()
print()
print(finalData)
print()
print()
for s in finalData:
    print()
    print()
    print()
    pretyPrint(s)

print("There are %s combonations"%len(finalData))
