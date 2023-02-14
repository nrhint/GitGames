//https://lazyfoo.net/tutorials/SDL/02_getting_an_image_ on_the_screen/index.php
#include <SDL2/SDL.h>
#include <stdio.h>
#include <time.h>
#include <math.h>
#include <vector>
#include "utils.cpp"

struct spriteDataStruct {
    SDL_Rect rect;
    double xSpeed, ySpeed;
    int lifetime = 0, colorOffset = 0;
};

const int SCREEN_WIDTH = 1500, SCREEN_HEIGHT = 800, RANDOM_FACTOR = 50, MIN_SPAWN_LIFE = 100;

//initalize window
bool init();

//load image
bool loadMedia();

//clean up and exit
void close();

SDL_Window *gWindow = NULL;
SDL_Surface *gSurface = NULL;
SDL_Surface *ballImage = NULL, *paddleImage = NULL;

bool init() {
    bool success = true;
    if (SDL_Init( SDL_INIT_VIDEO) < 0) {
        printf("ERROR: Could not initalize video\n");
        printf("SDL Error: %s\n", SDL_GetError());
        success = false;
    } else {
        gWindow = SDL_CreateWindow("SDL Tutorial - Display image", SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, SCREEN_WIDTH, SCREEN_HEIGHT, SDL_WINDOW_SHOWN);
    }
    gSurface = SDL_GetWindowSurface(gWindow);
    return success;
}

bool loadMedia() {
    bool success = true;

    ballImage = SDL_LoadBMP("./images/redHeart.bmp");
    paddleImage = SDL_LoadBMP("./images/purpleBarLarge.bmp");
    if (ballImage == NULL || paddleImage == NULL) {
        printf("ERROR: Unable to load image\n");
        printf("SDL Error: %s", SDL_GetError());
        success = false;
    }
    // SDL_SetColorKey(ballImage, SDL_TRUE, SDL_MapRGB(ballImage->format, 255, 255, 255));
    return success;
}

void close() {
    SDL_FreeSurface(ballImage);
    SDL_FreeSurface(paddleImage);
    SDL_FreeSurface(gSurface);
    ballImage = NULL, paddleImage = NULL, gSurface = NULL;

    SDL_DestroyWindow(gWindow);
    gWindow = NULL;

    SDL_Quit();
}

std::vector<struct spriteDataStruct> spawnBall(std::vector<struct spriteDataStruct> balls, int x, int y) {
    struct spriteDataStruct tmp;
    tmp = {
        tmp.rect.x = x,
        tmp.rect.y = y,
        tmp.rect.w = 19, tmp.rect.h = 19,
        tmp.xSpeed = rand()%3+1.0, tmp.ySpeed = rand()%3+1.0};
    balls.push_back(tmp);
    return balls;
}

int main(int argc, char *argv[]) {
    int counter = 0, paddleXVel = 0, paddleYVel = 0;
    static time_t startTime = time(NULL);
    srand(time(NULL));
    std::vector<struct spriteDataStruct> imageRects;
    struct spriteDataStruct tmp, paddleSprite;
    imageRects = spawnBall(imageRects, rand()%(SCREEN_WIDTH-19), rand()%(SCREEN_HEIGHT-19));
    if (!init()) {
        printf("Failed to initalize");
        exit(1);
    }
    if (!loadMedia()) {
        printf("Failed to load media");
        exit(1);
    }
    paddleSprite = {
        paddleSprite.rect.x = SCREEN_WIDTH/2, 
        paddleSprite.rect.y = SCREEN_HEIGHT - 50, 
        paddleSprite.rect.w = paddleImage->w, 
        paddleSprite.rect.h = paddleImage->h,
        paddleSprite.xSpeed = 50, 
        paddleSprite.ySpeed = 0 
    };
    SDL_Event e;
    bool quit = false;
    while(quit == false) {
        //Do game logic:
        while (SDL_PollEvent(&e)) {
            if (e.type == SDL_QUIT) {
                quit = true;
            } else if (e.type == SDL_KEYDOWN) {
                if (e.key.keysym.sym == SDLK_LEFT) {
                    paddleXVel = -paddleSprite.xSpeed;
                } else if (e.key.keysym.sym == SDLK_RIGHT) {
                    paddleXVel =  paddleSprite.xSpeed;
                }
            } else if (e.type == SDL_KEYUP) {
                if (e.key.keysym.sym == SDLK_LEFT) {
                    if (paddleXVel < 0) {
                        paddleXVel = 0;
                    }
                } else if (e.key.keysym.sym == SDLK_RIGHT) {
                    if (paddleXVel > 0) {
                        paddleXVel = 0;
                    }
                }
            }
        }
        paddleSprite.rect.x += paddleXVel;
        paddleSprite.rect.y += paddleYVel;
        if (paddleSprite.rect.x < 0) {
            paddleSprite.rect.x = 0;
        } else if (paddleSprite.rect.x+paddleSprite.rect.w > SCREEN_WIDTH) {
            paddleSprite.rect.x = SCREEN_WIDTH-paddleSprite.rect.w;
        }

        /*THE BALL(s)*/
        int ballCount = imageRects.size();
        for (int i = 0; i < ballCount; i++){
            imageRects.at(i).lifetime++;
            //clear the rect before modifying it:
            //check for direction changes and move the rect:
            if ((imageRects.at(i).rect.x <= 0) && (imageRects.at(i).xSpeed<0)) {
                // printf("Heart #%d speed changed from %f to ", i, imageRects.at(i).xSpeed);
                // if (imageRects.at(i).lifetime>MIN_SPAWN_LIFE) {
                //     imageRects = spawnBall(imageRects, imageRects.at(i).rect.x+1, imageRects.at(i).rect.y);
                // }
                imageRects.at(i).colorOffset += 10;
                imageRects.at(i).xSpeed = max(1.0, imageRects.at(i).xSpeed*(((double)((rand()%RANDOM_FACTOR)-(RANDOM_FACTOR/2))/100)+1));
                // printf("%f\n", imageRects.at(i).xSpeed);
            } else if (((imageRects.at(i).rect.x + imageRects.at(i).rect.w) >= SCREEN_WIDTH) && (imageRects.at(i).xSpeed > 0)) {
                // printf("Heart #%d speed changed from %f to ", i, imageRects.at(i).xSpeed);
                // if (imageRects.at(i).lifetime>MIN_SPAWN_LIFE) {
                //     imageRects = spawnBall(imageRects, imageRects.at(i).rect.x-1, imageRects.at(i).rect.y);
                // }
                imageRects.at(i).colorOffset += 10;
                imageRects.at(i).xSpeed = min(-1.0, -(imageRects.at(i).xSpeed*(((double)((rand()%RANDOM_FACTOR)-(RANDOM_FACTOR/2))/100)+1)));
                // printf("%f\n", imageRects.at(i).xSpeed);
            }
            if ((imageRects.at(i).rect.y <= 0) && (imageRects.at(i).ySpeed<0)) {
                // printf("Heart #%d speed changed from %f to ", i, imageRects.at(i).ySpeed);
                // if (imageRects.at(i).lifetime>MIN_SPAWN_LIFE) {
                //     imageRects = spawnBall(imageRects, imageRects.at(i).rect.x, imageRects.at(i).rect.y+1);
                // }
                imageRects.at(i).colorOffset += 10;
                imageRects.at(i).ySpeed = max(1.0, imageRects.at(i).ySpeed*(((double)((rand()%RANDOM_FACTOR)-(RANDOM_FACTOR/2))/100)+1));
                // printf("%f\n", imageRects.at(i).ySpeed);
            } else if (((imageRects.at(i).rect.y + imageRects.at(i).rect.h) >= SCREEN_HEIGHT) && (imageRects.at(i).ySpeed>0)) {
                // printf("Heart #%d speed changed from %f to ", i, imageRects.at(i).ySpeed);
                // if (imageRects.at(i).lifetime>MIN_SPAWN_LIFE) {
                //     imageRects = spawnBall(imageRects, imageRects.at(i).rect.x, imageRects.at(i).rect.y-1);
                // }
                imageRects.at(i).colorOffset += 10;
                imageRects.at(i).ySpeed = min(-1.0, -(imageRects.at(i).ySpeed*(((double)((rand()%RANDOM_FACTOR)-(RANDOM_FACTOR/2))/100)+1)));
                // printf("%f\n", imageRects.at(i).ySpeed);
            }
            if (checkForCollision(imageRects.at(i).rect, paddleSprite.rect)) {
                imageRects.at(i).ySpeed = -imageRects.at(i).ySpeed;
                if (imageRects.at(i).lifetime>MIN_SPAWN_LIFE) {
                    imageRects = spawnBall(imageRects, imageRects.at(i).rect.x, imageRects.at(i).rect.y);
                }
            }
            imageRects.at(i).rect.x+=imageRects.at(i).xSpeed;
            imageRects.at(i).rect.y+=imageRects.at(i).ySpeed;
            //blit the rect to the screen:
            SDL_BlitSurface(ballImage, NULL, gSurface, &imageRects.at(i).rect);
        }
        //update screen:
        SDL_BlitSurface(paddleImage, NULL, gSurface, &paddleSprite.rect);
        SDL_UpdateWindowSurface(gWindow);
        //Clear the old screen in prep for the new image
        for (int i = 0; i < imageRects.size(); i++){
            SDL_FillRect(gSurface, &imageRects.at(i).rect, 0);//SDL_MapRGB(gSurface->format, 0x00, 0x00, 0x00));
        }
        SDL_FillRect(gSurface, &paddleSprite.rect, 0);
        counter++;
        printf("ball count: %ld\n", imageRects.size());
        SDL_Delay(1000/30);
    }
    close();
    static time_t endTime = time(NULL);
    printf("This game ran at %ldfps\n", counter/(endTime-startTime));
    return 0;
}