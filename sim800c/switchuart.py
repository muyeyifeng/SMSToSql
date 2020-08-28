import utime
from machine import UART

#对串口回信操作
if raws.find("SMS Ready")>=0:
    pass
if raws.find("CMGR:")>=0:
    raws=raws.split('\r\n')
    for raw in raws:
        utime.sleep_ms(1000)
        if not (raw.find("+CMGR")>=0 or raw.find("OK")>=0) :
            locals={'raw':raw,
                    'num':num,
                    'uart':uart
                    }
            exec(open('./sendraw.py').read(),locals)
