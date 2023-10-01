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
        
        self.map = self.map_data_raw.splitlines()
    
    def get_help(self):
        with open("maps/map_info.txt", "r") as help_file:
            data = help_file.read()
        for line in data.splitlines():
            if line[0] != "#":
                print(line)

    