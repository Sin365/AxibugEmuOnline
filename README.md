# AxibugEmuOnline

### 一个跨平台的、自动化联机的、纯C#实现的、开源的模拟器项目

	用于游戏模拟器同步的联机纯C#实现

	AxibugEmuOnline.Server 是服务端 .Net8
	
	AxibugEmuOnline.Web 是Asp.Net Core(.Net 8)的WebApi

	AxibugEmuOnline.Client 是客户端 Unity
		
		- NES EmuCore NES模拟器核心
		
				VirtualNes （C++手动翻译到C#，并收纳民间各种扩展Mapper实现，接近于最全的游戏支持。实测，高倍速加速游戏的同时（几百fps），且进行网络同步，性能一致，完全同步）
				
				~~My Nes~~ (功能全，但是性能局限，不作为主要使用，但已经移植纯.Net Standard2.0归档)
				
				~~Emulator.NES~~ (较为初级，已经废弃)
		
		- 街机模拟器核心 Arcade EmuCore
				
				MAME.Net 来自于我另一个移植项目 ，http://git.axibug.com/sin365/MAME.Core 最终会迁移进来。源头上是MAME C/C++源码翻译C#

		- 其他核心，长期补充

## 意义

#### 1.跨平台

	PC、手机、PSV、PS3、WIIU、PS4等破解游戏机，跨平台联机的模拟器（虽然目前只有NES），这是得益于Unity本身的跨平台能力。
	
	目前其他跨平台联机，仅限于电脑和手机
	
#### 2.自动化联机

	玩家不用关心任何诸如IP或者任何联机配置，直接玩。服务器是直接提供的，玩家无感知。

	甚至不用手动创建房间，游玩就是房间。加入放也不用专门下载，直接选择房间之后，自动化下载完成游戏并进入游戏。

#### 3.柔性网络架构

	最终效果，不会出现延迟高时的卡顿和暂停等待，也不会出现房间创建者和加入者之间 因网络状况差异造成体验差异，保证公平。

	柔性指：根据网络延迟和木桶效应动态调整帧同步提前量。作为被动推帧。

	核心原理是用刚好无限趋近于物理延迟本身的缓冲作，来抵消中间等待。

	本来玩家的地理位置带来的物理延迟是不可抗因素，但我们把这部分拿利用起来对冲，任何耗时操作，队列，堆栈处理都在这部分完成。没有多余。

	最终达到了除了物理延迟之外，没有任何浪费，并即便是网络状况极差时，仅表现为操作延迟，而不是其他一些模拟器的顿帧卡顿。且同步一致性得以保证。

#### 4.各种有意义的探索（作为额外功能，和联机无关）

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


## 代码贡献/协作者

[AlienJack](https://github.com/AlienJack "AlienJack") 

## 引用

### 本项目使用，我自构建的HaoYueNet高性能网络库作为基础而开发

[HaoYueNet-Github](https://github.com/Sin365/HaoYueNet "HaoYueNet-Github")

[HaoYueNet-自建Git站点](http://git.axibug.com/sin365/HaoYueNet "HaoYueNet-自建Git站点")

[nesdev.org NES - 2.0 XML Database](https://forums.nesdev.org/viewtopic.php?t=19940 "nesdev.org - NES 2.0 XML Database")

[VirtuaNES](http://virtuanes.s1.xrea.com/ "VirtuaNES")

部分NES-Mapper扩展 https://github.com/yamanyandakure/VirtuaNES097

部分NES-Mapper扩展 [VirtuaNESex](https://github.com/pengan1987/VirtuaNESex "VirtuaNESex")


### NES 模拟器内核

~~模拟器内核采用 Emulator.NES  https://github.com/Xyene/Emulator.NES~~

~~这是一个单机的 NES模拟器C#实现，我在此基础上做修改~~

~~做帧缓，颜色查找下标缓存，做同步，加上网络库，并实现服务端。达到如上效果。~~

~~----~~

~~随后，我们选择了更为全面的MyNes作为Nes模拟器核心，以此做二次开发和魔改。并实现自己的服务端和客户端联机逻辑~~

~~----~~

然后我们又开始尝试把 VirtualNes 的内核翻译为C#，在尝试内核的路上越走越远……


### 关于 NES Mapper支持

众所周知 NES/FC 在整个生命周期中，有各种厂商扩展的各色卡带Mapper，

Mapper支持越多，通俗讲就是支持更多卡带。

由于我们最终使用的是 VirtualNes CPP翻译CSharp，

那么，理论上已经算拥有最全的模拟器Mapper支持，几乎没有不能运行的游戏（官方游戏）

而VirtualNes官方的Mapper的基础上，也有很多民间二次补充，（使得一些特殊的游戏也可以运行，比如 吞食天地2孔明传 中文版，福州外星科技等等，就得以支持）

我们的项目也必须支持上! 咱们也同步要进行一个补充

追加了特殊的失传Mapper 35,111,162,163,175,176,178,192,195,199,216 (from https://github.com/yamanyandakure/VirtuaNES097)

后续补充二次，修正 Mapper163 175 176 178 192 199 参照叶枫VirtuaNESex_src(20191105)


### 街机模拟器核心
	
	原本是我独立移植到Unity的C# MAME.Core实现
	
	最终会继承到本项目中
	
	http://git.axibug.com/sin365/MAME.Core