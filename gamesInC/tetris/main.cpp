//Nathan Hinton

#include <iostream>
#include <chrono>
#include <thread>

#include "piece.h"

#define GAME_WIDTH 11
#define GAME_HEIGHT 20
#define FPS 5000

int main(void){
    int frame_num = 0;
    Piece p;
    std::vector<std::vector<int>> full_map (GAME_HEIGHT, std::vector<int> (GAME_WIDTH, 0));
    bool run = true;
    while (true == run) {
        //Init for next frame:
        std::chrono::steady_clock::time_point frame_start = std::chrono::steady_clock::now();
        std::cout << "Rendering frame #" << frame_num ++ << std::endl;
        for (int xx = 0; xx < 20; xx ++) {
            std::cout << std::endl;
        }
        for (int row = 0; row < GAME_HEIGHT; row ++) {
            for (int index = 0; index < GAME_WIDTH; index ++) {
                std::cout << full_map[row][index] << " ";
            }
            std::cout << std::endl;
        }
        std::chrono::nanoseconds nanoseconds_to_sleep ((1000000000/FPS) - (std::chrono::steady_clock::now()-frame_start).count());
        std::cout << nanoseconds_to_sleep.count() << std::endl;
        std::this_thread::sleep_for(std::chrono::nanoseconds(nanoseconds_to_sleep));
    }
    return 0;
}