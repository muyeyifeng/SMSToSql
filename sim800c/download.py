import urequests
import os
r=urequests.get(url)
if r.status_code == 200 :
    with open(filename, 'wb') as f:
        f.write(r.content)
    print('download complete')
