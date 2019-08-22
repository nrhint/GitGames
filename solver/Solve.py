##Nathan Hinton



def test(rowCol):
    box = []
##    for x in finalLst:#Find numbers in the box
    if 0 <= rowCol[0] <= 2:
        l = finalLst[0:3]
        print(l)
        if 0 <= rowCol[1] <= 2:
            for x in l:box+=x[0:3]
        elif 3 <= rowCol[1] <= 5:
            for x in l:box+=x[3:6]
        else:#It is on the last COL:
            for x in l:box+=x[6:9]
##############
    elif 3 <= rowCol[0] <= 5:
        l = finalLst[3:6]
        if 0 <= rowCol[1] <= 2:
            for x in l:box+=x[0:3]
        elif 3 <= rowCol[1] <= 5:
            for x in l:box+=x[3:6]
        else:#It is on the last COL:
            for x in l:box+=x[6:9]
##############
    else:#It is on the last ROW:
        l = finalLst[6:9]
        if 0 <= rowCol[1] <= 2:
            for x in l:box+=x[0:3]
        elif 3 <= rowCol[1] <= 5:
            for x in l:box+=x[3:6]
        else:#It is on the last COL:
            for x in l:box+=x[6:9]
    print(box)





##TO DO:
from time import sleep

file = 'File0.txt'
numbers = [1, 2, 3, 4, 5, 6, 7, 8, 9]
ignore = ['n', 'm']
waitTime = 0

state = 'init'
while state != 'solved':
    if state == 'init':
        rowCol = (0, 0)
        rawData = "ERROR!"
        try:
            rawData = open(file, 'r').read()
        except FileNotFoundError:
            print("File not found. check the name.")
        #Load data to list:
        tempLst = []
        finalLst = []
        for i in rawData:
            if i in ignore:
                tempLst.append(0)
            elif i == ' ':
                pass
            elif i == '\n':
                finalLst.append(tempLst)
                tempLst = []
            else:
                tempLst.append(int(i))
        tempLst = []
        lastSolved = None
        state = 'findSpace'
################################################                
    elif state == 'findSpace':
        if rowCol == lastSolved:
            state = 'failed'
        row = finalLst[(rowCol[0]+1)%9]
        for x in range(len(rowCol[1::])):
            if row[x] == 0:
                rowCol = ((rowCol[0]+1)%9, x)
                state = 'solveSpace'
            else:
                state = 'change'
        #print(rowCol)
################################################                
    elif state == 'solveSpace':
        subState = 'methodA'
        while state == 'solveSpace':
            ################################################
            count = 0
            if subState == 'methodA':#Check row and colum against possible numbers
                if finalLst[rowCol[0]][rowCol[1]] != 0:
                    print("Space (%s, %s) already solved"%rowCol)
                    subState = 'solved'
                notNums = []
                for x in finalLst[rowCol[0]]:#Find numbers in row
                    if x != 0:
                        notNums.append(x)
                for x in finalLst:#Find numbers in colum
                    if x[rowCol[1]] != 0:
                        if x[rowCol[1]] not in notNums:
                            notNums.append(x[rowCol[1]])
                ##############
                box = []
                if 0 <= rowCol[0] <= 2:
                    l = finalLst[0:3]
                    if 0 <= rowCol[1] <= 2:
                        for x in l:box+=x[0:3]
                    elif 3 <= rowCol[1] <= 5:
                        for x in l:box+=x[3:6]
                    else:#It is on the last COL:
                        for x in l:box+=x[6:9]
            ##############
                elif 3 <= rowCol[0] <= 5:
                    l = finalLst[3:6]
                    if 0 <= rowCol[1] <= 2:
                        for x in l:box+=x[0:3]
                    elif 3 <= rowCol[1] <= 5:
                        for x in l:box+=x[3:6]
                    else:#It is on the last COL:
                        for x in l:box+=x[6:9]
            ##############
                else:#It is on the last ROW:
                    l = finalLst[6:9]
                    if 0 <= rowCol[1] <= 2:
                        for x in l:box+=x[0:3]
                    elif 3 <= rowCol[1] <= 5:
                        for x in l:box+=x[3:6]
                    else:#It is on the last COL:
                        for x in l:box+=x[6:9]
                cnt = 0
                for num in box:####Filter the 0's out of box
                    if num == 0:cnt += 1
                for d in range(cnt):
                    box.remove(0)
                #print(box)
                notNums += box
                #print(notNums)
                if len(notNums) < len(numbers) -1:
                    subState = 'failed'
                else:
                    notNums.sort()
                    for x in range(len(numbers)):
                        if notNums[x] != numbers[x]:
                            number = numbers[x]
                            break
                    #print(rowCol)
                    #print(number)
                    finalLst[rowCol[0]][rowCol[1]] = number
                    lastSolved = rowCol
                    subState = 'solved'
                    print('solved a space...')
            ################################################
            elif subState == 'failed' or count > 9:
                state = 'change'
                break
            ################################################
            elif subState == 'solved':
                state = 'findSpace'
            else:
                print('SUBsTATE ERROR! Not matching subState for state %s'%subState)
            #print(subState)
            sleep(1)
            count += 1
    elif state == 'change':
        if rowCol[1] == 8:
            rowCol = ((rowCol[0]+1)%9, 0)
        else:
            rowCol = (rowCol[0], (rowCol[1]+1)%9)
        state = 'findSpace'
    elif state == 'failed':
        print("Program failed to solve this puzzle.")
    else:
        print('STATE ERROR! Not matching state for state %s'%state)
        break
    #print(state)
    sleep(waitTime)
################################################                
