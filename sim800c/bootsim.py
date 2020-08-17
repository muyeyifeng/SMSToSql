from machine import Pin
#启动SIM800C
p2 = Pin(2, Pin.OUT)
p2.value(0)
utime.sleep_ms(3000) 
p2.value(1)
