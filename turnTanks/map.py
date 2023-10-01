##Nathan and Peter Hinton
##This will parse the map and will display the information

class GameMap:
    def __init__(self, map_file):
        try:
            with open(map_file, 'r') as file:
                self.map_data_raw = file.read()
        except FileNotFoundError:
            print("ERROR: MAP FILE NOT FOUND!!!")
            quit(1)
        
        self.map_data_raw = self.map_data_raw.splitlines()
        self.map = []
        self.players_on_map = []
        for item in self.map_data_raw:
            if "#SIZE:" == item[0:6]:
                self.width, self.height = item[6:].split("x")
            elif item == "#":
                pass
            elif item[0] == "#" and item[1:item.index(':')] not in self.players_on_map:
                self.players_on_map.append(item[1:item.index(':')])
            else: #This is a map line:
                self.map.append(item.split(" "))
    
    def get_players(self):
        return self.players_on_map

    def get_at(self, x, y):
        return self.map[x][y]

    def print_map(self, mask = [], player_data = {}):
        for line in self.map:
            line_string = ""
            for item in line:
                line_string += f"{item} "
            print(line_string)
    
    def get_help(self):
        with open("maps/map_info.txt", "r") as help_file:
            data = help_file.read()
        for line in data.splitlines():
            if line[0] != "#":
                print(line)

    