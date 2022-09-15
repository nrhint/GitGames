##Nathan Hinton
##This will have utilities for the subtraction blast program

def doesThisColide(pointList1, pointList2):
    # results = []
    for checkThisPoint in pointList1:
        if point_in_polygon(pointList2, checkThisPoint) == True:
            return True
        """
        for index in range(len(pointList2)):
            pointPair = (pointList2[index-1], pointList2[index])
            run = pointPair[0][0]-pointPair[1][0]
            rise = pointPair[0][1]-pointPair[1][1]
            try:
                b = pointPair[0][1]-((rise/run)*pointPair[0][0])
                results.append(checkThisPoint[1] < (rise/run)*checkThisPoint[0]+b and checkThisPoint[0] <(run/rise)*checkThisPoint[1]+b)
            except ZeroDivisionError:
                if rise == 0:
                    results.append(checkThisPoint[0] <= run)
                else:
                    results.append(checkThisPoint[1] <= rise)"""
    # if results.count(False) == 1 or results.count(True) == 1:
    #     print("Point found?")
    #print("Checked for colisions")
    return False

def point_in_polygon(polygon, point):
    """
    Raycasting Algorithm to find out whether a point is in a given polygon.
    Performs the even-odd-rule Algorithm to find out whether a point is in a given polygon.
    This runs in O(n) where n is the number of edges of the polygon.
     *
    :param polygon: an array representation of the polygon where polygon[i][0] is the x Value of the i-th point and polygon[i][1] is the y Value.
    :param point:   an array representation of the point where point[0] is its x Value and point[1] is its y Value
    :return: whether the point is in the polygon (not on the edge, just turn < into <= and > into >= for that)
    """

    # A point is in a polygon if a line from the point to infinity crosses the polygon an odd number of times
    odd = False
    # For each edge (In this case for each point of the polygon and the previous one)
    i = 0
    j = -1
    while i < len(polygon):
        #print(i, j)
        # If a line from the point into infinity crosses this edge
        # One point needs to be above, one below our y coordinate
        # ...and the edge doesn't cross our Y corrdinate before our x coordinate (but between our x coordinate and infinity)

        if (((polygon[i][1] > point[1]) != (polygon[j][1] > point[1])) and
            (point[0] > ((polygon[j][0] - polygon[i][0]) * (point[1] - polygon[i][1]) / (polygon[j][1] - polygon[i][1])) + polygon[i][0])):
            # Invert odd
            odd = not odd
        j = i
        i = i + 1
    # If the number of crossings was odd, the point is in the polygon
    return odd