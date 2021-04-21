为什么不使用github的wiki而是使用mkdocs做文档管理？

目前 [KSFramework](https://github.com/mr-kelly/KSFramework) 是使用mkdocs来做**在线文档** 而非使用github的wiki，这是为什么呢？



### 在windows下搭建wiki本地预览不成熟

gollum：[https://github.com/gollum/gollum](https://github.com/gollum/gollum)

A simple, Git-powered wiki with a sweet API and local frontend.

（一个简单的支持git wiki，提供本地预览）

在gollum的说明文档中提到，在windows下使用目前并不成熟，不建议在正式环境中使用

> **Warning**: Windows support is still in the works! [Many things do not work right now](https://github.com/gollum/gollum/issues/1044), and [many of the tests are currently failing](https://github.com/gollum/gollum/issues/1044#issuecomment-126784479). Proceed at your own risk!



### github wiki相关资料

安装本地预览工具：https://blog.csdn.net/zangxueyuan88/article/details/81115652

编辑指南：https://lpd-ios.github.io/2017/07/11/GitHub-Wiki-Introduction/





### mkdocs不兼容github的wiki

因为mkdocs是完全静态的html，而wiki是动态在server上利用markdown渲染出html的，所以无法兼容wiki。

可查看这个讨论：[Mkdocs-compatible wiki? #976](https://github.com/mkdocs/mkdocs/issues/976)