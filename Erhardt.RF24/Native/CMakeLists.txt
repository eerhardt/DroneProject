# g++ -g -shared -fPIC rf24lib.cpp -o Erhardt.RF24Lib.so -lrf24

find_package(RF24)

#FIND_PATH(NRF24_INCLUDE_DIR
#NAMES RF24.h nRF24L01.h
#PATHS /usr/include/nrf24 /usr/include/linux/nrf24 /usr/local/include
#)

#include_directories(${NRF24_INCLUDE_DIR})

add_library (Erhardt.RF24Lib pal_rf24lib.cpp)
