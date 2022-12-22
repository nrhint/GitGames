##Nathan Hinton
##This is the entry point for the serever

import queue
import socket
import sys
import threading
import _thread

import threadTasks

HOST = ''    # Symbolic name, meaning all available interfaces
PORT = 8888    # Arbitrary non-privileged port

s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
print('Socket created')

#Bind socket to local host and port
try:
    s.bind((HOST, PORT))
except socket.error as msg:
    print('Bind failed. Error Code : ' + str(msg[0]) + ' Message ' + msg[1])
    sys.exit()
    
print('Socket bind complete')

#Start listening on socket
s.listen(4)
print('Socket now listening')

q = queue.Queue()
_thread.start_new_thread(threadTasks.GameThread(2, q))
playerCount = 0

#now keep talking with the client
while 1:
    #wait to accept a connection - blocking call
    conn, addr = s.accept()
    print('Connected with ' + addr[0] + ':' + str(addr[1]))
    print("Starting game thread for connection %s" %addr[0])
    _thread.start_new_thread(threadTasks.PlayerThread (conn, q))
    threadTasks.PlayerThread(q)
    playerCount += 1
    q.put("playerCount%s"%playerCount)
s.close()