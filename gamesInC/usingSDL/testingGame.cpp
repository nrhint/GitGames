//https://lazyfoo.net/tutorials/SDL/02_getting_an_image_ on_the_screen/index.php
#include <SDL2/SDL.h>
#include <stdio.h>
#include <time.h>
#include <math.h>
#include <vector>
#include "utils.c"

const int SCREEN_WIDTH = 640, SCREEN_HEIGHT = 480, RANDOM_FACTOR = 50;

//initalize window
bool init();

//load image
bool loadMedia();

//clean up and exit
void close();

SDL_Window *gWindow = NULL;
SDL_Surface *gSurface = NULL;
SDL_Surface *gImage = NULL;

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

    gImage = SDL_LoadBMP("./images/heart.bmp");
    if (gImage == NULL) {
        printf("ERROR: Unable to load image\n");
        printf("SDL Error: %s", SDL_GetError());
        success = false;
    }
    SDL_SetColorKey(gImage, SDL_TRUE, SDL_MapRGB(gImage->format, 255, 255, 255));
    return success;
}

void close() {
    SDL_FreeSurface(gImage);
    gImage = NULL;

    SDL_DestroyWindow(gWindow);
    gWindow = NULL;

    SDL_Quit();
}

struct heartDataStruct {
    SDL_Rect rect;
    double xSpeed, ySpeed;
};

int main(int argc, char *argv[]) {
    int counter = 0;
    static time_t startTime = time(NULL);
    srand(time(NULL));
    std::vector<struct heartDataStruct> imageRects;
    struct heartDataStruct tmp;
    for (int i = 0; i < 5; i++) {
        tmp = {
            tmp.rect.w = 161, tmp.rect.h = 200,
            tmp.rect.x = rand()%(SCREEN_WIDTH-tmp.rect.w),
            tmp.rect.y = rand()%(SCREEN_HEIGHT-tmp.rect.h),
            tmp.xSpeed = rand()%3+1.0, tmp.ySpeed = rand()%3+1.0};
        imageRects.push_back(tmp);
    }
    if (!init()) {
        printf("Failed to initalize");
        exit(1);
    }
    if (!loadMedia()) {
        printf("Failed to load media");
        exit(1);
    }

    SDL_Event e;
    bool quit = false;
    while(quit == false) {
        while (SDL_PollEvent(&e)) {
            if (e.type == SDL_QUIT) {
                quit = true;
            }
        }
        //Do game logic:
        for (int i = 0; i < imageRects.size(); i++){
            //clear the rect before modifying it:
            //check for direction changes and move the rect:
            if ((imageRects.at(i).rect.x <= 0) && (imageRects.at(i).xSpeed<0)) {
                printf("Heart #%d speed changed from %f to ", i, imageRects.at(i).xSpeed);
                imageRects.at(i).xSpeed = max(1.0, imageRects.at(i).xSpeed*(((double)((rand()%RANDOM_FACTOR)-(RANDOM_FACTOR/2))/100)+1));
                printf("%f\n", imageRects.at(i).xSpeed);
            } else if (((imageRects.at(i).rect.x + imageRects.at(i).rect.w) >= SCREEN_WIDTH) && (imageRects.at(i).xSpeed > 0)) {
                printf("Heart #%d speed changed from %f to ", i, imageRects.at(i).xSpeed);
                imageRects.at(i).xSpeed = min(-1.0, -(imageRects.at(i).xSpeed*(((double)((rand()%RANDOM_FACTOR)-(RANDOM_FACTOR/2))/100)+1)));
                printf("%f\n", imageRects.at(i).xSpeed);
            }
            if ((imageRects.at(i).rect.y <= 0) && (imageRects.at(i).ySpeed<0)) {
                printf("Heart #%d speed changed from %f to ", i, imageRects.at(i).ySpeed);
                imageRects.at(i).ySpeed = max(1.0, imageRects.at(i).ySpeed*(((double)((rand()%RANDOM_FACTOR)-(RANDOM_FACTOR/2))/100)+1));
                printf("%f\n", imageRects.at(i).ySpeed);
            } else if (((imageRects.at(i).rect.y + imageRects.at(i).rect.h) >= SCREEN_HEIGHT) && (imageRects.at(i).ySpeed>0)) {
                printf("Heart #%d speed changed from %f to ", i, imageRects.at(i).ySpeed);
                imageRects.at(i).ySpeed = min(-1.0, -(imageRects.at(i).ySpeed*(((double)((rand()%RANDOM_FACTOR)-(RANDOM_FACTOR/2))/100)+1)));
                printf("%f\n", imageRects.at(i).ySpeed);
            }
            imageRects.at(i).rect.x+=imageRects.at(i).xSpeed;
            imageRects.at(i).rect.y+=imageRects.at(i).ySpeed;
            //blit the rect to the screen:
            SDL_BlitSurface(gImage, NULL, gSurface, &imageRects.at(i).rect);
        }
        //update screen:
        SDL_UpdateWindowSurface(gWindow);
        for (int i = 0; i < imageRects.size(); i++){
            SDL_FillRect(gSurface, &imageRects.at(i).rect,  SDL_MapRGB(gSurface->format, 0x00, 0x00, 0x00));
        }
        counter++;
        // SDL_Delay(1000/30);
    }
    close();
    static time_t endTime = time(NULL);
    printf("This game ran at %ldfps\n", counter/(endTime-startTime));
    return 0;
}