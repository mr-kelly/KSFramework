
![KSFramework](https://github.com/mr-kelly/KSFramework/blob/master/Docs/KSFramework-logo.png)

[![Build status](https://ci.appveyor.com/api/projects/status/lt34ynvl3lac62ln/branch/master?svg=true)](https://ci.appveyor.com/project/mr-kelly/ksframework/branch/master)

## Branch说明

本分支是KSFramework官方文档的分支，用来存储官方文档的Markdown源文件。

如果你觉得文档有欠缺或描述不清晰的，欢迎提出，更加欢迎把你的修改pull request到这儿来，让更多开发人员受益。



## 一起来维护文档？

### 搭建环境

安装[python](https://www.python.org/)环境，安装python的pip(python3自带可忽略)，如果是python2或没有安装pip可参考《[使用Mkdocs构建你的项目文档](https://www.cnblogs.com/zhaoqingqing/p/7501062.html)》

通过pip安装mkdocs，新版本的mkdocs(1.1.2)的多级目录配置和旧版本(1.0.4)有区别，目前yml的配置已升级到最新版本的格式。

```python
pip install mkdocs
```

mkdocs文档： [https://www.mkdocs.org/user-guide/writing-your-docs/](https://www.mkdocs.org/user-guide/writing-your-docs/)

中文文档(内容有些过时） [https://markdown-docs-zh.readthedocs.io/zh_CN/latest/](https://markdown-docs-zh.readthedocs.io/zh_CN/latest/)



### 编写新文档步骤

1. 创建新的md文件，编写内容，按照目录规范把md文件放到指定主题目录下
2. 在**mkdocs.yml** 中配置新加的md文件，并放在指定的层级下
3. 运行**server.bat** 开启本地服务器：在本地预览网站的生成效果 ，地址：http://127.0.0.1:8000
4. 本地预览没问题之后，运行**deploy.bat** 把生成后的网站推送到github，外网就会是最新的文档。

注意：需要先执行server.bat，再执行deploy.bat才能更新远程的文档。



### 文档编写注意事项

一个md文件只有一个h1，多个h2和多个h3，另外在mkdoc中，如果在一篇文章中出现两个h1则在大纲处只能显示出一个