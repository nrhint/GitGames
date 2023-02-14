//Nathan Hinton

#include <ctime>
#include <cstdlib>

#include "piece.h"

#define NUM_TYPES 16

Piece::Piece() {
    srand(time(NULL));
    int init_piece_type = rand()%NUM_TYPES;
    this->pos_y = 0;
    this->pos_x = 6;
    if (0 <= init_piece_type && 2 > init_piece_type) { //Long piece type 1
        this->piece_shape = {
            {0, 0, 1, 0, 0},
            {0, 0, 1, 0, 0},
            {0, 0, 1, 0, 0},
            {0, 0, 1, 0, 0},
            {0, 0, 1, 0, 0}
        };
    } else if (2 <= init_piece_type && 4 > init_piece_type) { //Long piece type 2
        this->piece_shape = {
            {0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0},
            {1, 1, 1, 1, 1},
            {0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0}
        };
    } else if (4 <= init_piece_type && 8 > init_piece_type) { //Square piece
        this->piece_shape = {
            {0, 0, 0, 0, 0},
            {0, 1, 1, 0, 0},
            {0, 1, 1, 0, 0},
            {0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0}
        };
    } else if (8 == init_piece_type) { //L piece type 1
        this->piece_shape = {
            {0, 0, 1, 0, 0},
            {0, 0, 1, 0, 0},
            {0, 0, 1, 0, 0},
            {0, 0, 1, 1, 0},
            {0, 0, 0, 0, 0}
        };
    } else if (9 == init_piece_type) { //L piece type 2
        this->piece_shape = {
            {0, 0, 0, 0, 0}, 
            {0, 0, 0, 1, 0}, 
            {1, 1, 1, 1, 0}, 
            {0, 0, 0, 0, 0}, 
            {0, 0, 0, 0, 0}
        };
    } else if (10 == init_piece_type) { //L piece type 3
        this->piece_shape ={
            {0, 0, 0, 0, 0}, 
            {0, 1, 1, 0, 0}, 
            {0, 0, 1, 0, 0}, 
            {0, 0, 1, 0, 0}, 
            {0, 0, 1, 0, 0}
        }; 
    } else if (11 == init_piece_type) { //L piece type 4
        this->piece_shape = {
            {0, 0, 0, 0, 0}, 
            {0, 0, 0, 0, 0}, 
            {0, 1, 1, 1, 1}, 
            {0, 1, 0, 0, 0}, 
            {0, 0, 0, 0, 0}
        };
    } else if (12 <= init_piece_type && 16 > init_piece_type) { //Long piece type 2
        this->piece_shape = {
            {0, 0, 0, 0, 0}, 
            {0, 0, 1, 0, 0}, 
            {0, 1, 1, 1, 0}, 
            {0, 0, 1, 0, 0}, 
            {0, 0, 0, 0, 0}
        };
    } else {
        throw std::__throw_invalid_argument;
    }
}

void Piece::update_pos(int y_direction){
    this->pos_x -= 1;
    this->pos_y += y_direction;
}