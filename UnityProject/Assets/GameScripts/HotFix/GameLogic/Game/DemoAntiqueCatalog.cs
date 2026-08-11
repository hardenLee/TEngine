using System.Collections.Generic;

namespace GameLogic
{
    /// <summary>
    /// 第一关临时内容池。接入 Luban 后仅替换此类的 LoadLevelOne，游戏流程无需改动。
    /// 图片地址与 YooAsset 地址一一对应，例如 Antique/1001。
    /// </summary>
    public static class DemoAntiqueCatalog
    {
        public static LevelDefinition LoadLevelOne()
        {
            var antiques = new List<AntiqueDefinition>
            {
                New(1001, "清·会发光的九龙壁", "夜里自动亮，疑似祖传氛围灯。", AntiqueVerdict.Fake, "塑料感都快溢出屏幕了。", "这龙珠的充电口你没看见？"),
                New(1002, "宋·五福临门山水图", "落款端庄，角落却有五只圆滚滚吉祥物。", AntiqueVerdict.Special, "真有这回事，但请单独封存。", "宋人再潮，也不至于提前办奥运会。", 150),
                New(1003, "唐·关公骑粉色雅迪浮雕", "刀工凌厉，续航更凌厉。", AntiqueVerdict.Fake, "关二爷过五关，不需要过充电桩。", "这坐骑的脚踏板已经出卖它了。"),
                New(1004, "汉·错金云纹铜镜", "纹饰清晰，背面有自然氧化痕迹。", AntiqueVerdict.Genuine, "眼力可以，这件入库。", "真东西被你踢回去，老板的血压也入库了。", 120),
                New(1005, "元·自带蓝牙的青铜鼎", "三足稳固，连接后可播放低音炮。", AntiqueVerdict.Fake, "三足鼎立，不包括蓝牙 5.3。", "鼎里都传来配对提示音了。"),
                New(1006, "民国·会唱跳的留声机", "摇柄一转，播放的是电子舞曲。", AntiqueVerdict.Special, "离谱，但确有改装价值。", "它不是古董，它是穿越失败的 DJ。", 150),
                New(1007, "明·青花缠枝莲罐", "胎体匀称，釉色温润，底款自然。", AntiqueVerdict.Genuine, "稳，真品入库。", "这件的年份比你的判断还老。", 120),
                New(1008, "清·乾隆御制折叠屏手机", "开机画面是龙纹，电量只剩一格。", AntiqueVerdict.Fake, "乾隆下江南，没下到供应链。", "御制两字救不了 Type-C 接口。"),
                New(1009, "唐·诗人同款保温杯", "杯身刻诗，内胆保温十二小时。", AntiqueVerdict.Special, "诗可以真，杯子先特殊处理。", "李白喝的是酒，不是 316 不锈钢。", 150),
                New(1010, "战国·饕餮纹玉璧", "沁色自然，纹路规整，无现代加工痕迹。", AntiqueVerdict.Genuine, "这眼力，今天能少挨几句骂。", "这么好的玉璧都能判错，你去鉴定石头吧。", 120),
            };
            return new LevelDefinition(1, "第一关：故宫文创部", 75f, 3, antiques);
        }

        private static AntiqueDefinition New(int id, string name, string description, AntiqueVerdict verdict,
            string correctLine, string wrongLine, int score = 100)
        {
            return new AntiqueDefinition(id, name, description, $"Antique/{id}", verdict, correctLine, wrongLine, score);
        }
    }
}
