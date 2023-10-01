##Nathan and Peter Hinton
##This will parse the map and will display the information

class GameMap:
    def __init__(self, map_file):
        try:
            with open(map_file, 'r') as file:
                self.map_data_raw = file.read()
        except FileNotFoundError:
            print("ERRR: MAP FILE NOT FOUND!!!")
            quit(1)
        
        self.map_data_raw = self.map_data_raw.splitlines()
        self.map = []
        for item in self.map_data_raw:
            if "SIZE:" in item[0:6]:
                self.width, self.height = item[6:].split("x")
            else: #This is a map line:
                self.map.append(item)
    
    def print_map(self, mask = [], player_data = {}):
        if mask == []:
            for line in self.map:
                print(line)
        else:
            print("The masking feature is not implimented yet. This will allow the players to only see things near their units and stuff")
    
    def get_help(self):
        with open("maps/map_info.txt", "r") as help_file:
            data = help_file.read()
        for line in data.splitlines():
            if line[0] != "#":
                print(line)

    