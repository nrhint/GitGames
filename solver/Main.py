##Nathan Hinton

##TO DO:

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
    end = (rowNum-1) *9+8
    row = []
    for item in data:
        if int(item)>=start and int(item)<=end:
            row.append(item)
    return row
def getCol(data, colNum):
    col = []
    for item in data:
        if int(item)%9 == colNum-1:
            col.append(item)
            #print(item)
    return col
def getBox(data, col, row):
    nums = []
    baseCol = int((col-1)/3)*3
    colRange = [baseCol, baseCol+1, baseCol+2]
    baseRow = int((row-1)/3)*3
    rowRange = [baseRow, baseRow+1, baseRow+2]
    for cols in colRange:
        for rows in rowRange:
            nums.append(getNumber(cols, rows))
    return nums
def getNumber(col, row):
    place = ((row-1)*9)-1+col
    return place
def findColRow(number):
    col = (number%9)+1
    row = int(number/9)+1
    return col, row
def checkForMatch(data, number):
    options = []
    if data[number] in numbers:
        return [number]
    col, row = findColRow(number)
    rowNums = getRow(data, row)#Cross hatch the place:
    colNums = getCol(data, col)
    box = getBox(data, col, row)#Get the numbers in the box
    nums = getValues(data, rowNums+colNums+box)
    for testValue in numbers:
        if testValue in nums:
            pass
        else:
            options.append(testValue)
    print(options)
    if len(options) == 1:
        return int(options[0])
    else:
        return 0
def method1(data):
    newData = data
    for place in data:
        newData = solveSpace(newData, place)
    return newData
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
    data.update({number:result})
    return data
def main():
    print("Main")
    #file = input("Filename: ")
    rawData = loadData(file)
    data = processData(rawData)
    print(checkForMatch(data, 63))
    return data

if __name__ == '__main__':data = main()
