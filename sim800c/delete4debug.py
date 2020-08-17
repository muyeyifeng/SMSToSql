import os

#测试时删除所有文件避免串口堵塞
name_list = os.listdir()
for name in name_list:
    #print("delete file :"+name)
    os.remove(name)
