
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



### 文档维护说明

安装python环境

1. 添加新的md文件，编写内容
2. **mkdocs.yml** 配置新加的md文件到此，配置目录结构
3. **server.bat** 开启本地服务器：在本地预览网站的生成效果 ，地址：http://127.0.0.1:8000
4. **deploy.bat** 把生成后的网站推送到github，会推送到默认的分支。



mkdocs文档： https://markdown-docs-zh.readthedocs.io/zh_CN/latest/