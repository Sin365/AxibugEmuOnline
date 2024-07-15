# 接口说明

这里说明，WebApi类的接口


### Nes游戏列表

```
{WebHost}/api/NesRomList?Page=<页码>&PageSize=<单大小>&SearchKey=<可选的模糊查询关键字>
```

Request:

```
/api/NesRomList?Page=1&PageSize=5&SearchKey=热血
```

Response:

```
{
    "page": 1,
    "maxPage": 5,
    "resultAllCount": 27,
    "gameList": [
        {
            "id": 584,
            "hash": "",
            "romName": "热血格斗传说",
            "url": "images/fcrom/Nekketsu%20Kakutou%20Densetsu%20(J).JPG",
            "imgUrl": "roms/fcrom/Nekketsu%20Kakutou%20Densetsu%20(J).zip"
        },
        {
            "id": 585,
            "hash": "",
            "romName": "热血硬派",
            "url": "images/fcrom/Nekketsu%20Kouha%20-%20Kunio%20Kun%20(J).JPG",
            "imgUrl": "roms/fcrom/Nekketsu%20Kouha%20-%20Kunio%20Kun%20(J).zip"
        },
        {
            "id": 586,
            "hash": "",
            "romName": "热血高校躲避球",
            "url": "images/fcrom/Nekketsu%20Koukou%20-%20Dodgeball%20Bu%20(J).JPG",
            "imgUrl": "roms/fcrom/Nekketsu%20Koukou%20-%20Dodgeball%20Bu%20(J).zip"
        },
        {
            "id": 587,
            "hash": "",
            "romName": "热血高校-足球篇",
            "url": "images/fcrom/Nekketsu%20Koukou%20Dodgeball%20Bu%20-%20Soccer%20Hen%20(J).JPG",
            "imgUrl": "roms/fcrom/Nekketsu%20Koukou%20Dodgeball%20Bu%20-%20Soccer%20Hen%20(J).zip"
        },
        {
            "id": 588,
            "hash": "",
            "romName": "热血新记录",
            "url": "images/fcrom/Nekketsu%20Shinkiroku%20-%20Harukanaru%20Kin%20Medal%20(J).JPG",
            "imgUrl": "roms/fcrom/Nekketsu%20Shinkiroku%20-%20Harukanaru%20Kin%20Medal%20(J).zip"
        }
    ]
}
```

序列化C#实体类示例

```
class Resp_GameList
{
    public int Page { get; set; }
    public int MaxPage { get; set; }
    public int ResultAllCount { get; set; }
    public List<Resp_RomInfo> GameList { get; set; }
}

public class Resp_RomInfo
{
    public int ID { get; set; }
    public string Hash { get; set; }
    public string RomName { get; set;}
    public string Url { get; set; }
    public string ImgUrl { get; set; }
}
```