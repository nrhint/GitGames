##Nathan Hinton
##

file = 'File1.txt'
numbers = ['1', '2', '3', '4', '5', '6', '7', '8', '9']

def loadData(file):
    rawData = "ERROR!"
    try:
        rawData = open(file, 'r').read()
    except FileNotFoundError:
        print("File not found. check the name.")
    return rawData
def processData(rawData):#I am going to store the numbers in a map so that I can have the program track guesses.
    data = {}
    x = 0
    for number in rawData:
        #print(x)
        if number in numbers:
            data.update({x:number})
            x +=1
        elif number == ' ' or number == ',' or number == ', ' or number == '\n':
            pass
        else:
            data.update({x:0})
            x +=1
    return data
def getRow(data, rowNum):
    start = (rowNum-1) *9
    end = (rowNum-1) *9+9
    row = []
    for item in data:
        if int(item)>=start and int(item)<=end:
            row.append(item)
    return row
def getCol(data, colNum):
    col = []
    for item in data:
        if int(int(item)%colNum) == 0:
            col.append(item)
            #print(item)
    return col
def getBox(data):
    pass
def findColRow(number):
    col = (number%9)+1
    row = int(number/9)+1
    return row, col
def checkForMatch(data, number):
    options = []
    if data[number] in numbers:
        return [number]
    row, col = findColRow(number)
    rowNums = getRow(data, row)
    
    colNums =getCol(data, col)
    print(rowNums)
    print(colNums)
##    for testValue in numbers:
##        if testValue in nums:
##            pass
##        else:
##            options.append(testValue)
    print()
    print()
    return options
def getValues(data, nums):
    dataOut = []
    try:
        for x in nums:
            if data[x] == 0:
                pass
            else:
                dataOut.append(data[x])
        return dataOut
    except TypeError:
        try:
            return data[nums]
        except IndexError:
            print("ERROR index out of range.")
            return -1
def solveSpace(data, number):
    result = checkForMatch(data, number)
    if len(result) == 1:
        print("Number found!")
        data.update({number:result})
    else:
        pass
def findUnsolvedSpace(data):
    pass
def main():
    print("Main")
    #file = input("Filename: ")
    rawData = loadData(file)
    data = processData(rawData)
    #Test displaying the third row:
    row3 = getRow(data, 3)
    #print(row3)
    #for x in row3:
        #print(getValues(data, x))
    print(checkForMatch(data, 63))
    return data

if __name__ == '__main__':data = main()
