# DroneProject
Flying a quadcopter using Raspberry Pi and .NET Core

# How to run

## Install RF24 radio and RF24 library
Instructions coming soon. I used the wiring shown in http://hack.lenotta.com/arduino-raspberry-pi-switching-light-with-nrf24l01/ "Hardware"/"Raspberry Pi" section. If you are using different CE/CSN layouts, you will need to change [which pins are used in the code](https://github.com/eerhardt/DroneProject/blob/e42b23b9856b7de37449cb2567f38077011889a8/Erhardt.Multiprotocol/Symax.cs#L183).

## Installing .NET Core
Follow the [installation instructions](https://github.com/dotnet/core/blob/master/samples/RaspberryPiInstructions.md) for Ubuntu.

## Building 
To build the project, use the following steps:

- From `SampleApp`
 - `dotnet publish`
 - Copy `bin/Debug/netcoreapp1.0/publish/` `*.dll` and `*.deps.json` to your app's directory
 - Copy and edit the runtimeconfig.json as described in https://github.com/dotnet/core/blob/master/samples/RaspberryPiInstructions.md#creating-an-app
- On your Raspberry Pi, from the `Erhardt.RF24/Native/` directory
 - `cmake .`
 - `make install`
 - Copy the `Erhardt.RF24Lib.Native.so` from the `bin` directory at the repo root into your app's directory

## Run 
Run using `dotnet appDir/SampleApp.dll`
