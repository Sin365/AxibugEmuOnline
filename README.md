# AxibugEmuOnline

用于游戏模拟器同步的联机.Net实现


AxibugEmuOnline.Server 是服务端 .Net 8

AxibugEmuOnline.Client 是客户端 Unity




##就是一种联机方式的探索

验证了一下 把模拟器帧缓存 走公网同步，实现联机的另一种方式

云游戏，但是不是视频流的方式，是同步模拟器帧缓存，+GZIP压缩。NES这种低分辨率+颜色查找表的方式。画面传输只需要9k/s


##TODO：

1.目前只同步了画面，操作CMD同步还没做。

2.以及多用户自行创建房间，和玩家选择要加入的房间列表还没做。



##简述客户端逻辑：

Player1主机才跑模拟器实例，然后Player1 会把渲染层的数据上报服务器。服务器广播。

Player2即二号手柄玩家，不运行模拟器实例，画面渲染来自网络同步的数据。

PS:场景中，UNES Test的Inspector勾选Player1作为玩家1，不勾选作为玩家2

*之前试过直接上报渲染层，但是这样会有6w左右大小的uint[]

*初步优化之后，采用只上报每一个像素对应颜色查找表的下标，这样就是一个byte[]了

*PorotoBuf 传输使用的是bytes，但是Porotbuff只会对数组里每一个byte进行位压缩，整个byte[]不压缩。于是C#先GZIP压缩之后，在扔给protobuf。对面再解压。超级马里奥最复杂的画面情况是9k每秒的样子/。



##引用


###本项目使用，我自构建的HaoYueNet高性能网络库作为基础而开发

[HaoYueNet-Github](https://github.com/Sin365/HaoYueNet "HaoYueNet-Github")

[HaoYueNet-自建Git站点](http://git.axibug.com/sin365/HaoYueNet "HaoYueNet-自建Git站点")

###模拟器内核

模拟器内核采用 Emulator.NES  https://github.com/Xyene/Emulator.NES

这是一个单机的 NES模拟器C#实现，我在此基础上做修改

做帧缓，颜色查找缓存，存同步，加上网络库，实现服务端。达到如上效果。



