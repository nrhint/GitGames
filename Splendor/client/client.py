##Nathan Hinton

import socket

HOST = "127.0.0.1"#socket.gethostbyname("www.hintonclan.org")  # The server's hostname or IP address
PORT = 8888  # The port used by the server

print(HOST)
#63.248.204.107
#63.248.204.107

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    s.connect((HOST, PORT))
    s.sendall(b"Hello, world")
    data = s.recv(1024)

print("Received %s" %data)
