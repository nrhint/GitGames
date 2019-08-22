GAP = 127
WHITE = [8, 10, 11, 20, 22, 30, 32, 42, 50, 51]
BLUE = [4, 7, 8, 11, 12, 15, 19, 23, 30, 34, 42]

def first(lst):
    return lst[0]

def rest(lst):
    return lst[1:]

def empty(lst):
    return lst == []

def solve(gap, blocks, sln):
    if gap == 0:
        print(sln)
        return True
    if gap < 0:
        return False
    if empty(blocks):
        return False
    useThisBlock = solve(gap - first(blocks), rest(blocks), [first(blocks)]+sln)
    dontUseThisBlock = solve(gap, rest(blocks), sln)
    return useThisBlock or dontUseThisBlock

print("WHITE")
print(solve(GAP, WHITE, []))
print("BLUE")
print(solve(GAP, BLUE, []))
