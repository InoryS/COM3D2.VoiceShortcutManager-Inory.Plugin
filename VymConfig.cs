using System;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.VoiceShortcutManager.Plugin
{
    //汎用音声設定用 設定値を2つ文字列で格納
    public class VymVoiceInfo
    {
        public string value; //引数の型に合わせてパースする
        public string value2; //引数の型に合わせてパースする
        public string[] voice;

        public VymVoiceInfo()
        {
        }

        //値は1つ
        public VymVoiceInfo(string value, string[] voice)
        {
            this.value = value;
            this.voice = voice;
        }

        //値は2つ
        public VymVoiceInfo(string value, string value2, string[] voice)
        {
            this.value = value;
            this.value2 = value2;
            this.voice = voice;
        }
    }

    //移動用のベクター情報
    public class VymMoveInfo
    {
        public Vector3 move;
        public bool useMoveValue;
        public string[] voice;

        public VymMoveInfo()
        {
        }

        public VymMoveInfo(Vector3 move, bool useMoveValue, string[] voice)
        {
            this.move = move;
            this.useMoveValue = useMoveValue;
            this.voice = voice;
        }
    }

    ///表示、モーション、ボイスセットを同時に変更する
    public class VymFaceMotionInfo
    {
        public string face;
        public string blend; //nullならブレンド変更なし
        public string[] tags; //表情詳細
        public string[] motion;
        public string loop; //最後のモーションのループ状態 falseで停止した場合次のモーションはCrossFadeしない ※設定ファイルに出力しないように文字列
        public string voiceSet; //ボイスセットファイル名
        public string[] voice;

        public VymFaceMotionInfo()
        {
        }

        //音声キーワードを先に設定
        public VymFaceMotionInfo(string[] voice)
        {
            this.voice = voice;
        }

        //表情のみを設定
        public VymFaceMotionInfo addFace(string face, string blend)
        {
            this.face = face;
            this.blend = blend;
            return this;
        }

        //表情詳細のみを設定
        public VymFaceMotionInfo addFaceTag(string[] tags)
        {
            this.tags = tags;
            return this;
        }

        //モーションのみを設定
        public VymFaceMotionInfo addMotion(string[] motion, bool loop)
        {
            this.motion = motion;
            this.loop = loop ? "true" : "false"; //小文字で設定
            return this;
        }

        //ボイスセットのみを設定
        public VymFaceMotionInfo addVoiceSet(string voiceSet)
        {
            this.voiceSet = voiceSet;
            return this;
        }
    }

    //設定変更用
    public class VymConfigInfo
    {
        public string name; //設定フィールド名
        public string value; //文字列以外の型はパースしてからSetValue
        public bool init; //trueなら再読込時と同様の初期化処理をする
        public string[] voice;

        public VymConfigInfo()
        {
        }

        public VymConfigInfo(string name, string value, bool init, string[] voice)
        {
            this.name = name;
            this.value = value;
            this.init = init;
            this.voice = voice;
        }
    }

    public class VymConfig
    {
        //コンストラクタ XML読み込みで必要
        public VymConfig()
        {
        }

        //VYMのConfig変更
        //boolは"toggle"の指定可能 設定の再初期処理が必要ならresetはtrue
        public VymConfigInfo[] vymConfig =
        {
            new VymConfigInfo("mainGuiFlag", "1", false, new string[] { "リモコン表示" }),
            new VymConfigInfo("mainGuiFlag", "0", false, new string[] { "リモコン非表示" }),
            new VymConfigInfo("osawariEnabled", "true", false, new string[] { "おさわり開始", "おさわり有効" }),
            new VymConfigInfo("osawariEnabled", "false", false, new string[] { "おさわり終了", "おさわり無効" }),
            //VR用
            new VymConfigInfo("vrCameraMoveLimit", "true", false, new string[] { "最短制限" }),
            new VymConfigInfo("vrCameraMoveLimit", "false", false, new string[] { "最短制限解除" }),
        };

        //メイド選択
        public string[] vymSelectFrontMaid = { "正面のメイド" };

        //名前でメインメイドを選択
        public VymVoiceInfo[] vymSelectMaidName =
        {
            new VymVoiceInfo("聖道 まりあ", new string[] { "無垢ちゃん" }),
        };

        public string[] vymPrevMaid = { "前のメイド" };
        public string[] vymNextMaid = { "次のメイド" };
        public string[] vymLeftMaid = { "左のメイド" };

        public string[] vymRightMaid = { "右のメイド" };

        //メイドをリンク・同期
        public VymVoiceInfo[] vymMaidLink =
        {
            new VymVoiceInfo("all", new string[] { "全リンク" }),
            new VymVoiceInfo("null", new string[] { "リンク解除" }),
            new VymVoiceInfo("left", new string[] { "左とリンク" }),
            new VymVoiceInfo("right", new string[] { "右とリンク" }),
            new VymVoiceInfo("sync", new string[] { "モーション同期", "同期する" }),
        };

        //VRカメラ移動
        public VymMoveInfo[] vymCamMove =
        {
            new VymMoveInfo(new Vector3(+1f, 0f, 0f), true, new string[] { "右に移動" }),
            new VymMoveInfo(new Vector3(-1f, 0f, 0f), true, new string[] { "左に移動" }),
            new VymMoveInfo(new Vector3(0f, +1f, 0f), true, new string[] { "上に移動" }),
            new VymMoveInfo(new Vector3(0f, -1f, 0f), true, new string[] { "下に移動" }),
            new VymMoveInfo(new Vector3(0f, 0f, +1f), true, new string[] { "前に移動" }),
            new VymMoveInfo(new Vector3(0f, 0f, -1f), true, new string[] { "後ろに移動" }),
            new VymMoveInfo(new Vector3(+0.05f, 0f, 0f), false, new string[] { "5センチ右" }),
            new VymMoveInfo(new Vector3(-0.05f, 0f, 0f), false, new string[] { "5センチ左" }),
            new VymMoveInfo(new Vector3(0f, +0.05f, 0f), false, new string[] { "5センチ上" }),
            new VymMoveInfo(new Vector3(0f, -0.05f, 0f), false, new string[] { "5センチ下" }),
            new VymMoveInfo(new Vector3(0f, 0f, +0.05f), false, new string[] { "5センチ前" }),
            new VymMoveInfo(new Vector3(0f, 0f, -0.05f), false, new string[] { "5センチ後ろ" }),
        };

        //カメラ距離
        public VymVoiceInfo[] vymCamDistance =
        {
            new VymVoiceInfo("-25%", new string[] { "近づく" }),
            new VymVoiceInfo("+25%", new string[] { "離れる" }),
        };

        //カメラ回転
        public string[] vymCamFront = { "メイド正面" };

        public string[] vymCamBack = { "メイド背面", "メイドの後ろ" };

        //カメラ部位正面
        public VymVoiceInfo[] vymCamTarget =
        {
            new VymVoiceInfo("0.3", "1", new string[] { "顔の前" }),
            new VymVoiceInfo("0.3", "-1", new string[] { "顔の後ろ", "頭の後ろ" }),
            new VymVoiceInfo("0.4", "2", new string[] { "胸の前" }),
            new VymVoiceInfo("0.4", "-2", new string[] { "胸の後ろ", "背中の前" }),
            new VymVoiceInfo("0.3", "3", new string[] { "腰の前" }),
            new VymVoiceInfo("0.3", "-3", new string[] { "腰の後ろ", "尻の前" }),
        };

        //男の頭に移動
        //第1引数: -1=表示中の最初の男, 0=ご主人様, 1～モブ男  第2引数:-1変更なし, 0=男の顔の向き, 1=顔, 2=胸, 3=股間
        public VymVoiceInfo[] vymCamManHead =
        {
            new VymVoiceInfo("-1", "1", new string[] { "男に移動", "男の位置" }), //メイドの顔方向
            new VymVoiceInfo("-1", "0", new string[] { "男視点" }), //男の顔の方向 1人称モードにはしない
            new VymVoiceInfo("0", "0", new string[] { "ご主人様視点" }),
        };

        //視点変更 一人称
        public VymVoiceInfo[] vymFps =
        {
            new VymVoiceInfo("0", new string[] { "一人称解除", "一人称終了", "FPS解除", "FPS終了" }),
            new VymVoiceInfo("1", new string[] { "一人称開始", "FPS開始" }),
        };

        //視点変更 メイド固定
        public VymVoiceInfo[] vymFollow =
        {
            new VymVoiceInfo("0", new string[] { "メイド固定解除", "固定解除" }),
            new VymVoiceInfo("1", new string[] { "メイド固定" }),
        };

        //視点変更 注視点
        public VymVoiceInfo[] vymLookPoint =
        {
            new VymVoiceInfo("0", new string[] { "胸に固定" }),
            new VymVoiceInfo("1", new string[] { "顔に固定" }),
            new VymVoiceInfo("2", new string[] { "腰に固定" }),
            new VymVoiceInfo("10", new string[] { "アングル固定" }), //10ならアングルだけON 11ならメイド固定もON
            new VymVoiceInfo("-10", new string[] { "アングル解除" }),
        };

        //地面判定 高さの先頭に"+"や"-"がついていたら相対値で変更
        public VymVoiceInfo[] vymBoneHitHeight =
        {
            new VymVoiceInfo("-1", new string[] { "地面下げる" }),
            new VymVoiceInfo("+1", new string[] { "地面上げる" }),
            new VymVoiceInfo("0", new string[] { "地面ゼロ" }),
        };

        //興奮値 先頭に"+"や"-"がついていたら相対値で変更
        public VymVoiceInfo[] vymExcite =
        {
            new VymVoiceInfo("-50", new string[] { "興奮下げる" }),
            new VymVoiceInfo("+50", new string[] { "興奮上げる" }),
            new VymVoiceInfo("0", new string[] { "興奮なし" }),
        };

        //男表示非表示 マイナスなら非表示 男はビット指定(1=ご主人様 2=男1 4=男2 8=男3 16=男4 32=男5)
        public VymVoiceInfo[] vymManVisible =
        {
            new VymVoiceInfo("1", new string[] { "ご主人様表示" }),
            new VymVoiceInfo("-1", new string[] { "ご主人様非表示" }),
            new VymVoiceInfo("2", new string[] { "モブ表示" }),
            new VymVoiceInfo("-255", new string[] { "男非表示" }), //0b11111111
            new VymVoiceInfo("-254", new string[] { "モブ非表示" }), //0b11111110
        };

        //射精 男はビット指定(1=ご主人様 2=男1 4=男2 8=男3 16=男4 32=男5)
        //強制射精(メイドとリンクしている場合のみ)  射精ロック(数値の前に "+"でロック "-"でロック解除)
        public VymVoiceInfo[] vymSyasei =
        {
            new VymVoiceInfo("1", new string[] { "射精する" }),
            new VymVoiceInfo("2", new string[] { "モブ射精" }),
            new VymVoiceInfo("255", new string[] { "全員射精" }), //0b11111111
            new VymVoiceInfo("+255", new string[] { "射精ロック" }),
            new VymVoiceInfo("-255", new string[] { "射精ロック解除" }),
        };

        //拭き取り
        public VymVoiceInfo[] vymFukitori =
        {
            new VymVoiceInfo("0", new string[] { "拭き取り" }),
            new VymVoiceInfo("1", new string[] { "顔拭き取り", "顔を拭く" }),
            new VymVoiceInfo("2", new string[] { "体拭き取り", "体を拭く" }),
        };

        //尿
        public VymVoiceInfo[] vymStartNyo =
        {
            new VymVoiceInfo("0", new string[] { "おしっこして", "おしっこかけて", "漏らして" }),
            new VymVoiceInfo("1", new string[] { "全員おしっこ" }),
            new VymVoiceInfo("2", new string[] { "同時におしっこ" }), //UNZIPのメインメイドとサブメイド
        };

        //潮
        public VymVoiceInfo[] vymStartSio =
        {
            new VymVoiceInfo("0", new string[] { "潮吹き", "潮吹いて" }),
            new VymVoiceInfo("1", new string[] { "全員潮吹き" }),
            new VymVoiceInfo("2", new string[] { "同時に潮吹き" }), //UNZIPのメインメイドとサブメイド
        };

        //バイブ強弱
        public VymVoiceInfo[] vymVibe =
        {
            new VymVoiceInfo("0", new string[] { "バイブOFF", "バイブ停止", "停止", "バイブ抜く" }),
            new VymVoiceInfo("1", new string[] { "バイブON", "バイブ入れる", "バイブ弱", "バイブ弱く" }),
            new VymVoiceInfo("2", new string[] { "バイブ強", "バイブ強く" }),
        };

        //バイブオート
        public VymVoiceInfo[] vymVibeAuto =
        {
            new VymVoiceInfo("+1", new string[] { "バイブオート" }), //順次切替
            new VymVoiceInfo("1", new string[] { "バイブじっくり" }),
            new VymVoiceInfo("2", new string[] { "バイブ激しく" }),
            new VymVoiceInfo("3", new string[] { "バイブほどほど" }),
        };

        //Kupa開度とキーワード 開度の先頭に "+"や"-" がついていたら相対値で変更
        public VymVoiceInfo[] vymKupa =
        {
            new VymVoiceInfo("0", new string[] { "あそこ閉じる", "おまんこ閉じる" }),
            new VymVoiceInfo("60", new string[] { "くぱぁ", "くぱぁする", "くぱぁして", "あそこひらく", "おまんこひらく" }),
            new VymVoiceInfo("+20", new string[] { "あそこ拡げる", "おまんこ拡げる", "あそこ拡げて", "おまんこ拡げて" }),
            new VymVoiceInfo("-20", new string[] { "あそこ閉める", "おまんこ閉める", "あそこ締めて", "おまんこ締めて" }),
        };

        public VymVoiceInfo[] vymAnal =
        {
            new VymVoiceInfo("0", new string[] { "アナル閉じる" }),
            new VymVoiceInfo("60", new string[] { "アナルくぱぁ", "アナルひらく" }),
            new VymVoiceInfo("+20", new string[] { "アナル拡げる", "アナル拡げて" }),
            new VymVoiceInfo("-20", new string[] { "アナル閉める", "アナル締めて" }),
        };

        public VymVoiceInfo[] vymBokki =
        {
            new VymVoiceInfo("0", new string[] { "クリ最小" }),
            new VymVoiceInfo("100", new string[] { "クリ最大" }),
            new VymVoiceInfo("+30", new string[] { "クリ大きく", "クリ拡大", "クリぼっき" }),
            new VymVoiceInfo("-30", new string[] { "クリ小さく", "クリ縮小" }),
        };

        //UNZIP再生  MotionAdjuxt.xml のモーションと同じ名称(.anmは不要)  DLC無しの状態で使えるものを優先
        public VymVoiceInfo[] vymUnzip =
        {
            new VymVoiceInfo("om_aibu_hibu_1_f", new string[] { "愛撫" }),
            new VymVoiceInfo("om_seijyoui_1_f", new string[] { "正常位" }),
            new VymVoiceInfo("manguri_1_f", new string[] { "まんぐり" }),
            new VymVoiceInfo("om_taimenkijyoui_1_f", new string[] { "対面騎乗位" }),
            new VymVoiceInfo("kijyoui_sumata_1_f", new string[] { "騎乗位素股" }),
            new VymVoiceInfo("om_taimenzai_1_f", new string[] { "対面座位" }),
            new VymVoiceInfo("om_kouhaii_1_f", new string[] { "こうはいい" }),
            new VymVoiceInfo("ritui_1_f", new string[] { "りつい" }),
            new VymVoiceInfo("haimenritui_1_f", new string[] { "背面立位" }),
            new VymVoiceInfo("sokui_1_f", new string[] { "そくい" }),
            new VymVoiceInfo("haimen_kijyoui_1_f", new string[] { "背面騎乗位" }),
            new VymVoiceInfo("haimenzai2_1_f", new string[] { "背面座位" }),
            new VymVoiceInfo("ekiben_1_f", new string[] { "駅弁" }),
            new VymVoiceInfo("haimenekiben_1_f", new string[] { "背面駅弁" }),
            new VymVoiceInfo("sukebeisu_sex_1_f", new string[] { "スケベ椅子" }),
            new VymVoiceInfo("tikan_aibu_1_f", new string[] { "痴漢" }),
            new VymVoiceInfo("tikan_sex_1_f", new string[] { "痴漢挿入" }),
            new VymVoiceInfo("ganmenkijyoui_1_f", new string[] { "顔面騎乗位" }),
            new VymVoiceInfo("sixnine_1_f", new string[] { "シックスナイン" }),
            new VymVoiceInfo("fera_mzi_1_f", new string[] { "フェラえむじ", "しゃがみフェラ" }),
            new VymVoiceInfo("om_fera_ir_1_f", new string[] { "フェライラマ", "イラマチオ" }),
            new VymVoiceInfo("asikoki_1_f", new string[] { "足こき" }),
            new VymVoiceInfo("paizuri_1_f", new string[] { "パイズリ" }),
            new VymVoiceInfo("paizuri_fera_1_f", new string[] { "パイズリフェラ" }),
            new VymVoiceInfo("om_onani_1_f", new string[] { "オナニー" }),
            new VymVoiceInfo("om_fera_onani_1_f", new string[] { "フェラオナニー" }),
            new VymVoiceInfo("wfera_1_f", new string[] { "ダブルフェラ" }),
            new VymVoiceInfo("wasikoki_1_f", new string[] { "ダブル足こき" }),
            new VymVoiceInfo("harem_housi_1_f", new string[] { "ハーレム奉仕" }),
            new VymVoiceInfo("ran3p_seijyoui_kuti_1_f", new string[] { "乱交3P" }),
            new VymVoiceInfo("ran3p_2ana_1_f", new string[] { "乱交にけつ", "乱交にあな" }),
        };

        //UNZIP派生モーション切り替え カンマ区切りで複数指定可 _も必要  夜伽中はそちらが優先されるので注意
        public VymVoiceInfo[] vymUnzipDerive =
        {
            new VymVoiceInfo(null, new string[] { "基本体位" }),
            new VymVoiceInfo("_gr", new string[] { "グラインドに変更", "グラインドして" }),
            new VymVoiceInfo("_daki,_kakae", new string[] { "抱きに変更", "抱える" }),
            new VymVoiceInfo("_hold", new string[] { "ホールドに変更", "ホールドして" }),
            new VymVoiceInfo("_69", new string[] { "シックスナインに変更" }),
            new VymVoiceInfo("_kiss", new string[] { "キスに変更", "キスする" }),
            new VymVoiceInfo("_aibu,_hibu_aibu", new string[] { "愛撫に変更", "愛撫する" }),
            new VymVoiceInfo("_momi,_ryoumomi", new string[] { "揉みに変更", "おっぱい揉む" }),
            new VymVoiceInfo("_kuti,_kutiosae,_kutifusagu", new string[] { "口に変更" }),
            new VymVoiceInfo("_cli", new string[] { "クリに変更", "クリさわる" }),
            new VymVoiceInfo("_hibu,_maeyubi,_hibuhiraki", new string[] { "秘部に変更", "あそこさわる" }),
            new VymVoiceInfo("_siri,_siriyubi", new string[] { "尻に変更", "尻さわる" }),
            new VymVoiceInfo("_name", new string[] { "舐めに変更", "舐めて" }),
            new VymVoiceInfo("_fera", new string[] { "フェラに変更", "フェラして" }),
            new VymVoiceInfo("_tekoki", new string[] { "手こきに変更", "手こきして" }),
        };

        //UNZIPランダムモーション再生  EditMotionSet以下のxmlファイル名(.xmlは不要)
        public VymVoiceInfo[] vymUnzipRandom =
        {
            new VymVoiceInfo("ems_オナニーセット", new string[] { "オナニーセット" }),
            new VymVoiceInfo("ems_正常位セット", new string[] { "正常位セット" }),
        };

        //前後のモーションへ切り替え ランダムモーションセット再生中は モーション、カテゴリ、モーションセットの切り替え
        public VymVoiceInfo[] vymUnzipChange =
        {
            new VymVoiceInfo("-1", new string[] { "前のモーション", "前の体位" }),
            new VymVoiceInfo("1", new string[] { "次のモーション", "次の体位", "体位変更" }), //ランダムモーション変更
            new VymVoiceInfo("2", new string[] { "カテゴリ変更" }), //ランダムカテゴリ変更
            new VymVoiceInfo("-3", new string[] { "前のモーションセット" }), //ランダムモーションセット変更
            new VymVoiceInfo("3", new string[] { "次のモーションセット", "モーションセット変更" }), //ランダムモーションセット変更
        };

        //抜く trueは強制外出し
        public VymVoiceInfo[] vymRemoveMotion =
        {
            new VymVoiceInfo("false", new string[] { "抜く", "ちんこ抜く", "待機" }),
            new VymVoiceInfo("true", new string[] { "抜いて射精", "抜いて出す", "抜いてかける" }),
        };

        public string[] vymInsertMotion = { "入れる", "ちんこ入れる", "挿入", "再挿入" };
        public string[] vymSumataMotion = { "素股", "素股して" };

        //後ろを使う
        public VymVoiceInfo[] vymAnalMode =
        {
            new VymVoiceInfo("false", new string[] { "前を使う", "前に入れる" }),
            new VymVoiceInfo("true", new string[] { "後ろを使う", "後ろに入れる" }),
        };

        //ボイスセット設定  EditVoiseSet以下のxmlファイル名(.xmlは不要)
        public VymVoiceInfo[] vymVoiceSet =
        {
            new VymVoiceInfo(null, new string[] { "ボイスセット解除" }),
            new VymVoiceInfo("evs_オナニー(初々)_連動", new string[] { "オナニーボイス" }),
            new VymVoiceInfo("evs_オナニー(慣れ)_連動", new string[] { "オナニーボイス慣れ" }),
            new VymVoiceInfo("evs_オナニー(結婚)_連動", new string[] { "オナニーボイス結婚" }),
            new VymVoiceInfo("evs_愛撫(初々)_連動", new string[] { "愛撫ボイス" }),
            new VymVoiceInfo("evs_愛撫(慣れ)_連動", new string[] { "愛撫ボイス慣れ" }),
            new VymVoiceInfo("evs_愛撫(結婚)_連動", new string[] { "愛撫ボイス結婚" }),
            new VymVoiceInfo("evs_素股(初々)_連動", new string[] { "素股ボイス" }),
            new VymVoiceInfo("evs_素股(慣れ)_連動", new string[] { "素股ボイス慣れ" }),
            new VymVoiceInfo("evs_素股(結婚)_連動", new string[] { "素股ボイス結婚" }),
            new VymVoiceInfo("evs_挿入(初々)_連動", new string[] { "挿入ボイス" }),
            new VymVoiceInfo("evs_挿入(慣れ)_連動", new string[] { "挿入ボイス慣れ" }),
            new VymVoiceInfo("evs_挿入(結婚)_連動", new string[] { "挿入ボイス結婚" }),
            new VymVoiceInfo("evs_挿入事後(中出し)", new string[] { "事後中出しボイス", "中出しボイス" }),
        };

        //キスボイスセット設定  EditVoiseSet以下のxmlファイル名(.xmlは不要)
        public VymVoiceInfo[] vymKissVoiceSet =
        {
            new VymVoiceInfo(null, new string[] { "キスボイス解除" }),
            new VymVoiceInfo("evs_キス(初々)", new string[] { "キスボイス" }),
            new VymVoiceInfo("evs_キス(慣れ)", new string[] { "キスボイス慣れ" }),
            new VymVoiceInfo("evs_キス(結婚)", new string[] { "キスボイス結婚" }),
            new VymVoiceInfo("evs_キス(嫌悪)", new string[] { "キスボイス嫌悪" }),
            new VymVoiceInfo("evs_キス(初々)_客", new string[] { "キスボイス客" }),
            new VymVoiceInfo("evs_キス(慣れ)_客", new string[] { "キスボイス慣れ客" }),
            new VymVoiceInfo("evs_キス(嫌悪)_客", new string[] { "キスボイス嫌悪客" }),
        };

        //表情とモーションとボイスセット切り替えをまとめて設定
        public VymFaceMotionInfo[] vymFaceMotion =
        {
            //表情変更 表情ブレンド 頬と涙は含まれていれば変更しない "よだれ"は含まれていない場合は削除される
            new VymFaceMotionInfo(new string[] { "普通の顔" }).addFace("通常", "頬０涙０"),
            new VymFaceMotionInfo(new string[] { "微笑み", "微笑んで" }).addFace("微笑み", "頬０涙０"),
            new VymFaceMotionInfo(new string[] { "優しさ", "優しい顔" }).addFace("優しさ", "頬０涙０"),
            new VymFaceMotionInfo(new string[] { "にっこり" }).addFace("にっこり", "頬０涙０"),
            new VymFaceMotionInfo(new string[] { "笑って" }).addFace("笑顔", "頬０涙０"),
            new VymFaceMotionInfo(new string[] { "怒って" }).addFace("少し怒り", "頬０涙０"),
            new VymFaceMotionInfo(new string[] { "泣いて" }).addFace("泣き", "頬０涙１"),
            new VymFaceMotionInfo(new string[] { "拗ねて" }).addFace("拗ね", "頬０涙０"),
            new VymFaceMotionInfo(new string[] { "ジト目" }).addFace("ダンスジト目", "頬０涙０"),
            new VymFaceMotionInfo(new string[] { "キス顔" }).addFace("接吻", "頬１涙０"),
            new VymFaceMotionInfo(new string[] { "閉じ目" }).addFace("閉じ目", "頬０涙０"),
            new VymFaceMotionInfo(new string[] { "困った顔" }).addFace("困った", "頬０涙０"),
            new VymFaceMotionInfo(new string[] { "びっくり顔" }).addFace("びっくり", "頬０涙０"),
            new VymFaceMotionInfo(new string[] { "恥ずかしい顔" }).addFace("恥ずかしい", "頬１涙０"),
            new VymFaceMotionInfo(new string[] { "発情顔" }).addFace("発情", "頬２涙０"),
            new VymFaceMotionInfo(new string[] { "エロ顔" }).addFace("エロ通常２", null),
            new VymFaceMotionInfo(new string[] { "エロ興奮", "興奮顔" }).addFace("エロ興奮３", null),
            new VymFaceMotionInfo(new string[] { "エロ羞恥", "羞恥顔" }).addFace("エロ羞恥１", null),
            new VymFaceMotionInfo(new string[] { "エロ期待" }).addFace("エロ期待", null),
            new VymFaceMotionInfo(new string[] { "エロ怯え" }).addFace("エロ怯え", null),
            new VymFaceMotionInfo(new string[] { "エロ舐め" }).addFace("エロ舐め愛情", null),
            new VymFaceMotionInfo(new string[] { "頬染める" }).addFace(null, "頬１"),
            new VymFaceMotionInfo(new string[] { "頬染めない", "頬戻す" }).addFace(null, "頬０"),
            new VymFaceMotionInfo(new string[] { "涙出す" }).addFace(null, "涙１"),
            new VymFaceMotionInfo(new string[] { "涙拭く" }).addFace(null, "涙０"),
            new VymFaceMotionInfo(new string[] { "よだれ" }).addFace(null, "よだれ"),
            new VymFaceMotionInfo(new string[] { "よだれ拭く", "よだれ無し" }).addFace(null, ""),

            //表情詳細設定 表情と両方設定されていたらこちらを優先
            //表情ブレンド用のタグ名と値を「:」で区切って指定します  (fbフェイスも eyeclose eyeclose2 のように指定)
            new VymFaceMotionInfo(new string[] { "普通の目", "目戻して" }).addFaceTag(new string[]
                { "eyeclose:0", "eyeclose2:0", "eyeclose3:0", "eyeclose6:0", "eyebig:0", "hitomis:0", "hitomih:0" }),
            new VymFaceMotionInfo(new string[] { "目を開けて" }).addFaceTag(new string[]
                { "eyeclose:0", "eyeclose2:0", "eyeclose3:0", "eyeclose6:0", "eyebig:0" }),
            new VymFaceMotionInfo(new string[] { "見開いて" }).addFaceTag(new string[]
                { "eyeclose:0", "eyeclose2:0", "eyebig:100" }),
            new VymFaceMotionInfo(new string[] { "目を閉じて", "目をつぶって" }).addFaceTag(new string[]
                { "eyeclose:100", "eyeclose2:0", "eyeclose3:0", "eyeclose6:0", "eyebig:0" }),
            new VymFaceMotionInfo(new string[] { "目をにっこり" }).addFaceTag(new string[]
                { "eyeclose:0", "eyeclose2:100", "eyebig:0" }),
            new VymFaceMotionInfo(new string[] { "少し目を開けて" }).addFaceTag(new string[]
                { "eyeclose:-10", "eyeclose2:-10" }),
            new VymFaceMotionInfo(new string[] { "少し目を閉じて", "目を細めて" }).addFaceTag(new string[]
                { "eyeclose:+10", "eyeclose2:-10", "eyebig:0" }),
            new VymFaceMotionInfo(new string[] { "少しにっこり" }).addFaceTag(new string[]
                { "eyeclose:-10", "eyeclose2:+10", "eyebig:0" }),
            new VymFaceMotionInfo(new string[] { "ウインク" }).addFaceTag(new string[]
                { "eyeclose:0", "eyeclose2:0", "eyeclose3:0", "eyeclose6:100", "eyebig:0" }),
            new VymFaceMotionInfo(new string[] { "口を閉じて" }).addFaceTag(new string[]
            {
                "mouthup:25", "moutha:0", "mouthi:0", "mouthc:0", "mouthdw:0", "mouthhe:0", "mouths:0", "mouthuphalf:0",
                "toothoff:0", "tangopen:0", "tangout:0", "tangup:0"
            }),
            new VymFaceMotionInfo(new string[] { "口を開けて", "口をひらいて" }).addFaceTag(new string[]
            {
                "mouthup:0", "moutha:100", "mouthi:0", "mouthc:0", "mouthdw:0", "mouthhe:0", "mouths:0",
                "mouthuphalf:0", "toothoff:0", "tangopen:0", "tangout:0", "tangup:0"
            }),
            new VymFaceMotionInfo(new string[] { "少し口を開けて" }).addFaceTag(new string[] { "moutha:+10" }),
            new VymFaceMotionInfo(new string[] { "少し口を閉じて" }).addFaceTag(new string[] { "moutha:-10" }),
            new VymFaceMotionInfo(new string[] { "口角上げて" }).addFaceTag(new string[] { "mouthup:+25" }),
            new VymFaceMotionInfo(new string[] { "口角下げて" }).addFaceTag(new string[] { "mouthup:-25" }),
            new VymFaceMotionInfo(new string[] { "口角戻して" }).addFaceTag(new string[] { "mouthup:0" }),
            new VymFaceMotionInfo(new string[] { "口細めて" }).addFaceTag(new string[] { "mouthc:+25" }),
            new VymFaceMotionInfo(new string[] { "口広げて" }).addFaceTag(new string[] { "mouthc:-25" }),
            new VymFaceMotionInfo(new string[] { "への字口" }).addFaceTag(new string[] { "mouthhe:+50", "mouthdw:0", }),
            new VymFaceMotionInfo(new string[] { "困った口" }).addFaceTag(new string[] { "mouthdw:+50", "mouthhe:0" }),
            new VymFaceMotionInfo(new string[] { "普通の眉", "眉戻して" }).addFaceTag(new string[]
                { "mayuup:0", "mayuha:0", "mayuv:0", "mayuw:0", "mayuvhalf:0" }),
            new VymFaceMotionInfo(new string[] { "眉上げて" }).addFaceTag(new string[] { "mayuup:+25" }),
            new VymFaceMotionInfo(new string[] { "眉下げて" }).addFaceTag(new string[] { "mayuup:-25" }),
            new VymFaceMotionInfo(new string[] { "ハの字眉" }).addFaceTag(new string[] { "mayuha:+50" }),
            new VymFaceMotionInfo(new string[] { "困った眉" }).addFaceTag(new string[] { "mayuw:+50" }),
            new VymFaceMotionInfo(new string[] { "舌出して" }).addFaceTag(new string[]
                { "tangopen:+25", "tangout:100", "tangup:0" }),
            new VymFaceMotionInfo(new string[] { "舌戻して" }).addFaceTag(new string[]
                { "tangopen:0", "tangout:0", "tangup:0" }),

            //モーション変更 (.anmは不要)
            new VymFaceMotionInfo(new string[] { "基本立ち" }).addMotion(new string[] { "maid_stand01" }, true),
            new VymFaceMotionInfo(new string[] { "ぽよんぽよん" }).addMotion(new string[] { "edit_pose21_mune_tate_f_once_" },
                true),
            new VymFaceMotionInfo(new string[] { "ぷるんぷるん" }).addMotion(new string[] { "edit_pose21_mune_yoko_f_once_" },
                true),
            new VymFaceMotionInfo(new string[] { "ソファ座り" }).addMotion(new string[] { "kaiwa_sofa_1_f" }, true),
            new VymFaceMotionInfo(new string[] { "膝立ち" }).addMotion(new string[] { "pose_02_f" }, true),
            new VymFaceMotionInfo(new string[] { "ピロートーク" }).addMotion(new string[] { "pillow_talk_f" }, true),
            new VymFaceMotionInfo(new string[] { "ピロートークキス" }).addMotion(new string[] { "pillow_talk_dakituki_kiss_f" },
                true),
            new VymFaceMotionInfo(new string[] { "気絶" }).addMotion(new string[] { "hanyou_kizetu_f" }, true),
            new VymFaceMotionInfo(new string[] { "犬ポーズ" }).addMotion(new string[] { "inu_pose_f" }, true),
            new VymFaceMotionInfo(new string[] { "もみもみ" }).addMotion(new string[] { "momi_momi_f" }, true), //胸物理固定
            new VymFaceMotionInfo(new string[] { "くるっと回る" }).addMotion(new string[] { "turn01" }, true),
            new VymFaceMotionInfo(new string[] { "体を隠す" }).addMotion(new string[] { "sys_munehide" }, true)
                .addFace("恥ずかしい", "頬０涙０"),
            new VymFaceMotionInfo(new string[] { "お辞儀する" }).addMotion(new string[] { "kaiwa_tati_ozigi_n_f_once_" },
                true),
            new VymFaceMotionInfo(new string[] { "バイバイ" }).addMotion(new string[] { "kaiwa_tati_teofuru_taiki_f" },
                true),
            //連続再生 待機モーションループ
            new VymFaceMotionInfo(new string[] { "お辞儀" }).addMotion(
                new string[] { "kaiwa_tati_ozigi_n_f_ONCE_", "kaiwa_tati_taiki_f" }, true),
            new VymFaceMotionInfo(new string[] { "敬礼" }).addMotion(
                new string[] { "kaiwa_tati_keirei_f_ONCE_", "kaiwa_tati_keirei_taiki_f" }, true),
            new VymFaceMotionInfo(new string[] { "ごめんね" }).addMotion(
                new string[] { "kaiwa_tati_ayamaru_f_ONCE_", "kaiwa_tati_ayamaru_taiki_f" }, true),
            new VymFaceMotionInfo(new string[] { "がっくり" }).addMotion(
                new string[]
                {
                    "kaiwa_tati_orz_f_once_", "kaiwa_tati_orz_taiki_f", "kaiwa_tati_orz_tatiagaru_f_ONCE_",
                    "maid_stand01"
                }, true),
            new VymFaceMotionInfo(new string[] { "ソファ手合わせ" }).addMotion(
                new string[]
                {
                    "kaiwa_sofa_teawase_f_ONCE_", "kaiwa_sofa_teawase_taiki_f", "kaiwa_sofa_teawase_end_f_ONCE_",
                    "kaiwa_sofa_1_f"
                }, true),

            //放尿ポーズ 待機モーションループ
            new VymFaceMotionInfo(new string[] { "露出放尿", "おしっこポーズ" })
                .addMotion(new string[] { "rosyutu_hounyou_f_once_", "rosyutu_hounyou_taiki_f" }, true)
                .addFace("恥ずかしい", "頬０涙０"),
            new VymFaceMotionInfo(new string[] { "犬放尿" })
                .addMotion(new string[] { "inu_hounyou_f_once_", "inu_taiki_f" }, true).addFace("恥ずかしい", "頬０涙０"),
            new VymFaceMotionInfo(new string[] { "変態放尿" })
                .addMotion(new string[] { "hentai_pose_hounyou_f_once_", "hentai_pose_03_f" }, true)
                .addFace("恥ずかしい", "頬０涙０"),

            //loopがfalseなので停止していたら次のモーションでCrossFadeしない
            new VymFaceMotionInfo(new string[] { "前に歩く" }).addMotion(new string[] { "aruki_1_idou_f_once_" }, false),

            //マイポーズ
            //new VymFaceMotionInfo(new string[]{"マイポーズテスト"}).addMotion(new string[]{"マイポーズ名"}, true),
        };
    }
}