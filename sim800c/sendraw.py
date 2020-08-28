import urequests
from machine import UART

#发送PDU消息
if len(raw)>8:
    url = '<URL>'
    response = urequests.get(url+'?raw='+raw)
    re=response.text
    if(re=='Error data' or re=='False'):
        pass
        #uart.write(re)
    elif(re=='True'):
        #uart.write(re)
        uart.write('AT+CMGD='+str(num)+'\r\n')
        #pass
        
