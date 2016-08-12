
# 版本号

KSFramework中的版本号，在Gihub推荐的[语义化版本规范](http://semver.org/)三位版本号基础上，后边增加了3项描述信息：

> **MAJOR . MINOR . PATCH** . *BUILD* . *TYPE* . *DESC*

- MAJOR 主版本号：当你做了不兼容的 API 修改，
- MINOR 次版本号：当你做了向下兼容的功能性新增，
- PATCH 修订号：当你做了向下兼容的问题修正。
- BUILD 编译号：使用全局唯一的标识，如持续集成系统（比如Jenkins/QuickBuild）任务中自动递进的Build号码，或SVN号。
- TYPE 版本类型：一个字符串，可以放入注入beta, alpha, release等标识。
- DESC 其它描述：字符串，放置一些额外的编译信息，如日期、语言等等杂项，自由发挥。

在KEngine中，内置的版本号解析类AppVersion，基于以上的准则进行解析。
为什么要有如上的设计？这要分别从Android、iOS的版本号规范说起。


## iOS版本号要求

iOS开发时，在Xcode中，有两个版本号的填写，分别是Version和Build;其中Version强制要求必须是三位数的版本号，如1.0.0，否则无法提交App Store；
而Build号则没有要求，你可以写上一个唯一数字如1234，也可以写上更长的版本号如1.0.0.1234（建议）。

iOS系统判断一个App的版本号，使用的是Version，即3位数版本号。在App Store中显示的时候，只显示三位数的版本号。


## Android版本号要求

Android开发时，有两个版本号需要填写，分别是Version和VersionCode，其中Version类似于iOS开发中的Build，可以写上比较长的版本号，没有限制。
而VersionCode则强制要求，必须是一个数字，并且在提交Store时，每一次都要比上一次大。

在显示的时候，Android应用一般显示为1.0.0.1234 (Build 1234)，即Version和VersionCode组合显示。

## KEngine中的Build号

KEngine中的Build号设计初衷，是为了让持续集成系统（如Jenkins）每一次编译出的包都是唯一的，绝不重复，是一个身份证。当你拿到一个版本后，只要看到Build号，你就可以追溯到这个版本的来源、编译时间、编译原因等等。

Build号的关键，是需要配合自动化构建系统，因为这个数字是自动生成、自动递增的。 另一个可选项是使用Svn号，但是Svn号随着提交次数，数字经常变得非常大，变得非常难看。

KEngine中的Build号，直接应该被用做Android的VersionCode号、也可以用作iOS的中的Build号。

## 版本号中的Type

KSFramework中AppVersion的版本号，Type被单独的列了出来。因为Type在实际项目开发中有着重要的作用。实际开发中，我们经常希望制作出不同类型的版本，如开发版，面向内部人员，带有调试信息；测试版，提供测试指令，面向测试人员；正式版，面向终端用户，去掉一切调试、测试信息。

有了Type字段后，在项目开发中，只需要判断一下Type字段，作出功能的区分，如：

```csharp
if (version.VersionType == "Alpha")
{
    // Do some development job
}
else if (version.VersionType == "Beta")
{
    // Do some beta job
}
else
{
    // Do nothing on Release !
}
```
