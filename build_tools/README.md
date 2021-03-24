本目录下的脚本搭配jenkins使用更佳，在我们实际项目中配合jenkins提供可视化打包流程，后续交由测试来进行打包和更新操作，解放客户端同事的工作。

制作更新文件的思路可以查看文档《[资源包更新](https://mr-kelly.github.io/KSFramework/advanced/autoupdate/)》，更多jenkins的资料也可以参考我的博客《[运维和打包](https://www.cnblogs.com/zhaoqingqing/tag/%E8%BF%90%E7%BB%B4%E5%92%8C%E6%89%93%E5%8C%85/)》



## 使用说明

1. 下载hfs，修改端口号为:8080，添加限速，保持hfs的端口和Appconfig.resUrl中设置的一致(建议把hfs切换到高级模式并导入本目录下的hfs.ini)
2. 双击生成filelist-xxx.bat，生成filelist
3. 双击gen_hotfix_res，制作更新资源包
4. 运行动hfs(可一直运行不关闭)，拖动cdn目录到hfs中
5. 运行游戏即可更新资源



## 文件服务器hfs

hfs是一款免费的资源服务器
	
hfs下载地址：http://www.rejetto.com/hfs/?f=dl

请自行下载hfs，并放到此目录下
	
关于hfs的使用可以参考我的博客《[文件服务器HFS](https://www.cnblogs.com/zhaoqingqing/p/9173317.html)》