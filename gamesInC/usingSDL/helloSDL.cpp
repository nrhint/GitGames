#include <SDL2/SDL.h>
#include <stdio.h>

const int SCREEN_WIDTH = 640, SCREEN_HEIGHT = 480;

int main(int argc, char* argv[]) {
    SDL_Window *window = NULL;
    SDL_Surface *screenSurface = NULL;
    if(SDL_Init(SDL_INIT_VIDEO) <0) {
        printf("Could not initalize SDL. SDL ERROR: %s\n", SDL_GetError());
        exit(1);
    }
    window = SDL_CreateWindow("SDL Tutorial", SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, SCREEN_WIDTH, SCREEN_HEIGHT, SDL_WINDOW_SHOWN);
    if (window == NULL) {
        printf("Failed to create window. SDL Error: %s\n", SDL_GetError());
        exit(1);
    }
    screenSurface = SDL_GetWindowSurface(window);
    SDL_FillRect(screenSurface, NULL, SDL_MapRGB(screenSurface->format, 0xFF, 0xFF, 0xFF));
    SDL_UpdateWindowSurface(window);

    SDL_Event e;
    bool quit = false;
    while(quit == false) {
        while (SDL_PollEvent(&e)) {
            if (e.type == SDL_QUIT) {
                quit = true;
            }
        }
    }
    SDL_DestroyWindow(window);
    SDL_Quit();
    return 0;
}