//Nathan Hinton

#ifndef PIECE_H
#define PIECE_H

#include <vector>

class Piece{
    private:
        int pos_x;
        int pos_y;
        std::vector<std::vector<int>> piece_shape;
    public:
        void update_pos(int y_direction);
        Piece();
};
#endif