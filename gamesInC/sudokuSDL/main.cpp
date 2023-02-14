//Nathan Hinton

#include <SDL2/SDL.h>
#include <time.h>
#include <stdlib.h>
#include "utils.cpp"
#include <vector>

struct number {
    int num, x, y;
};

int numbers[9][9] = {
    {1, 2, 3, 4, 5, 6, 7, 8, 9}, 
    {4, 0, 6, 0, 8, 9, 1, 2, 3},
    {7, 8, 9, 1, 2, 3, 4, 5, 6},
    {2, 3, 0, 5, 6, 7, 8, 9, 1}, 
    {5, 6, 0, 8, 9, 1, 0, 3, 4}, 
    {8, 9, 1, 2, 3, 4, 5, 6, 7}, 
    {3, 4, 5, 6, 0, 8, 9, 1, 2}, 
    {6, 0, 8, 9, 1, 2, 3, 4, 5}, 
    {9, 1, 2, 3, 4, 5, 6, 7, 8}
};

bool solve(int grid[9][9], int x, int y, std::vector<number> &path, int i = 1) {
    bool worked = false;
    if (grid[y][x] == 0) {
        while ((i < 10) && !worked) {
            worked = checkNumber(grid, i, x, y);
            i++;
        }
        if (10 == i) {
            path.pop_back();
            solve(grid, path.back().x, path.back().y, path, path.back().num);
            return false;
        }
        struct number tmp = {tmp.num = --i, tmp.x = x, tmp.y = y};
        path.push_back(tmp);
        for (int i = y; i < 9; i++) {
            for (int j = x+1; j < 9; j++) {
                if (grid[i][j] == 0) {
                    solve(grid, j, i, path);
                    break;
                }
            }
        }
    } else {
        int pos = x+(y*9)+1;
        solve(grid, pos%9, pos/9, path);
        return false;
    }
    return true;
}

void printSquare(int grid[9][9], const std::vector<number> &path) {
    int vIndex = 0;
    for (int i = 0; i < 9; i++) {
        for (int j = 0; j < 9; j++) {
            if ((path[vIndex].y == i) && (path[vIndex].x == j)) {
                printf("%d ", path[vIndex].num);
                vIndex ++;
            } else {
                printf("%d ", grid[i][j]);
            }
        }
        printf("\n");
    }
}

bool checkForCompletion(int grid[9][9]) {
    for (int i = 0; i < 9; i++) {
        for (int j = 0; j < 9; j++) {
            if (grid[i][j] == 0) {
                return false;
            }
        }
    }
    return true;
}

int main(void) {
    srand(time(NULL));
    std::vector<number> pathing;
    bool result = solve(numbers, 0, 0, pathing);
    printSquare(numbers, pathing);
    if (checkForCompletion(numbers)) {
        printf("Solved the puzzle!\n");
    }
    return 0;
}