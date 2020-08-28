#必要模块
import utime
import uos
from machine import Pin
from machine import UART


#初始化串口
uart = UART(0, baudrate=115200,rx=Pin(3, Pin.IN),tx=Pin(1, Pin.OUT),timeout=1000,rxbuf=512)

globals={'SSID':'<SSID>',
         'PASSWD':'<PASSWD>',
         'url':'<URL>'
         }

#主程序
exec(open('./connwifi.py').read(),globals)
exec(open('./update.py').read(),globals)
exec(open('./bootsim.py').read())

#关闭REPL的端口监听
uos.dupterm(None, 1)

count=1

while True:
    utime.sleep_ms(5*1000)
    try:
        uart.write('AT+CMGR='+str(count)+'\r\n')
        utime.sleep_ms(1000)
        if uart.any():
            bin_data = uart.read()
            raws='{}'.format(bin_data.decode())
            if len(raws)>20:
                locals={'raws':raws,
                        'num':count,
                        'uart':uart
                        }
                exec(open('./switchuart.py').read(),locals)
                count+=1
            else:
                count=1
    except:
        continue
