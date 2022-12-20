// Nathan Hinton
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

char *strcpy(char *dest, const char *src);

int main(void) {
    char *months[12] = {
    "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"
    };
    // months[0] = strcpy(months[0], "January");
    char **p = months;
    for(int ctr = 0; ctr < 12; ctr++) {
        char *t = p[ctr];
        
        for(int tmp = 0;  tmp < strlen(t); t++) {
            printf("%c", t);
        }
        printf("\n");
    }
    printf("%d\n", sizeof(months));
    return 0;
}