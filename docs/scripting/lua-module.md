
Lua是一种自由的脚本语言，可以给开发人员很大的自由度。

但是无规矩往往不成方圆，用得好的人如神仙，用不好的人却只能吐槽语言本身。KSFramework中强烈建议，使用文件模块化来组织脚本。

所谓模块化的Lua脚本，是指，尽可能将每一个Lua脚本文件视为一个模块。这将是实现热重载的一个重点

你可能在你的项目中见过如下代码：

```lua
// init.lua文件。一个典型的非模块化脚本示例

require('game')
require('skillmanager')
require('soundmanager')
require('something1')
require('something2')
require('something3')
require('something4')


start_game();


```


以上是一个典型的非模块Lua代码示例，问题来了：start_game()函数究竟在哪个文件里？我们无从可知。
而且，这样的非模块化脚本require的过程，是一个初始化的过程，会导入了大量的全局作用域的变量，这种写法往往在文件的require过程中就大量的执行了各种行为。

模块化的脚本，类似如下：

```lua
// init.lua文件
local game = require('game')
game.start_game();


// game.lua文件
local game = {}

game.skillmanager = require('skillmanager')

function game:start_game
    self.skillmanager:init()
end

return game

```


每一Lua文件，都尽可能不执行具体的行为，而是返回一个函数、返回值、或返回Table，由外部来决定它的行为。

其实这跟面向对象编程是一样的，比如C#、Java中需要定义class。在定义class的时候进行静态构造器执行功能是不常用的。


## 模块化与热重载

在KSFramework中的UI模块设计中，所有的UI脚本都必须在结尾处返回一个Table。C#端可以决定这个Table是否进行缓存：如果不缓存，每次都重新读取Lua脚本，返回新的Table，达到热重载目的；如果进行缓存，节省Lua语法词法分析的时间，提升性能。
