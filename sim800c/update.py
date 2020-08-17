import uhashlib
import ubinascii
import urequests
import os
#更新文件
#url
hashs=urequests.get(url+'esp8266/hash')
if hashs.status_code == 200 :
    hashs=hashs.content.decode().split('\n')
    hashs=hashs[0:-1]
    for h in hashs:
        tmp=h.split("  ")
        sha1=uhashlib.sha1()
        try:
            file=open(tmp[1]).read()
            sha1.update(file)
            scii=ubinascii.hexlify(sha1.digest())
            if not scii.decode()==tmp[0]:
                print('file {} will update'.format(tmp[1]))
                locals={'url':url+'esp8266/'+tmp[1],
                        'filename':tmp[1]}
                os.remove(tmp[1])
                exec(open('./download.py').read(),locals)
                
        except:
            print('file {} not exist'.format(tmp[1]))
            locals={'url':url+'esp8266/'+tmp[1],
                        'filename':tmp[1]}
            exec(open('./download.py').read(),locals)
else:
    print(hashs.status_code)
