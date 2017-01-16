
#include <assert.h>
#include <cstdlib>
#include <iostream>
#include <sstream>
#include <string>
#include <unistd.h>
#include <RF24/RF24.h>

using namespace std;

/*extern "C" int DoIt()
{
    RF24* radio = new RF24(RPI_V2_GPIO_P1_22,BCM2835_SPI_CS0, BCM2835_SPI_SPEED_8MHZ);
// Setup and configure rf radio
    radio->begin();

radio->setRetries(15,15);
  // optionally, increase the delay between retries & # of retries
//  radio->setRetries(15,15);
  // Dump the configuration of the rf unit for debugging
    radio->printDetails();

    uint8_t address[] = { 0xCC,0xCE,0xCC,0xCE,0xCC };
    uint8_t address2[] = { 0xCC,0xCE,0xCC,0xCE,0xCC };
   radio->openWritingPipe(address);
   radio->openReadingPipe(1, address2);
      radio->startListening();
    radio->stopListening();

printf("More Details...");
    radio->printDetails();
    //radio->openWritingPipe(pipes[0]);
    //radio->openReadingPipe(1,pipes[1]);

printf("Now sending...\n");
                        unsigned long time = millis();

                        bool ok = radio->writeFast( &time, sizeof(unsigned long), 1 );
                                                if (!ok){
                                        printf("failed.\n");
                        }

        return 29;
}*/

/*
Creates a new RF24 instance.
Returns the new RF24 instance or nullptr if something went wrong.
*/
extern "C" RF24* ErhardtRF24Lib_RF24Create2(uint8_t cePin, uint8_t csnPin)
{
    return new RF24(cePin, csnPin);
}

/*
Creates a new RF24 instance.
Returns the new RF24 instance or nullptr if something went wrong.
*/
extern "C" RF24* ErhardtRF24Lib_RF24Create3(uint8_t cePin, uint8_t csnPin, uint32_t spiSpeed)
{
    return new RF24(cePin, csnPin, spiSpeed);
}

extern "C" void ErhardtRF24Lib_RF24Destroy(RF24* radio)
{
    if (radio != nullptr)
    {
        delete radio;
    }
}

/*
Returns a bool, non-zero if successful; zero if not successful.
*/
extern "C" int32_t ErhardtRF24Lib_Begin(RF24* radio)
{
    assert(radio != nullptr);

    return radio->begin();
}

extern "C" void ErhardtRF24Lib_PrintDetails(RF24* radio)
{
    assert(radio != nullptr);

    return radio->printDetails();
}

extern "C" void ErhardtRF24Lib_StartListening(RF24* radio)
{
    assert(radio != nullptr);

    radio->startListening();
}

extern "C" void ErhardtRF24Lib_StopListening(RF24* radio)
{
    assert(radio != nullptr);
    
    radio->stopListening();
}

extern "C" int32_t ErhardtRF24Lib_Available(RF24* radio)
{
    assert(radio != nullptr);
    
    return radio->available() == true;
}

extern "C" void ErhardtRF24Lib_Read(RF24* radio, void* buf, uint8_t len)
{
    assert(radio != nullptr);

    radio->read(buf, len);
}
 
extern "C" int32_t ErhardtRF24Lib_Write(RF24* radio, const void* buf, uint8_t len)
{
    assert(radio != nullptr);

    return radio->write(buf, len);
}

extern "C" void ErhardtRF24Lib_OpenWritingPipe(RF24* radio, const uint8_t* address)
{
    assert(radio != nullptr);

    radio->openWritingPipe(address);
}

extern "C" void ErhardtRF24Lib_OpenReadingPipe(RF24* radio, uint8_t number, const uint8_t* address)
{
    assert(radio != nullptr);

    radio->openReadingPipe(number, address);
}

extern "C" uint8_t ErhardtRF24Lib_GetChannel(RF24* radio)
{
    assert(radio != nullptr);

    return radio->getChannel();
}

extern "C" void ErhardtRF24Lib_SetChannel(RF24* radio, uint8_t channel)
{
    assert(radio != nullptr);

    radio->setChannel(channel);
}

extern "C" uint8_t ErhardtRF24Lib_GetPayloadSize(RF24* radio)
{
    assert(radio != nullptr);

    return radio->getPayloadSize();
}

extern "C" void ErhardtRF24Lib_SetPayloadSize(RF24* radio, uint8_t size)
{
    assert(radio != nullptr);

    radio->setPayloadSize(size);
}

extern "C" int32_t ErhardtRF24Lib_TestCarrier(RF24* radio)
{
    assert(radio != nullptr);

    return radio->testCarrier() == true;
}

extern "C" void ErhardtRF24Lib_DisableCRC(RF24* radio)
{
    assert(radio != nullptr);

    radio->disableCRC();
}

extern "C" void ErhardtRF24Lib_EnableDynamicPayloads(RF24* radio)
{
    assert(radio != nullptr);

    radio->enableDynamicPayloads();
}

extern "C" void ErhardtRF24Lib_EnableDynamicAcknowledge(RF24* radio)
{
    assert(radio != nullptr);

    radio->enableDynamicAck();
}

extern "C" void ErhardtRF24Lib_EnableAcknowledgePayload(RF24* radio)
{
    assert(radio != nullptr);

    radio->enableAckPayload();
}

extern "C" void ErhardtRF24Lib_SetAutoAcknowledge(RF24* radio, int32_t enable)
{
    assert(radio != nullptr);

    radio->setAutoAck(enable != 0);
}

extern "C" int32_t ErhardtRF24Lib_SetDataRate(RF24* radio, int32_t speed)
{
    assert(radio != nullptr);

    rf24_datarate_e datarate;
    switch (speed)
    {
        case 0:
            datarate = RF24_1MBPS;
            break;
        case 1:
            datarate = RF24_2MBPS;
            break;
        case 2:
            datarate = RF24_250KBPS;
            break;
        default:
            return 0;
    }

    return radio->setDataRate(datarate);
}

extern "C" void ErhardtRF24Lib_SetAddressWidth(RF24* radio, int32_t width)
{
    assert(radio != nullptr);

    uint8_t addressWidth;
    switch (width)
    {
        case 0:
            addressWidth = 3;
            break;
        case 1:
            addressWidth = 4;
            break;
        case 2:
            addressWidth = 5;
            break;
        default:
            assert(false);
            return;
    }

    return radio->setAddressWidth(addressWidth);
}

extern "C" void ErhardtRF24Lib_SetRetries(RF24* radio, uint8_t delay, uint8_t count)
{
    assert(radio != nullptr);

    radio->setRetries(delay, count);
}

extern "C" uint32_t ErhardtRF24Lib_GetPowerAmplifierLevel(RF24* radio)
{
    assert(radio != nullptr);

    uint8_t paLevel = radio->getPALevel();
    switch (paLevel)
    {
        case RF24_PA_MIN:
            return 0;
        case RF24_PA_LOW:
            return 1;
        case RF24_PA_HIGH:
            return 2;
        case RF24_PA_MAX:
            return 3;
        default:
            assert(false);
            return 99;
    }
        
}

extern "C" void ErhardtRF24Lib_SetPowerAmplifierLevel(RF24* radio, uint32_t level)
{
    assert(radio != nullptr);

    uint8_t paLevel;
    switch (level)
    {
        case 0:
            paLevel = RF24_PA_MIN;
            break;
        case 1:
            paLevel = RF24_PA_LOW;
            break;
        case 2:
            paLevel = RF24_PA_HIGH;
            break;
        case 3:
            paLevel = RF24_PA_MAX;
            break;
        default:
            assert(false);
            return;
    }

    return radio->setPALevel(paLevel);
}
