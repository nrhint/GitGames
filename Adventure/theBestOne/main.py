## Nathan Hinton

from player import Player

##Load from a file:
playerName = input("Enter the player name")
data = None
try:
    with open("%s.plr"%playerName, 'r') as file:
        data = file.read()
except FileNotFoundError:
    print("That player was not found. You must be new here. Lets set you up.")



if None != data:
    if data.splitlines()[0] == playerName:
        pass
    else:
        psswd = print("Enter the player password: ")
        #TODO add file encryption through the hasd of the password

## Setup the player object:

player = Player(playerName)
