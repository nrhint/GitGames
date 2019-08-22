##Nathan Hinton
#Idea from Utah state camp.
#Trying to solve using simple functions and recursive calling.


cnt = 0
file = 'File1.txt'
numbers = [1, 2, 3, 4, 5, 6, 7, 8, 9]
cols = [0, 1, 2, 3, 4, 5, 6, 7, 8]
rows = [0, 9, 18, 27, 36, 45, 54, 63, 72]
rawData = open(file, 'r').read()
data = []
x = 0
for number in rawData:
    try:
        if int(number) in numbers:
            data.append(int(number))
            x +=1
    except ValueError:
        if number == ' ' or number == ',' or number == ', ' or number == '\n':
            pass
        else:
            data.append(0)
            x +=1

def isCorrect(data):
    for pos in cols:
        colDat = []
        for x in range(81):
            if x%9 == pos:
                if data[x] != 0:
                    colDat.append(data[x])
        if len(colDat) != len(set(colDat)):
            return False
    for pos in rows:
        rowDat = []
        for x in range(9):
            if data[pos+x] != 0:
                rowDat.append(data[pos+x])
        if len(rowDat) != len(set(rowDat)):
            return False
    return True

def solve(data, position, sol):
    print()
    #print(sol)
    global cnt
    cnt += 1
    if isCorrect(sol) == False:
        return False
    if 0 not in sol:
        print(sol)
        return True
    elif data[position] != 0:#check if the position is empty
        solve(data, position+1, sol)
    else:#Pick a number for the place.
        for x in numbers:
            sol[position] = x
            solve(data, position+1, sol)    
dataOrig = data
solve(data, 0, data)
