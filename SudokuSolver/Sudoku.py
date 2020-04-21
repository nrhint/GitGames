##Nathan Hinton
##This is for solving sudoku problems

#The file will be loaded as a long list of 81 numbers or spaces

file = 'easy3.puzzle'
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
puzzle = []
for x in rawData:
    puzzle.append(int(x))

def checkPuzle(puzzle):
    pos=0
    for x in puzzle:
        nums = returnNums(row(pos)+col(pos))
        for num in nums:
            if nums.count(num) > 1:
                print("PUZZLE INVALID!")
                return False
        pos +=1

def row(pos):
    return puzzle[int(pos/9)*9 : (int(pos/9)+1)*9]
def col(pos):
    dat = []
    pos = pos%9
    for x in range(0, 9):
        dat.append(puzzle[pos])
        pos += 9
    return dat
def returnNums(data):
    dat = []
    for x in data:
        if x != 0 and x not in dat:
            dat.append(x)
    return dat
def box(pos):
    dat = []
    rowNum = pos//27
    col = pos%9//3
    rows = row(rowNum*3*9)+row(rowNum*3+1*9)+row(rowNum*3+2*9)
    for x in range(len(rows)):
        if x%9//3 == col:
            dat.append(rows[x])
    return dat
def pretyPrint(puzzle):
    print(row(0))
    print(row(9))
    print(row(18))
    print('---------------------------')
    print(row(27))
    print(row(36))
    print(row(45))
    print('---------------------------')
    print(row(54))
    print(row(63))
    print(row(73))
    print('---------------------------')
def simpleSove(puzzle):
    solved =False
    blanks = []
    while solved == False:
        lastBlankNum = len(blanks)
        blanks = []
        pos = 0
        for x in puzzle:
            if x == 0:
                blanks.append(pos)
            pos +=1
##        print("Blanks: "+str(blanks))
##        for x in blanks:
##            print(puzzle[x])
        for pos in blanks:
            if len(returnNums(row(pos)+col(pos)+box(pos))) == 8:
                print("Solvable space...")
                for num in numbers:
                    if num not in returnNums(row(pos)+col(pos)+box(pos)):
                        puzzle[pos] = num
                        print(pos, num)
                        break
##            print(pos)
        if len(blanks) == lastBlankNum:
            if len(blanks) > 0:
                solved = 'not finished'
            else:
                solved = True
    print(solved)
##        input()
##po = puzzle.copy()
simpleSove(puzzle)
pretyPrint(puzzle)
