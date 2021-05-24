## 开发计划

- [ ] 文档的完善，补充近期新增的功能
- [ ] 增加一些组件，比如UI上显示3D模型，UI上显示特效
- [ ] 开发DEMO小游戏，延伸热重载的范围
- [ ] 对lua config 进行热重载
- [ ] Editor脚本限制使用Standard Shader
- [ ] 对常见ab资源进行缓存
- [ ] \#if UNITY_5 || UNITY_2017_1_OR_NEWER 改成只有一个宏
- [ ] 去掉所有未到的代码，减少IL2CPP的出包时间
- [ ] 场景加载的管理
- [ ] 下载更新脚本在mac上的正常运行
- [ ] 全部的www升级为UnityWebRequest

### UI方面

- [ ] 增加是否顶层UI的复选框，mmo横屏游戏常用

## 已完成

- [x] 给KSFramework增加下载更新功能
- [x] 增加对SpriteAtlas图集的管理
- [x] 编译excel的做成独立tablemlgui，已开源
- [x] tableml增加把配置数据导出lua
- [x] 处理掉代码中的Warning