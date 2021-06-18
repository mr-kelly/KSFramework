@echo off
::功能说明：
::		删除热更代码在主工程Assets里的link，跑ilruntime模式
::Time: 2021/6/18 11:00
::Author: qingqing-zhao(569032731@qq.com)

@echo on
cd %~dp0
set root=%cd%
set assets=%root%\Assets\hotfix

rd %assets% /S /Q


pause
