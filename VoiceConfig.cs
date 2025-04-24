using System;
using System.Collections.Generic;

namespace COM3D2.VoiceShortcutManager.Plugin
{
    public class YotogiVoiceInfo
    {
        public string command;
        public string[] alias;
        public string skill;

        public YotogiVoiceInfo()
        {
        }

        public YotogiVoiceInfo(string command, string[] alias, string skill)
        {
            this.command = command;
            this.alias = alias;
            this.skill = skill;
        }
    }

    public class ConfigInfo
    {
        public string name;
        public string value; //文字列以外の型はパースしてからSetValue
        public string[] voice;

        public ConfigInfo()
        {
        }

        public ConfigInfo(string name, string value, string[] voice)
        {
            this.name = name;
            this.value = value;
            this.voice = voice;
        }
    }

    //Slot指定でマスクと強制表示
    public class PropVoiceInfo
    {
        public string[] voice;
        public PropInfo[] props;
        public MaskInfo[] masks;

        public PropVoiceInfo()
        {
        }

        public PropVoiceInfo(string[] voice, PropInfo[] props, MaskInfo[] masks)
        {
            this.voice = voice;
            this.props = props;
            this.masks = masks;
        }
    }

    public class PropInfo
    {
        public string mpn; //MPN名

        public string filename; //アイテムファイル nullならリセット delなら削除

        //public int rid = 0;
        public bool temp = true; //一時的な変更
        //public bool noscale = false; //スケール対応

        public PropInfo()
        {
        }

        //public PropInfo(string mpn, string filename, int rid, bool temp, bool noscale)
        public PropInfo(string mpn, string filename, bool temp)
        {
            this.mpn = mpn;
            this.filename = filename;
            //this.rid = rid;
            this.temp = temp;
            //this.noscale = noscale;
        }
    }

    public class MaskInfo
    {
        public string slot; //スロット名
        public string visible; //true=マスク表示 false=マスク非表示 force=強制表示

        public MaskInfo()
        {
        }

        public MaskInfo(string slot, string visible)
        {
            this.slot = slot;
            this.visible = visible;
        }
    }

    // # inory modify
    public class ShapeKeyInfo
    {
        public string shapeKey; // shapeKey name
        public float value; // shapeKey value
        public string[] voice; // voice keyword

        public ShapeKeyInfo()
        {
        }

        public ShapeKeyInfo(string shapeKey, float value, string[] voice)
        {
            this.shapeKey = shapeKey;
            this.value = value;
            this.voice = voice;
        }
    }

    public class ShapeKeyAnimationInfo
    {
        public string shapeKey; // shapeKey name
        public float min; // animation Start value
        public float max; // animation End Value
        public float speed; // animation Speed
        public string[] startVoice; // animation Start Keyword
        public string[] stopVoice; // animation Stop Keyword

        public ShapeKeyAnimationInfo()
        {
        }

        public ShapeKeyAnimationInfo(string shapeKey, float min, float max, float speed, string[] startVoice,
            string[] stopVoice)
        {
            this.shapeKey = shapeKey;
            this.min = min;
            this.max = max;
            this.speed = speed;
            this.startVoice = startVoice;
            this.stopVoice = stopVoice;
        }
    }
    // # inory modify

    public class VoiceConfig
    {
        //コンストラクタ XML読み込みで必要
        public VoiceConfig()
        {
        }

        //メッセージウィンドウ操作
        public string[] autoModeOn = { "オート開始", "オートオン", "オートモード" };
        public string[] autoModeOff = { "オート終了", "オートオフ" };
        public string[] playVoice = { "ボイス再生" };
        public string[] playVoiceBefore = { "前のボイス" };
        public string[] yotogiParamMind = { "夜伽回復" };

        //このプラグインの設定変更
        public ConfigInfo[] config =
        {
            new ConfigInfo("vymEyeFaceLinkType", "-1", new string[] { "目線全員" }), //連携なし 全メイドが対象
            new ConfigInfo("vymEyeFaceLinkType", "0", new string[] { "目線個別" }), //目線顔向変更対象をVYMのメインメイドに限定
            //new ConfigInfo("vymEyeFaceLinkType", "2", new string[]{"目線同時"}),    //VYMのリンクメイドにも適用する

            new ConfigInfo("vymUndressLinkType", "-1", new string[] { "脱衣全員" }), //連携なし 全メイドが対象 夜伽の脱衣ボタンも連動
            new ConfigInfo("vymUndressLinkType", "0", new string[] { "脱衣個別" }), //脱衣対象をVYMのメインメイドに限定
            //new ConfigInfo("vymUndressLinkType", "2", new string[]{"脱衣同時"}),    //VYMのリンクメイドにも適用する

            new ConfigInfo("vymVibeLinkType", "0", new string[] { "バイブ個別" }), //メインメイドとUNZIPのサブメイド
            new ConfigInfo("vymVibeLinkType", "1", new string[] { "バイブリンク" }), //バイブ操作をVYMのリンクメイドにも適用する
            new ConfigInfo("vymVibeLinkType", "2", new string[] { "バイブ同時" }), //バイブ操作をVYMのUNZIPのメインとサブメイドに適用する

            new ConfigInfo("vymFaceLinkType", "0", new string[] { "表情個別" }), //メインメイドのみ
            new ConfigInfo("vymFaceLinkType", "1", new string[] { "表情リンク" }), //モーションをVYMのリンクメイドにも適用する
            new ConfigInfo("vymFaceLinkType", "2", new string[] { "表情同時" }), //モーションをVYMのUNZIPのメインとサブメイドに適用する

            new ConfigInfo("vymMotionLinkType", "0", new string[] { "モーション個別" }), //メインメイドのみ
            new ConfigInfo("vymMotionLinkType", "1", new string[] { "モーションリンク" }), //モーションをVYMのリンクメイドにも適用する
            new ConfigInfo("vymMotionLinkType", "2", new string[] { "モーション同時" }), //モーションをVYMのUNZIPのメインとサブメイドに適用する
        };

        //目線顔向
        public string[] eyeToCam = { "こっち見て", "こっちを見て", "目を見て", "目を向けて" };
        public string[] eyeToFront = { "あっち見て", "前見て", "前を見て" };
        public string[] eyeSorashi = { "目をそらして", "そっぽ見て" };
        public string[] headToCam = { "こっち向いて", "こっちを向いて", "顔を見せて", "顔を向けて" };
        public string[] headToFront = { "あっち向いて", "前向いて", "前を向いて" };
        public string[] headSorashi = { "顔をそらして", "そっぽ向いて" };

        //脱衣
        public string[] dressAll = { "服着て", "服を着て", "服を着せる" };
        public string[] undressAll = { "全部脱いで", "全部脱がす", "裸になって" };
        public string[] undressWear = { "服脱いで", "服を脱いで", "服を脱がす", "下着になって" };
        public string[] dressTop = { "上着着て", "上着着せる" };
        public string[] undressTop = { "上着脱いで", "上脱いで", "上着脱がす" };
        public string[] dressBottom = { "下履いて", "スカート履いて" };
        public string[] undressBottom = { "下脱いで", "スカート脱いで" };
        public string[] dressMizugi = { "水着着て" };
        public string[] undressMizugi = { "水着脱いで" };
        public string[] dressUnderWear = { "下着つけて", "下着をつけて", "下着着て", "下着を着て", "下着を着せる" };
        public string[] undressUnderWear = { "下着脱いで", "下着を脱いで", "下着を脱がす" };
        public string[] dressBra = { "ブラつけて", "ブラを着て", "ブラつける", "ブラをつける" };
        public string[] undressBra = { "ブラ外して", "ブラ脱いで", "ブラ外す", "ブラを外す" };
        public string[] dressPants = { "パンツ履いて", "パンツを履いて", "パンツ履かせる", "パンツを履かせる" };
        public string[] undressPants = { "パンツ脱いで", "パンツを脱いで", "パンツ脱がす", "パンツを脱がす" };
        public string[] dressStkg = { "靴下履いて", "靴下履かせる" };
        public string[] undressStkg = { "靴下脱いで", "靴下を脱がす" };
        public string[] dressShoes = { "靴履いて", "靴を履いて", "靴を履かせる" };
        public string[] undressShoes = { "靴脱いで", "靴を脱いで", "靴を脱がす" };
        public string[] dressAcc = { "アクセつけて" };
        public string[] undressAcc = { "アクセ外して" };

        public string[] modoshiPants = { "パンツ戻す" };
        public string[] zurashiPants = { "パンツずらす" };

        public string[] mekureFront = { "めくれ前", "前めくる" };
        public string[] mekureBack = { "めくれ後ろ", "後ろめくる" };
        public string[] mekureReset = { "めくれ戻す" };

        public string[] pororiTop = { "ポロリ", "はだける" };
        public string[] pororiTopReset = { "ポロリ戻す", "はだけ戻す" };
        public string[] pororiBottom = { "ずり下げ" };
        public string[] pororiBottomReset = { "ずり上げ" };

        public string[] manShowChinko = { "竿表示" };
        public string[] manHideChinko = { "竿非表示" };

        //# inory modified
        public string[] dressAccXXX = { "dress Acc XXX", "穿上阴蒂", "戴上阴蒂", "穿阴蒂", "穿上阴蒂配饰", "戴上阴蒂配饰", "穿阴蒂配饰" };
        public string[] undressAccXXX = { "undress Acc XXX", "脱下阴蒂", "脱掉阴蒂", "脱阴蒂", "脱下阴蒂配饰", "脱掉阴蒂配饰", "脱阴蒂配饰" };
        public string[] dressAccShippo = { "dress Acc Shippo", "穿上尾巴", "戴上尾巴", "穿尾巴" };
        public string[] undressAccShippo = { "undress Acc Shippo", "脱下尾巴", "脱掉尾巴", "脱尾巴" };
        public string[] dressAccVag = { "dress Acc Vag", "穿上小穴", "戴上小穴", "穿小穴", "穿上小穴配饰", "戴上小穴配饰", "穿小穴配饰" };
        public string[] undressAccVag = { "undress Acc Vag", "脱下小穴", "脱掉小穴", "脱小穴", "脱下小穴配饰", "脱掉小穴配饰", "脱小穴配饰" };
        public string[] dressAccAnl = { "dress Acc Anl", "穿上屁眼", "戴上屁眼", "穿屁眼", "穿上屁眼配饰", "戴上屁眼配饰", "穿屁眼配饰" };
        public string[] undressAccAnl = { "undress Acc Anl", "脱下屁眼", "脱掉屁眼", "脱屁眼", "脱下屁眼配饰", "脱掉屁眼配饰", "脱屁眼配饰" };
        public string[] dressAccSenaka = { "dress Acc Senaka", "穿上背部", "戴上背部", "穿背部", "穿上背部配饰", "戴上背部配饰", "穿背部配饰" };
        public string[] undressAccSenaka = { "undress Acc Senaka", "脱下背部", "脱掉背部", "脱背部", "脱下背部配饰", "脱掉背部配饰", "脱背部配饰" };
        public string[] dressAccHeso = { "dress Acc Heso", "穿上肚脐", "戴上肚脐", "穿肚脐", "穿上肚脐配饰", "戴上肚脐配饰", "穿肚脐配饰" };
        public string[] undressAccHeso = { "undress Acc Heso", "脱下肚脐", "脱掉肚脐", "脱肚脐", "脱下肚脐配饰", "脱掉肚脐配饰", "脱肚脐配饰" };
        public string[] dressAccHana = { "dress Acc Hana", "穿上鼻子", "戴上鼻子", "穿鼻子", "穿上鼻子配饰", "戴上鼻子配饰", "穿鼻子配饰" };
        public string[] undressAccHana = { "undress Acc Hana", "脱下鼻子", "脱掉鼻子", "脱鼻子", "脱下鼻子配饰", "脱掉鼻子配饰", "脱鼻子配饰" };
        public string[] dressAccAshi = { "dress Acc Hana", "穿上脚踝", "戴上脚踝", "穿脚踝", "穿上脚踝配饰", "戴上脚踝配饰", "穿脚踝配饰" };
        public string[] undressAccAshi = { "undress Acc Hana", "脱下脚踝", "脱掉脚踝", "脱脚踝", "脱下脚踝配饰", "脱掉脚踝配饰", "脱脚踝配饰" };
        //# inory modified

#if COM3D2_5
		public string[] manDressAll = {"服を着る","男着衣"};
		public string[] manUndressAll = {"全裸になる","男全裸"};
		public string[] manUndressWear = {"服を脱ぐ", "男脱衣"};
		public string[] manDressPants = {"パンツ履く"};
		public string[] manUndressPants = {"パンツ脱ぐ"};
		public string[] manDressShoes = {"靴を履く"};
		public string[] manUndressShoes = {"靴を脱ぐ"};
#endif


        //Prop設定 + マスク 強制表示
        public List<PropVoiceInfo> propVoiceList = new List<PropVoiceInfo>();

        //夜伽用音声別名リスト
        public List<YotogiVoiceInfo> yotogiVoiceList = new List<YotogiVoiceInfo>();

        // # inory modify
        public List<ShapeKeyInfo> shapeKeyList = new List<ShapeKeyInfo>();

        // # inory modify
        public List<ShapeKeyAnimationInfo> shapeKeyAnimationList = new List<ShapeKeyAnimationInfo>();

        private void addYotogiList(string command, string[] keywords)
        {
            addYotogiList(command, keywords, null);
        }

        private void addYotogiList(string command, string[] keywords, string skill)
        {
            yotogiVoiceList.Add(new YotogiVoiceInfo(command, keywords, skill));
        }

        //コマンド初期化
        public void initDefault()
        {
            //PropとMask
            propVoiceList.Add(new PropVoiceInfo(new string[] { "ブラ表示" }, null,
                new MaskInfo[] { new MaskInfo("bra", "force") }));
            propVoiceList.Add(new PropVoiceInfo(new string[] { "手袋つける" }, null,
                new MaskInfo[] { new MaskInfo("glove", "true") }));
            propVoiceList.Add(new PropVoiceInfo(new string[] { "手袋外す" }, null,
                new MaskInfo[] { new MaskInfo("glove", "false") }));

            propVoiceList.Add(new PropVoiceInfo(new string[] { "バイブ入れる" },
                new PropInfo[] { new PropInfo("accvag", "accVag_VibePink_I_.menu", true) },
                new MaskInfo[] { new MaskInfo("accVag", "true") }));
            propVoiceList.Add(new PropVoiceInfo(new string[] { "バイブ抜く", "バイブ外す" },
                new PropInfo[] { new PropInfo("accvag", null, true) },
                new MaskInfo[] { new MaskInfo("accVag", "false") }));

            propVoiceList.Add(new PropVoiceInfo(new string[] { "ハート目" },
                new PropInfo[]
                {
                    new PropInfo("eye_hi", "_i_skinhi_nky003.menu", true),
                    new PropInfo("eye_hi_r", "_i_skinhi_nky003.menu", true)
                }, null));
            propVoiceList.Add(new PropVoiceInfo(new string[] { "ハイライト無し" },
                new PropInfo[]
                {
                    new PropInfo("eye_hi", "_i_skinhi002.menu", true),
                    new PropInfo("eye_hi_r", "_i_skinhi002.menu", true)
                }, null));
            propVoiceList.Add(new PropVoiceInfo(new string[] { "ハイライト戻す" },
                new PropInfo[] { new PropInfo("eye_hi", null, true), new PropInfo("eye_hi_r", null, true) }, null));

            propVoiceList.Add(new PropVoiceInfo(new string[] { "リップ無し" },
                new PropInfo[] { new PropInfo("lip", "_i_lip_del.menu", true) }, null));
            propVoiceList.Add(new PropVoiceInfo(new string[] { "リップ戻す" },
                new PropInfo[] { new PropInfo("lip", null, true) }, null));

            // # inory modify
            // shapeKey example
            shapeKeyList.Add(new ShapeKeyInfo
            {
                shapeKey = "tits_nipple_kupa",
                value = 1,
                voice = new string[] { "maximal nipple kupa" }
            });

            shapeKeyList.Add(new ShapeKeyInfo
            {
                shapeKey = "tits_nipple_kupa",
                value = 0,
                voice = new string[] { "minimal nipple kupa" }
            });

            shapeKeyAnimationList.Add(new ShapeKeyAnimationInfo
            {
                shapeKey = "tits_nipple_kupa",
                min = 0,
                max = 1,
                speed = 0.5f,
                startVoice = new string[] { "start nipple kupa animate" },
                stopVoice = new string[] { "stop nipple kupa animate" }
            });
            // # inory modify

            //夜伽コマンド
            //汎用コマンドより優先するもの
            addYotogiList("動いてもらう", new string[] { "動いて" });

            //複数スキル
            addYotogiList("責める", new string[] { "奉仕して", "洗って", "こすって" }, "マットプレイ");
            addYotogiList("責める", new string[] { "すまたして", "あそこでこすって", "こすりつけて" }, "素股");
            addYotogiList("責める", new string[] { "舐める", "舐めるよ", "舐めて", "しゃぶって" }, "シックスナイン");
            addYotogiList("責める", new string[] { "イラマする", "つっこむ" }, "イラマチオ");
            addYotogiList("責める", new string[] { "責めて", "さわって", "いじって" }, "百合愛撫");
            addYotogiList("責める", new string[] { "舐めて", "舐めあって" }, "百合クンニ");
            addYotogiList("責める", new string[] { "責めて", "バイブ入れて" }, "(百合バイブ|百合双頭バイブ)");
            addYotogiList("責める", new string[] { "責めて", "貝合わせして", "あそこ合わせて" }, "百合貝合わせ");
            addYotogiList("責める", new string[] { "責めて", "バイブ入れる", "バイブ入れるよ" }, "バイブ責め");
            addYotogiList("責める", new string[] { "顔に乗って", "押し付けて" }, "顔面騎乗位");
            addYotogiList("責める", new string[] { "責めて", "入れる", "入れて", "動いて" }, "騎乗位");
            addYotogiList("責める", new string[] { "入れる", "入れて", "入れるよ", "動くよ" },
                "(セックス|アナル|正常位|後背位|座位|立位|測位|駅弁|寝バック|性交)");
            addYotogiList("責める", new string[] { "さわる", "さわるよ", "いじる", "いじるよ" }, "愛撫");
            addYotogiList("責める", new string[] { "入れる", "入れて", "入れるよ" });
            addYotogiList("強く責める", new string[] { "強くして", "もっとこすって" }, "マットプレイ");
            addYotogiList("強く責める", new string[] { "強くして", "もっと舐めて" }, "百合クンニ");
            addYotogiList("強く責める", new string[] { "強くして", "もっと責めて" }, "百合");
            addYotogiList("強く責める", new string[] { "強くして", "もっと動いて" }, "騎乗位");
            addYotogiList("強く責める", new string[] { "強くする", "強くするよ" });
            addYotogiList("止める", new string[] { "やめて", "止まって", "抜いて" }, "騎乗位");
            addYotogiList("止める", new string[] { "やめる", "やめるよ", "抜く", "抜くよ", "止まって" },
                "(セックス|アナル|正常位|後背位|座位|立位|測位|駅弁|寝バック|性交)");
            addYotogiList("止める", new string[] { "やめる", "やめるよ", "やめて" });
            addYotogiList("奉仕させる", new string[] { "奉仕して", "尻舐めして", "お尻舐めて", "アナル舐めて" }, "尻舐め");
            addYotogiList("奉仕させる", new string[] { "奉仕して", "パイズリして", "挟んで", "胸で挟んで" }, "パイズリ");
            addYotogiList("奉仕させる", new string[] { "奉仕して", "手コキして", "手でして", "しごいて" }, "手コキ");
            addYotogiList("奉仕させる", new string[] { "奉仕して", "ご奉仕して" });
            addYotogiList("奉仕して頂く", new string[] { "奉仕して", "奉仕してください", "挟んでください" }, "パイズリ");
            addYotogiList("奉仕して頂く", new string[] { "奉仕して", "奉仕してください", "しごいてください" }, "手コキ");
            addYotogiList("奉仕して頂く", new string[] { "奉仕して", "奉仕してください", "足でしてください" }, "足コキ");
            addYotogiList("奉仕して頂く", new string[] { "奉仕して", "奉仕してください", "お尻にしてください" }, "アナル");
            addYotogiList("奉仕して頂く", new string[] { "奉仕して", "奉仕してください" });
            addYotogiList("胸を揉みながら", new string[] { "胸を揉んで", "胸揉んで", "おっぱい揉んで" }, "百合");
            addYotogiList("胸を揉みながら", new string[] { "胸を揉む", "胸揉むよ", "おっぱい揉む", "おっぱい揉むよ" });

            //発情
            addYotogiList("発情させる", new string[] { "発情して", "アヘって", "イきまくって" });
            addYotogiList("オホらせる", new string[] { "おほって" });
            addYotogiList("ラブバイブで発情させる", new string[] { "発情させる", "発情して", "アヘって", "バイブで発情して" });
            addYotogiList("覚醒させる", new string[] { "覚醒して", "覚醒してください" });
            //開始・停止
            addYotogiList("責めさせる", new string[] { "責めて", "責めるよ", "入れる", "入れるよ", "入れて" });
            addYotogiList("責めさせる_A", new string[] { "責めさせる", "責めて" });
            addYotogiList("責めさせる_B", new string[] { "責めさせる", "責めて" });
            addYotogiList("責めさせる_C", new string[] { "責めさせる", "責めて" });
            addYotogiList("責めて頂く", new string[] { "責めてください" });
            addYotogiList("責めさせて頂く", new string[] { "責めさせてください", "責めます" });
            addYotogiList("責める_A", new string[] { "責める", "責めるよ" });
            addYotogiList("責める_B", new string[] { "責める", "責めるよ" });
            addYotogiList("責める_C", new string[] { "責める", "責めるよ" });
            addYotogiList("止めさせて頂く", new string[] { "やめさせていただきます", "やめさせてください", "やめます" });
            addYotogiList("止めさせる", new string[] { "やめさせる", "やめて", "やめる", "やめるよ" });
            addYotogiList("止めてもらう", new string[] { "やめてもらう", "やめてください" });
            addYotogiList("止めて頂く", new string[] { "やめてもらう", "やめてください" });
            addYotogiList("止めて貰う", new string[] { "やめてもらう", "やめて" });
            addYotogiList("やめる", new string[] { "やめて" });
            addYotogiList("奉仕させて頂く", new string[] { "奉仕させてください", "ご奉仕します" });
            addYotogiList("奉仕してもらう", new string[] { "奉仕して", "ご奉仕して" });
            addYotogiList("奉仕セックスさせる", new string[] { "奉仕して", "ご奉仕して", "ご奉仕セックスして" });
            addYotogiList("奉仕を任せる", new string[] { "おまかせする" });
            addYotogiList("強く責めさせて頂く", new string[] { "強く責めます", "強くします" });
            addYotogiList("強く責めさせる", new string[] { "強く責めて", "強くして", "強くするよ" });
            addYotogiList("強く責めて頂く", new string[] { "強く責めてください", "強くしてください" });
            addYotogiList("強く責める_A", new string[] { "強く責める", "強くする", "強くするよ" });
            addYotogiList("激しくメイドを求める", new string[] { "激しく求める", "もっと求める", "もっとする" });
            addYotogiList("激しく責める", new string[] { "激しくする", "激しくするよ", "激しくして" });
            addYotogiList("激しく責める_A", new string[] { "激しく責める", "激しくする", "激しくするよ" });
            addYotogiList("激しく犯す", new string[] { "激しくする" });
            addYotogiList("激しく奉仕させる", new string[] { "激しくして", "強くして" });
            addYotogiList("激しく舐める", new string[] { "激しく舐めるよ" });
            addYotogiList("丁寧にオナホでシゴいて頂く", new string[] { "オナホでシゴいてください", "シゴいてください" });
            addYotogiList("丁寧に愛撫する", new string[] { "丁寧にする", "丁寧にするよ", "丁寧にさわる" });
            addYotogiList("丁寧に手で奉仕させる", new string[] { "丁寧に奉仕して", "丁寧にして", "手で奉仕して" });
            addYotogiList("丁寧に責めさせる", new string[] { "丁寧に責めて", "丁寧にして" });
            addYotogiList("丁寧に責めさせる_A", new string[] { "丁寧に責めて", "丁寧にして" });
            addYotogiList("丁寧に責めさせる_B", new string[] { "丁寧に責めて", "丁寧にして" });
            addYotogiList("丁寧に責めさせる_C", new string[] { "丁寧に責めて", "丁寧にして" });
            addYotogiList("丁寧に責める", new string[] { "丁寧にして", "じっくり責めて" });
            addYotogiList("丁寧に素股させる", new string[] { "丁寧にすまたして", "じっくりすまたして" });
            addYotogiList("丁寧に掃除させてもらう", new string[] { "お掃除させて", "お掃除するよ", "綺麗にするよ" });
            addYotogiList("丁寧に掃除させて頂く", new string[] { "お掃除させていただきます", "綺麗にさせていただきます" });
            addYotogiList("丁寧に足で奉仕して頂く", new string[] { "丁寧に足で奉仕してください", "丁寧に足でしてください", "丁寧に奉仕してください" });
            addYotogiList("丁寧に奉仕させて頂く", new string[] { "丁寧に奉仕させてください", "丁寧に奉仕します" });
            addYotogiList("丁寧に奉仕させる", new string[] { "丁寧に奉仕して", "丁寧にして" });
            addYotogiList("丁寧に奉仕して頂く", new string[] { "丁寧に奉仕してください", "丁寧に奉仕します", "丁寧にしてください" });
            addYotogiList("丁寧に咥えさせる", new string[] { "丁寧に咥えて", "丁寧にしゃぶって", "じっくり咥えて", "じっくりしゃぶって" });
            addYotogiList("丁寧に舐めさせてもらう", new string[] { "丁寧に舐めさせて", "丁寧に舐めるよ", "じっくり舐めて" });
            addYotogiList("丁寧に舐めさせて頂く", new string[] { "丁寧に舐めさせてください", "丁寧に舐めます", "じっくり舐めます" });
            addYotogiList("丁寧に舐める", new string[] { "丁寧に舐めるよ", "じっくり舐める", "じっくり舐めるよ" });
            //行為
            addYotogiList("咥えさせる", new string[] { "咥えて", "しゃぶって", "お口でして" });
            addYotogiList("咥えて頂く", new string[] { "咥えてください", "しゃぶってください" });
            addYotogiList("フェラしながら", new string[] { "フェラして", "しゃぶって" });
            addYotogiList("口を責める", new string[] { "口にするよ", "口に入れるよ" });
            addYotogiList("キスおねだり", new string[] { "キスおねだりして", "キスして欲しい？" });
            addYotogiList("キスさせながら", new string[] { "キスして" });
            addYotogiList("キスさせる", new string[] { "キスして" });
            addYotogiList("キスして頂きながら", new string[] { "キスしてください" });
            addYotogiList("キスして頂く", new string[] { "キスしてください" });
            addYotogiList("キスしながら", new string[] { "キスする", "キスするよ", "キスして" });
            addYotogiList("キスする", new string[] { "キスする", "キスするよ", "キスして" });
            addYotogiList("キスだけでもさせてやる", new string[] { "キスさせてやる" });
            addYotogiList("キスでイカせる", new string[] { "キスでイって" });
            addYotogiList("キスでイかせる", new string[] { "キスでイって" });
            addYotogiList("キスで興奮を煽る", new string[] { "キスで興奮して" });
            addYotogiList("キスをさせる", new string[] { "キスして" });
            addYotogiList("キスをしてビッチにする", new string[] { "キスをする", "キスする", "キスでビッチになって" });
            addYotogiList("キスをして気持ちを確かめ合う", new string[] { "キスしよう", "気持ちを確かめ合おう" });
            addYotogiList("キスをして緊張を解す", new string[] { "リラックスして" });
            addYotogiList("キスをして夢中にさせる", new string[] { "キスで夢中にさせる", "キスする", "夢中にさせる" });
            addYotogiList("キスをして様子を見る", new string[] { "キスで様子を見る", "キスする", "様子を見る" });
            addYotogiList("キスをしながら", new string[] { "キスする", "キスするよ", "キスして" });
            addYotogiList("キスをする", new string[] { "キスする", "キスするよ", "キスして" });
            addYotogiList("ディープキス", new string[] { "ベロチュー", "ベロチューする", "ベロチューするよ", "ベロチューして" });
            addYotogiList("ディープキスをして頂く", new string[] { "ディープキスしてください", "ベロチューしてください" });
            addYotogiList("ディープキスをする", new string[] { "ディープキスして", "ディープキスするよ", "ベロチューする", "ベロチューするよ", "ベロチューして" });
            addYotogiList("胸を揉みながらイチャイチャする", new string[] { "いちゃいちゃする", "いちゃいちゃしよ" });
            addYotogiList("胸を揉みながらキス", new string[] { "揉みながらキス" });
            addYotogiList("胸を揉みながらキスしながら", new string[] { "揉みながらキス", "揉みながらキスするよ" });
            addYotogiList("胸を揉む", new string[] { "胸を揉む", "胸揉むよ", "おっぱい揉む", "おっぱい揉むよ" });
            addYotogiList("胸揉み", new string[] { "胸を揉む", "胸揉むよ", "おっぱい揉む", "おっぱい揉むよ" });
            addYotogiList("胸を虐める", new string[] { "胸いじめるよ" });
            addYotogiList("胸を見る", new string[] { "胸見せて", "胸を見せて", "おっぱい見せて" });
            addYotogiList("胸を使う", new string[] { "胸を使って", "胸使って", "おっぱい使って" });
            addYotogiList("胸を責め快楽を与える", new string[] { "胸を責める", "胸で気持ちよくなって" });
            addYotogiList("胸を嬲りながら", new string[] { "胸を嬲る", "おっぱいを嬲る", "おっぱい嬲るよ" });
            addYotogiList("胸を揉ませて頂く", new string[] { "胸を揉ませてください", "胸を揉みます" });
            addYotogiList("胸を揉ませながら", new string[] { "胸を揉んで", "おっぱい揉んで" });
            addYotogiList("胸を揉ませながらキス", new string[] { "揉ませながらキス" });
            addYotogiList("胸を揉まれたことを指摘する", new string[] { "揉まれてるよ", "揉まれちゃったね" });
            addYotogiList("胸を揉む＆尻穴を弄る", new string[] { "", "胸と尻を弄る", "胸とアナルを弄る" });
            addYotogiList("胸を揉むのを見せつける", new string[] { "胸を揉む", "見せつける" });
            addYotogiList("胸を揉んでビッチにする", new string[] { "胸を揉む", "ビッチになって" });
            addYotogiList("胸を揉んで気持ち良くさせる", new string[] { "気持ちよくなって", "胸で気持ちよくなって" });
            addYotogiList("胸を揉んで見せつける", new string[] { "胸を揉む", "胸を見せて", "おっぱい見せて" });
            addYotogiList("胸を揉んで自分に夢中にさせる", new string[] { "夢中にさせる", "夢中になって" });
            addYotogiList("胸洗いをさせる", new string[] { "胸洗いして", "胸で洗って", "おっぱいで洗って" });
            addYotogiList("まずは胸に触る", new string[] { "胸に触る", "胸触るよ", "おっぱい触る", "おっぱい触るよ" });
            addYotogiList("尻コキ", new string[] { "尻コキする", "尻でこする" });
            addYotogiList("尻を叩いていただく", new string[] { "尻を叩いてください" });
            addYotogiList("尻を叩かせながら", new string[] { "尻叩いて", "尻を叩いて", "尻を叩かせる" });
            addYotogiList("尻を叩きながら", new string[] { "尻叩く", "尻を叩く", "お尻叩くよ", "叩くよ" });
            addYotogiList("尻を叩く", new string[] { "尻叩く", "尻を叩く", "尻叩いて", "尻を叩かせる" });
            addYotogiList("尻を撫でる", new string[] { "尻撫でる", "お尻撫でるよ" });
            addYotogiList("尻を舐める", new string[] { "尻舐める", "お尻舐めるよ" });
            addYotogiList("尻穴セックス&前穴愛撫", new string[] { "", "尻と前を責める", "尻と前を責める", "前後を責める" });
            addYotogiList("尻穴を責めさせながら", new string[] { "尻穴責めて", "アナル責めて" });
            addYotogiList("尻穴を責めながら", new string[] { "尻穴責める", "アナル責める", "お尻弄るよ", "アナル弄るよ" });
            addYotogiList("尻穴を責める", new string[] { "尻穴責める", "アナル責める", "お尻弄るよ", "アナル弄るよ" });
            addYotogiList("尻穴を弄りながら", new string[] { "尻穴弄る", "尻穴弄るよ", "お尻弄るよ", "アナル弄るよ" });
            addYotogiList("尻穴を弄る", new string[] { "尻穴弄る", "尻穴弄るよ", "お尻弄るよ", "アナル弄るよ" });
            addYotogiList("尻穴弄り", new string[] { "尻穴弄る", "尻穴弄るよ", "お尻弄るよ", "アナル弄るよ" });
            addYotogiList("尻穴弄りして頂く", new string[] { "尻穴いじってください", "アナルいじってください" });
            addYotogiList("クリトリスでイカせる", new string[] { "クリでイって", "クリトリスでイって" });
            addYotogiList("クリトリスを責めさせながら", new string[] { "クリ責めて", "クリを責めて", "クリ触って" });
            addYotogiList("クリトリスを責めさせる", new string[] { "クリ責めて", "クリを責めて", "クリ触って" });
            addYotogiList("クリトリスを責めながら", new string[] { "クリを責める", "クリ責めるよ", "クリ触るよ" });
            addYotogiList("クリトリスを責める", new string[] { "クリを責める", "クリ責めるよ", "クリ触るよ" });
            addYotogiList("クリトリスを摘み上げる", new string[] { "クリを摘み上げる", "クリを摘まむ", "クリ摘まむよ" });
            addYotogiList("クリトリスを弄りながら", new string[] { "クリトリスを弄る", "クリを弄る", "クリ弄るよ" });
            addYotogiList("くぱぁさせながら", new string[] { "くぱぁして", "あそこ拡げて", "おまんこ拡げて" });
            addYotogiList("くぱぁさせる", new string[] { "くぱぁして", "あそこ拡げて", "おまんこ拡げて" });
            addYotogiList("くぱぁポーズをとらせる", new string[] { "くぱぁポーズして", "くぱぁして", "あそこ拡げて", "おまんこ拡げて" });
            addYotogiList("くぱぁポーズをとる", new string[] { "くぱぁポーズして", "くぱぁして", "あそこ拡げて", "おまんこ拡げて" });
            addYotogiList("くぱぁポーズを取る", new string[] { "くぱぁポーズして", "くぱぁして", "あそこ拡げて", "おまんこ拡げて" });
            addYotogiList("秘部を拡げさせる", new string[] { "くぱぁして", "あそこ拡げて", "おまんこ拡げて" });
            addYotogiList("秘部を拡げる", new string[] { "くぱぁして", "あそこ拡げて", "おまんこ拡げて" });
            addYotogiList("グラインド", new string[] { "グラインドして", "腰を回して", "グラインドする", "腰回すよ" });
            addYotogiList("グラインドさせながら", new string[] { "グラインドして", "腰を回して", "グラインドする", "腰回すよ" });
            addYotogiList("グラインドさせる", new string[] { "グラインドして", "腰を回して", "グラインドする", "腰回すよ" });
            addYotogiList("グラインドしてもらう", new string[] { "グラインドして", "腰を回して", "グラインドする", "腰回すよ" });
            addYotogiList("グラインドして見せつける", new string[] { "グラインドして", "腰を回して", "グラインドする", "腰回すよ" });
            addYotogiList("グラインドして頂きながら", new string[] { "グラインドしてください", "腰を回してください" });
            addYotogiList("グラインドして頂く", new string[] { "グラインドしてください", "腰を回してください" });
            addYotogiList("グラインドしながら", new string[] { "グラインドして", "腰を回して", "グラインドする", "腰回すよ" });
            addYotogiList("グラインドする", new string[] { "グラインドして", "腰を回して", "グラインドする", "腰回すよ" });
            addYotogiList("グラインドで責める", new string[] { "グラインドして", "腰を回して", "グラインドする", "腰回すよ" });
            addYotogiList("グラインド奉仕", new string[] { "グラインドして", "腰を回して", "グラインドする", "腰回すよ" });
            addYotogiList("グラインド奉仕を旦那に見せつける", new string[] { "グラインドして", "腰を回して", "グラインドする", "腰回すよ" });
            addYotogiList("舐めさせてもらう", new string[] { "舐めさせて", "舐めるよ" });
            addYotogiList("舐めさせて頂く", new string[] { "舐めさせていただきます", "舐めます", "舐めるよ" });
            addYotogiList("舐めさせる", new string[] { "舐めて" });
            addYotogiList("舐めしゃぶり奉仕", new string[] { "舐めて奉仕して", "しゃぶって奉仕して" });
            addYotogiList("舐める", new string[] { "舐める", "舐めるよ", "舐めて" });
            addYotogiList("舐めるのを止める", new string[] { "やめる", "やめるよ" });
            addYotogiList("クンニ", new string[] { "クンニする", "クンニするよ", "舐める", "舐めるよ" });
            addYotogiList("クンニする", new string[] { "クンニするよ", "舐める", "舐めるよ" });
            addYotogiList("手コキ", new string[] { "手コキして", "手でして", "手でこすって" });
            addYotogiList("手コキしながら咥える", new string[] { "手コキしながら咥えて", "こすりながらしゃぶって" });
            addYotogiList("手コキでイカせてもらう", new string[] { "手コキでイかせて", "手でイかせて" });
            addYotogiList("手コキでイカせる", new string[] { "手コキでイかせて", "手でイかせて" });
            addYotogiList("手コキで奉仕", new string[] { "手コキして", "手でして", "手でこすって" });
            addYotogiList("手でしてもらう", new string[] { "手コキして", "手でして", "手でこすって" });
            addYotogiList("手でもしてもらう", new string[] { "手でもして", "手コキして", "手でして", "手でこすって" });
            addYotogiList("手で奉仕させる", new string[] { "手コキして", "手でして", "手でこすって" });
            addYotogiList("順手コキ", new string[] { "手コキして", "手でして", "手でこすって" });
            //絶頂
            addYotogiList("イカさせて頂く", new string[] { "イかせてください", "イかせていただきます" });
            addYotogiList("イカせる", new string[] { "イけ", "イって", "イっちゃえ" });
            addYotogiList("イってもらう", new string[] { "イってください", "イって" });
            addYotogiList("イって頂く", new string[] { "イってください", "イって" });
            addYotogiList("絶頂", new string[] { "絶頂して", "イけ", "イって", "イっちゃえ" });
            addYotogiList("絶頂&射精", new string[] { "一緒にイくよ", "一緒にイって" });
            addYotogiList("絶頂させる", new string[] { "絶頂して", "イけ", "イって", "イっちゃえ" });
            addYotogiList("絶頂焦らし", new string[] { "焦らす", "イかせない", "イっちゃダメ" });
            addYotogiList("潮吹き絶頂", new string[] { "潮吹いて", "潮吹いてイって" });
            addYotogiList("連続絶頂", new string[] { "イきまくって", "イきまくれ", "何度もイって", "何度もイかせる" });
            addYotogiList("連続絶頂告白", new string[] { "どうだった？" });
            addYotogiList("射精", new string[] { "射精するよ", "出すよ", "出すぞ", "出るよ" });
            addYotogiList("射精させてもらう", new string[] { "射精させて", "出させて", "イかせて" });
            addYotogiList("射精させて頂く", new string[] { "射精させてください", "出させてください", "イかせてください" });
            addYotogiList("射精させる", new string[] { "射精させて", "出すよ", "出させて" });
            addYotogiList("射精する", new string[] { "射精するよ", "出すよ", "出すぞ", "出るよ" });
            addYotogiList("大量射精", new string[] { "いっぱい出す", "いっぱい出すよ", "いっぱい出るよ" });
            addYotogiList("大量射精する", new string[] { "いっぱい出す", "いっぱい出すよ", "いっぱい出るよ" });
            addYotogiList("中に出させて頂く", new string[] { "中に出させてください", "中に出します" });
            addYotogiList("中出し", new string[] { "中出しする", "中に出す", "中に出すよ" });
            addYotogiList("中出し&口内射精", new string[] { "", "中と口に出す", "同時に出すぞ", "同時に出すよ" });
            addYotogiList("中出し＆絶頂", new string[] { "", "中に出す", "中に出すよ", "中で一緒に", "一緒にイって" });
            addYotogiList("中出しおねだり", new string[] { "おねだりして", "中出しおねだりして", "中に欲しい？" });
            addYotogiList("中出しさせて頂く", new string[] { "中出しさせてください", "中出しします" });
            addYotogiList("中出しさせる", new string[] { "中出しして", "中に出す", "中に出して" });
            addYotogiList("中出しする", new string[] { "中出しするよ", "中に出す", "中に出すよ", "中に出すぞ" });
            addYotogiList("中出し感想", new string[] { "中出しどうだった？" });
            addYotogiList("膣内に注ぎ込む", new string[] { "注ぎ込む", "奥に出すよ", "子宮に注ぐ" });
            addYotogiList("孕ませおねだり", new string[] { "おねだりして", "孕みたい？", "妊娠したい？", "受精したい？", "赤ちゃん欲しい？" });
            addYotogiList("孕ませる", new string[] { "孕んで", "孕め", "妊娠して", "受精して", "赤ちゃん作ろう" });
            addYotogiList("孕ませ連続射精", new string[] { "孕んで", "孕め", "妊娠して", "受精して", "赤ちゃん作ろう" });
            addYotogiList("種付け", new string[] { "孕んで", "孕め", "妊娠して", "受精して", "赤ちゃん作ろう" });
            addYotogiList("種付けする", new string[] { "孕んで", "孕め", "妊娠して", "受精して", "赤ちゃん作ろう" });
            addYotogiList("連続種付け", new string[] { "孕ませる", "孕め", "妊娠しろ", "受精しろ" });
            addYotogiList("尻穴に注ぎ込む", new string[] { "注ぎ込む", "しりに注ぐ", "アナルに注ぐ" });
            addYotogiList("連続尻穴射精", new string[] { "しりに出す", "おしりに出すよ" });
            addYotogiList("顔にかける", new string[] { "顔に出す", "顔にかける", "ぶっかける" });
            addYotogiList("顔にぶっかけ", new string[] { "顔に出す", "顔にかける", "ぶっかける" });
            addYotogiList("顔に掛ける", new string[] { "顔に出す", "顔にかける", "ぶっかける" });
            addYotogiList("顔に出す", new string[] { "顔に出す", "顔にかける", "ぶっかける" });
            addYotogiList("顔に出す２", new string[] { "同時に出す", "同時にかける" });
            addYotogiList("顔射", new string[] { "顔に出す", "顔にかける", "ぶっかける" });
            addYotogiList("顔射&絶頂", new string[] { "", "顔射絶頂", "顔射でイって", "かけられながらイって" });
            addYotogiList("顔射２", new string[] { "そっちの顔に出すよ", "そっちにかけるよ" });
            addYotogiList("顔射させて頂く", new string[] { "顔射させてください", "顔射します", "顔に出させてください" });
            addYotogiList("顔射させる", new string[] { "顔に出す", "顔に出して", "ぶっかける", "ぶっかけて" });
            addYotogiList("顔射する", new string[] { "顔に出す", "顔にかける", "ぶっかける" });
            addYotogiList("ドヘンタイぶっかけ", new string[] { "ぶっかける" });
            addYotogiList("胸にぶっかけ", new string[] { "胸にかける", "胸にかけるよ" });
            addYotogiList("腹にぶっかけ", new string[] { "腹にかける", "腹にかけるよ" });
            addYotogiList("外に出させて頂く", new string[] { "そと出しさせてください", "外に出します", "出させてください" });
            addYotogiList("外出し", new string[] { "", "そと出し", "外に出す", "外に出すよ", "ぶっかける" });
            addYotogiList("外出し&顔射", new string[] { "", "同時にかける", "同時にかけるぞ", "同時にかけるよ", "ぶっかける" });
            addYotogiList("外出し＆絶頂", new string[] { "", "そと出し", "外に出す", "外に出すよ", "ぶっかける" });
            addYotogiList("外出しさせて頂く", new string[] { "", "そと出しさせて頂く", "外に出します", "出させてください" });
            addYotogiList("外出しさせる", new string[] { "", "そと出しさせる", "そと出しして", "外に出して", "ぶっかけて" });
            addYotogiList("外出しする", new string[] { "", "そと出しする", "外に出す", "外に出すよ", "ぶっかける" });
            addYotogiList("口に出す", new string[] { "口に出すよ" });
            addYotogiList("口に出す２", new string[] { "こっちに出す", "こっちの口に出す", "こっちに出すよ" });
            addYotogiList("口内に射精させて頂く", new string[] { "口に出させてください" });
            addYotogiList("口内射精", new string[] { "口に出す", "口に出すよ" });
            addYotogiList("口内射精&絶頂", new string[] { "", "口でイって", "一緒にイって" });
            addYotogiList("口内射精２", new string[] { "そっちに出す", "そっちの口に出す" });
            addYotogiList("口内射精させる", new string[] { "口に出させる" });
            addYotogiList("口内射精する", new string[] { "口に出す", "口に出すよ" });
            addYotogiList("口内出しおねだり", new string[] { "おねだりして", "口におねだりして" });
            addYotogiList("精液の感想を言わせる", new string[] { "精液おいしい？" });
            addYotogiList("精液を飲み込ませる", new string[] { "精液飲んで", "ごっくんして" });
            addYotogiList("精液飲み", new string[] { "精液飲んで", "ごっくんして" });
            //その他コマンド
            addYotogiList("『……話してほしいな』", new string[] { "", "話してほしいな" });
            addYotogiList("『アナルセックスの感想を聞かせて』", new string[] { "感想を聞かせて", "感想は？", "どうだった？" });
            addYotogiList("『オナホ扱いされる気分はどうか？』", new string[] { "気分はどう？", "オナホの気分は？" });
            addYotogiList("『オレへの気持ちを告白させる』", new string[] { "気持ちを告白して", "告白して" });
            addYotogiList("『お客様にどんな風に触られてる？』", new string[] { "どんな風に触られてる？" });
            addYotogiList("『お客様にどんな奉仕をしたの？』", new string[] { "どんな奉仕をしたの？" });
            addYotogiList("『お客様の前でオナニーしたの？』", new string[] { "オナニーしたの？" });
            addYotogiList("『お客様の前で楽しんでくれた？』", new string[] { "楽しんでくれた？" });
            addYotogiList("『キスするね……』", new string[] { "キスするね" });
            addYotogiList("『ココにチンポを……』", new string[] { "ココにチンポを" });
            addYotogiList("『ご主人様とどっちがいい？』", new string[] { "どっちがいい？" });
            addYotogiList("『セックスの感想を聞かせて』", new string[] { "感想を聞かせて", "感想は？", "どうだった？" });
            addYotogiList("『そっちからおねだりしてよ』", new string[] { "おねだりしてよ" });
            addYotogiList("『ちゃんと中出ししてもらった？』", new string[] { "中出ししてもらった？" });
            addYotogiList("『もう一度聞くけど……俺の方がいいよね？』", new string[] { "俺の方がいいよね？" });
            addYotogiList("『もう慣れてきたよね……？』", new string[] { "もう慣れてきたよね" });
            addYotogiList("『もっと詳細にされた事を告白して』", new string[] { "された事を告白して", "もっと詳細に" });
            addYotogiList("『嘘をつかずに、された事を答えてね』", new string[] { "された事を答えてね", "答えてね" });
            addYotogiList("『何回お客様をイカせたの？』", new string[] { "何回イカせたの？" });
            addYotogiList("『興奮してる、変態なの？』", new string[] { "変態なの？" });
            addYotogiList("『激しくしている所、見せてあげよう』", new string[] { "見せてあげよう" });
            addYotogiList("『今、どんな気持ち？』", new string[] { "どんな気持ち？" });
            addYotogiList("『自分からいっぱい搾り取ったの？』", new string[] { "いっぱい搾り取ったの？" });
            addYotogiList("『自分で動いて気持ち良くしたの？』", new string[] { "動いて気持ち良くしたの？" });
            addYotogiList("『女王様に従います』", new string[] { "従います" });
            addYotogiList("『誰のセックスが一番気持ちいい？』", new string[] { "誰が一番気持ちいい？", "誰がよかった？" });
            addYotogiList("『恥ずかしがらず教えてほしいな』", new string[] { "教えてほしいな" });
            addYotogiList("『中出ししている所を見せつける』", new string[] { "見せつける" });
            addYotogiList("『彼に何をしてあげたの？』", new string[] { "何をしてあげたの？" });
            addYotogiList("『目の前で中出しするね』", new string[] { "中出しするね" });
            addYotogiList("【報告】オナニーさせる", new string[] { "オナニーさせる", "オナニーしたの？", "オナニーして", "オナって" });
            addYotogiList("【報告】キスしながら", new string[] { "キスしながら", "キスしたの？", "キスして" });
            addYotogiList("【報告】くぱぁさせながら", new string[] { "くぱぁさせながら", "くぱぁしたの？", "くぱぁして" });
            addYotogiList("【報告】胸を揉みながら", new string[] { "胸を揉みながら", "胸揉まれたの？", "胸揉むよ", "おっぱい揉むよ" });
            addYotogiList("【報告】自分の胸を揉ませながら", new string[] { "自分の胸を揉ませながら", "自分で揉んだの？", "胸揉んで", "おっぱい揉んで" });
            addYotogiList("【報告】素股させる", new string[] { "すまたさせる", "すまたしたの？", "すまたして" });
            addYotogiList("【報告】放尿した事を報告する", new string[] { "放尿した事を報告する", "おしっこしたの？", "漏らしたの？" });
            addYotogiList("【報告】乱暴に責める", new string[] { "乱暴に責める", "乱暴されたの？" });
            addYotogiList("２人に会話してもらう", new string[] { "会話して", "ふたりで会話して" });
            addYotogiList("Aに外出しする", new string[] { "", "そと出しする", "そと出しするよ", "外に出すよ" });
            addYotogiList("Aに顔射", new string[] { "顔に出すよ" });
            addYotogiList("Aに玉弄りさせる", new string[] { "玉いじって" });
            addYotogiList("Aに口内射精", new string[] { "口に出すよ" });
            addYotogiList("Aに尻穴を責めさせる", new string[] { "尻穴責めて", "お尻にして" });
            addYotogiList("Aに中出しする", new string[] { "", "中出しするよ", "中に出すよ" });
            addYotogiList("Aに丁寧に咥えさせる", new string[] { "丁寧に咥えて" });
            addYotogiList("Aに壷洗いさせる", new string[] { "つほ洗いして", "あそこで洗って" });
            addYotogiList("Aに咥えさせる", new string[] { "咥えて", "しゃぶって" });
            addYotogiList("Aを強く責める", new string[] { "強く責める", "強くするよ" });
            addYotogiList("Aを責める", new string[] { "責める", "責めるよ" });
            addYotogiList("Aを絶頂させる", new string[] { "絶頂させる", "イって" });
            addYotogiList("Bに顔射", new string[] { "そっちの顔に出すよ" });
            addYotogiList("Bに玉弄りさせる", new string[] { "そっちも玉いじって" });
            addYotogiList("Bに口内射精", new string[] { "そっちの口に出すよ" });
            addYotogiList("Bに尻穴を責めさせる", new string[] { "そっちも尻穴責めて" });
            addYotogiList("Bに丁寧に咥えさせる", new string[] { "そっちも丁寧に咥えて" });
            addYotogiList("Bに壷洗いさせる", new string[] { "そっちもつぼ洗いして", "そっちも洗って" });
            addYotogiList("Bに咥えさせる", new string[] { "そっちも咥えて", "そっちもしゃぶって" });
            addYotogiList("Bをクンニでイカせる", new string[] { "クンニでイって" });
            addYotogiList("Bを強く責める", new string[] { "そっちも強くするよ" });
            addYotogiList("Bを責める", new string[] { "そっちも責めるよ" });
            addYotogiList("Bを絶頂させる", new string[] { "そっちもイって" });
            addYotogiList("M豚で惨めなメイドを罵倒する", new string[] { "罵倒する" });
            addYotogiList("アナル弄り", new string[] { "アナル弄るよ", "尻穴いじるよ", "お尻いじるよ" });
            addYotogiList("イチャイチャする", new string[] { "イチャイチャしよ" });
            addYotogiList("いちゃいちゃする", new string[] { "いちゃいちゃしよ" });
            addYotogiList("いっぱいぺろぺろする", new string[] { "いっぱい舐める", "いっぱい舐めるよ" });
            addYotogiList("いっぱい責める", new string[] { "いっぱい責めるよ", "いっぱいするよ" });
            addYotogiList("イラマチオ", new string[] { "つっこむ", "しゃぶって", "しゃぶれ" });
            addYotogiList("イラマチオする", new string[] { "つっこむ", "しゃぶって", "しゃぶれ" });
            addYotogiList("エッチにイジめる", new string[] { "いじめる" });
            addYotogiList("えっちに弄る", new string[] { "いじるよ" });
            addYotogiList("おっぱいに甘える", new string[] { "甘えさせて", "おっぱいして" });
            addYotogiList("オナホでシゴいて頂く", new string[] { "オナホでしごいてください", "しごいてください", "オナホでしごいて" });
            addYotogiList("オナホで苛めて頂く", new string[] { "オナホで苛めてください", "苛めてください", "オナホで苛めて" });
            addYotogiList("オナニーさせる", new string[] { "オナニーして", "オナニー見せて", "おなって" });
            addYotogiList("オナニーして頂く", new string[] { "オナニーしてください" });
            addYotogiList("オナニーしながら", new string[] { "オナニーして", "おなって" });
            addYotogiList("おねだりオナニー", new string[] { "おねだりして" });
            addYotogiList("おねだりさせる", new string[] { "おねだりして" });
            addYotogiList("おもらしさせる", new string[] { "おもらしして", "漏らして" });
            addYotogiList("お客様にセックスの実況を聞かせる", new string[] { "セックスの実況を聞かせる", "実況を聞かせる" });
            addYotogiList("お客様に強く責めさせる", new string[] { "強く責めさせる" });
            addYotogiList("お客様に責めさせる", new string[] { "責めさせる" });
            addYotogiList("お客様に丁寧に責めさせる", new string[] { "丁寧に責めさせる" });
            addYotogiList("お客様に奉仕させる", new string[] { "お客様に奉仕して", "奉仕して" });
            addYotogiList("お客様のモノを丁寧に咥えさせる", new string[] { "丁寧に咥えさせる", "丁寧に咥えて" });
            addYotogiList("お客様のモノを咥えさせる", new string[] { "咥えさせる" });
            addYotogiList("お客様を止めさせる", new string[] { "やめさせる" });
            addYotogiList("お情けでフェラさせてやる", new string[] { "フェラさせてやる" });
            addYotogiList("お尻の穴を弄りながら", new string[] { "尻穴弄る", "お尻弄るよ", "アナル弄る" });
            addYotogiList("お尻を見る", new string[] { "お尻見せて", "お尻を見せて" });
            addYotogiList("お掃除フェラ", new string[] { "お掃除フェラして", "お掃除して" });
            addYotogiList("がに股ポーズを取らせる", new string[] { "がに股ポーズして", "がに股になって" });
            addYotogiList("ぐりぐりと踏んで頂く", new string[] { "ぐりぐりと踏んでください", "踏んでください" });
            addYotogiList("こき下ろして頂く", new string[] { "こき下ろしてください" });
            addYotogiList("こんな風にキスされたの？", new string[] { "こんな風にされたの？" });
            addYotogiList("こんな風に胸を揉まれたの？", new string[] { "こんな風に揉まれたの？" });
            addYotogiList("ご主人様を貶める", new string[] { "おとしめてください", "おとしめて" });
            addYotogiList("しっかり咥えろと罵る", new string[] { "しっかり咥えろ" });
            addYotogiList("スワッピング連続中出し", new string[] { "連続中出し", "連続で出すよ", "何度も出す" });
            addYotogiList("セックスの感想を聞く", new string[] { "感想を聞かせて", "セックスどうだった？", "気持ちよかった？" });
            addYotogiList("セックス中のメイドにおねだりさせる", new string[] { "おねだりして" });
            addYotogiList("セックス中のメイドに感想を聞く", new string[] { "感想を聞く", "気持ちいい？" });
            addYotogiList("ダブルピースしながら", new string[] { "ダブルピースして", "ダブピして" });
            addYotogiList("ダブルピースをしながら", new string[] { "ダブルピースして", "ダブピして" });
            addYotogiList("たわし洗いをさせる", new string[] { "たわし洗いして" });
            addYotogiList("チェンジする", new string[] { "チェンジして", "入れ替わって" });
            addYotogiList("ちょっと強引にイラマチオする", new string[] { "イラマチオする", "つっこむ" });
            addYotogiList("ちんちんポーズを取らせる", new string[] { "ちんちんポーズして", "ちんちんして" });
            addYotogiList("ちんちんをさせる", new string[] { "ちんちんして" });
            addYotogiList("チンポの感想を聞く", new string[] { "チンポどうだった？", "チンポよかった？" });
            addYotogiList("どこが敏感になっているのか言わせる", new string[] { "どこが敏感になってる？", "どこが感じる？" });
            addYotogiList("とても動いて頂く", new string[] { "とても動いてください", "たくさん動いてください", "いっぱい動いてください" });
            addYotogiList("パートナーに外出しさせる", new string[] { "", "パートナーにそと出しさせる", "そと出しさせる" });
            addYotogiList("パートナーに中出しさせる", new string[] { "中出しさせる" });
            addYotogiList("パートナーを絶頂させる", new string[] { "絶頂させる", "絶頂して", "イって" });
            addYotogiList("パイズリ", new string[] { "パイズリして", "胸でして", "胸で挟んで" });
            addYotogiList("バイブで責める", new string[] { "バイブで責めるよ", "バイブ入れるよ", "バイブでするよ" });
            addYotogiList("バイブの感想を言わせる", new string[] { "バイブどう？", "バイブ気持ちいい？" });
            addYotogiList("バイブをグラインドさせる", new string[] { "バイブまわすよ" });
            addYotogiList("バイブをぐりぐりしながら", new string[] { "ぐりぐりする", "ぐりぐりするよ" });
            addYotogiList("バイブを舐めさせる", new string[] { "バイブ舐めて" });
            addYotogiList("バイブ挿入+寸止め告白", new string[] { "寸止め" });
            addYotogiList("バイブ挿入+連続寸止め懇願", new string[] { "連続寸止め", "イかせてほしい？" });
            addYotogiList("ハメ撮り告白撮影", new string[] { "ハメ撮り告白", "告白して", "どんな気分？" });
            addYotogiList("ピースしながら", new string[] { "ピースして" });
            addYotogiList("ピースをしながら", new string[] { "ピースして" });
            addYotogiList("ペニスで激しく口を責める", new string[] { "激しく口を責める", "激しくするよ", "もっとしゃぶって" });
            addYotogiList("ペニスで口を責める", new string[] { "口を責める", "入れるよ", "つっこむ", "しゃぶって" });
            addYotogiList("ペニスと指で責める", new string[] { "責める", "指で責めて" });
            addYotogiList("ペニスをグラインドして詰る", new string[] { "グラインドして", "ペニスを詰って" });
            addYotogiList("ペニスをしごいて射精させる", new string[] { "しごいて射精させて", "しごいてイかせて" });
            addYotogiList("ペニスをしごきながら", new string[] { "しごいて", "ちんこしごいて", "チンポしごいて" });
            addYotogiList("ペニスをねだらせる", new string[] { "おねだりして", "なにが欲しい？", "入れて欲しい？" });
            addYotogiList("ペニスを扱いて頂く", new string[] { "しごいてください", "ちんこしごいて", "チンポしごいて" });
            addYotogiList("ペニスを擦りつけてその気にさせる", new string[] { "こすりつける", "こすりつけるよ", "チンポこすりつけるよ", "その気にさせる" });
            addYotogiList("ペニスを擦りつける", new string[] { "こすりつける", "こすりつけるよ", "チンポこすりつけるよ" });
            addYotogiList("ペニスを擦り付ける", new string[] { "こすりつける", "こすりつけるよ", "チンポこすりつけるよ" });
            addYotogiList("ペニスを比べさせる", new string[] { "どっちがいい？", "どっちが大きい？" });
            addYotogiList("ぺろぺろする", new string[] { "ぺろぺろして", "ぺろぺろするよ", "舐めるよ" });
            addYotogiList("ポーズをとらせるA", new string[] { "おすわりポーズ", "おすわりして" });
            addYotogiList("ポーズをとらせるB", new string[] { "おっぱいポーズ", "おっぱい寄せて" });
            addYotogiList("ポーズをとらせるC", new string[] { "セクシーポーズ", "足を上げて" });
            addYotogiList("ホールドおねだり", new string[] { "ホールドおねだりして", "おねだりして", "ぎゅってしてほしい？" });
            addYotogiList("マゾであると認めさせる", new string[] { "マゾを認めろ", "マゾなんだろ" });
            addYotogiList("ママにして貰う", new string[] { "ママ、して", "ママおねがい" });
            addYotogiList("ママにディープキスしてもらう", new string[] { "ベロチューして" });
            addYotogiList("みんなで責める", new string[] { "みんなでするよ" });
            addYotogiList("みんなに奉仕させる", new string[] { "みんなに奉仕して", "みんなにして" });
            addYotogiList("ムチで胸を叩く", new string[] { "胸を叩く", "おっぱい叩く" });
            addYotogiList("ムチで秘部を叩く", new string[] { "秘部を叩く", "あそこを叩く", "おまんこ叩く" });
            addYotogiList("メイドが自分の胸を揉みながら", new string[] { "胸揉んで", "おっぱい揉んで", "自分で揉んで" });
            addYotogiList("メイドが囁く", new string[] { "ささやいて", "声聞かせて" });
            addYotogiList("メイドにオナニーさせる", new string[] { "オナニーして" });
            addYotogiList("メイドにキスさせながら", new string[] { "キスして" });
            addYotogiList("メイドをお客様に強く責めさせる", new string[] { "強く責めさせる" });
            addYotogiList("メイドをお客様に責めさせる", new string[] { "責めさせる" });
            addYotogiList("ラブバイブでイカせる", new string[] { "バイブでイって", "イって" });
            addYotogiList("ラブバイブでグラインドする", new string[] { "グラインドする", "バイブまわすよ" });
            addYotogiList("ラブバイブで強く責める", new string[] { "強く責める", "強くするよ" });
            addYotogiList("ラブバイブで高速責め", new string[] { "高速責め", "速くするよ" });
            addYotogiList("ラブバイブで最奥責め", new string[] { "最奥責め", "奥を責めるよ" });
            addYotogiList("ラブバイブで責める", new string[] { "責める", "バイブ入れるよ" });
            addYotogiList("ラブバイブを止める", new string[] { "とめる", "バイブとめる" });
            addYotogiList("リードさせる", new string[] { "リードして" });
            addYotogiList("ロウを胸に垂らす", new string[] { "胸に垂らす", "おっぱいに垂らす" });
            addYotogiList("ロウを秘部に垂らす", new string[] { "秘部に垂らす", "あそこに垂らす", "おまんこに垂らす" });
            addYotogiList("ローションを塗る", new string[] { "ローション塗って" });
            addYotogiList("愛液を見せつける", new string[] { "いっぱい出てるよ", "べとべとだよ", "びちょびちょだよ" });
            addYotogiList("愛液を味わう", new string[] { "愛液舐めるよ", "愛液飲むよ", "愛液吸うよ" });
            addYotogiList("愛撫しあう", new string[] { "愛撫しよう", "触りあう" });
            addYotogiList("愛撫する", new string[] { "さわるよ", " いじるよ" });
            addYotogiList("愛撫中のメイドにおねだりさせる", new string[] { "愛撫おねだりして" });
            addYotogiList("愛撫中のメイドに感想を聞く", new string[] { "愛撫どう？", "愛撫気持ちいい？" });
            addYotogiList("圧迫しながら抽送する", new string[] { "抽送する" });
            addYotogiList("一人に咥えさせるA", new string[] { "そっちも咥えて", "そっちもしゃぶって" });
            addYotogiList("一人に咥えさせるB", new string[] { "ひとりで咥えて", "ひとりでしゃぶって" });
            addYotogiList("淫語実況", new string[] { "実況して", "淫語実況して", "エロ実況して" });
            addYotogiList("淫乱だと罵倒する", new string[] { "淫乱だ", "淫乱め" });
            addYotogiList("淫乱な女に玉弄りさせる", new string[] { "玉弄りして" });
            addYotogiList("淫乱抽送", new string[] { "淫乱に動いて", "エッチに動いて" });
            addYotogiList("汚い尻穴を責めていただく", new string[] { "汚い尻穴を責めてください", "尻穴を責めてください" });
            addYotogiList("汚い尻穴を舐めて頂く", new string[] { "汚い尻穴を舐めてください", "尻穴を舐めてください" });
            addYotogiList("横に咥える", new string[] { "横に咥えて" });
            addYotogiList("下品セックス", new string[] { "下品にして", "下品になれ" });
            addYotogiList("何が気持ちいいのか語らせる", new string[] { "なにが気持ちいい？" });
            addYotogiList("我慢できず乱暴に犯してしまう", new string[] { "もう我慢できない", "我慢できない" });
            addYotogiList("会話させる", new string[] { "会話して" });
            addYotogiList("会話する", new string[] { "会話して" });
            addYotogiList("会話をする", new string[] { "会話して" });
            addYotogiList("学んだ知識を教えてもらう", new string[] { "教えて" });
            addYotogiList("感じているかいやらしく聞かせる", new string[] { "感じてる？", "感じちゃってる？" });
            addYotogiList("感じているかいやらしく聞く", new string[] { "感じてる？", "感じちゃってる？" });
            addYotogiList("感想を言ってもらう", new string[] { "感想言って", "どんな気持ち？" });
            addYotogiList("感想を言わせる", new string[] { "感想言って", "どんな気持ち？" });
            addYotogiList("感想を聞く", new string[] { "感想を聞かせて", "どんな気持ち？" });
            addYotogiList("甘えさせる", new string[] { "甘えて" });
            addYotogiList("甘やかしてもらう", new string[] { "甘やかして" });
            addYotogiList("甘やかして貰う", new string[] { "甘やかして" });
            addYotogiList("甘やかしなでなで", new string[] { "甘やかして", "なでなでして" });
            addYotogiList("亀頭を中心に奉仕", new string[] { "亀頭を奉仕して", "さきっぽ奉仕して", "さきっぽして" });
            addYotogiList("亀頭責め", new string[] { "亀頭責めて", "さきっぽ責めて", "さきっぽして" });
            addYotogiList("亀頭弄りして頂く", new string[] { "亀頭いじってください", "さきっぽいじってください" });
            addYotogiList("詰って頂く", new string[] { "なじってください" });
            addYotogiList("脚にロウを垂らす", new string[] { "脚に垂らす" });
            addYotogiList("脚に高温ロウを垂らす", new string[] { "脚に垂らす" });
            addYotogiList("脚をムチで叩く", new string[] { "脚を叩く" });
            addYotogiList("脚を一本ムチで叩く", new string[] { "脚を叩く" });
            addYotogiList("休憩させる", new string[] { "休憩して", "休んで" });
            addYotogiList("強くオナニーさせる", new string[] { "強くして", "もっとオナニーして", "もっとオナって" });
            addYotogiList("強くバイブで責める", new string[] { "強く責める", "強くする", "強くするよ" });
            addYotogiList("強くペニスと指で責める", new string[] { "強く責める", "強くする", "強くするよ" });
            addYotogiList("強くママにして貰う", new string[] { "強くして", "ママもっとして" });
            addYotogiList("強く素股させる", new string[] { "強くすまたして", "強くこすって", "強くして" });
            addYotogiList("強く二人に奉仕させる", new string[] { "強く奉仕して", "強くふたりで奉仕して" });
            addYotogiList("強く犯させる", new string[] { "強く犯して", "強く犯す" });
            addYotogiList("強く奉仕セックスさせる", new string[] { "強くして", "もっと奉仕して" });
            addYotogiList("強めに奉仕させる", new string[] { "強めに奉仕して", "強くして", "もっと奉仕して" });
            addYotogiList("強めに奉仕してもらう", new string[] { "強めに奉仕して", "強くして", "もっと奉仕して" });
            addYotogiList("強めに咥えさせる", new string[] { "強めに咥えて", "強く咥えて", "強くしゃぶって", "もっとしゃぶって" });
            addYotogiList("強めに咥えて頂く", new string[] { "強くくわえてください" });
            addYotogiList("強引に責める", new string[] { "強引にする", "強くするよ", "強くするぞ" });
            addYotogiList("強引に電マ責め", new string[] { "強くする", "強くするよ", "強くするぞ" });
            addYotogiList("強制お掃除フェラ", new string[] { "掃除しろ", "掃除させる" });
            addYotogiList("胸だけでも揉ませてやる", new string[] { "揉ませてやる" });
            addYotogiList("胸とお尻を揉みながら", new string[] { "胸とお尻揉むよ", "両方揉むよ" });
            addYotogiList("胸にロウを垂らす", new string[] { "胸に垂らす", "おっぱいに垂らす" });
            addYotogiList("胸に高温ロウを垂らす", new string[] { "胸に垂らす", "おっぱいに垂らす" });
            addYotogiList("胸をムチで叩く", new string[] { "胸を叩く", "おっぱい叩く" });
            addYotogiList("胸を一本ムチで叩く", new string[] { "胸を叩く", "おっぱい叩く" });
            addYotogiList("胸を叩く", new string[] { "胸を叩く", "おっぱい叩く" });
            addYotogiList("玉を踏んで頂く", new string[] { "玉を踏んでください", "踏んでください" });
            addYotogiList("玉を弄って頂く", new string[] { "玉を弄ってください" });
            addYotogiList("玉を弄らせる", new string[] { "玉弄って", "玉を弄って" });
            addYotogiList("玉を舐めさせる", new string[] { "玉舐めて", "玉を舐めて" });
            addYotogiList("玉舐め", new string[] { "玉舐めて", "玉を舐めて" });
            addYotogiList("金玉を揉んで頂く", new string[] { "金玉を揉んでください", "玉を揉んでください" });
            addYotogiList("金玉奉仕", new string[] { "金玉奉仕して", "玉に奉仕して" });
            addYotogiList("靴を舐める", new string[] { "靴舐めて", "靴を舐めて" });
            addYotogiList("犬ポーズを取らせる", new string[] { "犬ポーズ", "犬ポーズして", "わんわんポーズして" });
            addYotogiList("見下して罵って頂く", new string[] { "罵ってください" });
            addYotogiList("言葉責め", new string[] { "言葉責めして", "なじって" });
            addYotogiList("交互に突く", new string[] { "交互に突くよ" });
            addYotogiList("交互パイズリ", new string[] { "交互にパイズリして", "交互にして" });
            addYotogiList("口を塞ぎながら", new string[] { "口を塞ぐ", "口を塞ぐよ" });
            addYotogiList("口を塞ぐ", new string[] { "口を塞ぐよ" });
            addYotogiList("口を覆う", new string[] { "口を覆って", "くち覆って" });
            addYotogiList("口汚く詰って頂く", new string[] { "なじってください" });
            addYotogiList("口汚く罵っていただく", new string[] { "口汚く罵ってください", "罵ってください" });
            addYotogiList("口汚く罵って頂く", new string[] { "口汚く罵ってください", "罵ってください" });
            addYotogiList("喉奥を責めながら", new string[] { "奥を責める", "奥まで咥えて", "奥までしゃぶって" });
            addYotogiList("高速責め", new string[] { "速くするよ" });
            addYotogiList("告白させる", new string[] { "告白して", "どんな気持ち？" });
            addYotogiList("今の気持ちを告白させる", new string[] { "告白して", "どんな気持ち？" });
            addYotogiList("根元舐め", new string[] { "根本舐めて", "根本を舐めて" });
            addYotogiList("最奥を擦らせる", new string[] { "奥をこする", "奥をこすって", "一番奥をこすって" });
            addYotogiList("最奥を責めながら", new string[] { "奥を責める", "奥を責めるよ", "奥責めるよ" });
            addYotogiList("最奥を責める", new string[] { "一番奥を責めて", "奥を責めて" });
            addYotogiList("最奥責め", new string[] { "奥を責める", "奥を責めるよ", "奥責めるよ" });
            addYotogiList("撮影しながら", new string[] { "奥を責める", "ハメ撮り", "ハメ撮りするよ" });
            addYotogiList("撮影する", new string[] { "撮るよ", "ハメ撮り", "ハメ撮りするよ" });
            addYotogiList("擦りつけさせて頂く", new string[] { "こすりつけさせてください", "こすりつけます" });
            addYotogiList("擦りつける", new string[] { "こすりつけるよ", "こするよ" });
            addYotogiList("惨めなメイドを罵倒する", new string[] { "罵倒する" });
            addYotogiList("残念ながら舐めるのを止める", new string[] { "やめさせていただきます" });
            addYotogiList("四つん這い尻舐め手コキ", new string[] { "手コキして", "尻舐め手コキして" });
            addYotogiList("指で愛撫", new string[] { "指でするよ" });
            addYotogiList("指で弄りながら", new string[] { "指でするよ", "指でいじるよ" });
            addYotogiList("指を増やしてオナニーさせる", new string[] { "指を増やして" });
            addYotogiList("私は変態ですと言わせる", new string[] { "変態ですと言って" });
            addYotogiList("自分が動く", new string[] { "こっちが動くよ" });
            addYotogiList("自分で胸を揉みながら", new string[] { "胸を揉んで", "胸揉んで", "おっぱい揉んで", "自分で揉んで" });
            addYotogiList("自分で動く", new string[] { "自分で動いて", "こっちが動くよ" });
            addYotogiList("自分の胸を揉ませながら", new string[] { "胸を揉んで", "胸揉んで", "おっぱい揉んで", "自分で揉んで" });
            addYotogiList("自分の胸を揉みながら", new string[] { "胸を揉んで", "胸揉んで", "おっぱい揉んで", "自分で揉んで" });
            addYotogiList("自分の胸を揉みながら＆特殊愛撫", new string[] { "胸を揉んで", "胸揉んで", "おっぱい揉んで", "自分で揉んで" });
            addYotogiList("邪淫に翻弄する", new string[] { "翻弄して" });
            addYotogiList("主従逆転でオナホで苛めて頂く", new string[] { "苛めてください" });
            addYotogiList("主従逆転で口汚く罵って頂く", new string[] { "罵ってください" });
            addYotogiList("手マンさせて頂く", new string[] { "手マンさせてください", "手マンします" });
            addYotogiList("手マンする", new string[] { "手マンするよ", "指でするよ" });
            addYotogiList("首を絞めながら", new string[] { "首を絞める" });
            addYotogiList("首絞めながら", new string[] { "首を絞める" });
            addYotogiList("熟練した技術でたわし洗いをさせる", new string[] { "たわし洗いして" });
            addYotogiList("熟練した技術で胸洗いをさせる", new string[] { "胸で洗って" });
            addYotogiList("熟練した技術で乳首を舐めさせる", new string[] { "乳首で洗って" });
            addYotogiList("熟練した技術で壺洗いをさせる", new string[] { "ツボ洗いして", "あそこで洗って" });
            addYotogiList("女王様にお尻の穴を弄って頂く", new string[] { "尻穴を弄ってください", "アナルを弄ってください" });
            addYotogiList("女王様に詰って頂く", new string[] { "なじってください" });
            addYotogiList("女王様に止めて頂く", new string[] { "やめてください" });
            addYotogiList("女王様に射精させて頂く", new string[] { "出させてください" });
            addYotogiList("女王様に手で嬲って頂く", new string[] { "なぶってください" });
            addYotogiList("女王様に素股して頂く", new string[] { "すまたしてください" });
            addYotogiList("女王様に丁寧に手で嬲って頂く", new string[] { "丁寧になぶってください" });
            addYotogiList("女王様に秘部を押し付けられる", new string[] { "押し付けてください", "あそこを押し付けてください" });
            addYotogiList("女王様に弄ばれる", new string[] { "もてあそんでください" });
            addYotogiList("小馬鹿にする", new string[] { "小馬鹿にしてください" });
            addYotogiList("焦らされる", new string[] { "焦らして" });
            addYotogiList("焦らしている所を見せつける", new string[] { "焦らす" });
            addYotogiList("焦らしてみる", new string[] { "焦らす" });
            addYotogiList("焦らして様子を見る", new string[] { "焦らす", "様子を見る" });
            addYotogiList("焦らしながら", new string[] { "焦らす" });
            addYotogiList("焦らし続ける", new string[] { "焦らす" });
            addYotogiList("焦らすように亀頭を舐める", new string[] { "亀頭を焦らして", "亀頭を舐めて", "先っぽ舐めて" });
            addYotogiList("上級奉仕パイズリ", new string[] { "パイズリ", "パイズリして", "胸でして" });
            addYotogiList("上級奉仕フェラチオ", new string[] { "フェラチオ", "フェラして", "しゃぶって" });
            addYotogiList("上級奉仕全身洗い", new string[] { "全身で洗って", "洗って" });
            addYotogiList("触りっこ", new string[] { "触りっこしよ" });
            addYotogiList("深く咥える", new string[] { "深く咥えて", "奥まで咥えて" });
            addYotogiList("寸止め", new string[] { "寸止めする" });
            addYotogiList("性癖覚醒【いじられ好き】", new string[] { "", "いじられ好き" });
            addYotogiList("性癖覚醒【お尻中出し大好き】", new string[] { "", "お尻中出し大好き" });
            addYotogiList("性癖覚醒【お漏らし癖】", new string[] { "", "お漏らし癖" });
            addYotogiList("性癖覚醒【ごっくん大好き】", new string[] { "", "ごっくん大好き" });
            addYotogiList("性癖覚醒【ドＭの素質】", new string[] { "", "ドＭの素質" });
            addYotogiList("性癖覚醒【ドS女王様】", new string[] { "", "ドS女王様" });
            addYotogiList("性癖覚醒【ムチがご褒美】", new string[] { "", "ムチがご褒美" });
            addYotogiList("性癖覚醒【ロウがご褒美】", new string[] { "", "ロウがご褒美" });
            addYotogiList("性癖覚醒【甘えん坊】", new string[] { "", "甘えん坊" });
            addYotogiList("性癖覚醒【中出し大好き】", new string[] { "", "中出し大好き" });
            addYotogiList("性癖覚醒【桃色泡姫】", new string[] { "", "桃色泡姫" });
            addYotogiList("性癖覚醒【罵倒がご褒美】", new string[] { "", "罵倒がご褒美" });
            addYotogiList("性癖覚醒【敏感なクリトリス】", new string[] { "", "敏感なクリトリス", "クリを敏感にして" });
            addYotogiList("性癖覚醒【敏感体質】", new string[] { "", "敏感体質", "敏感になって" });
            addYotogiList("性癖覚醒【変態の素質】", new string[] { "", "変態の素質", "変態になって" });
            addYotogiList("聖水を浴びさせて頂く", new string[] { "聖水をかけてください" });
            addYotogiList("声を出させる", new string[] { "声出して" });
            addYotogiList("洗わせる", new string[] { "洗って" });
            addYotogiList("洗わせる２", new string[] { "こっちも洗って", "逆向きで洗って" });
            addYotogiList("煽られてしまう", new string[] { "煽ってください" });
            addYotogiList("前穴を弄る", new string[] { "前穴弄って", "前を弄って", "あそこ弄って", "おまんこ弄って" });
            addYotogiList("素股", new string[] { "すまたして", "股でこすって", "あそこでこすって" });
            addYotogiList("素股させる", new string[] { "すまたして", "股でこすって", "あそこでこすって" });
            addYotogiList("素股で焦らして頂く", new string[] { "すまたで焦らしてください", "股で焦らしてください", "あそこで焦らしてください" });
            addYotogiList("素股で焦らす", new string[] { "すまたで焦らして", "股で焦らして", "あそこで焦らして" });
            addYotogiList("素股をさせる", new string[] { "すまたして", "股でこすって", "あそこでこすって" });
            addYotogiList("足で奉仕して頂く", new string[] { "足で奉仕してください", "足で踏んでください", "足でしてください" });
            addYotogiList("足を舐める", new string[] { "足を舐めて", "足舐めて" });
            addYotogiList("足洗い", new string[] { "足で洗って", "足でして" });
            addYotogiList("太腿を舐める", new string[] { "", "ふとももを舐める", "ふともも舐めるよ" });
            addYotogiList("大胆に責める", new string[] { "大胆に責めて", "大胆に責めるよ", "大胆に動いて" });
            addYotogiList("叩きつけ押さえつける", new string[] { "押さえつける" });
            addYotogiList("恥ずかしいポーズを取らせながら", new string[] { "恥ずかしいポーズして", "エッチなポーズして" });
            addYotogiList("注入したままセックスする", new string[] { "セックスする", "このままする" });
            addYotogiList("調教状況告白", new string[] { "気分はどう？", "どんな気持ち？" });
            addYotogiList("超・みんなで責める", new string[] { "みんなで責める", "みんなでするよ" });
            addYotogiList("超・みんなに奉仕させる", new string[] { "みんなに奉仕させる", "みんなに奉仕して", "みんなにして" });
            addYotogiList("超・責める", new string[] { "責める" });
            addYotogiList("超・奉仕させる", new string[] { "奉仕させる", "もっと奉仕して" });
            addYotogiList("痛烈にビンタをして頂く", new string[] { "ビンタしてください" });
            addYotogiList("抵抗させる", new string[] { "抵抗して", "嫌がって" });
            addYotogiList("電マ告白", new string[] { "でんまどうだった？" });
            addYotogiList("電マ責め", new string[] { "でんまで責める" });
            addYotogiList("電流告白", new string[] { "電気どうだった？" });
            addYotogiList("踏まれながら", new string[] { "踏んで", "踏んでください" });
            addYotogiList("頭を掴みながら", new string[] { "頭をつかむ" });
            addYotogiList("頭を踏んであげる", new string[] { "踏んであげる" });
            addYotogiList("頭を撫でさせる", new string[] { "撫でさせる", "頭を撫でる", "撫でる" });
            addYotogiList("頭を撫でてもらう", new string[] { "撫でてもらう", "頭撫でて", "撫でて" });
            addYotogiList("頭を撫でてやる", new string[] { "撫でてやる", "頭を撫でる", "頭撫でるよ", "撫でるよ" });
            addYotogiList("頭を撫でて頂く", new string[] { "頭を撫でてください", "撫でてください" });
            addYotogiList("頭を撫でながら", new string[] { "頭を撫でる", "頭撫でるよ", "撫でるよ" });
            addYotogiList("頭を撫でる", new string[] { "頭撫でるよ", "撫でるよ" });
            addYotogiList("頭を抑えつける", new string[] { "抑えつける" });
            //addYotogiList("動いてもらう", new string[]{"動いて"});
            addYotogiList("動いて頂く", new string[] { "動いてください" });
            addYotogiList("内緒で愛撫_おねだりさせる", new string[] { "おねだりして" });
            addYotogiList("二人に止めさせる", new string[] { "やめて" });
            addYotogiList("二人に丁寧に奉仕させる", new string[] { "丁寧に奉仕して", "丁寧に舐めて" });
            addYotogiList("二人に奉仕させる", new string[] { "奉仕して", "ふたりで奉仕して", "ふたりで舐めて" });
            addYotogiList("二人の自分の胸を揉ませながら", new string[] { "胸揉んで", "おっぱい揉んで", "自分で揉んで" });
            addYotogiList("二人を責める", new string[] { "二人で動いて" });
            addYotogiList("肉便器おねだり", new string[] { "おねだり", "おねだりして", "おねだりしろ" });
            addYotogiList("肉便器だと罵倒する", new string[] { "肉便器め", "罵倒する" });
            addYotogiList("乳首でイカせる", new string[] { "乳首でイって" });
            addYotogiList("乳首をつまむ", new string[] { "乳首つまむよ", "乳首をつまむよ" });
            addYotogiList("乳首を摘まみ上げる", new string[] { "乳首をつまむ", "乳首つまむよ", "摘まみ上げる" });
            addYotogiList("乳首を弄って緊張を解す", new string[] { "緊張をほぐす" });
            addYotogiList("乳首を舐めさせる", new string[] { "乳首舐めて" });
            addYotogiList("乳首電流", new string[] { "電気流すよ" });
            addYotogiList("熱烈にキスをして頂きながら", new string[] { "キスしてください" });
            addYotogiList("馬鹿にしてもらう", new string[] { "馬鹿にしてください" });
            addYotogiList("背中にロウを垂らす", new string[] { "背中に垂らす" });
            addYotogiList("背中に高温ロウを垂らす", new string[] { "背中に垂らす" });
            addYotogiList("背中をムチで叩く", new string[] { "背中を叩く" });
            addYotogiList("背中を一本ムチで叩く", new string[] { "背中を叩く" });
            addYotogiList("犯させる", new string[] { "犯して" });
            addYotogiList("秘部にロウを垂らす", new string[] { "秘部に垂らす", "あそこに垂らす", "おまんこに垂らす" });
            addYotogiList("秘部に高温ロウを垂らす", new string[] { "秘部に垂らす", "あそこに垂らす", "おまんこに垂らす" });
            addYotogiList("秘部のみ責める", new string[] { "あそこを責める", "おまんこ責める", "おまんこ責めるよ" });
            addYotogiList("秘部をムチで叩く", new string[] { "秘部を叩く", "あそこを叩く", "おまんこ叩く" });
            addYotogiList("秘部を一本ムチで叩く", new string[] { "秘部を叩く", "あそこを叩く", "おまんこ叩く" });
            addYotogiList("秘部を押し付けられる", new string[] { "押し付けて", "あそこ押し付けて", "おまんこ押し付けて" });
            addYotogiList("秘部を見る", new string[] { "あそこ見せて", "おまんこ見せて" });
            addYotogiList("秘部を弄りながら", new string[] { "あそこ弄る", "あそこ弄るよ", "おまんこ弄る", "おまんこ弄るよ" });
            addYotogiList("百合顔面騎乗位", new string[] { "顔に乗って" });
            addYotogiList("夫の目の前で胸を揉みながら", new string[] { "胸を揉む", "胸揉んで" });
            addYotogiList("腹を殴る", new string[] { "腹パンする" });
            addYotogiList("変態だといやらしく詰る", new string[] { "変態だ", "変態だな" });
            addYotogiList("変態ポーズを取る", new string[] { "変態ポーズして" });
            addYotogiList("鞭でお尻をぶって頂く", new string[] { "お尻をぶってください", "ムチでぶってください" });
            addYotogiList("放出させる", new string[] { "吹き出せ", "漏らせ", "ぶちまけろ" });
            addYotogiList("放尿させる", new string[] { "放尿して", "おしっこして", "漏らして" });
            addYotogiList("放尿してもらう", new string[] { "放尿して", "おしっこして", "おしっこかけて" });
            addYotogiList("放尿して頂く", new string[] { "放尿してください", "おしっこしてください", "かけてください", "おしっこかけてください" });
            addYotogiList("放尿する", new string[] { "放尿して", "おしっこして", "漏らして" });
            addYotogiList("放尿をさせる", new string[] { "放尿して", "おしっこして", "漏らして" });
            addYotogiList("放尿実況させる", new string[] { "おしっこ実況して" });
            addYotogiList("放尿絶頂", new string[] { "イきながら漏らして", "漏らしてイって", "おしっこして", "漏らして" });
            addYotogiList("頬を叩く", new string[] { "ビンタする", "ひっぱたく" });
            addYotogiList("密着して囁いてもらう", new string[] { "ささやいて" });
            addYotogiList("密着しながら", new string[] { "密着して" });
            addYotogiList("夢中になって貪る", new string[] { "貪る", "貪るよ" });
            addYotogiList("目線を隠しながら", new string[] { "目を隠して", "目線隠して" });
            addYotogiList("癒してもらう", new string[] { "癒して" });
            addYotogiList("乱暴に責めさせる", new string[] { "乱暴にして" });
            addYotogiList("乱暴に責める", new string[] { "乱暴に責めるよ", "乱暴にする", "乱暴にするよ" });
            addYotogiList("乱暴に犯させる", new string[] { "乱暴に犯して", "乱暴に犯す" });
            addYotogiList("裏筋を舐めさせる", new string[] { "裏筋舐めて", "裏筋を舐めて" });
            addYotogiList("裏筋を舐め上げる", new string[] { "裏筋舐めて", "裏筋を舐めて", "舐め上げて" });
            addYotogiList("両胸を揉みながら", new string[] { "両胸揉んで", "胸揉んで", "おっぱい揉んで", "両胸揉むよ", "胸揉むよ" });
            addYotogiList("両胸を揉む", new string[] { "両胸揉んで", "胸揉んで", "おっぱい揉んで", "自分で揉んで" });
            addYotogiList("両穴愛撫", new string[] { "両方触るよ" });
            addYotogiList("両手コキ", new string[] { "両手でして", "両手でしごいて" });
            addYotogiList("両手でしごいていただく", new string[] { "両手でしごいてください", "両手でしてください" });
            addYotogiList("両手で激しくして見せつける", new string[] { "両手で激しくして" });
            addYotogiList("両手で弄りながら", new string[] { "両手で弄って" });
            addYotogiList("隷属させる", new string[] { "隷属しろ" });
            addYotogiList("恋人同士のようにキスをする", new string[] { "キスして", "キスをして", "ベロチューして", "ベロチューするよ" });
            addYotogiList("恋人同士の様に胸を揉む", new string[] { "胸を揉んで", "胸を揉ませて" });
            addYotogiList("弄ばれる", new string[] { "弄んで" });
            addYotogiList("腕を持ちながら", new string[] { "腕を持つ", "腕持つよ" });
            addYotogiList("壺洗いをさせる", new string[] { "ツボ洗いして", "あそこで洗って" });
            addYotogiList("壺洗いをさせる2", new string[] { "もっとツボ洗いして", "こっちもツボ洗いして" });
            addYotogiList("嬲り倒して頂く", new string[] { "嬲り倒してください" });
            addYotogiList("嬲るようにキスをしながら", new string[] { "キスしながら", "嬲るようにキスさせる", "キスさせる" });
            addYotogiList("浣腸液を注入する", new string[] { "注入する", "浣腸する" });
            addYotogiList("膣内放尿", new string[] { "中に放尿", "中におしっこ" });
            addYotogiList("貪らせる", new string[] { "貪って" });
            //優先度低
            addYotogiList("ドＭの脚に高温ロウを垂らす", new string[] { "どえむの脚に垂らす", "脚に垂らす" });
            addYotogiList("ドＭの脚を一本ムチで叩く", new string[] { "どえむの脚を叩く", "脚を叩く" });
            addYotogiList("ドＭの胸に高温ロウを垂らす", new string[] { "どえむの胸に垂らす", "胸に垂らす" });
            addYotogiList("ドＭの胸を一本ムチで叩く", new string[] { "どえむの胸を叩く", "胸を叩く" });
            addYotogiList("ドMの首を絞めながら", new string[] { "どえむの首を絞める", "首を絞める" });
            addYotogiList("ドＭの尻を叩きながら", new string[] { "どえむの尻を叩く", "尻を叩く" });
            addYotogiList("ドＭの背中に高温ロウを垂らす", new string[] { "どえむの背中に垂らす", "背中に垂らす" });
            addYotogiList("ドＭの背中を一本ムチで叩く", new string[] { "どえむの背中を叩く", "背中を叩く" });
            addYotogiList("ドＭの秘部に高温ロウを垂らす", new string[] { "どえむのあそこに垂らす", "あそこに垂らす" });
            addYotogiList("ドＭの秘部を一本ムチで叩く", new string[] { "どえむのあそこを叩く", "あそこを叩く" });
            //スワッピング
            addYotogiList("はるのに外出し", new string[] { "", "そと出しする", "外に出す", "はるのにぶっかける" });
            addYotogiList("はるのに顔射", new string[] { "顔射する", "顔にかける", "はるのにぶっかける" });
            addYotogiList("はるのに中出し", new string[] { "中出しする", "中に出す", "はるのに出す" });
            addYotogiList("はるのを絶頂させる", new string[] { "はるのもイって" });
            addYotogiList("千秋に外出しさせる", new string[] { "", "そと出しさせる" });
            addYotogiList("千秋に強く責めさせるA", new string[] { "強く責めさせる", "強く責めて" });
            addYotogiList("千秋に強く責めさせるB", new string[] { "強く責めさせる", "強く責めて" });
            addYotogiList("千秋に強く責めさせるC", new string[] { "強く責めさせる", "強く責めて" });
            addYotogiList("千秋に止めさせる", new string[] { "止めさせる", "止めて" });
            addYotogiList("千秋に止めさせるA", new string[] { "止めさせる", "止めて" });
            addYotogiList("千秋に止めさせるB", new string[] { "止めさせる", "止めて" });
            addYotogiList("千秋に止めさせるC", new string[] { "止めさせる", "止めて" });
            addYotogiList("千秋に責めさせるA", new string[] { "責めさせる", "責めて" });
            addYotogiList("千秋に責めさせるB", new string[] { "責めさせる", "責めて" });
            addYotogiList("千秋に責めさせるC", new string[] { "責めさせる", "責めて" });
            addYotogiList("千秋に中出しさせる", new string[] { "中出しさせる", "中出しして" });
            addYotogiList("千秋に丁寧に奉仕させる", new string[] { "丁寧に奉仕させる", "丁寧に奉仕して" });
            addYotogiList("千秋に奉仕させる", new string[] { "奉仕させる" });
            addYotogiList("千里に外出し", new string[] { "", "そと出しする", "外に出す", "千里にぶっかける" });
            addYotogiList("千里に顔射させる", new string[] { "顔射する", "顔にかける", "千里にぶっかける" });
            addYotogiList("千里に口内射精させる", new string[] { "口に出す", "口に出す", "千里の口に出す" });
            addYotogiList("千里に射精", new string[] { "射精する", "千里に出す" });
            addYotogiList("千里に中出し", new string[] { "中出しする", "中に出す", "千里に出す" });
            addYotogiList("拳斗に外出しさせる", new string[] { "", "そと出しさせる" });
            addYotogiList("拳斗に強く責めさせるA", new string[] { "強く責めさせる" });
            addYotogiList("拳斗に強く責めさせるB", new string[] { "強く責めさせる" });
            addYotogiList("拳斗に止めさせる", new string[] { "止めさせる" });
            addYotogiList("拳斗に止めさせるA", new string[] { "止めさせる" });
            addYotogiList("拳斗に止めさせるB", new string[] { "止めさせる" });
            addYotogiList("拳斗に責めさせるA", new string[] { "責めさせる" });
            addYotogiList("拳斗に責めさせるB", new string[] { "責めさせる" });
            addYotogiList("拳斗に中出しさせる", new string[] { "中出しさせる" });
            addYotogiList("拳斗に丁寧に奉仕させる", new string[] { "丁寧に奉仕させる" });
            addYotogiList("拳斗に奉仕させる", new string[] { "奉仕させる" });
            addYotogiList("美希に外出し", new string[] { "", "そと出しする", "美希にぶっかける" });
            addYotogiList("美希に外出しさせる", new string[] { "", "そと出しして", "美希にぶっかけて" });
            addYotogiList("美希に顔射", new string[] { "顔射する", "顔にかける", "美希にぶっかける" });
            addYotogiList("美希に顔射させる", new string[] { "顔射して", "顔にかけて", "美希にぶっかけて" });
            addYotogiList("美希に口内射精させる", new string[] { "口に出して", "美希の口に出して" });
            addYotogiList("美希に中出し", new string[] { "中出しする", "中に出す", "美希に出す" });
            addYotogiList("美希に中出しさせる", new string[] { "中出しして", "中に出して", "美希に出して" });
            addYotogiList("富彦に外出しさせる", new string[] { "", "そと出しさせる" });
            addYotogiList("富彦に強く責めさせるA", new string[] { "強く責めさせる", "強く責めて" });
            addYotogiList("富彦に強く責めさせるB", new string[] { "強く責めさせる", "強く責めて" });
            addYotogiList("富彦に強く責めさせるC", new string[] { "強く責めさせる", "強く責めて" });
            addYotogiList("富彦に強く責めさせるD", new string[] { "強く責めさせる", "強く責めて" });
            addYotogiList("富彦に止めさせるA", new string[] { "止めさせる", "止めて" });
            addYotogiList("富彦に止めさせるB", new string[] { "止めさせる", "止めて" });
            addYotogiList("富彦に止めさせるC", new string[] { "止めさせる", "止めて" });
            addYotogiList("富彦に止めさせるD", new string[] { "止めさせる", "止めて" });
            addYotogiList("富彦に責めさせるA", new string[] { "責めさせる", "責めて" });
            addYotogiList("富彦に責めさせるB", new string[] { "責めさせる", "責めて" });
            addYotogiList("富彦に責めさせるC", new string[] { "責めさせる", "責めて" });
            addYotogiList("富彦に責めさせるD", new string[] { "責めさせる", "責めて" });
            addYotogiList("富彦に中出しさせる", new string[] { "中出しさせる", "中出しして" });

            //CM3D2
            addYotogiList("M豚を絞って頂く", new string[] { "絞ってください" });
            addYotogiList("M豚を躾けて頂く", new string[] { "躾けてください" });
            addYotogiList("イカせられる", new string[] { "イかせて", "イっちゃえ" });
            addYotogiList("オナニーさせて頂く", new string[] { "オナニーさせていただきます" });
            addYotogiList("オナニーさせながら", new string[] { "オナニして" });
            addYotogiList("オナニーを問い詰めてもらう", new string[] { "問い詰めて" });
            addYotogiList("おねだりさせるA", new string[] { "おねだりして" });
            addYotogiList("おねだりさせるB", new string[] { "そっちもおねだりして" });
            addYotogiList("お客様との夜伽を告白させる", new string[] { "告白して" });
            addYotogiList("お客様との夜伽を聞く", new string[] { "夜伽どうだった？" });
            addYotogiList("お尻の奥をガンガン突く", new string[] { "尻奥を突く", "ガンガン突く" });
            addYotogiList("キスをさせながら", new string[] { "キスさせる" });
            addYotogiList("くぱぁってする", new string[] { "くぱぁする", "くぱぁってして", "くぱぁして" });
            addYotogiList("グラインドして責める", new string[] { "回して責める" });
            addYotogiList("クリトリスを弄らせる", new string[] { "クリトリス弄って", "クリを弄って" });
            addYotogiList("クリトリスを弄る", new string[] { "クリトリス弄るよ", "クリを弄るよ" });
            addYotogiList("クンニさせて頂く", new string[] { "クンニしてさせてください" });
            addYotogiList("クンニさせる", new string[] { "クンニして" });
            addYotogiList("パイズリさせる", new string[] { "パイズリして" });
            addYotogiList("パンツで苛めて頂く", new string[] { "パンツで苛めてください" });
            addYotogiList("ビッチに誘わせる", new string[] { "誘って" });
            addYotogiList("ぶっかけ外出しさせる", new string[] { "外にぶっかけさせる" });
            addYotogiList("ぶっかけ外出しする", new string[] { "外にぶっかける" });
            addYotogiList("ぶっかけ中出しさせる", new string[] { "中出しさせる", "ぶっかける" });
            addYotogiList("ぶっかけ中出しする", new string[] { "中出しする", "ぶっかけて" });
            addYotogiList("ペニスを握らせる", new string[] { "ペニス握って", "ちんちん握って" });
            addYotogiList("ムリヤリ尻穴に注ぎ込む", new string[] { "無理やり尻出し" });
            addYotogiList("ムリヤリ膣内に注ぎ込む", new string[] { "無理やり中出し" });
            addYotogiList("愛撫させる", new string[] { "愛撫して" });
            addYotogiList("汚いモノを絞って頂く", new string[] { "汚いモノを絞ってください", "絞ってください" });
            addYotogiList("汚い尻を叩いて頂く", new string[] { "汚い尻を叩いてください", "叩いてください" });
            addYotogiList("汚い尻穴を責めて頂く", new string[] { "汚い尻穴を責めてください", "尻穴を責めてください" });
            addYotogiList("奥を責めながら", new string[] { "奥を責める", "奥責めるよ" });
            addYotogiList("奥を責める", new string[] { "奥責めるよ" });
            addYotogiList("押し付けて頂く", new string[] { "押し付けてください" });
            addYotogiList("下手な奉仕だと罵る", new string[] { "下手な奉仕だ" });
            addYotogiList("感想を聞くA", new string[] { "感想を聞く", "感想は？" });
            addYotogiList("感想を聞くB", new string[] { "感想を聞く", "感想は？" });
            addYotogiList("感想を聞くC", new string[] { "感想を聞く", "感想は？" });
            addYotogiList("顔に掛ける&バイブ絶頂", new string[] { "顔とバイブでイって" });
            addYotogiList("詰ってもらう", new string[] { "詰ってください" });
            addYotogiList("逆手で奉仕して頂く", new string[] { "逆手でしてください" });
            addYotogiList("強く奉仕させる", new string[] { "強く奉仕して" });
            addYotogiList("強く奉仕して頂く", new string[] { "強く奉仕してください", "強くしてください" });
            addYotogiList("激しくオナニーさせる", new string[] { "激しくオナニーして" });
            addYotogiList("激しく口を責める", new string[] { "激しく責める" });
            addYotogiList("激しく口を責める&バイブ責め", new string[] { "激しく責める", "激しく口とバイブ責め" });
            addYotogiList("激しく犯させる", new string[] { "激しく犯して" });
            addYotogiList("口に出す&バイブ絶頂", new string[] { "口とバイブでイって" });
            addYotogiList("口を責める&バイブ責め", new string[] { "口とバイブ責め" });
            addYotogiList("口汚く罵らせる", new string[] { "口汚く罵って", "罵って" });
            addYotogiList("口内射精させて頂く", new string[] { "口に出させてください", "口に出します" });
            addYotogiList("喉奥に擦りつけさせる", new string[] { "こすりつけて", "喉奥にこすりつけて", "喉こすって" });
            addYotogiList("喉奥に擦りつける", new string[] { "こすりつけるよ", "喉奥こすりつけるよ", "喉こするよ" });
            addYotogiList("喉奥に擦り付けさせる", new string[] { "こすりつけて", "喉奥にこすりつけて", "喉こすって" });
            addYotogiList("喉奥を責めながらバイブを強くする", new string[] { "バイブを強くする" });
            addYotogiList("喉奥を責めながらローソクを垂らす", new string[] { "ローソクを垂らす" });
            addYotogiList("喉奥を責める", new string[] { "喉奥責めるよ", "喉責めるよ" });
            addYotogiList("三人に丁寧に奉仕させる", new string[] { "三人で丁寧に奉仕して", "三人で丁寧にして" });
            addYotogiList("三人に奉仕させる", new string[] { "三人で奉仕して" });
            addYotogiList("残念だが止めさせて頂く", new string[] { "やめさせていただきます", "やめます" });
            addYotogiList("四つん這い尻舐め", new string[] { "尻舐めして", "お尻舐めて" });
            addYotogiList("子宮をコンコンする", new string[] { "コンコンするよ", "奥コンコンするよ" });
            addYotogiList("指三本で弄らせる", new string[] { "指三本で弄って" });
            addYotogiList("自分の胸を揉んで頂きながら", new string[] { "胸を揉んでください" });
            addYotogiList("実況させる", new string[] { "実況して" });
            addYotogiList("手コキさせる", new string[] { "手コキして" });
            addYotogiList("種付けさせる", new string[] { "種付けする" });
            addYotogiList("尻コキさせる", new string[] { "尻コキして", "お尻でして", "尻でこすって" });
            addYotogiList("尻を叩いて頂く", new string[] { "尻を叩いてください" });
            addYotogiList("尻を揉む", new string[] { "尻を揉むよ" });
            addYotogiList("尻穴を拡げさせながら", new string[] { "尻穴拡げて", "お尻拡げて" });
            addYotogiList("尻穴を拡げながら", new string[] { "尻穴拡げて", "お尻拡げて" });
            addYotogiList("尻穴を責められる", new string[] { "尻穴責めて", "お尻責めて" });
            addYotogiList("尻穴を弄らせながら", new string[] { "尻穴いじって", "お尻いじって" });
            addYotogiList("尻穴を舐めて頂く", new string[] { "尻穴を舐めてください" });
            addYotogiList("責められる", new string[] { "責めて" });
            addYotogiList("絶頂放尿", new string[] { "イきながら漏らして" });
            addYotogiList("丁寧に責められる", new string[] { "丁寧にして" });
            addYotogiList("丁寧に洗わせる", new string[] { "丁寧に洗って" });
            addYotogiList("丁寧に咥えて頂く", new string[] { "丁寧に咥えて" });
            addYotogiList("丁寧に舐めさせる", new string[] { "丁寧に舐めて" });
            addYotogiList("丁寧に舐められる", new string[] { "丁寧に舐めて" });
            addYotogiList("童貞を罵って頂く", new string[] { "童貞を罵ってください" });
            addYotogiList("二人に足で奉仕して頂く", new string[] { "足でしてください" });
            addYotogiList("二人に丁寧に足で奉仕して頂く", new string[] { "足で丁寧にしてください" });
            addYotogiList("乳首を捻り上げる", new string[] { "捻り上げる", "乳首捻るよ" });
            addYotogiList("悲鳴を聞きながら", new string[] { "悲鳴を聞く" });
            addYotogiList("秘部に擦りつける", new string[] { "アソコにこすりつけるよ", "アソコこするよ" });
            addYotogiList("秘部を開かせながら", new string[] { "くぱぁして" });
            addYotogiList("秘部を開く", new string[] { "くぱぁして" });
            addYotogiList("秘部を責めながら", new string[] { "アソコ責めるよ" });
            addYotogiList("非常に激しく犯す", new string[] { "犯しまくる" });
            addYotogiList("服従を誓わせる", new string[] { "服従を誓え" });
            addYotogiList("物凄く激しく責める", new string[] { "物凄く責める" });
            addYotogiList("変態ポーズをとらせる", new string[] { "変態ポーズして" });
            addYotogiList("抱え上げる", new string[] { "だっこするよ" });
            addYotogiList("乱暴に愛撫させる", new string[] { "乱暴に弄らせる", "乱暴に弄って" });
            addYotogiList("乱暴に輪姦させる", new string[] { "乱暴にまわわせる", "乱暴にまわして" });
            addYotogiList("両手で扱いてもらう", new string[] { "両手でしごいて" });
            addYotogiList("両手で奉仕させる", new string[] { "両手でして" });
            addYotogiList("輪姦させる", new string[] { "まわさせる", "まわして" });
            addYotogiList("舐められる", new string[] { "舐めて" });
        }
    }
}