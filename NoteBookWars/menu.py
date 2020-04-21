##Nathan Hinton
##This is the menu file


def menu():#This wull return the information from the game file in a list
    i = input('Which level would you like to play? ')
    try:
        file = open('level%s.l'%i, 'r')
    except FileNotFoundError:
        print("FILE NOT FOUND!")
        menu()
    
