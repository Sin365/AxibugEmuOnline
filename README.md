# AxibugEmuOnline

	用于游戏模拟器同步的联机.Net实现

	AxibugEmuOnline.Server 是服务端 .Net 8

	AxibugEmuOnline.Client 是客户端 Unity

	AxibugEmuOnline.Web 是Asp.Net Core(.Net 8)的WebApi

## 意义

#### 1.跨平台

	PC、手机、PSV、PS3、WIIU、PS4等破解游戏机，跨平台联机的模拟器（虽然目前只有NES），这是得益于Unity本身的跨平台能力。
	
	目前其他跨平台联机，仅限于电脑和手机
	
#### 2.良好体验为目标

	以现代化联机方式作为基本方向
	
#### 3.各种有意义的探索（作为额外功能）

	应该是Unity引擎中对于模拟器内核的画面接入良好的范例
	
	除了联机同步之外，模拟器本身的一些云游戏探索，如用模拟器帧缓存做云游戏
	
	3.1 帧缓存云游戏概念

		验证了一下 把模拟器帧缓存 走公网同步，实现联机的另一种方式

		云游戏，但是不是视频流的方式，是同步模拟器帧缓存，+GZIP压缩。NES这种低分辨率+颜色查找表的方式。画面传输只需要9k/s
		
	3.2 帧缓存云游戏TODO：

		1.目前只同步了画面，操作CMD同步还没做。

		2.以及多用户自行创建房间，和玩家选择要加入的房间列表还没做。

	3.3 帧缓存云游戏简述客户端逻辑：

		Player1主机才跑模拟器实例，然后Player1 会把渲染层的数据上报服务器。服务器广播。

		Player2即二号手柄玩家，不运行模拟器实例，画面渲染来自网络同步的数据。

		PS:场景中，UNES Test的Inspector勾选Player1作为玩家1，不勾选作为玩家2

		*之前试过直接上报渲染层，但是这样会有6w左右大小的uint[]

		*初步优化之后，采用只上报每一个像素对应颜色查找表的下标，这样就是一个byte[]了

		*PorotoBuf 传输使用的是bytes，但是Porotbuff只会对数组里每一个byte进行位压缩，整个byte[]不压缩。于是C#先GZIP压缩之后，在扔给protobuf。对面再解压。超级马里奥最复杂的画面情况是9k每秒的样子/。

## 引用

### 本项目使用，我自构建的HaoYueNet高性能网络库作为基础而开发

[HaoYueNet-Github](https://github.com/Sin365/HaoYueNet "HaoYueNet-Github")

[HaoYueNet-自建Git站点](http://git.axibug.com/sin365/HaoYueNet "HaoYueNet-自建Git站点")

### 模拟器内核

~~模拟器内核采用 Emulator.NES  https://github.com/Xyene/Emulator.NES~~

~~这是一个单机的 NES模拟器C#实现，我在此基础上做修改~~

~~做帧缓，颜色查找下标缓存，做同步，加上网络库，并实现服务端。达到如上效果。~~

随后，我们选择了更为全面的MyNes作为Nes模拟器核心，以此做二次开发和魔改。并实现自己的服务端和客户端联机逻辑



