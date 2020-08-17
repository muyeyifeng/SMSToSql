import network 
#连接wifi
sta_if = network.WLAN(network.STA_IF)
sta_if.active(True)
if not sta_if.isconnected():
    sta_if.connect(SSID,PASSWD)
    while not sta_if.isconnected():
        pass
