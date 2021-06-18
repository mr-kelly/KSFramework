@echo off
::功能说明：
::		1.把热更代码link到主工程的Assets目录下
::		2.相当于使用c#开发，不使用ILRuntime
::		3.全部的热更代码都需要放在HotFix_Project/scripts/下
::	注意事项
::		1.此方法仅用于你在开发期，出包的时候需要使用ILRuntime才能在IOS上热更C#
::		2.相当于在开发期抛开ilruntime的解释器直接跑C#源代码，不易发现性能问题
::		3.ilruntime下有些代码注意小事项在原生中会被忽略，到ilruntime中可能会成为性能问题，或有语法问题
::		4.在ilruntime不建议使用foreach，反射，Linq
::Time: 2021/6/18 10:58
::Author: qingqing-zhao(569032731@qq.com)

@echo on
cd %~dp0
set root=%cd%
set hotfix=%root%\..\HotFix_Project\scripts
set assets=%root%\Assets\hotfix

rd %assets% /S /Q
mklink /j %assets% %hotfix%

pause
