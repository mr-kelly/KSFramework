
KSFramework使用SLua作为脚本引擎，并扩充一些基础函数，用于UI的开发。

# using (namespace)

类似于C#中的using，取代了SLua默认的import。using一个命名空间，将这个命名空间内的所有class提升到当前作用域。

# import (path)

进行脚本的导入，路径基于KSFramework中配置的Product/Lua路径。

# new (table, ctor)

类似于Java中的new，对一个Lua table进行new，将会创建一个新的table，旧的table作为__index传入。模拟面向对象编程。

# extends (class, base)

类似于Java中的extends，声明两个的table的继承关系。

# foreach (csharp_ienumerable)
对C#中的IEnumerable类进行迭代。
