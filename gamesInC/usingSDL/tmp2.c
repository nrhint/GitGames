struct student {
	char *first_name;
	char *last_name;
	struct student *next;
};

struct student *delete_student(struct student *first, char *last_name, char *first_name);

#include <stdlib.h>
#include <stdio.h>
struct student *delete_student(struct student *first, char *last_name, char *first_name) {
    //Begin
    struct student *curr = first, *prev = NULL;
    int found = 0;
    //loop until the last names are matching
    while (curr->last_name != last_name && curr != NULL) {
        prev = curr;
        curr = curr->next;
    }
    //while in the last names that match check for first names that
    //match then break to stop the pointers there
    while (curr->last_name == last_name) {
        if (curr->first_name == first_name) {
            found = 1;
            break;
        } else {
            prev = curr;
            curr = curr->next;
        }
    }
    if (found == 1) {
        //apply logic to modify the linked list:
        if (prev == NULL) {
            //The first item matched, inncriment curr to point to second item and free the first
            prev = curr;
            curr = curr->next;
            free(prev);
            return curr;
        } else if (curr == NULL) {
            //The item was not found in the list
            printf("Student %s %s was not found in list", last_name, first_name);
        } else if (curr->next == NULL) {
            //The last item in the list
            //free the current and then set the prev to null
            free(curr);
            prev->next = NULL;
        } else {
            //Remove the current item from the list
            prev->next = curr->next;
            free(curr);
        }
    } else {
        printf("Student %s %s was not found in list", last_name, first_name);
    }
    //return the list
    return first;
}

int main(void) {
    struct student *students;
    students = (struct student *) malloc(sizeof(struct student));
    students->first_name = "Camille";
    students->last_name = "Fenton";
    struct student *tmp;
    tmp = (struct student *) malloc(sizeof(struct student));
    tmp->first_name = "Nathan";
    tmp->last_name = "Hinton";
    students->next = tmp;
    students->next = tmp;
    struct student *tmp2;
    tmp2 = (struct student *) malloc(sizeof(struct student));
    tmp2->first_name = "Eric";
    tmp2->last_name = "Hinton";
    tmp->next = tmp2;
    students = delete_student(students, "Fenton", "Camille");
    return 0;
}