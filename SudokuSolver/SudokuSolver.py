##Nathan Hinton
#This shold solve the sudoku problems using several methods. The first will be trying to solve just using the numbers in the colum then next will be using the rows then the boxex. These methods will be additive meaning that if one does not work you should be able to pass the output from one into another for furthur refinement.

file = 'easy1.puzzle'
numbers = [1, 2, 3, 4, 5, 6, 7, 8, 9]
rawData = open(file, 'r').read()

#Parse the file, for every blank make a entry with every number possible

rawData = rawData.replace('n', '0')
print(rawData)
rawData = rawData.replace('\n', '')#Remove newlines

#Chack for the correct file length:
if len(rawData) !=81:
    print("FILE LENGTH INVALID!")
    quit()

origPuzzle = []

for x in rawData:
    if int(x) == 0:
        origPuzzle.append(0)
    else:
        origPuzzle.append(int(x))

print(origPuzzle)

#Make the first method for returning the data in a row
def returnRow(row, inp):
    if 0 <= row and row <= 8:#If the row is between 0 and 8
        return inp[row*9:row*9+9]
    else:
        print("INVALID ROW INPUT!")
        quit()

#This function will strip all of the 0's from a input
def strip0(data):
    while 0 in data:
        data.remove(0)
    return data

#solving for a point by chacking if the row has all of the other numbers allowed
def pointSolveWithRow(pos, inp):
    row = pos//9
    data = strip0(returnRow(row, inp))
    if len(data) == 8:#There is only one blank left in the row then put in the correct number
        for x in numbers:#Subtract the lists:
            if x not in data:#The missing number will be x
                inp[pos] = x
                return inp
    else:
        return inp

#def returnCol(
print()
print()

#colSolve(0, origPuzzle)
#41 = 8

print()
print("Finished")
