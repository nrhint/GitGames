##Nathan and Peter Hinton

dbg = True

class Player:
    def __init__(self, map_file, internal_player_name):
        self.internal_player_name = internal_player_name
        if dbg:
            self.display_player_name = self.internal_player_name
            print(f"Created player {self.display_player_name}...")
        else:
            self.display_player_name = input("Initing a player, enter your name: ")

        ##Parse the map file to look for things are are already owned by this player:
        self.buildings_and_units = {}
        with open(map_file, 'r') as file:
            map_lines = file.read().splitlines()
        for line in map_lines:
            if self.internal_player_name in line:
                item = line.split(":")[1].split(",")
                self.buildings_and_units.update({item[2]:[item[0], item[1]]})
                print(line)