# AxibugEmuOnline

#### 这是一个跨平台的，包含游戏机同服的，多人联机的，纯C#实现的，基于Unity客户端，.Net9实现服务端，模拟器开源项目。

#### 注：本项目是完整的项目实现，包含客户端，服务端，网站API。 

#### **并不是基于RetroArch，Libretro等项目的套壳项目，也并不是XX前端**。请不要混淆。具体您可以看代码。

#### A cross platform, multiplayer online, Net 9 Server , Unity Client , game emulator used C#. （on PSV/PS3,4/XBOX/3DS/Swith/PC/Mobile/or more...)

#### It's not a shell project based on projects such as RetroArch and Libretro。Please do not confuse. You can see the code for details.

## 意义

#### 1.跨平台

	PC、Android、iOS、PSVita、PS3、PS4、WIIU、XBOX360、XBOXONE等破解游戏机，以及Android汽车车机，跨平台联机的模拟器，这是得益于Unity本身的跨平台能力。
	
	目前其他跨平台多核心模拟器关键是跨平台统一联机的，几乎很少（好像我自己没看到）
	
	什么叫多端互通啊?(端茶)
	
#### 2.极简游戏体验 和 自动化联机

	玩家任何事情都不用关心

	玩家不用关心任何诸如IP或者任何联机配置，直接玩。服务器是直接提供的，玩家无感知。

	做到只要你有设备，不管你是什么设备，有网络，就可以玩。

	使用：

	玩家：启动程序 -> 选游戏 -> 玩！ （登录后台按设备自动登录的，游戏是自动下载的，进入游戏后，房间是自动创建的，玩家无感）

	加入者玩家：启动程序 -> 选房间 -> 玩！（游戏是根据房间自动下载的，手柄是自动分配的，玩家无感）

#### 3.柔性网络架构

	最终效果，不会出现延迟高时的卡顿和暂停等待，也不会出现房间创建者和加入者之间 因网络状况差异造成体验差异，保证公平。

	柔性指：根据网络延迟和木桶效应动态调整帧同步提前量。作为被动推帧。

	核心原理是用刚好无限趋近于物理延迟本身的缓冲作，来抵消中间等待。

	本来玩家的地理位置带来的物理延迟是不可抗因素，但我们把这部分拿利用起来对冲，任何耗时操作，队列，堆栈处理都在这部分完成。没有多余。

	最终达到了除了物理延迟之外，没有任何浪费，并即便是网络状况极差时，仅表现为操作延迟，而不是其他一些模拟器的顿帧卡顿。且同步一致性得以保证。


### 一个跨平台的、自动化联机的、纯C#实现的、开源的模拟器项目

	用于游戏模拟器同步的联机纯C#实现

	AxibugEmuOnline.Server 是服务端 .Net9
	
	AxibugEmuOnline.Web 是Asp.Net Core(.Net 9)的WebApi

	AxibugEmuOnline.Client 是客户端 Unity
	
	AxibugEmuOnline.Client.PSVita 是PSVita平台，因较为特殊，单独归档。本质是Client的分支。

### 模拟器核心 

	- NES/FC EmuCore NES/FC 模拟器核心
		
		VirtuaNes.Core（C#）：我们自己参照VirtuaNes源码将C++手动翻译到C#重写，并收纳民间各种扩展Mapper实现，接近于最全的游戏支持。实测，高倍速加速游戏的同时（几百fps），且进行网络同步，性能一致，完全同步
		
	- Arcade EmuCore 街机模拟器核心
				
		MAME.Core 来自于我另一个移植项目 - https://github.com/sin365/MAME.Core 源头上是将著名街机模拟器核心 MAME C/C++源码核心逻辑翻译到C#重写的项目MAME.NET。
			MAME.Core 改变MAME.NET解耦了其中的WIN32API和GDI依赖，并大量unsafe指针化操作优化程序效率，以及改写CPU时钟推进方式，移除了不必要的Cheat和CPU-Debug行为。使其成为更注重运行的高性能模拟器核心。
			并作为本项目的主要街机核心。

		支持平台：
		-CPS1
		-CPS2
		-NEOGEO
		-IGS
		-以及其他更老的街机平台。
		
	- 8Bit EmuCore

		Essgee.Unity 来自于我另一个移植项目 ，https://github.com/Sin365/Essgee.Unity

		按照Unity的方式，重写了一些逻辑。补全了本不支持的，GB，GBC即时存档。
		
		支持平台：
		- GameBoy
		- GameBoyColor
		- ColecoVision
		- GameGear
		- MasterSystem
		- SC3000
		- SG1000
		
	- WS/WSC WonderSwan/WonderSwan Color EmuCore 万代神奇天鹅模拟器核心

		Essgee.Unity 来自于我另一个移植项目 ，https://github.com/Sin365/StoicGoose.Unity 即将并入本项目。

		支持平台：
		- WonderSwan
		- WonderSwan Color

	- 其他核心：

		长期补充

## 代码贡献/协作者

[AlienJack](https://github.com/AlienJack "AlienJack") 


## NES 模拟器内核

最后，我们又花了一个多月把 VirtualNes 的C++源码徒手翻译为C#，在尝试内核的路上越走越远……

### 关于 NES Mapper支持

众所周知 NES/FC 在整个生命周期中，有各种厂商扩展的各色卡带Mapper，

Mapper支持越多，通俗讲就是支持更多卡带。

由于我们最终使用的是 VirtualNes CPP翻译CSharp，

那么，理论上已经算拥有最全的模拟器Mapper支持，几乎没有不能运行的游戏（官方游戏）

而VirtualNes官方的Mapper的基础上，也有很多民间二次补充，（使得一些特殊的游戏也可以运行，比如 吞食天地2孔明传 中文版，福州外星科技等等，就得以支持）

我们的项目也必须支持上! 咱们也同步要进行一个补充

追加了特殊的失传Mapper:35,111,162,163,175,176,178,192,195,199,216 (from https://github.com/yamanyandakure/VirtuaNES097)

后续补充二次，修正 Mapper:163,175,176,178,192,199 参照叶枫VirtuaNESex_src(20191105)

后续补充第三次，修正Mapper:191 支持madcell大字汉化的《热血时代剧》《热血物语》《快打旋风》《双截龙3》, 添加Mapper253 支持外星《龙珠 中文》 （参照VirtuaNES Plus 翻译代码)


## 街机模拟器核心 CPS1 / NEOGEO / PGM / Taito(b) / Tehkan / or other MAME platform
	
	原本是我独立移植到Unity的C# MAME.Core实现
	http://git.axibug.com/sin365/MAME.Core
	
## 8bit 其他模拟器核心 GameBoy / GameBoyColor / ColecoVision / GameGear / MasterSystem / SC3000 / SG1000
	
	原本是我独立移植到Unity的实现
	https://github.com/Sin365/Essgee.Unity
	按照Unity的方式，重写了一些逻辑。补全了本不支持的，GB，GBC即时存档。
	干掉绝大多数GC

## WS/WSC 模拟器核心 WonderSwan / WonderSwan Color
	
	原本是我独立移植到Unity的实现
	https://github.com/Sin365/StoicGoose.Unity

## 各种有意义的探索（作为额外功能，和核心功能 PS：联机无关）

	应该是Unity引擎中对于模拟器内核的画面接入良好的范例
	除了联机同步之外，模拟器本身的一些云游戏探索，如用模拟器帧缓存做视频直播

## 废弃内容 

~~模拟器内核采用 Emulator.NES  https://github.com/Xyene/Emulator.NES~~

~~这是一个单机的 NES模拟器C#实现，我在此基础上做修改~~

~~做帧缓，颜色查找下标缓存，做同步，加上网络库，并实现服务端。达到如上效果。~~

~~随后，我们选择了更为全面的MyNes作为Nes模拟器核心，以此做二次开发和魔改。并实现自己的服务端和客户端联机逻辑~

~~Emulator.NES~~ (较为初级，不再使用)

~~My Nes~~ (功能全，但是性能局限，不再使用，但已经移植纯.Net Standard2.0归档)

## 引用 和 致谢 Acknowledgements & Attribution

* HaoYueNet-Github - https://github.com/Sin365/HaoYueNet 本项目使用，我自构建的HaoYueNet高性能网络库作为基础而开发
* HaoYueNet-自建Git站点 - http://git.axibug.com/sin365/HaoYueNet
* AxiReplay - https://github.com/Sin365/AxiReplay 自写的一套Replay系统，用于Replay支持和部分用于网络Input逻辑。
* MAME.Core - http://git.axibug.com/sin365/MAME.Core
* ShaderToy - https://www.shadertoy.com/
* VirtuaNES - http://virtuanes.s1.xrea.com/
* 部分NES-Mapper扩展 - VirtuaNES097 https://github.com/yamanyandakure/VirtuaNES097
* 部分NES-Mapper扩展 - VirtuaNESex https://github.com/pengan1987/VirtuaNESex
* 部分NES-Mapper扩展 - VirtuaNESPlus/VirtuaNESUp https://github.com/dragon2snow/VirtuaNESUp
* nesdev.org NES - 2.0 XML Database - https://forums.nesdev.org/viewtopic.php?t=19940
* Essgee - https://github.com/xdanieldzd/Essgee
* No-Intro - http://www.no-intro.org project; see the [DAT-o-MATIC website](https://datomatic.no-intro.org) for official downloads.
* MAME-NET - https://www.codeproject.com/Articles/1275365/MAME-NET
* MAME-Multiple Arcade Machine Emulator - https://github.com/mamedev
* MSDN - https://msdn.microsoft.com
* BizHawk M68000 and Z80 code - https://github.com/TASEmulators/BizHawk/tree/master/src/BizHawk.Emulation.Cores/CPUs
* VCMAME detail by Bryan McPhail - https://www.codeproject.com/Articles/4923/VCMAME-Multiple-Arcade-Machine-Emulator-for-Visual
* MAME and MAMEUI Visual C Project Files - http://www.mikesarcade.com/arcade/vcmame.html
* CPS1.NET - https://www.codeproject.com/Articles/998595/CPS1-NET-A-Csharp-Based-CPS1-MAME-Emulator
* Essgee.Unity - https://github.com/Sin365/Essgee.Unity
* Essgee uses [DejaVu](https://dejavu-fonts.github.io) Sans Condensed as its OSD font; see the [DejaVu Fonts License](https://dejavu-fonts.github.io/License.html) for applicable information.
* StoicGoose.Unity - http://github.com/sin365/StoicGoose.Unity
* The XML data files in `No-Intro` were created by the [No-Intro](http://www.no-intro.org) project; see the [DAT-o-MATIC website](https://datomatic.no-intro.org) for official downloads.
* My personal thanks and gratitude to the late Near, who has always been encouraging and inspiring on my amateur emulator developer journey. They are sorely missed.