cmake_minimum_required(VERSION 2.8)
project(Erhardt.RF24Lib.Native)

set(CMAKE_INSTALL_PREFIX ../../bin)
set(CMAKE_SHARED_LIBRARY_PREFIX "")
add_definitions(-std=c++11)

include_directories(/usr/local/include)

FIND_LIBRARY(NRF24_LIBRARY
    NAMES rf24
    PATHS /usr/lib /usr/local/lib
)

add_library (Erhardt.RF24Lib.Native
    SHARED
    pal_rf24lib.cpp
)

target_link_libraries(Erhardt.RF24Lib.Native
     ${NRF24_LIBRARY}
)

install (TARGETS Erhardt.RF24Lib.Native DESTINATION .)
