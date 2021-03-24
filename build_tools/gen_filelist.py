"""
Author: zhaoqingqing(569032731@qq.com)
Date: 2019/11/23 19:26
Desc: 1.遍历当前目录下的所有文件生成filelist
        格式：filepath,版本号,文件大小
      2.生成version.txt到Product目录

     在python3.7.4+win10下测试通过
"""

# coding=utf-8
import hashlib
import os
import shutil
import sys
import time
import gen_hotfix_res

filelist_name = 'filelist.txt'
version_name = 'version.txt'
dst_path = ""  # 目标路径


def isIgnore(name):
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
    if bname.find(".") < 0:
        return True
    if bname.startswith("."):
        return True
    if bname.startswith("filelist"):
        return True
    if bname.startswith("version"):
        return True
    return False


def GetFileMd5(filename):
    if not os.path.isfile(filename):
        return
    myhash = hashlib.md5()
    f = open(filename, 'rb')
    # 分段读取文件并计算md5,对大文件友好
    while True:
        b = f.read(8096)
        if not b:
            break
        myhash.update(b)
    f.close()
    return myhash.hexdigest()


def makeFileLists(dir, save_path):
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

    new_str = ""
    for fpathe, dirs, fs in os.walk(os.path.abspath(os.path.dirname(dst_path))):
        for f in fs:
            path_name = os.path.join(fpathe, f)
            if not isIgnore(path_name):
                sname = path_name.replace(dir, "")
                version = GetFileMd5(path_name)
                size = str(os.path.getsize(path_name));
                line = "{0},{1},{2}".format(sname, version, size)
                new_str = new_str + line + "\n"
                # print("find:" ,line)

    # 生成filelist
    old_str = ""
    if not os.path.exists(bak_filelist):
        with open(save_path, 'w', encoding='utf8') as fw:
            fw.write(new_str)
            fw.flush()
        print("filelist生成完成")
    else:
        sw = os.path.isfile(bak_filelist) and open(bak_filelist, 'r')
        if sw:
            old_str = sw.read()
            sw.close()
        if new_str == old_str and os.path.exists(save_path):
            print("filelist无需更新")
        else:
            sw = open(save_path, "w")
            sw.write(new_str)
            sw.close()
            print("filelist有更新，生成完成")


if __name__ == "__main__":
    try:
        print("参数列表：", str(sys.argv))
        # 未传入参数则使用脚本所在路径
        dst_path = r"E:\Code\KSFramework\KSFramework\Product\Bundles\Windows\\"
        bak_path = sys.argv[0]
        platform = "Windows"
        if (len(sys.argv) >= 3):
            dst_path = sys.argv[1]
            bak_path = sys.argv[2]
            platform = sys.argv[3]
        dir = os.path.abspath(os.path.dirname(dst_path)) + "\\"
        bak_dir = os.path.abspath(os.path.dirname(bak_path)) + "\\"
        bak_filelist = bak_dir + filelist_name + '.bak'
        if os.path.exists(dir + filelist_name):
            shutil.copyfile(dir + filelist_name, bak_filelist)
            print("备份filelist文件：", bak_filelist)

        print("要生成filelist的目录为：", dir)
        makeFileLists(dir, dir + filelist_name)

        # 生成version.txt 1.生成zip 2.写入version.txt 3.删除zip
        product_dir = dir + '../../'
        gen_hotfix_res.zip_dir(product_dir + "Lua", product_dir + "lua.zip", False)
        gen_hotfix_res.zip_dir(product_dir + "Setting", product_dir + "setting.zip", False)
        ver_list = [product_dir + "lua.zip", product_dir + "setting.zip", dir + filelist_name]
        gen_hotfix_res.genVersion(product_dir + platform + "-" + version_name, ver_list)
        os.remove(product_dir + "lua.zip")
        os.remove(product_dir + "setting.zip")
    except Exception as ex:
        print
        'Exception:\r\n'
        print
        ex
    os.system("pause")
