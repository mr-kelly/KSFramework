
![KSFramework](https://github.com/mr-kelly/KSFramework/blob/master/Docs/KSFramework-logo.png)

[![Build status](https://ci.appveyor.com/api/projects/status/lt34ynvl3lac62ln/branch/master?svg=true)](https://ci.appveyor.com/project/mr-kelly/ksframework/branch/master)

### Branch说明

本分支是KSFramework官方文档的分支，用来存储官方文档的Markdown源文件。

如果你觉得文档有欠缺或描述不清晰的，欢迎提出，更加欢迎你pull request到这儿来，让更多开发人员受益。



### 关于KSFramework

[**K**Engine](https://github.com/mr-kelly/KEngine) + [**S**Lua](https://github.com/mr-kelly/slua)+ **Framework** = KSFramework


**[KSFramework](https://github.com/mr-kelly/KSFramework)是一个整合KEngine、SLua的Unity 5 Asset Bundle开发框架，并为程序、美术、策划、运营提供辅助工具集。**

---------------------

**热重载**是KSFramework的开发重点——在不重启游戏的前提下，重载代码、配置表可立刻看到修改效果，最大限度的提升开发、调试的速度，方便运营阶段热更新。

对于程序人员，可以使用AssetBundle加载与打包、脚本化的UI、配置表代码自动生成、下载更新等基础功能模块，大大减少游戏周边基础功能的工作量；

对于策划人员，使用Excel进行编辑，可以在编辑过程中添加注释、图标、预编译指令，KSFramework会根据配置内容自动生成代码供程序使用。

对于美术人员，只需将项目需要用到资源放到指定目录，将会自动的生成Asset Bundle；程序加载Asset Bundle跟Resources.Load一样方便。


对于运营人员，利用KSFramework的热重载特性，可以针对运营需求，在项目运行过程中配置表、脚本代码在用户无知觉的情况下进行热更新。



## 一起来维护文档？

### 搭建环境

安装python环境，安装python的pip(python3自带)，如果是python2或没有pip可参考《[使用Mkdocs构建你的项目文档](https://www.cnblogs.com/zhaoqingqing/p/7501062.html)》

通过pip安装mkdocs，新版本的mkdocs去掉原来的多级目录支持，同时也支持到了新版本

```python
pip install mkdocs
```

mkdocs文档： https://www.mkdocs.org/

（中文文档目前有些过时） https://markdown-docs-zh.readthedocs.io/zh_CN/latest/

### 旧版本mkdocs?

新版本的mkdocs去掉自定义左侧目录的功能(把多篇文章放在一大类下)，新旧配置格式差异对比

| mkdocs版本 | 单篇文档的格式                          |
| ---------- | --------------------------------------- |
| 1.0.4      | - [index.md, "简介", "KSFramework介绍"] |
| 1.1.2      | - "KSFramework介绍": index.md           |

安装旧版本的mkdocs

```python
pip install mkdocs==1.0.4
```

如果安装失败，使用这个历史版本

> pip install --no-index --find-links=/your_offline_packages/ package_name

```
pip install --no-index --find-links=/mkdocs-1.0.4-py2.py3-none-any.whl/ mkdocs
```

PS：在我的电脑上安装回旧版本的mkdocs后，依然无法使用旧版配置生成html，所以格式改成了新版本的格式



### 编写新文档

1. 添加新的md文件，编写内容，按照目录规范把md文件放到指定目录下
2. **mkdocs.yml** 配置新加的md文件到此，配置目录结构
3. **server.bat** 开启本地服务器：在本地预览网站的生成效果 ，地址：http://127.0.0.1:8000
4. **deploy.bat** 把生成后的网站推送到github，会推送到默认的分支。



