import urequests
#发送PDU消息
# raw
if len(raw)>8:
    url = 'http://192.168.1.26/'
    response = urequests.get('http://192.168.1.26/?raw='+raw)
    #print(response.text)
