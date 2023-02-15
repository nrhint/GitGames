//Nathan Hinton

#pragma once

#ifndef PIECE_H
#define PIECE_H

#include <vector>

struct positions {
    int x = -1;
    int y = -1;
};

class Piece{
    private:
        int pos_x = 0;
        int pos_y = 2;
        int old_x = 0;
        int old_y = 2;
        //Width and height based from the middle square
        //Using positions to store 2 ints
        positions piece_width = {0, 0};
        positions piece_height = {0, 0};
        bool landed = false;
        bool finalized = false;//true when piece is mapped to permanent map
        std::vector<std::vector<int>> piece_shape;
    public:
        void test_pos_update(int y_direction, bool down = false);
        void finalize_pos_update();
        void cancel_move();
        bool is_landed();
        void set_landed();
        bool is_finalized();
        void set_finalized();
        std::vector<positions> get_draw_positions();
        Piece();
};

#endif