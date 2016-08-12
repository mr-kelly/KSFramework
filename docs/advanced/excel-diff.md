# Excel如何进行SVN协作、差异比较？

嗯，这是一个令人困惑的问题。
游戏开发、程序开发时，使用Excel可以添加文档、注释、图标、批注等等各种辅助信息；

但是Excel是非纯文本格式，在使用SVN、Git等版本管理软件时，多人进行编辑就会非常容易造成冲突，无法自动合并。 而且在冲突以后，我们很难得知究竟别人改动了哪里。

因此，总结一下以下方法，可以对Excel表格进行差异比较：

- 使用Beyond Compare比较Excel
- 使用TSV表格代替Excel
- TortoiseSVN的Excel表格比较
- Excel共享工作簿

## 使用Beyond Compare比较Excel

![Paste_Image.png](http://upload-images.jianshu.io/upload_images/1835687-0becccf484f6f3cd.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)

商业软件Beyond Compare具有Excel比较功能， 它类似首先把Excel当前打开Sheet转化成TSV，再进行比较。详情可查看[Beyond Compare的官方说明](http://www.scootersoftware.com/support.php?zz=kb_multisheetexcel)


## 使用TSV表格代替Excel


[KEngine](https://github.com/mr-kelly/KEngine)中，策划编辑的配置表经过编译，正是会变成TSV格式的文本文件。

原则上，策划编辑的配置表建议使用Excel的方式，可以方便的添加各种辅助信息；但也为了照顾部分人的习惯，也同时支持了直接编辑TSV文件方式：

![策划编辑TSV源文件](http://upload-images.jianshu.io/upload_images/1835687-bba2bb4e7f82e49a.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)

TSV源编辑文件，以.tsv格式为后缀放置到配置表源文件目录，变会进行编译。规范要求跟Excel一样，头部三行分别是列名、类型、注释。

TSV是一种纯文本格式，在Excel中对TSV表格做的润饰，如设置背景色、列宽等，能看到效果，但都不能被保存。

参照[KEngine](https://github.com/mr-kelly/KEngine)源码中的KEngine.UnityProject/Product/SettingSource/AppConfig+TSV.tsv文件。

## TortoiseSVN的Excel表格比较

TortoiseSVN客户端时具有Excel比较功能，发生文件冲突后，双击冲突的文件，它就直接调用Excel了，打开多个窗口了。冲突的部分，会用背景色红色标红，但是它的体验非常不好，经常让人摸不着头脑，搞不清楚哪里是改过的，哪里是删掉的；更何况，本身我的Excel文件里就有各种不同的背景色，十分混乱。不推荐。

## Excel共享工作簿

貌似微软提供了Excel文件的多人协作功能，这个没有用过，不论述了；
