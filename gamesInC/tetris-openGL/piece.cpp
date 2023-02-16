//Nathan Hinton

#include <ctime>
#include <cstdlib>
#include <iostream>

#include "piece.h"
#include "defaults.h"

#define NUM_TYPES 16

Piece::Piece() {
    srand(time(NULL));
    int init_piece_type = rand()%NUM_TYPES;
    for (int xx = 0; xx < 30; xx++){
        std::cout << 2+rand()%(GAME_WIDTH-4) << std::endl;
    }
    this->pos_x = 2+rand()%(GAME_WIDTH-4);
    this->old_x = this->pos_x;
    if (0 <= init_piece_type && 2 > init_piece_type) { //Long piece type 1
        this->piece_shape = {
            {0, 0, 1, 0, 0},
            {0, 0, 1, 0, 0},
            {0, 0, 1, 0, 0},
            {0, 0, 1, 0, 0},
            {0, 0, 1, 0, 0}
        };
        this->piece_height = positions {-2, 2};
    } else if (2 <= init_piece_type && 4 > init_piece_type) { //Long piece type 2
        this->piece_shape = {
            {0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0},
            {1, 1, 1, 1, 1},
            {0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0}
        };
        this->piece_width = positions {-2, 2};
    } else if (4 <= init_piece_type && 8 > init_piece_type) { //Square piece
        this->piece_shape = {
            {0, 0, 0, 0, 0},
            {0, 1, 1, 0, 0},
            {0, 1, 1, 0, 0},
            {0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0}
        };
        this->piece_height = positions {0, 1};
        this->piece_width = positions {-1, 0};
    } else if (8 == init_piece_type) { //L piece type 1
        this->piece_shape = {
            {0, 0, 1, 0, 0},
            {0, 0, 1, 0, 0},
            {0, 0, 1, 0, 0},
            {0, 0, 1, 1, 0},
            {0, 0, 0, 0, 0}
        };
        this->piece_height = positions {-1, 2};
        this->piece_width = positions {0, 1};
    } else if (9 == init_piece_type) { //L piece type 2
        this->piece_shape = {
            {0, 0, 0, 0, 0}, 
            {0, 0, 0, 1, 0}, 
            {1, 1, 1, 1, 0}, 
            {0, 0, 0, 0, 0}, 
            {0, 0, 0, 0, 0}
        };
        this->piece_height = positions {0, 1};
        this->piece_width = positions {-2, 1};
    } else if (10 == init_piece_type) { //L piece type 3
        this->piece_shape ={
            {0, 0, 0, 0, 0}, 
            {0, 1, 1, 0, 0}, 
            {0, 0, 1, 0, 0}, 
            {0, 0, 1, 0, 0}, 
            {0, 0, 1, 0, 0}
        }; 
        this->piece_height = positions {-2, 1};
        this->piece_width = positions {-1, 0};
    } else if (11 == init_piece_type) { //L piece type 4
        this->piece_shape = {
            {0, 0, 0, 0, 0}, 
            {0, 0, 0, 0, 0}, 
            {0, 1, 1, 1, 1}, 
            {0, 1, 0, 0, 0}, 
            {0, 0, 0, 0, 0}
        };
        this->piece_height = positions {-1, 0};
        this->piece_width = positions {-1, 2};
    } else if (12 <= init_piece_type && 16 > init_piece_type) { //Long piece type 2
        this->piece_shape = {
            {0, 0, 0, 0, 0}, 
            {0, 0, 1, 0, 0}, 
            {0, 1, 1, 1, 0}, 
            {0, 0, 1, 0, 0}, 
            {0, 0, 0, 0, 0}
        };
        this->piece_height = positions {-1, 1};
        this->piece_width = positions {-1, 1};
    } else {
        throw std::__throw_invalid_argument;
    }
}

void Piece::test_pos_update(int x_direction, bool down){
    if (0 <= old_x + x_direction + this->piece_width.x && GAME_WIDTH > old_x + x_direction + this->piece_width.y) {
            this->pos_x += x_direction;
    } else {
        // std::cout << "Piece " << this << " tried to leave grid";
    }
    if (GAME_HEIGHT > old_y + 1 - this->piece_height.x && down){
        this->pos_y ++;
    } else if (true == down) {
        this->set_landed();
    }
}

void Piece::finalize_pos_update() {
    this->old_x = this->pos_x;
    this->old_y = this->pos_y;
}

void Piece::cancel_move() {
    this->pos_x = this->old_x;
    this->pos_y = this->old_y;
}

bool Piece::is_landed() {
    return this->landed;
}

void Piece::set_landed() {
    this->landed = true;
    // std::cout << "Piece landed" << std::endl;
}

bool Piece::is_finalized() {
    return this->finalized;
}

void Piece::set_finalized() {
    this->finalized = true;
}

std::vector<positions> Piece::get_draw_positions() {
    std::vector<positions> result;
    for (int yy = -2; yy < 3; yy++) {
        for (int xx = -2; xx < 3; xx++) {
            if (1 == this->piece_shape[yy+2][xx+2]) {
                result.push_back(positions {this->pos_x + xx, this->pos_y + yy});
            }
        }
    }
    return result;
}