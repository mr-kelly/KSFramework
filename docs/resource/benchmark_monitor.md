# 性能统计器

## 操作步骤

1. 打开Appconfig.cs把IsSaveCostToFile开关打开
2. 运行游戏，耗时统计数据就会记录下来
3. 在Editor下中菜单栏点击 **KEngine** - **Open PresistenDataPath Folder**，就能看到记录的数据文件了



![资源加载和函数执行耗时表](./../images/resource/benchmark_monitor_files.png)



## 界面打开耗时统计表

监视器统计到的界面耗时数据如下所示，对应profiler_ui_2021-7-29 17.40.49.csv

| UI名字     | 操作(函数) | 耗时(ms) |
| ---------- | ---------- | -------- |
| Billboard  | LoadAB     | 0.127    |
| Login      | LoadAB     | 0.516    |
| Login      | OnInit     | 0.004    |
| Navbar     | LoadAB     | 0.109    |
| Main       | LoadAB     | 0.103    |
| Navbar     | OnInit     | 0        |
| Main       | OnInit     | 0.003    |
| UIRoleInfo | LoadAB     | 0.008    |
| UIRoleInfo | OnInit     | 0        |

## 加载资源耗时统计表

监视器统计到的加载资源耗时数据如下所示，对应profiler_loadab_2021-7-29 17.40.48.csv

| AB资源                       | 耗时   |
| ---------------------------- | ------ |
| uiatlas/atlas_common.ab      | 2.623  |
| uiatlas/anim1/0002.ab        | 4.169  |
| uiatlas/anim1/0004.ab        | 4.169  |
| scene/scene1001/scene1001.ab | 4.17   |
| scene/scene1002/scene1002.ab | 10.911 |
