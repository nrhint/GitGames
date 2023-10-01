##Nathan Hinton
##This is the main entry point for turn tanks

from map import GameMap
from player import Player


##Test script:
##Load the map:
default_map = "./maps/14x14.map"
game_map = GameMap(default_map)
game_map.print_map()
internal_player_names = game_map.get_players()

players = []
for name in internal_player_names:
    players.append(Player(default_map, name))

print("Finished")