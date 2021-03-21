"""
Author: zhaoqingqing(569032731@qq.com)
Date: 2021/3/3 20:01
Desc:发布热更新资源到cdn
        压缩lua和setting目录为zip文件
        拷贝ab文件到cdn目录下
    在python3.7.4+win10下测试通过
"""

# coding=utf-8
import os
import shutil
import sys
import zipfile
import gen_filelist

version_name = 'version.txt'

def zip_dir(dirname, zipfilename,backup):
    if os.path.exists(zipfilename):
        if backup :
            shutil.copyfile(zipfilename, zipfilename+'.bak')
        os.remove(zipfilename)

    filelist = []
    if os.path.isfile(dirname):
        filelist.append(dirname)
    else:
        for root, dirs, files in os.walk(dirname):
            for dir in dirs:
                filelist.append(os.path.join(root, dir))
            for name in files:
                filelist.append(os.path.join(root, name))

    zf = zipfile.ZipFile(zipfilename, "w", zipfile.zlib.DEFLATED)
    for tar in filelist:
        arcname = tar[len(dirname):]
        # 参数二：zip文件夹内的名字，可以保留到去掉根目录的层级
        zf.write(tar, arcname)
    zf.close()

def genVersion(fname, name_list):
    f = open(fname, "w")
    for name in name_list:
        if os.path.exists(name):
            version = gen_filelist.GetFileMd5(name)
            size = str(os.path.getsize(name));
            bname = os.path.basename(name)
            line = "{0},{1},{2}{3}".format(bname, version, size,"\n")
            f.write(line)
        else:
            print("genVersion路径不存在", name)
    f.close()
    print("version更新完成")

if __name__ == "__main__":
    try:
        start_path = sys.argv[0]
        #start_path = r'E:\Code\KSFramework\build_tools'
        if (len(sys.argv) >= 2):
            start_path = sys.argv[1]
        dst_root = start_path + "\cdn\\"
        src_path = start_path + r'\..\KSFramework\Product\\'
        if not os.path.exists(dst_root):
            os.makedirs(dst_root)
            print("not exist path,create", dst_root)
        print(dst_root,src_path)

        #压缩为zip，经测试对同一目录多次zip，md5相同(文件未改变的情况)
        zip_dir(src_path + 'Lua', dst_root + 'lua.zip',True)
        zip_dir(src_path + 'Setting', dst_root + 'setting.zip',True)
        print("生成zip文件完成")

        src_ab = src_path + '\Bundles\\'
        dst_ab = dst_root + 'Bundles\\'
        if os.path.exists(dst_ab):
            shutil.rmtree(dst_ab)
            print("exist path,delete", dst_ab)
        shutil.copytree(src_ab, dst_ab)
        print("同步ab文件{0}->{1} 完成".format(src_ab, dst_ab))

        genVersion(dst_root+version_name,[dst_root+"lua.zip",dst_root+'setting.zip',dst_ab + 'Windows\\filelist.txt'])
    except Exception as ex:
        print
        'Exception:\r\n'
        print
        ex
    os.system("pause")
