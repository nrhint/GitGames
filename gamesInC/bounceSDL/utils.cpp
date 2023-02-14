//Nathan Hinton

double max(double a, double b) {
    if (a>b) {
        return a;
    }
    return b;
}

double min(double a, double b) {
    if (a>b) {
        return b;
    }
    return a;
}

bool checkForCollision(SDL_Rect recta, SDL_Rect rectb, bool checkCenter = false) {//Returns 0 if b passes through the top of a
    if ((recta.y < rectb.y+rectb.h) && (recta.y > rectb.y)) {
        if ((recta.x < rectb.x+rectb.w) && (recta.x > rectb.x)) {
            return true;
        }
    }
    if ((recta.y+recta.h < rectb.y+rectb.h) && (recta.y+recta.h > rectb.y)) {
        if ((recta.x+recta.w < rectb.x+rectb.w) && (recta.x+recta.w > rectb.x)) {
            return true;
        }
    }
    return false;
}