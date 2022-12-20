//Nathan Hinton

#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <ncurses.h>

struct staticBox{
    int topx;
    int topy;
    int botx;
    int boty;
};

struct movingBox{
    double topx;
    double topy;
    double botx;
    double boty;
    double direction;
    double speed;

};

struct linkedBbox {
    struct bbox *bbox;
    struct linkedBbox *next;
};

double cos(double x);
double sin(double x);
double myAbs(double x);

void drawBoard(struct movingBox player, struct staticBox board, struct linkedBbox bricks);
void updatePlayer(struct movingBox *player, struct staticBox board);

int main(void) {
    int gameOver = 0, counter = 0, gameSpeed = 100000, boardWidth = 8, boardHeight = 8;
    struct staticBox board = {0, 0, boardWidth, boardHeight};
    struct movingBox player = {0, 0, 0, 0, 90, 1};
    struct linkedBbox bricks;
    while (!gameOver) {
        printf("%d\n", counter);
        drawBoard(player, board, bricks);
        updatePlayer(&player, board);
        printf("%d", counter);
        counter++;
        usleep(gameSpeed);
    }
    return 0;
}

void updatePlayer(struct movingBox *player, struct staticBox board) {
    double xMovement = cos(((*player).direction*M_PI)/180)*(*player).speed;
    double yMovement = sin(((*player).direction*M_PI)/180)*(*player).speed;
    (*player).topx += myAbs(fmod(xMovement, (double) board.botx));
    (*player).botx += myAbs(fmod(xMovement, (double) board.botx));
    (*player).topy += myAbs(fmod(yMovement, (double) board.boty));
    (*player).boty += myAbs(fmod(yMovement, (double) board.boty));
    
}

void drawBoard(struct movingBox player, struct staticBox board, struct linkedBbox bricks) {
    for (int jj = -1; jj < board.boty+2; jj++) {
        printf("|");
        for (int kk = 0; kk < board.botx; kk++) {
            if (jj < 0 || jj > board.boty) {
                printf(" - ");
            } else if (jj <= player.topy && jj >= player.boty && kk <= player.topx && kk >= player.botx) {
                printf(" O ");
            } else {
                printf("   ");
            }
        }
        printf("|");
        printf("\n");
    }
}

double myAbs(double x) {
    if (x < 0) {
        return -x;
    } else {
        return x;
    }
}