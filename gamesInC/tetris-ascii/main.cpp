//Nathan Hinton

#include <iostream>
#include <chrono>
#include <thread>
#include <curses.h>

#include "piece.h"
#include "defaults.h"

int main(void){
    static const char newline = '\n';
    int level = FPS;
    int frame_num = 0;
    int move_piece_direction = 0;
    int ch;
    Piece p;
    std::vector<std::vector<int>> landed_map (GAME_HEIGHT, std::vector<int> (GAME_WIDTH, 0));
    std::vector<std::vector<int>> full_map (GAME_HEIGHT, std::vector<int> (GAME_WIDTH, 0));
    std::vector<Piece> pieces;
    bool run = true;
    bool active_piece = false;
    bool fast_down = false;
    initscr();
    // noecho();
    nodelay(stdscr, TRUE);
    scrollok(stdscr, true);
    keypad(stdscr, TRUE);
    start_color();
    init_pair(1, COLOR_BLACK, COLOR_WHITE);
    while (true == run) {
        //Init for next frame:
        std::chrono::steady_clock::time_point frame_start = std::chrono::steady_clock::now();
        // std::cout << "Rendering frame #" << frame_num ++ << std::endl;
        //Game logic:
        full_map = landed_map;
        if (false == active_piece) {
            pieces.push_back(Piece());
            active_piece = true;
        }
        //Get keyboard input
        // move_piece_direction = 0;
        ch = getch();
        switch(ch) {
            case KEY_LEFT: 
                move_piece_direction = -1;
                break;
            case KEY_RIGHT:
                move_piece_direction = 1;
                break;
            case KEY_DOWN:
                fast_down = true;
                break;
            default:
                move_piece_direction = 0;
                fast_down = false;
        }
        bool piece_moved = false;
        for (auto piece = pieces.begin(); piece != pieces.end(); ++piece) {
            if (false == piece->is_landed()) {
                //Check to make sure that the piece does not collide with anything
                std::vector<positions> piece_positions;
                if (0 == frame_num%10) {
                    fast_down = true;
                }
                piece->test_pos_update(move_piece_direction, fast_down);
                piece_positions = piece->get_draw_positions();
                if (true == fast_down) {
                    for (auto current_position = piece_positions.begin(); current_position != piece_positions.end(); ++current_position) {
                        if (1 == full_map[current_position->y][current_position->x]) {
                            piece->set_landed();
                        }
                    }
                }
                //Either by a collision or by hitting the bottom
                piece_positions = piece->get_draw_positions();
                if (false == piece->is_landed()) {
                    for (auto current_position = piece_positions.begin(); current_position != piece_positions.end(); ++current_position) {
                        full_map[current_position->y][current_position->x] = 1;
                    }
                    piece->finalize_pos_update();
                    piece_moved = true;
                } else {
                    piece->cancel_move();
                }
            } 
            if ( true == piece->is_landed() && false == piece->is_finalized()) {
                std::vector<positions> piece_positions = piece->get_draw_positions();
                for (auto current_position = piece_positions.begin(); current_position != piece_positions.end(); ++current_position) {
                    landed_map[current_position->y][current_position->x] = 1;
                }
                //reset the map with the landed map. This prevents flickers of 0's
                full_map = landed_map;
                piece->set_finalized();
            }
        }
        if (false == piece_moved) {
            active_piece = false;
        }
        //Check for loose condition
        for (int index = 0; index < GAME_WIDTH; index++) {
            if (1 == landed_map[SCREEN_TOP_CUTOFF][index]) {
                waddstr(stdscr, "GAME OVER!\n\n");
                wrefresh(stdscr);
                return 0;
            }
        }
        //Check for lines to be cleared:
        for (auto row = landed_map.begin(); row != landed_map.end(); ++row) {
            bool clear = true;
            for (int index = 0; GAME_WIDTH > index; index ++) {
                if (0 == row->at(index)) {
                    clear = false;
                    break;
                }
            }
            if (true == clear) {
                landed_map.erase(row);
                landed_map.insert(landed_map.begin()+1, std::vector<int> (GAME_WIDTH, 0));
            }
        }
        //Draw the frame
        for (int xx = 0; xx < 30; xx ++) {
            waddch(stdscr, newline);
            // std::cout << std::endl;
        }
        for (int row = SCREEN_TOP_CUTOFF; row < GAME_HEIGHT; row ++) {
            for (int index = 0; index < GAME_WIDTH; index ++) {
                if (0 == full_map[row][index]){
                    waddstr(stdscr, "0 ");
                    // std::cout << full_map[row][index] << " ";
                } else {
                    attron(COLOR_PAIR(1));
                    waddstr(stdscr, "1 ");
                    attroff(COLOR_PAIR(1));
                    // std::cout << "\033[7m1 \033[0m";
                }
            }
            waddch(stdscr, newline);
            // std::cout << std::endl;
        }
        wrefresh(stdscr);
        frame_num ++;
        std::chrono::nanoseconds nanoseconds_to_sleep ((1000000000/level) - (std::chrono::steady_clock::now()-frame_start).count());
        // std::cout << nanoseconds_to_sleep.count() << std::endl;
        std::this_thread::sleep_for(std::chrono::nanoseconds(nanoseconds_to_sleep));
    }
    return 0;
}