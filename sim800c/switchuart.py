import utime
#对串口回信操作
#raws:
if raws.find("SMS Ready")>=0:
    pass
if raws.find("CMGR:")>=0:
    #uart.write(raws)
    raws=raws.split('\r\n')
    for raw in raws:
        utime.sleep_ms(1000)
        if not (raw.find("+CMGR")>=0 or raw.find("OK")>=0) :
            locals={'raw':raw}
            exec(open('./sendraw.py').read(),locals)
            #sendraw(raw)
