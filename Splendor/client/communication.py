##Nathan Hinton

import socket

class Communicate:
    def __init__(self, HOST = "127.0.0.1", PORT = 8888):
        print("Initalizing connection to server...")
        self.HOST = HOST
        self.PORT = PORT
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.sock.connect((self.HOST, self.PORT))
        print("Connection established!")

    def sendData(self, data):
        self.sock.sendall(data)
    
    def getData(self):
        pass


