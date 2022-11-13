##Nathan Hinton

import pygame

pygame.init()
screen = pygame.display.set_mode((800, 640))
pygame.display.set_caption('Idle Breakout')

background = pygame.Surface(screen.get_size())
background = background.convert()
background.fill((0, 0, 0))

running = True
while running:
    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            running = False #End the main loop
    ##Update screen:
    print("Updating the screen")
    screen.blit(background, (0, 0))
    pygame.display.flip()

pygame.quit()

print("Finished!")