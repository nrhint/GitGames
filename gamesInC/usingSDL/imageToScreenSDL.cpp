//https://lazyfoo.net/tutorials/SDL/02_getting_an_image_ on_the_screen/index.php
#include <SDL2/SDL.h>
#include <stdio.h>
#include <time.h>

const int SCREEN_WIDTH = 640, SCREEN_HEIGHT = 480;

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
    // gImage->format->Amask = 0xFF000000;
    // SDL_SetColorKey(gImage, SDL_SetColorKey, SDL_MapRGB(gImage->format, 0, 0, 0, 255));
    // SDL_SetSurfaceBlendMode(gImage, SDL_BLENDMODE_BLEND);
    // gImage->format->Amask = 0xFF000000;
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

int main(int argc, char *argv[]) {
    int counter = 1000;
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
        //update screen:
        SDL_BlitSurface(gImage, NULL, gSurface, NULL);
        SDL_UpdateWindowSurface(gWindow);
    }
    close();
    return 0;
}