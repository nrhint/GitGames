##NAthan Hinton
##This is for loading and saveing data

import pickle

def loadData(playerName):
    try:
        fileName = open(str(playerName), 'rb')
        data = pickle.load(fileName)
        fileName.close()
        print(data)
        if len(data) != 8: #Check to make sure I have all the data
            print("File loading error. The length of data was %s"%len(data))
            print(data)
            raise Exception
    except FileNotFoundError:
        print("Player file not found. Creating a new player...")
        data = [playerName, [[50, 1, 10]], 20, 5, 10, 50, 0.10, 0]
    return data##This will return a list of objects.

def saveData(data):
    print(data)
    toWrite = []
    toWrite.append(data[0])
    toWrite.append([])
    for item in data[1]:
        print(item)
        try:
            toWrite[1].append([item.value, item.minExpire, item.maxExpire])
        except AttributeError:
            toWrite[1].append([item[0], item[1], item[2]])
    toWrite.append(data[2])
    toWrite.append(data[3])
    toWrite.append(data[4])
    toWrite.append(data[5])
    toWrite.append(data[6])
    toWrite.append(data[7])
    print(toWrite)
    with open(str(data[0]), 'wb') as file:
        pickle.dump(toWrite, file)
    print("Saved at %s"%data[0])
