# 接口说明

这里说明，WebApi类的接口


### 基本通用参数

```
platform 模拟器所在平台
enum DeviceType //设备类型
{
    DeviceType_Default = 0;//通用
	PC = 1;
	Android = 2;
	IOS = 3;
	PSV = 4;
	PS3 = 5;
	PS4 = 6;
	XBOX360 = 7;
	XBOXONE = 8;
	WiiU = 9;
	Nintendo3DS = 10;
	AndroidCarApp = 11;//汽车Android车机
}

PType Rom所属平台

enum RomPlatformType
{
    All = 0;
    Nes = 1;
    Master_System = 2;
	Game_Gear = 3;
	Game_Boy = 4;
	Game_Boy_Color = 5;
	Coleco_Vision = 6;
	SC_3000 = 7;
	SG_1000 = 8;
}

游戏类型

```

### 基本信息检查

```
{WebHost}/api/CheckStandInfo?platform=<平台编号>&version=<版本>
```
Request:

```
http://emu.axibug.com/api/CheckStandInfo?platform=1&version=1.0.0.0
```

Response:

```
{
    "needUpdateClient": 0,
    "serverIp": "139.186.160.243",
    "serverPort": 10492,
    "clientVersion": "0.0.0.1",
    "downLoadUrl": ""
}
```

序列化C#实体类示例

```
class Resp_CheckStandInfo
{
    public int needUpdateClient { get; set; }
    public string serverIp { get; set; }
    public ushort serverPort { get; set; }
    public string clientVersion { get; set; }
    public string downLoadUrl { get; set; }
}
```

### Nes游戏列表

```
{WebHost}/api/RomList?Page=<页码>&PageSize=<单大小>&SearchKey=<可选的模糊查询关键字>&PType=<平台枚举（int）>&GType=<游戏类型枚举>
```

Request:

```
http://emu.axibug.com/api/RomList?Page=0&PageSize=5&SearchKey=%热血&PType=1&GType=1
```

Response:

```
{
    "page": 0,
    "maxPage": 2,
    "resultAllCount": 6,
    "gameList": [
        {
            "orderid": 0,
            "id": 190,
            "ptype": 1,
            "romName": "热血物语",
            "gType": "ACT",
            "desc": "设有拳击及体力槽的热血系列节目。是带有RPG色彩的街头斗殴节目。",
            "url": "roms/fcrom/Downtown%20-%20Nekketsu%20Monogatari%20(J).zip",
            "imgUrl": "images/fcrom/Downtown%20-%20Nekketsu%20Monogatari%20(J).JPG",
            "hash": "62e2b8ee88d894d9adea32273c9bbfc3",
            "stars": 0,
            "playcount": 72
        },
        {
            "orderid": 1,
            "id": 460,
            "ptype": 1,
            "romName": "热血时代剧(热血道中记)",
            "gType": "ACT",
            "desc": "以古代日本为舞台展开的热血系列节目。设有多种必杀技可使用。",
            "url": "roms/fcrom/Kunio%20Kun%20No%20Jidaigekidayo%20Zenin%20Shuugou%20(J).zip",
            "imgUrl": "images/fcrom/Kunio%20Kun%20No%20Jidaigekidayo%20Zenin%20Shuugou%20(J).JPG",
            "hash": "45c4a12530d64e774f0ef7aebe211b81",
            "stars": 0,
            "playcount": 0
        },
        {
            "orderid": 2,
            "id": 585,
            "ptype": 1,
            "romName": "热血硬派",
            "gType": "ACT",
            "desc": "有着多种模式的格斗节目。各版面均有不同的趣味性。是热血系列游戏最初的作品。",
            "url": "roms/fcrom/Nekketsu%20Kouha%20-%20Kunio%20Kun%20(J).zip",
            "imgUrl": "images/fcrom/Nekketsu%20Kouha%20-%20Kunio%20Kun%20(J).JPG",
            "hash": "a731fd23627e0db18c88d1fff4bc8039",
            "stars": 0,
            "playcount": 0
        },
        {
            "orderid": 3,
            "id": 674,
            "ptype": 1,
            "romName": "热血物语(美版)",
            "gType": "ACT",
            "desc": "设有拳击及体力槽的热血系列节目。是带有RPG色彩的街头斗殴节目。",
            "url": "roms/fcrom/River%20City%20Brawl%20(J).zip",
            "imgUrl": "images/fcrom/River%20City%20Brawl%20(J).JPG",
            "hash": "956381cb137c4cadee36249ad16a79fa",
            "stars": 0,
            "playcount": 0
        },
        {
            "orderid": 4,
            "id": 826,
            "ptype": 1,
            "romName": "热血时代剧美版(热血道中记美版)",
            "gType": "ACT",
            "desc": "以古代日本为舞台展开的热血系列节目。设有多种必杀技可使用。",
            "url": "roms/fcrom/Technos%20Samurai%20-%20Downtown%20Special%20(J).zip",
            "imgUrl": "images/fcrom/Technos%20Samurai%20-%20Downtown%20Special%20(J).JPG",
            "hash": "b96fb8afda11a73c1693fb6a197347c4",
            "stars": 0,
            "playcount": 0
        }
    ]
}
```

序列化C#实体类示例

```
      class Resp_GameList
      {
          public int page { get; set; }
          public int maxPage { get; set; }
          public int resultAllCount { get; set; }
          public List<Resp_RomInfo> gameList { get; set; }
      }

      public class Resp_RomInfo
      {
          public int orderid { get; set; }
          public int id { get; set; }
          public int ptype { get; set; }
          public string romName { get; set; }
          public string gType { get; set; }
          public string desc { get; set; }
          public string url { get; set; }
          public string imgUrl { get; set; }
          public string hash { get; set; }
          public int stars { get; set; }
          public int playcount { get; set; }
      }
```


### 单个Rom游戏详情

```
{WebHost}/api/RomInfo?PType=<平台枚举（int）>&RomID=<RomID>
```

Request:

```
http://emu.axibug.com/api/RomInfo?PType=1&RomID=5
```

Response:

```
{
    "orderid": 0,//单个查询就没有排序id了
    "id": 5,
    "romName": "1999强手棋",
    "gType": "TAB",
    "desc": "以世纪末地球危机为题材的桌棋冒险节目。游戏者要存储宇宙能量帮助地球摆脱各方面危机最多可4人同时进行",
    "url": "roms/fcrom/1999%20-%20Hore,%20Mitakotoka!%20Seikimatsu%20(J).zip",
    "imgUrl": "images/fcrom/1999%20-%20Hore,%20Mitakotoka!%20Seikimatsu%20(J).JPG",
    "hash": "",
    "stars": 0
}
```


序列化Class 参照如上 Resp_RomInfo