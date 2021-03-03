"""
Author: zhaoqingqing(569032731@qq.com)
Date: 2021/3/3 15:01
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


def zip_dir(dirname, zipfilename):
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

        src_lua = src_path + 'Lua'
        src_setting = src_path + 'Setting'
        zip_dir(src_lua, dst_root + 'lua.zip')
        zip_dir(src_setting, dst_root + 'setting.zip')
        print("生成zip文件完成")

        src_ab = src_path + '\Bundles\\'
        dst_ab = dst_root + 'Bundles\\'
        print("同步ab文件{0}->{1}".format(src_ab, dst_ab))
        if os.path.exists(dst_ab):
            shutil.rmtree(dst_ab)
            print("exist path,delete", dst_ab)
        shutil.copytree(src_ab, dst_ab)


    except Exception as ex:
        print
        'Exception:\r\n'
        print
        ex
    os.system("pause")
