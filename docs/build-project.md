### 工程打包方法

使用菜单栏的**KEngine** - **Autobuilder** - **Build Android**或(其它平台)

不建议使用**File** - **Build Settings** - **Build**，因为Autobuilder会处理一些操作，

比如自动生成xlua代码，自动拷贝Product目录下的资源到StreamingAssets目录下，并且生成的安装包会自动放在**Product/Apps/Android**或**Product/Apps/Windows**下，整个过程是自动化的，可和Jenkins等自动化打包平台集成。

### Link 资源

打包一次后在StreamAsset目录会Link product的资源，Link之后两边的资源是同步的