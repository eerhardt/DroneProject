# DroneProject
Flying a quadcopter using Raspberry Pi and .NET Core

## Building 
To build the project, use the following steps:

- On your Raspberry Pi, from the `Erhardt.RF24/Native/` directory
 - `cmake .`
 - `make install`
 - Copy the `Erhardt.RF24Lib.Native.so` from the `bin` directory at the repo root into your project directory
- From `SampleApp`
 - `dotnet publish`
 - Copy `bin\Debug\netcoreapp1.0\*.dll` to your project directory

## Run 
Run using `./corerun SampleApp.dll`
 
