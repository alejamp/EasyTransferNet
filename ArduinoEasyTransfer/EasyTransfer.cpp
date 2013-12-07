#include "EasyTransfer.h"

//Captures address and size of struct
void EasyTransfer::begin( HardwareSerial *theSerial)
{
	_serial = theSerial;
	header = false;
	currentMessageType = -1;
}

void EasyTransfer::RegisterMessageType(uint8_t * ptr, uint8_t length)
{
	STRUCT_DESC sd;

	sd.address = ptr;
	sd.size = length;
	//dynamic creation of rx parsing buffer in RAM
	sd.rx_buffer = (uint8_t*) malloc(length);
	
	message_types[message_types_count] = sd;
	message_types_count++;	
}

//Sends out struct in binary, with header, length info and checksum
void EasyTransfer::sendData(byte message_type){
  STRUCT_DESC mt = message_types[message_type];
  uint8_t CS = mt.size;
  _serial->write(0x06);
  _serial->write(0x85);
  _serial->write(message_type);
  _serial->write(mt.size);
  for(int i = 0; i<mt.size; i++){
    CS^=*(mt.address+i);
    _serial->write(*(mt.address+i));
  }
  _serial->write(CS);

}

boolean EasyTransfer::receiveData(){
	//_serial->println(header);
  //start off by looking for the header bytes. If they were already found in a previous call, skip it.
  if(rx_len == 0 || header){
  //this size check may be redundant due to the size check below, but for now I'll leave it the way it is.
    if(_serial->available() >= 3){
		//this will block until a 0x06 is found or buffer size becomes less then 3.
		while(!header && (_serial->read() != 0x06)) {
			//This will trash any preamble junk in the serial buffer
			//but we need to make sure there is enough in the buffer to process while we trash the rest
			//if the buffer becomes too empty, we will escape and try again on the next call
			if(_serial->available() < 3)
				return false;
		}
		if (header || _serial->read() == 0x85){
			// get message type
			header = true;
			if (currentMessageType < 0)
			{
				if(_serial->available() < 1) return false; // type, len, data, cs
				currentMessageType = _serial->read();
				if (currentMessageType > -1) mt = message_types[currentMessageType];
			}
			if (rx_len == 0)
			{
				if(_serial->available() < 1) return false; // type, len, data, cs
				rx_len = _serial->read();
				mt.rx_len = rx_len;
			}

			//make sure the binary structs on both Arduinos are the same size.
			if(rx_len != mt.size){
			  //mt.rx_len = 0;
				 rx_len = 0;
				 header= false;
				 currentMessageType = -1;
			  return false;
			}
      }
    }
  }
  
  //we get here if we already found the header bytes, the struct size matched what we know, and now we are byte aligned.
  if(rx_len != 0){
    while(_serial->available() && mt.rx_array_inx <= rx_len){
      mt.rx_buffer[mt.rx_array_inx++] = _serial->read();
    }
    
    if(rx_len == (mt.rx_array_inx-1)){
      //seem to have got whole message
      //last uint8_t is CS
      calc_CS = rx_len;
      for (int i = 0; i<rx_len; i++){
        calc_CS^=mt.rx_buffer[i];
      } 
      
      if(calc_CS == mt.rx_buffer[mt.rx_array_inx-1]){//CS good
        memcpy(mt.address,mt.rx_buffer,mt.size);
		mt.rx_len = 0;
		rx_len = 0;
		mt.rx_array_inx = 0;
		currentMessageType = -1;
     	header= false;
		return true;
		}
		
	  else{
	  //failed checksum, need to clear this out anyway
		mt.rx_len = 0;
		rx_len=0;
		mt.rx_array_inx = 0;
		currentMessageType= -1;
        header= false;
		return false;
	  }
        
    }
  }
  
  return false;
}