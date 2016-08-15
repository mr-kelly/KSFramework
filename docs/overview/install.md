

## 从产品包安装

您可以从[KSFramework Release](https://github.com/mr-kelly/KSFramework/releases)页面下载最新版本的产品包。

将压缩包解压后，直接双击场景KSFramework/Assets/Game.unity，或直接用Unity打开KSFramework目录。

> 如遇到无法下载的网络问题, 备选下载站:
> - [KSFramework Appveyor Artifacts](https://ci.appveyor.com/project/mr-kelly/ksframework/branch/master/artifacts): 包含每次提交的构建结果
> - [KSFramework OSChina镜像](http://git.oschina.net/mrkelly/KSFramework)): 国内的镜像Git


## 从源码安装

获取到源码后，需要通过git submodule命令获取KEngine和SLua
```shell
git submodule init
git submodule update
```
拉取submodule后，Windows下双击执行源码Install.bat进行安装，把KEngine和SLua相关代码链接到KSFramework各目录，然后用Unity打开
