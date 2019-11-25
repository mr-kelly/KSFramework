"""
Author: zhaoqingqing(569032731@qq.com)
Date: 2019/11/23 19:26
Desc: 遍历当前目录下的所有文件生成filelist
    在python3.7.4+win10下测试通过
"""

# coding=utf-8
import os
import sys
import time


filelist_name='filelist.txt'
version_name='version.txt'

def isIgnore(name):
    # if name.find("/")<0:
    #     return True
    if name.endswith(".py"):
        return True
    if name.endswith(".bat"):
        return True
    if name.endswith(".manifest"):
        return True
    if name.endswith(".py"):
        return True
    if name.endswith(".meta"):
        return True
    bname = os.path.basename(name)
    if bname.find(".")<0:
        return True
    if bname.startswith("."):
        return True
    if bname.startswith("filelist"):
        return True
    if bname.startswith("version"):
        return True
    return False

def makeFileLists(dir,save_path):
    '''
        makeFileList 函数用于包含多层目录的文件列表创建
        Params:
            dir     ：最上层的目录路径
        Usage:
            makeFileLists('dir')
    '''
    # 判断路径是否存在
    if not os.path.exists(dir):
        print(dir, '，目录不存在，请检查')
        return

    new_str =""
    for fpathe, dirs, fs in os.walk(os.path.abspath(os.path.dirname(sys.argv[0]))):
        for f in fs:
            path_name = os.path.join(fpathe, f)
            if not isIgnore(path_name):
                sname = path_name.replace(dir,"")
                version = str(int(os.path.getmtime(path_name)))
                size = str(os.path.getsize(path_name));
                line = "{0},{1},{2}".format(sname,version,size)
                new_str = new_str +line+"\n"
                #print("find:" ,line)


    #生成filelist
    old_str = ""
    sw = os.path.isfile(save_path) and open(save_path, 'r')
    if sw:
        old_str=sw.read()
        sw.close()
    if new_str !=old_str:
        sw = open(save_path,"w")
        sw.write(new_str)
        sw.close()
        print("filelist有更新，生成完成")
    else:
        print("filelist无需更新")


def genVersion(fname,name_list):
    f=open(fname,"w")
    for name in name_list:
        if os.path.exists(name):
            version = str(int(os.path.getmtime(name)))
            bname = os.path.basename(name)
            line = "{0}={1}{2}".format(bname,version,"\n")
            f.write(line)
    f.close()


if __name__ == "__main__":
    try:
        dir = os.path.abspath(os.path.dirname(sys.argv[0])) + "\\"
        print("当前目录为：",dir)
        makeFileLists(dir,dir+filelist_name)
        genVersion(dir+version_name,[filelist_name])
    except Exception as ex:
        print
        'Exception:\r\n'
        print
        ex
    os.system("pause")