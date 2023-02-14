//Nathan Hinton
#include "utils.hpp"
#include <stdlib.h>

bool checkNumber(int grid[9][9], int numberToCheck, int x, int y) {
    int currentCol[9] = {0}, currentRow[9] = {0}, currentBox[9] = {0};
    if (getHorizontalNumbers(grid, y, numberToCheck) && getVerticalNumbers(grid, x, numberToCheck) && getGridNumbers(grid, x, y, numberToCheck)) {
        return true;
    }
    return false;
}

bool getVerticalNumbers(int grid[9][9], int x, int numberToCheck) {
    for (int i = 0; i < 9; i++) {
        if (grid[i][x] == numberToCheck) {
            return false;
        }
    }
    return true;
}
bool getHorizontalNumbers(int grid[9][9], int y, int numberToCheck) {
    for (int i = 0; i < 9; i++) {
        if (grid[y][i] == numberToCheck) {
            return false;
        }
    }
    return true;
}

bool getGridNumbers(int grid[9][9], int row, int col, int numberToCheck) {
    for (int i = 0; i < 3; i++) {
        for (int j = 0; j < 3; j++) {
            if (grid[((col/3)*3)+i][((row/3)*3)+j] == numberToCheck) {
                return false;
            }
        }
    }
    return true;
}