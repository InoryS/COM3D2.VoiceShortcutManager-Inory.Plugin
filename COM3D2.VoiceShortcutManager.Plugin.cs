using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using UnityEngine;
using UnityEngine.Windows.Speech;
using UnityInjector;
using UnityInjector.Attributes;

/**
音声認識で夜伽コマンド等を制御 ＋ ギアメニューにショートカットキーボタン配置 ＋ テンキー表示

com3d2.vibeyourmaid.plugin.dll (ver2.1.0.0以降) が必要
build :
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:library /lib:"..\COM3D2\Sybaris" /lib:"..\COM3D2\Sybaris\UnityInjector" /lib:"..\COM3D2\COM3D2x64_Data\Managed" /r:UnityEngine.dll /r:UnityEngine.VR.dll /r:UnityInjector.dll /r:Assembly-CSharp.dll /r:Assembly-CSharp-firstpass.dll /r:com3d2.vibeyourmaid.plugin.dll COM3D2.VoiceShortcutManager.Plugin.cs VoiceConfig.cs VymConfig.cs GearMenu.cs
*/

#if COM3D2_5
[assembly: AssemblyTitle("VoiceShortcutManager COM3D2.5")]
#else
[assembly: AssemblyTitle("VoiceShortcutManager COM3D2")]
#endif
[assembly: AssemblyVersion("2.1.0.0")]

namespace COM3D2.VoiceShortcutManager.Plugin
{
	[
		PluginName("COM3D2.VoiceShortcutManager"),
		PluginVersion("2.1"),
    	DefaultExecutionOrder(-10) //プラグインの実行順を他のプラグインより前にする
	]
	public class VoiceShortcutManager : PluginBase
	{
		const string VERSION = "2.1";

		private bool bVR = false;

		//テンキー表示状態
		public bool keyboardVisible = false;

		//マイクの有効状態
		private bool micON = false;

		//シーンの情報
		private int sceneLevel = -1;
		private string sceneName = "";

		//夜伽スキル変更チェック用
		Coroutine yotogiCheckCoroutine;
		private GameObject yotogiCommandUnit = null;
		private GameObject yotogiCommandButton = null;
		private int yotogiCommandCount = 0;

		//GUI情報
		private GUIInfo gui;
		//イベントを透過させない Enterのみ遅延実行で透過させる制御に利用
		private bool guiStopPropagation = true;

		//ギアメニューに配置するボタン
		private GameObject keyboardButton;
		private GameObject micButton;
		private GameObject[] gearMenuButton;

		//Updateで送信するボタンに対応したキー
		MenuInfo sendMenu = null;

		//アイコン画像キャッシュ
		private Dictionary<string, byte[]> icons = new Dictionary<string, byte[]>();

		//音声認識
		KeywordRecognizer keywordRecognizer;
		Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();
		//音声別名 スキル種別毎に配列で格納
		Dictionary<string, List<YotogiVoiceInfo>> yotogiVoiceKeywoards = new Dictionary<string, List<YotogiVoiceInfo>>();

		//夜伽コマンド別名ラベル
		private string yotogiAliasLabel = "";

		//設定ファイル
		Config cfg = null;
		VoiceConfig voiceCfg = null;
		VymConfig vymCfg = null;

		//設定ファイルパス
		const string configPath = @"Sybaris\UnityInjector\Config\VoiceShortcutManager\";
		const string configFile =  configPath+"VoiceShortcutManager.xml";
		const string voiceConfigFile =  configPath+"VoiceConfig.xml";
		const string vymConfigFile =  configPath+"VymConfig.xml";

		//設定ファイルの更新日時 再取得チェック用
		DateTime configFileTime;
		DateTime voiceConfigFileTime;
		DateTime vymConfigFileTime;

		//VibeYourMaidのクラス メソッド確認に利用 nullなら無効判定
		Type vymType = null;

		//設定ファイル
		public class Config
		{
			public Config(){}

			//バージョン番号 設定ファイル上書き保存判定に利用
			public string version = "";

			//起動時に音声認識をONにする
			public bool micActivateOnStart = false;

			//音声認識を利用するならtrue
			public bool micEnabled = true;
			public string micLabelON = "音声認識 ON";
			public string micLabelOFF = "音声認識 OFF";

			//テンキー表示を利用するならtrue
			public bool keyboardEnabled = true;
			public string keyboardLabelON = "テンキー表示 ON";
			public string keyboardLabelOFF = "テンキー表示";

			//テンキーのサイズと色
			public int keybordOffsetY = 60;  //テンキーの表示位置 マウス位置からのオフセット
			public int buttonSize = 32;
			public int fontSize = 16;
			public Color32 color = new Color32(96, 96, 96, 240);

			//夜伽コマンドボタンが無効状態でも音声で実行する
			public bool forceVoiceYotogiCommand = false;

			//ポップアップのテキストに改行が含まれていたら表示したままにする 
			public bool keepExplanationYotogi = false;
			//夜伽シーン以外
			public bool keepExplanation = false;

			public bool useUndressManager = true; //夜伽シーンでは脱衣UIを利用 対象メイド設定は無視される
			public bool playVoiceMaidPos = true; //ボイス再生時にメイドの位置から再生

			//VibeYourMaid連携有効
			public bool vymEnabled = true;
			public bool vymCamMoveEnabled = true; //個別有効設定 カメラ移動
			public bool vymUnzipEnabled = true;   //個別有効設定 UNZIP
			public bool vymFaceMotionEnabled = true;  //個別有効設定 表情とモーション

			//対象メイドリンク種別  0:メインメイドのみ 1:メイン＋リンクしているメイド 2:UNZIPのメインとサブメイド 9:表示中のメイド全員
			public int vymUndressLinkType = 1;  //脱衣の変更対象 -1なら全員脱衣
			public int vymEyeFaceLinkType = 1;  //目線と顔向の変更対象
			public int vymVibeLinkType = 1;     //VibeYourMaidのバイブ操作時
			public int vymFaceLinkType = 0;     //VibeYourMaidの表情変更時
			public int vymMotionLinkType = 0;   //VibeYourMaidのモーション変更時

			public float faceFadeTime = 0.5f;   //VibeYourMaidの表情切替のクロスフェードタイム
			public float motionFadeTime = 0.7f; //VibeYourMaidのモーション切替のクロスフェードタイム
			
			//ショートカットボタンリスト
			public List<MenuInfo> menuList = new List<MenuInfo>();

			public bool playVoicceMaidPos = false;
		}

		//ギアメニューに配置するボタンの情報
		public class MenuInfo
		{
			public string name;  //ボタン名 アイコンファイル名
			public string label; //ボタンラベル
			public string[] voice; //音声呼び出し
			public Keys key; //ショートカットキー
			public bool shift;
			public bool ctrl;
			public bool alt;
			public string sceneRegex;
			public string sceneId;

			public MenuInfo(){}

			public MenuInfo(string name, string label, string[] voice, Keys key, bool shift, bool ctrl, bool alt, string sceneRegex, string sceneId)
			{
				this.name = name;
				this.label = label;
				this.voice = voice;
				this.key = key;
				this.shift = shift;
				this.ctrl = ctrl;
				this.alt = alt;
				this.sceneRegex = sceneRegex;
				this.sceneId = sceneId;
			}
		}

		////////////////////////////////////////////////////////////////
		//初期化時に1回だけ実行
		void Awake()
		{
			GameObject.DontDestroyOnLoad(this);

			bVR = GameMain.Instance.VRMode;
		}

		//開始時に1回だけ実行
		void Start()
		{
			//デフォルトのConfig生成
			cfg = new Config();

			//フォルダ作成
			if (!System.IO.Directory.Exists(configPath)) {
				System.IO.Directory.CreateDirectory(configPath);
			}

			//設定ファイル読み込み
			loadConfig();

			//GUI初期化
			initGUI();

			//音声設定ファイル読み込み
			if (System.IO.File.Exists(voiceConfigFile)) {
				loadVoiceConfig();
				//バージョンが上がっていても保存しない（上書きされて改行もされるため）
				//if (newVersion) saveVoiceConfig();
			} else {
				voiceCfg = new VoiceConfig(); //初期化
				voiceCfg.initDefault();
				saveVoiceConfig();
			}

			//VibeYourMaid設定ファイル読み込み
			if (System.IO.File.Exists(vymConfigFile)) {
				loadVymConfig();
			} else {
				vymCfg = new VymConfig(); //初期化
				saveVymConfig();
			}

			//キャッシュ更新
			createVoiceCache();

			//VibeYourMaidクラスチェック  cfg.vymEnabled=trueならチェック時のエラーを表示
			try {
				vymType = Type.GetType("CM3D2.VibeYourMaid.Plugin.API, com3d2.vibeyourmaid.plugin");
				if (vymType == null) {
					if (cfg.vymEnabled) Debug.LogError("["+GetType().Name+"] VibeYourMaid API not found");
				} else if (vymType.GetMethod("vymGetVersion") == null) {
					if (cfg.vymEnabled) Debug.LogError("["+GetType().Name+"] VibeYourMaid vymGetVersion() not found");
					vymType = null; //無効化
				} else {
					Debug.Log("["+GetType().Name+"] VibeYourMaid ("+vymGetVersion()+") found");
				}
			} catch (Exception e) { Debug.LogError(e); }
		}

		//シーンロード時に呼ばれる
		void OnLevelWasLoaded(int level)
		{
			this.sceneLevel = level;
			this.sceneName = GameMain.Instance.GetNowSceneName();

			//ラベルを閉じて初期化
			if (GearMenu.Buttons.keepExplanation) {
				GearMenu.Buttons.VisibleExplanation(null, false);
				if (micButton) GearMenu.Buttons.SetText(micButton, micON ? cfg.micLabelON : cfg.micLabelOFF);
			}

			//夜伽シーンなら終了チェック用コルーチン開始 
			if (YotogiManager.instans || YotogiOldManager.instans) {
				//コルーチン開始
				if (this.yotogiCheckCoroutine == null) this.yotogiCheckCoroutine = StartCoroutine(yotogiCheck());

				//GearMenuのポップアップ設定 夜伽のみ
				GearMenu.Buttons.keepExplanation = cfg.keepExplanationYotogi;
			} else {
				//コルーチン停止
				if (this.yotogiCheckCoroutine != null) {
					StopCoroutine(this.yotogiCheckCoroutine);
					this.yotogiCheckCoroutine = null;
				}
				//GearMenuのポップアップ設定 夜伽以外
				GearMenu.Buttons.keepExplanation = cfg.keepExplanation;
			}

			//キーボード表示ボタン
			if (cfg.keyboardEnabled && !this.keyboardButton) {
				byte[] icon = null;
				if (this.icons.ContainsKey("Keyboard")) icon = this.icons["Keyboard"];
				this.keyboardButton = GearMenu.Buttons.Add("Keyboard", cfg.keyboardLabelOFF, icon, (go) => toggleKeyboard());
			}

			//マイクボタン追加
			if (cfg.micEnabled && !this.micButton) {
				//初回追加時に有効にする
				this.micON = cfg.micActivateOnStart;

				this.micButton = GearMenu.Buttons.Add("Mic", "", null, (go) => toggleMic());
				//無効状態にボタンを変更
				if (this.micON) enableMic();
				else disableMic();
			}
			else {
				//音声認識が有効だったらキーワードを再設定
				if (this.micON) {
					disableMic();
					enableMic();
				}
			}

			//その他のボタン
			for (int i=0; i<cfg.menuList.Count(); i++) {
				//for内はスコープが効かない
				addButton(i);
			}
		}

		//fpsに関係なく一定時間間隔で実行される
		/*public void FixedUpdate()
		{
			//他のプラグインが認識できるようにこのタイミングで実行
			if (this.sendMenu != null) {
				sendMenuKey(this.sendMenu);
			}
		}*/

		//毎フレーム呼ばれる 更新処理
		void Update()
		{
			//他のプラグインが認識できるようにこのタイミングで実行
			if (this.sendMenu != null) {
				sendMenuKey(this.sendMenu);
			}

			//パネル上のクリックは下に伝播させない ModsSlider用にEnterのみ無効化してから遅延実行する制御をおこなっている
			if (this.keyboardVisible && this.guiStopPropagation) {
				Vector2 pos = new Vector2(Input.mousePosition.x, (float)UnityEngine.Screen.height - Input.mousePosition.y);
				if (this.gui.rect.Contains(pos)) Input.ResetInputAxes();
			}
		}

		//GUI表示処理
		void OnGUI()
		{
			if (keyboardVisible) this.gui.rect = GUI.Window(2024020301, gui.rect, drawGUI, "", gui.gsWin);
		}

		////////////////////////////////////////////////////////////////
		//設定ファイル読み込みと設定からの各種初期化
		private void loadConfig()
		{
			//設定ファイル読み込み 読み込み時に引数なしでnewされる
			if (System.IO.File.Exists(configFile)) {
				//更新されていなければ終了
				if (configFileTime >= System.IO.File.GetLastWriteTime(configFile)) return;
				try {
					System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Config));
					using (System.IO.StreamReader sr = new System.IO.StreamReader(configFile, new System.Text.UTF8Encoding(false))) {
						cfg = (Config)serializer.Deserialize(sr); //XMLファイルから読み込み
					}
					Debug.Log("[VoiceShortcutManager] VoiceShortcutManager.xml Loaded");
					//バージョンが変わっていたら保存
					if (cfg.version != VERSION) {
						cfg.version = VERSION;
						using (System.IO.StreamWriter sw = new System.IO.StreamWriter(configFile, false, new System.Text.UTF8Encoding(false))) {
							serializer.Serialize(sw, cfg); //XMLファイルに保存
						}
						Debug.Log("[VoiceShortcutManager] VoiceShortcutManager.xml Updated to "+VERSION);
					}
					//更新日時 再読み込みチェック用
					configFileTime = System.IO.File.GetLastWriteTime(configFile);
				} catch (Exception e) {
					Debug.LogError("[VoiceShortcutManager] VoiceShortcutManager.xml Error : "+e);
				}
			}
			//ファイルがないならデフォルト設定にサンプル追加
			else {
				//サンプル設定
				cfg.menuList.Add(new MenuInfo("PropMyItem", "PropMyItem", null, Keys.I, true, false, false, null, null));
				cfg.menuList.Add(new MenuInfo("ModsSlider", "ModsSlider", null, Keys.F5, false, false, false, "^SceneEdit$", null));
				cfg.menuList.Add(new MenuInfo("YotogiSlider", "YotogiSlider", null, Keys.F5, false, false, false, "^SceneYotogi$", null));
				cfg.menuList.Add(new MenuInfo("Pause", "Pause", new string[]{"ポーズ","停止"}, Keys.Enter, false, false, false, "^(SceneDance_|SceneVR)", null));
				//保存
				cfg.version = VERSION;
				System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Config));
				using (System.IO.StreamWriter sw = new System.IO.StreamWriter(configFile, false, new System.Text.UTF8Encoding(false))) {
					serializer.Serialize(sw, cfg); //XMLファイルに保存
				}
				Debug.Log("[VoiceShortcutManager] VoiceShortcutManager.xml Created");
				//更新日時 再読み込みチェック用
				configFileTime = System.IO.File.GetLastWriteTime(configFile);
			}

			//ボタンオブジェクト格納用配列
			this.gearMenuButton = new GameObject[cfg.menuList.Count()];

			//アイコン読み込み
			string[] buttons = {"Keyboard", "MicON", "MicOFF", "MicDisabled"};
			foreach (string button in buttons) {
				if (System.IO.File.Exists(configPath+button+".png")) {
					byte[] bytes = System.IO.File.ReadAllBytes(configPath+button+".png");
					if (bytes != null && bytes.Length > 0) {
						if (this.icons.ContainsKey(button)) this.icons.Remove(button);
						this.icons.Add(button, bytes);
					}
				}
			}
			foreach (MenuInfo menu in cfg.menuList) {
				if (System.IO.File.Exists(configPath+menu.name+".png")) {
					byte[] bytes = System.IO.File.ReadAllBytes(configPath+menu.name+".png");
					if (bytes != null && bytes.Length > 0) {
						if (this.icons.ContainsKey(menu.name)) this.icons.Remove(menu.name);
						this.icons.Add(menu.name, bytes);
					}
				}
				//設定のsceneIdに ^ と $ がついていたら除去しておく
				if (menu.sceneId != null) {
					menu.sceneId = menu.sceneId.TrimStart('^');
					menu.sceneId = menu.sceneId.TrimEnd('$');
				}
			}

			//GearMenuのポップアップ設定 夜伽のみ
			if (YotogiManager.instans || YotogiOldManager.instans) {
				GearMenu.Buttons.keepExplanation = cfg.keepExplanationYotogi;
			} else {
				//GearMenuのポップアップ設定 夜伽以外
				GearMenu.Buttons.keepExplanation = cfg.keepExplanation;
			}

		}

		////////////////////////////////////////////////////////////////
		// システムショートカット

		//テンキーのトグル
		public void toggleKeyboard()
		{
			keyboardVisible = !keyboardVisible;
			guiStopPropagation = keyboardVisible;
			GearMenu.Buttons.SetText(keyboardButton, (keyboardVisible ? cfg.keyboardLabelON : cfg.keyboardLabelOFF));

			//位置変更 すぐドラッグ移動できるようにマウスの下に表示 ツールバーには重ねない
			Vector2 pos = new Vector2(Input.mousePosition.x, (float)UnityEngine.Screen.height - Input.mousePosition.y + cfg.keybordOffsetY);
			gui.rect.x = pos.x-10;
			gui.rect.y = pos.y-10;
		}

		//マイクボタンのトグル
		public void toggleMic()
		{
			micON = !micON;
			if (micON) enableMic(); //有効化
			else disableMic(); //無効化
		}

		//有効シーンかどうかチェック
		private bool isSceneValid(MenuInfo menu, string sceneName, int sceneLevel)
		{
			return (menu.sceneRegex == null || Regex.IsMatch(sceneName, menu.sceneRegex)) && (menu.sceneId == null || Regex.IsMatch(sceneLevel.ToString(), "^"+menu.sceneId+"$"));
		}

		//ボタンをギアメニューに追加
		private void addButton(int idx)
		{
			MenuInfo menu = cfg.menuList[idx];

			bool bSceneValid = isSceneValid(menu, this.sceneName, this.sceneLevel);
			if (this.gearMenuButton[idx]) {
				//追加済み 無効シーンなら除去
				if (!bSceneValid) {
					GearMenu.Buttons.Remove(this.gearMenuButton[idx]);
					this.gearMenuButton[idx] = null;
				}
			} else {
				//有効シーンなら追加 ラベルがなければ追加しない
				if (bSceneValid && menu.name != null &&  menu.name != "") {
					//追加
					byte[] icon = null;
					if (this.icons.ContainsKey(menu.name)) icon = this.icons[menu.name];
					this.gearMenuButton[idx] = GearMenu.Buttons.Add(menu.name, menu.label, icon, (go) => sendMenuKey(menu));
				}
			}
		}

		//次フレームのUpdateでキー送信場合はここでセット
		private void setSendMenu(MenuInfo menu)
		{
			this.sendMenu = menu;
		}
		//MenuInfoのキー情報を送信
		private void sendMenuKey(MenuInfo menu)
		{
			sendKey(menu.key, menu.shift, menu.ctrl, menu.alt);
			this.sendMenu = null;
		}

		////////////////////////////////////////////////////////////////
		// 音声関連
		#region Voice
		
		//音声設定ファイル読み込み
		private void loadVoiceConfig()
		{
			try {
				System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(VoiceConfig));
				using (System.IO.StreamReader sr = new System.IO.StreamReader(voiceConfigFile, new System.Text.UTF8Encoding(false))) {
					//XMLファイルから読み込み voiceCfgは引数なしでnewされてXMLの値が設定される
					voiceCfg = (VoiceConfig)serializer.Deserialize(sr);
				}
				//更新日時
				voiceConfigFileTime = System.IO.File.GetLastWriteTime(voiceConfigFile);
				Debug.Log("[VoiceShortcutManager] VoiceConfig.xml Loaded");
			} catch (Exception e) {
				Debug.LogError("[VoiceShortcutManager] VoiceConfig.xml Error : "+e);
			}

		}
		//音声設定ファイル保存 基本ファイルがない場合のみ
		private void saveVoiceConfig()
		{
			System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(VoiceConfig));
			using (System.IO.StreamWriter sw = new System.IO.StreamWriter(voiceConfigFile, false, new System.Text.UTF8Encoding(false))) {
				serializer.Serialize(sw, voiceCfg); //XMLファイルに保存
			}
			//更新日時
			voiceConfigFileTime = System.IO.File.GetLastWriteTime(voiceConfigFile);
			Debug.Log("[VoiceShortcutManager] VoiceConfig.xml Created");
		}
		//夜伽用の音声設定からDictionary作成
		private void createVoiceCache()
		{
			//キャッシュクリア
			yotogiVoiceKeywoards.Clear();

			//別名キャッシュ作成 スキルごとに格納
			foreach (YotogiVoiceInfo voiceInfo in voiceCfg.yotogiVoiceList) {
				if (voiceInfo.alias.Length == 0) continue; //別名設定なしならスキップ
				if (yotogiVoiceKeywoards.ContainsKey(voiceInfo.command)) {
					List<YotogiVoiceInfo> list = yotogiVoiceKeywoards[voiceInfo.command];
					foreach (YotogiVoiceInfo info in list) {
						if (voiceInfo.skill == info.skill) {
							Debug.LogError("YotogiVoice設定が重複しています : "+voiceInfo.command);
						}
					}
					list.Add(voiceInfo);
				} else {
					List<YotogiVoiceInfo> list = new List<YotogiVoiceInfo>{};
					list.Add(voiceInfo);
					yotogiVoiceKeywoards.Add(voiceInfo.command, list);
				}
			}
		}

		//音声設定ファイル読み込み
		private void loadVymConfig()
		{
			try {
				System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(VymConfig));
				using (System.IO.StreamReader sr = new System.IO.StreamReader(vymConfigFile, new System.Text.UTF8Encoding(false))) {
					//XMLファイルから読み込み vymCfgは引数なしでnewされてXMLの値が設定される
					vymCfg = (VymConfig)serializer.Deserialize(sr);
				}
				//更新日時
				vymConfigFileTime = System.IO.File.GetLastWriteTime(vymConfigFile);
				Debug.Log("[VoiceShortcutManager] "+vymConfigFile+" Loaded");
			} catch (Exception e) {
				Debug.LogError("[VoiceShortcutManager] "+vymConfigFile+" Error : "+e);
			}

		}
		//音声設定ファイル保存 基本ファイルがない場合のみ
		private void saveVymConfig()
		{
			System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(VymConfig));
			using (System.IO.StreamWriter sw = new System.IO.StreamWriter(vymConfigFile, false, new System.Text.UTF8Encoding(false))) {
				serializer.Serialize(sw, vymCfg); //XMLファイルに保存
			}
			//更新日時
			vymConfigFileTime = System.IO.File.GetLastWriteTime(vymConfigFile);
			Debug.Log("[VoiceShortcutManager] "+vymConfigFile+" Created");
		}


		//音声認識有効化
		private void enableMic()
		{
			//設定ファイル再読み込み 更新されていなけば読み込まない
			loadConfig();

			//音声設定ファイル
			if (System.IO.File.Exists(voiceConfigFile)) {
				//ファイルが更新されていたら読み込み
				if (voiceConfigFileTime < System.IO.File.GetLastWriteTime(voiceConfigFile)) {
					loadVoiceConfig();
				}
			} else {
				//ファイルがなくなっていたら初期化して出力
				voiceCfg = new VoiceConfig();
				voiceCfg.initDefault();
				saveVoiceConfig();
			}
			//キャッシュ更新
			createVoiceCache();

			//VYM設定ファイル
			if (System.IO.File.Exists(vymConfigFile)) {
				//ファイルが更新されていたら読み込み
				if (vymConfigFileTime < System.IO.File.GetLastWriteTime(vymConfigFile)) {
					loadVymConfig();
				}
			} else {
				//ファイルがなくなっていたら初期化して出力
				vymCfg = new VymConfig();
				saveVymConfig();
			}

			//シーンに対応したキーワードを設定して音声認識開始
			setVoiceKeywords();

			if (keywords.Count() == 0) {
				//ラベルとアイコン変更
				GearMenu.Buttons.SetText(micButton, cfg.micLabelON+" (no keyword)");
				if (this.icons.ContainsKey("MicDisabled")) GearMenu.Buttons.SetTexture(micButton, this.icons["MicDisabled"]);
			} else {
				//ラベルとアイコン変更
				string menuText = cfg.micLabelON;
				if (yotogiAliasLabel != "") menuText += "\n"+yotogiAliasLabel;
				GearMenu.Buttons.SetText(micButton, menuText);

				if (this.icons.ContainsKey("MicON")) GearMenu.Buttons.SetTexture(micButton, this.icons["MicON"]);

				//キーワードを認識対象にセット
				keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
				//コールバック設定
				keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;

				//開始
				keywordRecognizer.Start();

				Debug.Log("音声認識開始 : "+keywords.Count()+" keywords");
			}
		}

		//音声認識無効化  キーワード一覧を空にしてkeywordRecognizerを破棄する
		private void disableMic()
		{
			//ラベルとアイコン変更
			GearMenu.Buttons.SetText(micButton, cfg.micLabelOFF);
			if (this.icons.ContainsKey("MicOFF")) GearMenu.Buttons.SetTexture(micButton, this.icons["MicOFF"]);

			if (keywords.Count() > 0) {
				//キーワードをクリア
				keywords.Clear();
				//無効化
				keywordRecognizer.Dispose();
			}
		}

		//夜伽コマンドの変更を1秒おきにチェック
        private IEnumerator yotogiCheck()
        {
			while (true) {
				yield return new WaitForSeconds(1f); //1秒待機
				if (this.micON) {
					//夜伽が切り替わってボタンが破棄されていたら音声認識再設定
					if (YotogiManager.instans && YotogiManager.instans.play_mgr && YotogiManager.instans.play_mgr.playingSkill != null) {
						if (!this.yotogiCommandButton) {
							disableMic();
							enableMic();
						}
					}
					//経営切替の夜伽  ※無効なコマンドは非表示になっている
					else if (YotogiOldManager.instans && YotogiOldManager.instans.play_mgr) {
						if (!this.yotogiCommandButton) {
							disableMic();
							enableMic();
						}
						//ボタンリストがあれば数を比較
						else if (this.yotogiCommandUnit && this.yotogiCommandCount != this.yotogiCommandUnit.transform.childCount) {
							disableMic();
							enableMic();
						}
					}
				}
			}
		}

		//音声認識時のコールバック実行処理
		private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
		{
			System.Action keywordAction;
			if (keywords.TryGetValue(args.text, out keywordAction)) {
				keywordAction.Invoke();
			}
		}

		//音声認識キーワードを設定
		private void setVoiceKeywords()
		{
			//キーワードは空に
			keywords.Clear();

			//ショートカットキー呼び出し
			foreach (MenuInfo menuInfo in cfg.menuList) {
				MenuInfo menu = menuInfo; //スコープ対策
				if (menu.voice == null) continue;
				if (!isSceneValid(menu, sceneName, sceneLevel)) continue; //シーン無効

				foreach (string keyword in menu.voice) {
					string k = keyword; //スコープ対策
					if (keywords.ContainsKey(keyword)) {
						Debug.LogError("VoiceConfig.xml ショートカットボイスが重複しています : "+keyword);
					} else {
						keywords.Add(keyword, () => {
							//実行時にも一応シーンをチェック ボタンは表示されないはず
							if (isSceneValid(menu, sceneName, sceneLevel)) {
								Debug.Log("Voice sendKey : "+k+" → "+menu.name);
								//sendMenuKey(menu);
								setSendMenu(menu);
							} else {
								Debug.Log("Voice sendKey : 無効なシーンです");
							}
						});
					}
				}
			}

			//システム操作キーワード
			setSystemKeyword();

			//夜伽シーンキーワード設定
			setYotogiKeywords();

			//視線 顔向き
			setFaceKeywoards();

			//脱衣
			setUndressKeywoards();

			//VYM連携
			setVymKeywoards();

			//DEBUG
			if (!keywords.ContainsKey("テスト")) {
				keywords.Add("テスト", () => {
					Debug.Log("Voice Test : テスト");
				});
			}
		}

		//夜伽コマンドのスキルとキーワードに対応する声設定とボタンを取得
		//夜伽が切り替わった時も再取得する
		//先頭が「-」がスキル名 「cm:」がコマンド
		private void setYotogiKeywords()
		{
			this.yotogiAliasLabel = "";

			if (!YotogiManager.instans && !YotogiOldManager.instans) return;
			this.yotogiCommandUnit = GameObject.Find("/UI Root/YotogiPlayPanel/CommandViewer/SkillViewer/MaskGroup/SkillGroup/CommandParent/CommandUnit");
			if (!this.yotogiCommandUnit) return;

			string skillName = "";
			List<GameObject> commands = new List<GameObject>();
			foreach (Transform command in this.yotogiCommandUnit.transform) {
				if (command.name.StartsWith("cm:")) {
					commands.Add(command.gameObject);
				} else if (command.name.StartsWith("-")) {
					skillName = command.name.Substring(1);
					Debug.Log("skillName="+skillName);
				}
			}
			bool first = true;
			foreach (GameObject command in commands) {
				UIButton uiButton = command.GetComponent<UIButton>();
				if (uiButton == null) continue;
				string keyword = command.name.Substring(3);
				string aliasLabel = "";

				Debug.Log("Voice Keyword : "+keyword);
				bool addCommand = true; //元のコマンドのキーワードも登録 ※別名の最初が空文字なら登録しない
				if (yotogiVoiceKeywoards.ContainsKey(keyword)) {
					//夜伽の別名情報
					YotogiVoiceInfo voiceInfo = null;
					//スキル指定をチェック
					List<YotogiVoiceInfo> voiceInfos = yotogiVoiceKeywoards[keyword];
					foreach (YotogiVoiceInfo info in voiceInfos) {
						if (info.skill == null || info.skill == "") {
							//スキル指定なし 個別設定を優先するのでループは抜けない
							voiceInfo = info;
						}
						else if (Regex.IsMatch(skillName, info.skill)) {
							voiceInfo = info;
							break;
						}
					}
					//別名がある場合
					if (voiceInfo != null && voiceInfo.alias != null) {
						foreach (string alias in voiceInfo.alias) {
							if (alias == null || alias == "") addCommand = false; //元コマンドは追加しない
							else {
								if (keywords.ContainsKey(alias)) {
									Debug.LogError("VoiceConfig.xml 夜伽別名が重複しています : "+keyword+" → "+alias);
								} else {
									Debug.Log("        Alias : → "+alias);
									string _alias = alias; //スコープ対策
									if (aliasLabel == "") aliasLabel = alias;
									else aliasLabel += "  |  "+alias;
									keywords.Add(alias, () => {
										Debug.Log("YotogiCommand : "+keyword+" ("+_alias+")");
										if (uiButton) {
											if (uiButton.isEnabled) {
												//ボタンが有効な場合のみクリック送信
												uiButton.SendMessage("OnClick");
											} else if (cfg.forceVoiceYotogiCommand) {
												//無効な夜伽コマンドも強制実行
												uiButton.isEnabled = true;
												uiButton.SendMessage("OnClick");
												uiButton.isEnabled = false;
											}
										}
									});
								}
							}
						}
					}
				}
				if (addCommand) {
					//『』「」は除去する
					if (keyword.StartsWith("『")) {
						keyword = keyword.Substring(1).Replace("』", "");
					}
					else if (keyword.StartsWith("「")) {
						keyword = keyword.Substring(1).Replace("」", "");
					}
					if (keywords.ContainsKey(keyword)) {
						Debug.LogError("VoiceConfig.xml 夜伽コマンド名が重複しています : "+keyword);
					} else {
						keywords.Add(keyword, () => {
							Debug.Log("YotogiCommand : "+keyword);
							if (uiButton) {
								if (uiButton.isEnabled) {
									//ボタンが有効な場合のみクリック送信
									uiButton.SendMessage("OnClick");
								} else if (cfg.forceVoiceYotogiCommand) {
									//無効な夜伽コマンドも強制実行
									uiButton.isEnabled = true;
									uiButton.SendMessage("OnClick");
									uiButton.isEnabled = false;
								}
							}
						});
					}
				}

				//表示用ラベル
				if (aliasLabel != "") {
					//if (this.yotogiAliasLabel != "") this.yotogiAliasLabel += "\n";
					this.yotogiAliasLabel += "\n【 "+keyword+" 】\n  → " + (addCommand ? keyword+"  |  " : "") + aliasLabel + "  ";
				} else {
					this.yotogiAliasLabel += "\n【 "+keyword+" 】\n";
				}

				//スキル変更チェック用 yotogiCommandButtonのGameObjectが破棄されているかFixedUpdateでチェックする
				if (first) {
					this.yotogiCommandButton = command;
					first = false;
				}
			}

			//経営切替でコマンドボタンの数が変わったかのチェック用
			this.yotogiCommandCount = this.yotogiCommandUnit.transform.childCount;
		}

		//キーワードのコールバック設定  音声に対応するメソッドをInvokeで呼び出す
		private void setVoiceKeywordInvoke(string[] voices, string name)
		{
			setVoiceKeywordInvoke(voices, name, null);
		}
		private void setVoiceKeywordInvoke(string[] voices, string name, object[] args)
		{
			foreach (string keyword in voices) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice "+name+" : "+k);
					MethodInfo method = this.GetType().GetMethod(name);
					if (method == null) Debug.LogError(name+" not found");
					else method.Invoke(this, args);
				});
			}
		}

		#endregion

		////////////////////////////////////////////////////////////////
		// システム・設定周り
		#region SystemVoice

		private void setSystemKeyword()
		{
			//オートモード
			foreach (string keyword in voiceCfg.autoModeOn) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice scriptAutoOn : "+k);
					setAutoMode(true);
				});
			}
			foreach (string keyword in voiceCfg.autoModeOff) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice scriptAutoOff : "+k);
					setAutoMode(false);
				});
			}
			foreach (string keyword in voiceCfg.playVoice) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice playVoice : "+k);
					playVoice(0);
				});
			}
			foreach (string keyword in voiceCfg.playVoiceBefore) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice playVoiceBefore : "+k);
					playVoice(1);
				});
			}

			//自分のConfig変更
			foreach (ConfigInfo info in voiceCfg.config) {
				ConfigInfo value = info; //スコープ対策
				foreach (string keyword in info.voice) {
					if (keywords.ContainsKey(keyword)) continue;
					string k = keyword; //スコープ対策
					keywords.Add(keyword, () => {
						Debug.Log("Voice config ("+value+") : "+k);
						setConfig(value);
					});
				}
			}
		}

		//オートモード切替
		public void setAutoMode(bool auto)
		{
			ADVKagManager advKag = (ADVKagManager)GameMain.Instance.ScriptMgr.adv_kag;
			if (GameMain.Instance.MsgWnd) {
				if (advKag.auto_mode != auto) {
					GameMain.Instance.MsgWnd.CallEvent(MessageWindowMgr.MessageWindowUnderButton.Auto);
				}
			}
			advKag.auto_mode = auto;
		}

		//メッセージウィンドウのボイス再生 VOICEボタンを押す処理を事項
		public void playVoice(int skipCount)
		{
			//ログのボイスデータの情報から直近のボイスを再生
			if (GameMain.Instance.MsgWnd != null) {
				FieldInfo backlogUnitInfo = typeof(MessageWindowMgr).GetField("m_listBacklogUnit", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
				List<BacklogCtrl.BacklogUnit> backlogList = (List<BacklogCtrl.BacklogUnit>)backlogUnitInfo.GetValue(GameMain.Instance.MsgWnd);
				//新しい順
				for (int i=backlogList.Count-1; i>=0; i--) {
					BacklogCtrl.BacklogUnit unit = backlogList[i];
					
					//音声なしはスキップ
					if (String.IsNullOrEmpty(unit.m_voiceId)) continue;

					//指定回数のログ音声はスキップ
					if (skipCount > 0) {
						skipCount--;
					} else {
						if (cfg.playVoicceMaidPos) {
							//名前からメイドを特定できたら再生
							for (int j = 0; j < GameMain.Instance.CharacterMgr.GetStockMaidCount(); j++) {
								Maid maid = GameMain.Instance.CharacterMgr.GetStockMaid(j);
								if (maid.Visible && (maid.status.firstName == unit.m_speakerName || maid.status.lastName == unit.m_speakerName)) {
									Debug.Log("playVoice : "+unit.m_speakerName+" : "+unit.m_voiceId);
									maid.AudioMan.LoadPlay(unit.m_voiceId, 0f, false, false);
									return;
								}
							}
						}
						//メイドが見つからなかったら通常再生
						GameMain.Instance.SoundMgr.PlayDummyVoice(unit.m_voiceId, 0f, false, false, 50, AudioSourceMgr.Type.Voice);
						return;
					}
				}
				//再生されなかったら通常の再生処理実行
				GameMain.Instance.MsgWnd.CallEvent(MessageWindowMgr.MessageWindowUnderButton.Voice);
			}
		}

		//設定変更
		public void setConfig(ConfigInfo info)
		{
			try {
				//データ型に合わせて設定
				FieldInfo cfgFieldInfo = cfg.GetType().GetField(info.name);
				if (cfgFieldInfo != null) {
					if (cfgFieldInfo.FieldType == typeof(bool)) {
						bool value;
						if (info.value == "toggle") value = !(bool)cfgFieldInfo.GetValue(cfg);
						else value = bool.Parse(info.value);
						cfgFieldInfo.SetValue(cfg, value);
					} else if (cfgFieldInfo.FieldType == typeof(int)) {
						cfgFieldInfo.SetValue(cfg, int.Parse(info.value));
					} else if (cfgFieldInfo.FieldType == typeof(float)) {
						cfgFieldInfo.SetValue(cfg, float.Parse(info.value));
					} else if (cfgFieldInfo.FieldType == typeof(string)) {
						cfgFieldInfo.SetValue(cfg, info.value);
					} else {
						Debug.Log("setConfig : "+info.name+" type="+cfgFieldInfo.FieldType+" is not support");
						return;
					}
					Debug.Log("setConfig : "+info.name+" value="+info.value);
				}
			} catch (Exception e) { UnityEngine.Debug.LogError(e); }
		}

		#endregion

		////////////////////////////////////////////////////////////////
        //VibeYourMaid連携
		#region VibeYourMaid

		//VymVoiceinfoのキーワードに対応した連携メソッド呼び出し
		private void setVymKeywordInvoke(string[] keywords, string name, object[] args)
		{
			setVymKeywordInvoke(keywords, name, null, args);
		}
		//VymVoiceinfoのキーワードに対応した連携メソッド呼び出し ログ表示用ラベルあり
		private void setVymKeywordInvoke(string[] voices, string name, string label, object[] args)
		{
			foreach (string keyword in voices) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice "+name+ ( label==null ? "" : (" ("+label+")") ) +" : "+k);
					MethodInfo method = vymType.GetMethod(name);
					if (method == null) Debug.LogError(name+" not found");
					else method.Invoke(null, args);
				});
			}
		}

		//キーワード登録
		private void setVymKeywoards()
		{
			//リンク設定が無効かdllがなければキーワードは登録しない
			if (!cfg.vymEnabled || vymType == null) return;

			//正面のメイドをメインメイドにする
			setVymKeywordInvoke(vymCfg.vymSelectFrontMaid, "vymSelectFrontMaid", null);
			//名前でメインメイドを選択
			foreach (VymVoiceInfo info in vymCfg.vymSelectMaidName) {
				string value = info.value;
				setVymKeywordInvoke(info.voice, "vymSelectMaidName", info.value, new object[]{value});
			}
			//前のメイド カメラ移動なし
			setVymKeywordInvoke(vymCfg.vymPrevMaid, "vymPrevMaid", new object[]{false});
			//次のメイド カメラ移動なし
			setVymKeywordInvoke(vymCfg.vymNextMaid, "vymNextMaid", new object[]{false});
			//前のメイド カメラ移動なし
			setVymKeywordInvoke(vymCfg.vymLeftMaid, "vymLeftMaid", new object[]{false});
			//次のメイド カメラ移動なし
			setVymKeywordInvoke(vymCfg.vymRightMaid, "vymRightMaid", new object[]{false});

			//リンク制御
			foreach (VymVoiceInfo info in vymCfg.vymMaidLink) {
				string value = info.value;
				setVymKeywordInvoke(info.voice, "vymMaidLink", info.value, new object[]{value});
			}

			if (cfg.vymCamMoveEnabled) {
				//VR移動
				if (bVR) {
					foreach (VymMoveInfo info in vymCfg.vymCamMove) {
						setVymKeywordInvoke(info.voice, "vymCamMove", new object[]{info.move, info.useMoveValue});
					}
				}

				//メイドとカメラの距離
				foreach (VymVoiceInfo info in vymCfg.vymCamDistance) {
					float value = float.Parse(info.value.Replace("%", ""));
					int moveType = 0;
					if (info.value.EndsWith("%")) moveType = 2;
					else if (info.value.StartsWith("+") || info.value.StartsWith("-")) moveType = 1; //offset
					setVymKeywordInvoke(info.voice, "vymCamDistance", info.value, new object[]{value, moveType});
				}

				//メイドの正面にカメラを移動 body0の正面
				setVymKeywordInvoke(vymCfg.vymCamFront, "vymCamAround", "front", new object[]{1, 0f});
				//メイドの後ろにカメラを移動 body0の後ろ
				setVymKeywordInvoke(vymCfg.vymCamBack, "vymCamAround", "back", new object[]{1, 180f});

				//メイドの顔の部位の正面にカメラを移動
				foreach (VymVoiceInfo info in vymCfg.vymCamTarget) {
					float value = float.Parse(info.value);
					int value2 = int.Parse(info.value2);
					setVymKeywordInvoke(info.voice, "vymCamTarget", info.value+" "+info.value2, new object[]{value, Math.Abs(value2), value2 < 0});
				}

				//ご主人様の頭にカメラを移動
				foreach (VymVoiceInfo info in vymCfg.vymCamManHead) {
					int value = int.Parse(info.value); //男指定
					int value2 = int.Parse(info.value2); //視線方向指定
					setVymKeywordInvoke(info.voice, "vymCamManHead", info.value+" "+info.value2, new object[]{value, value2});
				}
			}
			
			//一人称
			foreach (VymVoiceInfo info in vymCfg.vymFps) {
				int fps = int.Parse(info.value);
				setVymKeywordInvoke(info.voice, "vymLookPoint", "Fps "+info.value, new object[]{fps, -1, -1});
			}
			//メイド固定
			foreach (VymVoiceInfo info in vymCfg.vymFollow) {
				int follow = int.Parse(info.value);
				setVymKeywordInvoke(info.voice, "vymLookPoint", "Follow "+info.value, new object[]{-1, follow, -1});
			}
			//メイド固定 注視点
			foreach (VymVoiceInfo info in vymCfg.vymLookPoint) {
				int lookPoint = int.Parse(info.value);
				int follow = lookPoint == 11 ? 1 : -1; //アングル固定時はメイドも固定
				setVymKeywordInvoke(info.voice, "vymLookPoint", info.value, new object[]{-1, follow, lookPoint});
			}

			//地面判定
			foreach (VymVoiceInfo info in vymCfg.vymBoneHitHeight) {
				float value = float.Parse(info.value);
				bool offset = info.value.StartsWith("+") || info.value.StartsWith("-");
				setVymKeywordInvoke(info.voice, "vymBoneHitHeight", info.value, new object[]{value, offset});
			}

			//興奮値
			foreach (VymVoiceInfo info in vymCfg.vymExcite) {
				int value = int.Parse(info.value);
				bool offset = info.value.StartsWith("+") || info.value.StartsWith("-");
				setVymKeywordInvoke(info.voice, "vymExcite", info.value, new object[]{value, offset});
			}

			//男表示
			foreach (VymVoiceInfo info in vymCfg.vymManVisible) {
				int value = int.Parse(info.value);
				setVymKeywordInvoke(info.voice, "vymManVisible", info.value, new object[]{value});
			}
			//男強制射精
			foreach (VymVoiceInfo info in vymCfg.vymSyasei) {
				int value = int.Parse(info.value);
				bool lockMode = info.value.StartsWith("+") || info.value.StartsWith("-");
				setVymKeywordInvoke(info.voice, "vymSyasei", info.value, new object[]{value, lockMode});
			}

			//拭き取り
			foreach (VymVoiceInfo info in vymCfg.vymFukitori) {
				int value = int.Parse(info.value);
				setVymKeywordInvoke(info.voice, "vymFukitori", info.value, new object[]{value});
			}

			//尿
			foreach (VymVoiceInfo info in vymCfg.vymStartNyo) {
				int value = int.Parse(info.value);
				setVymKeywordInvoke(info.voice, "vymStartNyo", info.value, new object[]{value});
			}
			//潮
			foreach (VymVoiceInfo info in vymCfg.vymStartSio) {
				int value = int.Parse(info.value);
				setVymKeywordInvoke(info.voice, "vymStartSio", info.value, new object[]{value});
			}

			//バイブ切り替え
			foreach (VymVoiceInfo info in vymCfg.vymVibe) {
				int value = int.Parse(info.value);
				setVymKeywordInvoke(info.voice, "vymVibe", info.value, new object[]{value, cfg.vymVibeLinkType});
			}
			//バイブオート
			foreach (VymVoiceInfo info in vymCfg.vymVibeAuto) {
				int value = int.Parse(info.value);
				bool offset = info.value.StartsWith("+") || info.value.StartsWith("-");
				setVymKeywordInvoke(info.voice, "vymVibeAuto", info.value, new object[]{value, offset, cfg.vymVibeLinkType});
			}

			//クパ
			foreach (VymVoiceInfo info in vymCfg.vymKupa) {
				float value = float.Parse(info.value);
				bool offset = info.value.StartsWith("+") || info.value.StartsWith("-");
				setVymKeywordInvoke(info.voice, "vymKupa", info.value, new object[]{value, offset});
			}
			//アナル
			foreach (VymVoiceInfo info in vymCfg.vymAnal) {
				float value = float.Parse(info.value);
				bool offset = info.value.StartsWith("+") || info.value.StartsWith("-");
				setVymKeywordInvoke(info.voice, "vymAnal", info.value, new object[]{value, offset});
			}
			//クリ
			foreach (VymVoiceInfo info in vymCfg.vymBokki) {
				float value = float.Parse(info.value);
				bool offset = info.value.StartsWith("+") || info.value.StartsWith("-");
				setVymKeywordInvoke(info.voice, "vymBokki", info.value, new object[]{value, offset});
			}

			if (cfg.vymUnzipEnabled) {
				//UNZIPモーション
				foreach (VymVoiceInfo info in vymCfg.vymUnzip) {
					string value = info.value;
					setVymKeywordInvoke(info.voice, "vymUnzip", info.value, new object[]{value});
				}
				//UNZIP派生モーション
				foreach (VymVoiceInfo info in vymCfg.vymUnzipDerive) {
					string[] value = null;
					if (info.value != null) value = info.value.Split(',');
					setVymKeywordInvoke(info.voice, "vymUnzipDerive", info.value, new object[]{value});
				}
				//UNZIPランダムモーション
				foreach (VymVoiceInfo info in vymCfg.vymUnzipRandom) {
					string value = info.value;
					setVymKeywordInvoke(info.voice, "vymUnzipRandom", info.value, new object[]{value});
				}
				//UNZIPモーション切替
				foreach (VymVoiceInfo info in vymCfg.vymUnzipChange) {
					int value = int.Parse(info.value);
					setVymKeywordInvoke(info.voice, "vymUnzipChange", info.value, new object[]{value});
				}

				//抜く trueなら強制外だし
				foreach (VymVoiceInfo info in vymCfg.vymRemoveMotion) {
					bool value = bool.Parse(info.value);
					setVymKeywordInvoke(info.voice, "vymRemoveMotion", info.value, new object[]{value});
				}
				//再挿入
				setVymKeywordInvoke(vymCfg.vymInsertMotion, "vymInsertMotion", null);
				//素股
				setVymKeywordInvoke(vymCfg.vymSumataMotion, "vymSumataMotion", null);

				//後ろを使う 前を使う
				foreach (VymVoiceInfo info in vymCfg.vymAnalMode) {
					bool value = bool.Parse(info.value);
					setVymKeywordInvoke(info.voice, "vymAnalMode", info.value, new object[]{value});
				}
			}

			//ボイスセット
			foreach (VymVoiceInfo info in vymCfg.vymVoiceSet) {
				string value = info.value;
				setVymKeywordInvoke(info.voice, "vymVoiceSet", info.value, new object[]{value});
			}
			//キスボイスセット
			foreach (VymVoiceInfo info in vymCfg.vymKissVoiceSet) {
				string value = info.value;
				setVymKeywordInvoke(info.voice, "vymKissVoiceSet", info.value, new object[]{value});
			}

			if (cfg.vymFaceMotionEnabled) {
				//表情とモーションとボイスセットは1つのキーワードで呼び出せるように設定
				//マイポーズ 連続モーション  フェードは共通設定
				foreach (VymFaceMotionInfo info in vymCfg.vymFaceMotion) {
					string face = info.face;
					string blend = info.blend;
					string[] tags = info.tags;
					string[] motion = info.motion;
					bool loop = info.loop != null && info.loop == "true"; //文字列になっている
					string voiceSet = info.voiceSet;
					foreach (string keyword in info.voice) {
						if (keywords.ContainsKey(keyword)) continue;
						string k = keyword; //スコープ対策
						keywords.Add(keyword, () => {
							//ボイスセット
							if (voiceSet != null) {
								string name = "vymVoiceSet";
								Debug.Log("Voice "+name+" : "+k);
								if (vymType.GetMethod(name) == null) Debug.LogError(name+" not found");
								else vymVoiceSet(voiceSet);
							}
							//モーション
							if (motion != null) {
								string name = "vymMotion";
								Debug.Log("Voice "+name+" : "+k);
								if (vymType.GetMethod(name) == null) Debug.LogError(name+" not found");
								else vymMotion(motion, new float[]{cfg.motionFadeTime}, loop);
							}
							//表情 タグを優先
							if (tags != null) {
								string name = "vymFaceBlend";
								Debug.Log("Voice "+name+" : "+k);
								if (vymType.GetMethod(name) == null) Debug.LogError(name+" not found");
								else vymFaceBlend(tags);
							} else {
								string name = "vymFace";
								Debug.Log("Voice "+name+" ("+face+") : "+k);
								if (vymType.GetMethod(name) == null) Debug.LogError(name+" not found");
								else vymFace(face, blend, cfg.faceFadeTime);
							}
						});
					}
				}
			}

			//VYMのConfig変更
			foreach (VymConfigInfo info in vymCfg.vymConfig) {
				VymConfigInfo info2 = info; //スコープ対策
				foreach (string keyword in info.voice) {
					if (keywords.ContainsKey(keyword)) continue;
					string k = keyword; //スコープ対策
					keywords.Add(keyword, () => {
						string name = "vymGetConfig";
						Debug.Log("Voice vymConfig : "+k);
						if (vymType.GetMethod(name) == null) Debug.LogError(name+" not found");
						else vymGetConfig(info2);
					});
				}
			}

		}

		//メソッドが無いとエラーになるのでメソッドの外で呼び出す前に有無をチェック
		private string vymGetVersion() { return CM3D2.VibeYourMaid.Plugin.API.vymGetVersion(); }

		private Maid vymGetFrontMaid() { return CM3D2.VibeYourMaid.Plugin.API.vymGetFrontMaid(); }
		private List<Maid> vymGetMaidList(int linkType) { return CM3D2.VibeYourMaid.Plugin.API.vymGetMaidList(linkType); }

		private void vymFace(string face, string blend, float fadeFace) { CM3D2.VibeYourMaid.Plugin.API.vymFace(face, blend, fadeFace, cfg.vymFaceLinkType); }
		private void vymFaceBlend(string[] tags) { CM3D2.VibeYourMaid.Plugin.API.vymFaceBlend(tags, cfg.vymFaceLinkType); }
		private void vymMotion(string[] motions, float[] motionFades, bool loop) {
			CM3D2.VibeYourMaid.Plugin.API.vymMotion(motions, motionFades, loop, cfg.vymMotionLinkType);
		}
		private void vymVoiceSet(string voiceFile) { CM3D2.VibeYourMaid.Plugin.API.vymVoiceSet(voiceFile); }

		//VibeYourMaidの設定変更 bool,int,float,string型のみ 
		private void vymGetConfig(VymConfigInfo info)
		{
			try {
				//VYM設定取得
				CM3D2.VibeYourMaid.Plugin.VibeYourMaid.VibeYourMaidCfgWriting cfgw = CM3D2.VibeYourMaid.Plugin.API.vymGetConfig();
				//データ型に合わせて設定
				FieldInfo cfgwFieldInfo = cfgw.GetType().GetField(info.name);
				if (cfgwFieldInfo != null) {
					if (cfgwFieldInfo.FieldType == typeof(bool)) {
						bool value;
						if (info.value == "toggle") value = !(bool)cfgwFieldInfo.GetValue(cfgw);
						else value = bool.Parse(info.value);
						cfgwFieldInfo.SetValue(cfgw, value);
					} else if (cfgwFieldInfo.FieldType == typeof(int)) {
						cfgwFieldInfo.SetValue(cfgw, int.Parse(info.value));
					} else if (cfgwFieldInfo.FieldType == typeof(float)) {
						cfgwFieldInfo.SetValue(cfgw, float.Parse(info.value));
					} else if (cfgwFieldInfo.FieldType == typeof(string)) {
						cfgwFieldInfo.SetValue(cfgw, info.value);
					} else {
						Debug.Log("vymConfig : "+info.name+" type="+cfgwFieldInfo.FieldType+" is not support");
						return;
					}

					Debug.Log("vymConfig : "+info.name+" value="+info.value);
					if (info.init) CM3D2.VibeYourMaid.Plugin.API.vymInitConfig();
				}
			} catch (Exception e) { UnityEngine.Debug.LogError(e); }
		}

		//Invokeで呼んでいるので未使用 API変更チェック用に残している 
		private void vymSelectFrontMaid() { CM3D2.VibeYourMaid.Plugin.API.vymSelectFrontMaid(); }
        private void vymSelectMaidName(string name) { CM3D2.VibeYourMaid.Plugin.API.vymSelectMaidName(name); }
		private void vymPrevMaid(bool camChange) { CM3D2.VibeYourMaid.Plugin.API.vymPrevMaid(camChange); }
        private void vymNextMaid(bool camChange) { CM3D2.VibeYourMaid.Plugin.API.vymNextMaid(camChange); }
		private void vymLeftMaid(bool camChange) { CM3D2.VibeYourMaid.Plugin.API.vymLeftMaid(camChange); }
        private void vymRightMaid(bool camChange) { CM3D2.VibeYourMaid.Plugin.API.vymRightMaid(camChange); }
        private void vymMaidLink(string type) { CM3D2.VibeYourMaid.Plugin.API.vymMaidLink(type); }
		private void vymManVisible(int manVisible) { CM3D2.VibeYourMaid.Plugin.API.vymManVisible(manVisible); }

		private void vymCamMove(Vector3 move, bool useMoveValue) { CM3D2.VibeYourMaid.Plugin.API.vymCamMove(move, useMoveValue); }
		private void vymCamDistance(float distance, int moveType) { CM3D2.VibeYourMaid.Plugin.API.vymCamDistance(distance, moveType); }
		private void vymCamAround(int aroundType, float angleOffset) { CM3D2.VibeYourMaid.Plugin.API.vymCamAround(aroundType, angleOffset); }
		private void vymCamTarget(float distance, int target, bool backword) { CM3D2.VibeYourMaid.Plugin.API.vymCamTarget(distance, target, backword); }
		private void vymCamManHead(int manID, int target) { CM3D2.VibeYourMaid.Plugin.API.vymCamManHead(manID, target); }
		private void vymLookPoint(int fpsMod, int follow, int lookPoint) { CM3D2.VibeYourMaid.Plugin.API.vymLookPoint(fpsMod, follow, lookPoint); }

		private void vymBoneHitHeight(float height, bool offset) { CM3D2.VibeYourMaid.Plugin.API.vymBoneHitHeight(height, offset); }
		private void vymExcite(int excite, bool offset) { CM3D2.VibeYourMaid.Plugin.API.vymExcite(excite, offset); }
		private void vymSyasei(int man, bool lockMode) { CM3D2.VibeYourMaid.Plugin.API.vymSyasei(man, lockMode); }
		private void vymFukitori(int mode) { CM3D2.VibeYourMaid.Plugin.API.vymFukitori(mode); }
        private void vymStartNyo(int linkType) { CM3D2.VibeYourMaid.Plugin.API.vymStartNyo(linkType); }
        private void vymStartSio(int linkType) { CM3D2.VibeYourMaid.Plugin.API.vymStartSio(linkType); }

		private void vymVibe(int level) { CM3D2.VibeYourMaid.Plugin.API.vymVibe(level, cfg.vymVibeLinkType); }
		private void vymVibeAuto(int auto, bool offset) { CM3D2.VibeYourMaid.Plugin.API.vymVibeAuto(auto, offset, cfg.vymVibeLinkType); }
        private void vymKupa(float kupa, bool offset) { CM3D2.VibeYourMaid.Plugin.API.vymKupa(kupa, offset); }
        private void vymAnal(float kupa, bool offset) { CM3D2.VibeYourMaid.Plugin.API.vymAnal(kupa, offset); }
        private void vymBokki(float bokki, bool offset) { CM3D2.VibeYourMaid.Plugin.API.vymBokki(bokki, offset); }

		private void vymUnzip(string motion) { CM3D2.VibeYourMaid.Plugin.API.vymUnzip(motion); }
		private void vymUnzipDerive(string[] derives) { CM3D2.VibeYourMaid.Plugin.API.vymUnzipDerive(derives); }
		private void vymUnzipRandom(string emsFile) { CM3D2.VibeYourMaid.Plugin.API.vymUnzipRandom(emsFile); }
		private void vymUnzipChange(int changeTYpe) { CM3D2.VibeYourMaid.Plugin.API.vymUnzipChange(changeTYpe); }

        private void vymRemoveMotion(bool syasei) { CM3D2.VibeYourMaid.Plugin.API.vymRemoveMotion(syasei); }
        private void vymInsertMotion() { CM3D2.VibeYourMaid.Plugin.API.vymInsertMotion(); }
        private void vymSumataMotion() { CM3D2.VibeYourMaid.Plugin.API.vymSumataMotion(); }
		private void vymAnalMode(bool analMode) { CM3D2.VibeYourMaid.Plugin.API.vymAnalMode(analMode); }

		private void vymKissVoiceSet(string voiceFile) { CM3D2.VibeYourMaid.Plugin.API.vymKissVoiceSet(voiceFile); }

		#endregion

		////////////////////////////////////////////////////////////////
		// 目線 顔向き
		#region Face

		//対象になるメイドを取得 顔向き用
		private List<Maid> getFaceMaidList()
		{
			if (vymType != null) {
				return vymGetMaidList(cfg.vymEyeFaceLinkType);
			}
			return GameMain.Instance.CharacterMgr.GetStockMaidList();
		}

		private void setFaceKeywoards()
		{
			foreach (string keyword in voiceCfg.eyeToCam) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice eyeToCam : "+k);
					voiceEyeToCam(true);
				});
			}
			foreach (string keyword in voiceCfg.eyeToFront) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice eyeToFront : "+k);
					voiceEyeToCam(false);
				});
			}
			foreach (string keyword in voiceCfg.eyeSorashi) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice eyeSorashi : "+k);
					voiceEyeSorashi();
				});
			}
			foreach (string keyword in voiceCfg.headToCam) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice headToCam : "+k);
					voiceHeadToCam(true);
				});
			}
			foreach (string keyword in voiceCfg.headToFront) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice headToFront : "+k);
					voiceHeadToCam(false);
				});
			}
			foreach (string keyword in voiceCfg.headSorashi) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice headSorashi : "+k);
					voiceHeadSorashi();
				});
			}
		}

		//こっち見て 目だけ動かす
		public void voiceEyeToCam(bool enabled)
		{
			//中央のメイド取得

			//すべてのメイド（仮）
			foreach (Maid maid in getFaceMaidList()) {
				if (maid.Visible) {
					if (enabled) {
						if (maid.body0.boHeadToCam) maid.EyeToCamera(Maid.EyeMoveType.目と顔を向ける, 1.0f);
						else maid.EyeToCamera(Maid.EyeMoveType.目だけ向ける, 1.0f);
					}
					maid.body0.boEyeToCam = enabled;
				}
			}			
		}
		//こっち向いて 目モ向ける そらすときは顔だけ
		public void voiceHeadToCam(bool enabled)
		{
			//中央のメイド取得

			//すべてのメイド（仮）
			foreach (Maid maid in getFaceMaidList()) {
				if (maid.Visible) {
					maid.EyeToCamera(Maid.EyeMoveType.目と顔を向ける, 1.0f);
					maid.body0.boHeadToCam = enabled;
				}
			}			
		}
		//目をそらす
		public void voiceEyeSorashi()
		{
			//中央のメイド取得

			//すべてのメイド（仮）
			foreach (Maid maid in getFaceMaidList()) {
				if (maid.Visible) {
					maid.EyeToCamera(Maid.EyeMoveType.目だけそらす, 1.0f);
				}
			}			
		}
		//顔をそらす
		public void voiceHeadSorashi()
		{
			//中央のメイド取得

			//すべてのメイド（仮）
			foreach (Maid maid in getFaceMaidList()) {
				if (maid.Visible) {
					maid.EyeToCamera(Maid.EyeMoveType.顔をそらす, 1.0f);
				}
			}			
		}

		#endregion

		////////////////////////////////////////////////////////////////
		// 脱衣関連
		#region Undressing

		//対象になるメイドを取得 脱衣用
		private List<Maid> getUndressMaidList()
		{
			if (vymType != null) {
				return vymGetMaidList(cfg.vymUndressLinkType);
			}
			return GameMain.Instance.CharacterMgr.GetStockMaidList();
		}

		//音声キーワード設定
		private void setUndressKeywoards()
		{
			//動作確認が面倒なのでInvokeは使わない
			//setVoiceKeywordInvoke(voiceCfg.dressAll, "dressAll");
			//setVoiceKeywordInvoke(voiceCfg.undressAll, "undressAll");

			//メイド脱衣
			foreach (string keyword in voiceCfg.dressAll) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice dressAll : "+k);
					dressAll();
				});
			}
			foreach (string keyword in voiceCfg.undressAll) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice undressAll : "+k);
					undressAll();
				});
			}
			foreach (string keyword in voiceCfg.undressWear) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice undressWear : "+k);
					undressWear();
				});
			}
			foreach (string keyword in voiceCfg.undressTop) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice undressTop : "+k);
					undressTop();
				});
			}
			foreach (string keyword in voiceCfg.dressBottom) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice dressBottom : "+k);
					dressBottom();
				});
			}
			foreach (string keyword in voiceCfg.undressBottom) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice undressBottom : "+k);
					undressBottom();
				});
			}
			foreach (string keyword in voiceCfg.dressMizugi) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice dressMizugi : "+k);
					dressMizugi();
				});
			}
			foreach (string keyword in voiceCfg.undressMizugi) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice undressMizugi : "+k);
					undressMizugi();
				});
			}
			foreach (string keyword in voiceCfg.dressUnderWear) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice dressUnderWear : "+k);
					dressUnderWear();
				});
			}
			foreach (string keyword in voiceCfg.undressUnderWear) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice undressUnderWear : "+k);
					undressUnderWear();
				});
			}
			foreach (string keyword in voiceCfg.dressBra) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice dressBra : "+k);
					dressBra();
				});
			}
			foreach (string keyword in voiceCfg.undressBra) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice undressBra : "+k);
					undressBra();
				});
			}
			foreach (string keyword in voiceCfg.dressPants) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice dressPants : "+k);
					dressPants();
				});
			}
			foreach (string keyword in voiceCfg.undressPants) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice undressPants : "+k);
					undressPants();
				});
			}
			foreach (string keyword in voiceCfg.dressShoes) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice dressShoes : "+k);
					dressShoes();
				});
			}
			foreach (string keyword in voiceCfg.undressShoes) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice undressShoes : "+k);
					undressShoes();
				});
			}
			foreach (string keyword in voiceCfg.dressAcc) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice dressAcc : "+k);
					dressAcc();
				});
			}
			foreach (string keyword in voiceCfg.undressAcc) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice undressAcc : "+k);
					undressAcc();
				});
			}

			//めくれ・ずらし・ぽろり
			foreach (string keyword in voiceCfg.modoshiPants) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice modoshiPants : "+k);
					modoshiPants();
				});
			}
			foreach (string keyword in voiceCfg.zurashiPants) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice zurashiPants : "+k);
					zurashiPants();
				});
			}

			foreach (string keyword in voiceCfg.mekureFront) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice mekureFront : "+k);
					mekureFront();
				});
			}
			foreach (string keyword in voiceCfg.mekureBack) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice mekureBack : "+k);
					mekureBack();
				});
			}
			foreach (string keyword in voiceCfg.mekureReset) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice mekureReset : "+k);
					mekureReset();
				});
			}

			foreach (string keyword in voiceCfg.pororiTop) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice pororiTop : "+k);
					pororiTop();
				});
			}
			foreach (string keyword in voiceCfg.pororiTopReset) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice pororiTopReset : "+k);
					pororiTopReset();
				});
			}
			foreach (string keyword in voiceCfg.pororiBottom) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice pororiBottom : "+k);
					pororiBottom();
				});
			}
			foreach (string keyword in voiceCfg.pororiBottomReset) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice pororiBottomReset : "+k);
					pororiBottomReset();
				});
			}

			//竿表示
			foreach (string keyword in voiceCfg.manShowChinko) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice manShowChinko : "+k);
					manChinkoVisible(true);
				});
			}
			foreach (string keyword in voiceCfg.manHideChinko) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice manHideChinko : "+k);
					manChinkoVisible(false);
				});
			}

			//男脱衣
			#if COM3D2_5
			foreach (string keyword in voiceCfg.manDressAll) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice manDressAll : "+k);
					manDressAll();
				});
			}
			foreach (string keyword in voiceCfg.manUndressAll) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice manUndressAll : "+k);
					manUndressAll();
				});
			}
			foreach (string keyword in voiceCfg.manUndressWear) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice manUndressWear : "+k);
					manUndressWear();
				});
			}
			foreach (string keyword in voiceCfg.manDressPants) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice manDressPants : "+k);
					manDressPants();
				});
			}
			foreach (string keyword in voiceCfg.manUndressPants) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice manUndressPants : "+k);
					manUndressPants();
				});
			}
			foreach (string keyword in voiceCfg.manDressShoes) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice manDressShoes : "+k);
					manDressShoes();
				});
			}
			foreach (string keyword in voiceCfg.manUndressShoes) {
				if (keywords.ContainsKey(keyword)) continue;
				string k = keyword; //スコープ対策
				keywords.Add(keyword, () => {
					Debug.Log("Voice manUndressShoes : "+k);
					manUndressShoes();
				});
			}
			#endif

			//Prop変更
			foreach (PropVoiceInfo propVoice in voiceCfg.propVoiceList) {
				PropInfo[] props = propVoice.props;
				MaskInfo[] masks = propVoice.masks;
				foreach (string keyword in propVoice.voice) {
					if (keywords.ContainsKey(keyword)) continue;
					string k = keyword; //スコープ対策
					keywords.Add(keyword, () => {
						Debug.Log("setPropMask : "+k);
						setPropMask(props, masks);
					});
				}
			}

		}

		//UnitType { 全脱衣, 全着衣, トップス, ボトムス, ブラジャー, パンツ, ソックス, シューズ, ヘッドドレス, メガネ, 背中, 手袋, Max }
		//夜伽シーンの脱衣パネル取得
		private UndressingManager getUndressingManager()
		{
			//無効設定時
			if (!cfg.useUndressManager) return null;

			if (YotogiManager.instans && YotogiManager.instans.play_mgr && YotogiManager.instans.play_mgr.playingSkill != null) {
				GameObject obj = GameObject.Find("UI Root/YotogiPlayPanel/UndressingViewer/UndressingViewer/MaskGroup/UndressParent");
				if (obj) return obj.GetComponent<UndressingManager>();
			}
			if (YotogiOldManager.instans && YotogiOldManager.instans.play_mgr) {
				GameObject obj = GameObject.Find("UI Root/YotogiPlayPanel/UndressingViewer/UndressingViewer/MaskGroup/UndressParent");
				if (obj) return obj.GetComponent<UndressingManager>();
			}
			return null;
		}

		private void setMaskWear(Maid maid, bool mask)
		{
			maid.body0.SetMask(TBody.SlotID.wear, mask);
			maid.body0.SetMask(TBody.SlotID.onepiece, mask);
			maid.body0.SetMask(TBody.SlotID.skirt, mask);
			maid.body0.SetMask(TBody.SlotID.shoes, mask);
			maid.body0.SetMask(TBody.SlotID.accSenaka, mask);
			#if COM3D2_5
			maid.body0.SetMask(TBody.SlotID.jacket, mask);
			maid.body0.SetMask(TBody.SlotID.vest, mask);
			maid.body0.SetMask(TBody.SlotID.shirt, mask);
			#endif
		}
		private void setMaskUnderware(Maid maid, bool mask)
		{
			maid.body0.SetMask(TBody.SlotID.mizugi, mask);
			maid.body0.SetMask(TBody.SlotID.bra, mask);
			maid.body0.SetMask(TBody.SlotID.panz, mask);
			#if COM3D2_5
			maid.body0.SetMask(TBody.SlotID.mizugi_top, mask);
			maid.body0.SetMask(TBody.SlotID.mizugi_buttom, mask);
			maid.body0.SetMask(TBody.SlotID.slip, mask);
			#endif
		}
		private void setMaskOther(Maid maid, bool mask)
		{
			maid.body0.SetMask(TBody.SlotID.stkg, mask); //靴下
			maid.body0.SetMask(TBody.SlotID.glove, mask);
			maid.body0.SetMask(TBody.SlotID.accUde, mask);
			maid.body0.SetMask(TBody.SlotID.accKubi, mask);
		}
		private void setMaskAcc(Maid maid, bool mask)
		{
			maid.body0.SetMask(TBody.SlotID.headset, mask);
			maid.body0.SetMask(TBody.SlotID.accHat, mask);
			maid.body0.SetMask(TBody.SlotID.accHead, mask);
			maid.body0.SetMask(TBody.SlotID.accKubiwa, mask);
			maid.body0.SetMask(TBody.SlotID.accMiMiR, mask);
			maid.body0.SetMask(TBody.SlotID.accMiMiL, mask);
			maid.body0.SetMask(TBody.SlotID.megane, mask);
			maid.body0.SetMask(TBody.SlotID.accXXX, mask);
			maid.body0.SetMask(TBody.SlotID.accAshi, mask);
			maid.body0.SetMask(TBody.SlotID.accShippo, mask);
			maid.body0.SetMask(TBody.SlotID.accHana, mask);
			maid.body0.SetMask(TBody.SlotID.accNipR, mask);
			maid.body0.SetMask(TBody.SlotID.accNipL, mask);
			maid.body0.SetMask(TBody.SlotID.accHeso, mask);
		}

		//全部着る
		public void dressAll()
		{
			UndressingManager undressManager = getUndressingManager();
			if (undressManager != null) {
				undressManager.SetMaskMode(UndressingManager.UnitType.全着衣, UndressingManager.MaskStatus.On); //Onで実行
			} else {
				foreach (Maid maid in getUndressMaidList()) {
					setMaskWear(maid, true);
					setMaskUnderware(maid, true);
					setMaskOther(maid, true);
				}
			}
		}
		//全部脱がす
		public void undressAll()
		{
			UndressingManager undressManager = getUndressingManager();
			if (undressManager != null) {
				undressManager.SetMaskMode(UndressingManager.UnitType.全脱衣, UndressingManager.MaskStatus.On); //Onで実行
			} else {
				foreach (Maid maid in getUndressMaidList()) {
					if (maid.Visible) {
						setMaskWear(maid, false);
						setMaskUnderware(maid, false);
						setMaskOther(maid, false);
					}
				}
			}
		}

		//服だけ脱がす
		public void undressWear()
		{
			UndressingManager undressManager = getUndressingManager();
			if (undressManager != null) {
				undressManager.SetMaskMode(UndressingManager.UnitType.トップス, UndressingManager.MaskStatus.On); //Onが非表示
				undressManager.SetMaskMode(UndressingManager.UnitType.ボトムス, UndressingManager.MaskStatus.On); //Onが非表示
				undressManager.SetMaskMode(UndressingManager.UnitType.ブラジャー, UndressingManager.MaskStatus.On); //Onが非表示
				undressManager.SetMaskMode(UndressingManager.UnitType.パンツ, UndressingManager.MaskStatus.On); //Onが非表示
			} else {
				foreach (Maid maid in getUndressMaidList()) {
					if (maid.Visible) {
						setMaskWear(maid, false);
					}
				}
			}
		}
		//服の上だけ着せる
		public void dressTop()
		{
			UndressingManager undressManager = getUndressingManager();
			if (undressManager != null) {
				undressManager.SetMaskMode(UndressingManager.UnitType.トップス, UndressingManager.MaskStatus.Off); //Offが表示
			} else {
				foreach (Maid maid in getUndressMaidList()) {
					if (maid.Visible) {
						maid.body0.SetMask(TBody.SlotID.wear, true);
						maid.body0.SetMask(TBody.SlotID.onepiece, true);
		                maid.body0.SetMask(TBody.SlotID.accSenaka, true);
					}
				}
			}
		}
		//服の上だけ脱がす
		public void undressTop()
		{
			UndressingManager undressManager = getUndressingManager();
			if (undressManager != null) {
				undressManager.SetMaskMode(UndressingManager.UnitType.トップス, UndressingManager.MaskStatus.On); //Onが非表示
			} else {
				foreach (Maid maid in getUndressMaidList()) {
					if (maid.Visible) {
						maid.body0.SetMask(TBody.SlotID.wear, false);
						maid.body0.SetMask(TBody.SlotID.onepiece, false);
		                maid.body0.SetMask(TBody.SlotID.accSenaka, false);
					}
				}
			}
		}
		//服の下だけ着せる
		public void dressBottom()
		{
			UndressingManager undressManager = getUndressingManager();
			if (undressManager != null) {
				undressManager.SetMaskMode(UndressingManager.UnitType.ボトムス, UndressingManager.MaskStatus.Off); //Offが表示
			} else {
				foreach (Maid maid in getUndressMaidList()) {
					if (maid.Visible) {
						maid.body0.SetMask(TBody.SlotID.skirt, true);
					}
				}
			}
		}
		//服の下だけ脱がす
		public void undressBottom()
		{
			UndressingManager undressManager = getUndressingManager();
			if (undressManager != null) {
				undressManager.SetMaskMode(UndressingManager.UnitType.ボトムス, UndressingManager.MaskStatus.On); //Onが非表示
			} else {
				foreach (Maid maid in getUndressMaidList()) {
					if (maid.Visible) {
						maid.body0.SetMask(TBody.SlotID.skirt, false);
					}
				}
			}
		}

		//水着着て
		public void dressMizugi()
		{
			UndressingManager undressManager = getUndressingManager();
			if (undressManager != null) {
				undressManager.SetMaskMode(UndressingManager.UnitType.トップス, UndressingManager.MaskStatus.Off); //Offが表示
			} else {
				foreach (Maid maid in getUndressMaidList()) {
					if (maid.Visible) {
						maid.body0.SetMask(TBody.SlotID.mizugi, true);
					}
				}
			}
		}
		//水着脱いで
		public void undressMizugi()
		{
			UndressingManager undressManager = getUndressingManager();
			if (undressManager != null) {
				undressManager.SetMaskMode(UndressingManager.UnitType.トップス, UndressingManager.MaskStatus.On); //Onが非表示
			} else {
				foreach (Maid maid in getUndressMaidList()) {
					if (maid.Visible) {
						maid.body0.SetMask(TBody.SlotID.mizugi, false);
					}
				}
			}
		}

		//下着つけて 夜伽以外は水着も含む
		public void dressUnderWear()
		{
			UndressingManager undressManager = getUndressingManager();
			if (undressManager != null) {
				undressManager.SetMaskMode(UndressingManager.UnitType.ブラジャー, UndressingManager.MaskStatus.Off); //Offが表示
				undressManager.SetMaskMode(UndressingManager.UnitType.パンツ, UndressingManager.MaskStatus.Off); //Offが表示
			} else {
				foreach (Maid maid in getUndressMaidList()) {
					if (maid.Visible) {
						setMaskUnderware(maid, true);
					}
				}
			}
		}
		//下着脱いで 夜伽以外は水着も含む
		public void undressUnderWear()
		{
			UndressingManager undressManager = getUndressingManager();
			if (undressManager != null) {
				undressManager.SetMaskMode(UndressingManager.UnitType.ブラジャー, UndressingManager.MaskStatus.On); //Onが非表示
				undressManager.SetMaskMode(UndressingManager.UnitType.パンツ, UndressingManager.MaskStatus.On); //Onが非表示
			} else {
				foreach (Maid maid in getUndressMaidList()) {
					if (maid.Visible) {
						setMaskUnderware(maid, false);
					}
				}
			}
		}
		//ブラつけて
		public void dressBra()
		{
			UndressingManager undressManager = getUndressingManager();
			if (undressManager != null) {
				undressManager.SetMaskMode(UndressingManager.UnitType.ブラジャー, UndressingManager.MaskStatus.Off); //Offが表示
			} else {
				foreach (Maid maid in getUndressMaidList()) {
					if (maid.Visible) {
						maid.body0.SetMask(TBody.SlotID.bra, true);
					}
				}
			}
		}
		//ブラ外して
		public void undressBra()
		{
			UndressingManager undressManager = getUndressingManager();
			if (undressManager != null) {
				undressManager.SetMaskMode(UndressingManager.UnitType.ブラジャー, UndressingManager.MaskStatus.On); //Onが非表示
			} else {
				foreach (Maid maid in getUndressMaidList()) {
					if (maid.Visible) {
						maid.body0.SetMask(TBody.SlotID.bra, false);
					}
				}
			}
		}
		//パンツ履いて
		public void dressPants()
		{
			UndressingManager undressManager = getUndressingManager();
			if (undressManager != null) {
				undressManager.SetMaskMode(UndressingManager.UnitType.パンツ, UndressingManager.MaskStatus.Off); //Offが表示
			} else {
				foreach (Maid maid in getUndressMaidList()) {
					if (maid.Visible) {
						maid.body0.SetMask(TBody.SlotID.panz, true);
					}
				}
			}
		}
		//パンツ脱いで
		public void undressPants()
		{
			UndressingManager undressManager = getUndressingManager();
			if (undressManager != null) {
				undressManager.SetMaskMode(UndressingManager.UnitType.パンツ, UndressingManager.MaskStatus.On); //Onが非表示
			} else {
				foreach (Maid maid in getUndressMaidList()) {
					if (maid.Visible) {
						maid.body0.SetMask(TBody.SlotID.panz, false);
					}
				}
			}
		}
		//靴履いて
		public void dressShoes()
		{
			UndressingManager undressManager = getUndressingManager();
			if (undressManager != null) {
				undressManager.SetMaskMode(UndressingManager.UnitType.シューズ, UndressingManager.MaskStatus.Off); //Offが表示
			} else {
				foreach (Maid maid in getUndressMaidList()) {
					if (maid.Visible) {
						maid.body0.SetMask(TBody.SlotID.shoes, true);
					}
				}
			}
		}
		//靴脱いで
		public void undressShoes()
		{
			UndressingManager undressManager = getUndressingManager();
			if (undressManager != null) {
				undressManager.SetMaskMode(UndressingManager.UnitType.シューズ, UndressingManager.MaskStatus.On); //Onが非表示
			} else {
				foreach (Maid maid in getUndressMaidList()) {
					if (maid.Visible) {
						maid.body0.SetMask(TBody.SlotID.shoes, false);
					}
				}
			}
		}
		//アクセをすべてつける
		public void dressAcc()
		{
			UndressingManager undressManager = getUndressingManager();
			if (undressManager != null) {
				undressManager.SetMaskMode(UndressingManager.UnitType.ヘッドドレス, UndressingManager.MaskStatus.Off); //Offが表示
				undressManager.SetMaskMode(UndressingManager.UnitType.メガネ, UndressingManager.MaskStatus.Off); //Offが表示
			}
			//その他のアクセもマスク設定
			foreach (Maid maid in getUndressMaidList()) {
				if (maid.Visible) {
					setMaskAcc(maid, true);
				}
			}
		}
		//アクセをすべて外す
		public void undressAcc()
		{
			UndressingManager undressManager = getUndressingManager();
			if (undressManager != null) {
				undressManager.SetMaskMode(UndressingManager.UnitType.ヘッドドレス, UndressingManager.MaskStatus.On); //Onが非表示
				undressManager.SetMaskMode(UndressingManager.UnitType.メガネ, UndressingManager.MaskStatus.On); //Onが非表示
			}
			//その他のアクセもマスク設定
			foreach (Maid maid in getUndressMaidList()) {
				if (maid.Visible) {
					setMaskAcc(maid, false);
				}
			}
		}

		//パンツ戻す
		public void modoshiPants()
		{
			foreach (Maid maid in getUndressMaidList()) {
				if (maid.Visible) modoshiPants(maid);
			}
		}
		public void modoshiPants(Maid maid)
		{
			if (maid.mekureController.IsSupportedCostumeType(MaidExtension.MaidCostumeChangeController.CostumeType.Zurasi)) {
				maid.mekureController.SetEnabledCostumeType(MaidExtension.MaidCostumeChangeController.CostumeType.Zurasi, false, null);
			}
		}
		//パンツずらす
		public void zurashiPants()
		{
			foreach (Maid maid in getUndressMaidList()) {
				if (maid.Visible) zurashiPants(maid);
			}
		}
		public void zurashiPants(Maid maid)
		{
			if (maid.mekureController.IsSupportedCostumeType(MaidExtension.MaidCostumeChangeController.CostumeType.Zurasi)) {
				maid.mekureController.SetEnabledCostumeType(MaidExtension.MaidCostumeChangeController.CostumeType.Zurasi, true, null);
			}
		}

		//めくれ戻す
		public void mekureReset()
		{
			foreach (Maid maid in getUndressMaidList()) {
				if (maid.Visible) mekureReset(maid);
			}
		}
		public void mekureReset(Maid maid)
		{
			if (maid.mekureController.IsSupportedCostumeType(MaidExtension.MaidCostumeChangeController.CostumeType.MekureFront)) {
				maid.mekureController.SetEnabledCostumeType(MaidExtension.MaidCostumeChangeController.CostumeType.MekureFront, false, null);
			}
			if (maid.mekureController.IsSupportedCostumeType(MaidExtension.MaidCostumeChangeController.CostumeType.MekureBack)) {
				maid.mekureController.SetEnabledCostumeType(MaidExtension.MaidCostumeChangeController.CostumeType.MekureBack, false, null);
			}
		}

		//めくれ
		public void mekureFront()
		{
			foreach (Maid maid in getUndressMaidList()) {
				if (maid.Visible) mekureFront(maid);
			}
		}
		public void mekureBack()
		{
			foreach (Maid maid in getUndressMaidList()) {
				if (maid.Visible) mekureBack(maid);
			}
		}
		public void mekureFront(Maid maid)
		{
			if (maid.mekureController.IsSupportedCostumeType(MaidExtension.MaidCostumeChangeController.CostumeType.MekureFront)) {
				bool flag = maid.mekureController.IsEnabledCostumeType(MaidExtension.MaidCostumeChangeController.CostumeType.MekureFront);
				if (!flag) {
					maid.mekureController.SetEnabledCostumeType(MaidExtension.MaidCostumeChangeController.CostumeType.MekureFront, true, null);
					maid.mekureController.SetEnabledCostumeType(MaidExtension.MaidCostumeChangeController.CostumeType.MekureBack, false, null);
				}
			}
		}
		public void mekureBack(Maid maid)
		{
			if (maid.mekureController.IsSupportedCostumeType(MaidExtension.MaidCostumeChangeController.CostumeType.MekureBack)) {
				bool flag = maid.mekureController.IsEnabledCostumeType(MaidExtension.MaidCostumeChangeController.CostumeType.MekureBack);
				if (!flag) {
					maid.mekureController.SetEnabledCostumeType(MaidExtension.MaidCostumeChangeController.CostumeType.MekureBack, true, null);
					maid.mekureController.SetEnabledCostumeType(MaidExtension.MaidCostumeChangeController.CostumeType.MekureFront, false, null);
				}
			}
		}

		//ぽろり上 CRCボディのはだけにも対応
		public void pororiTop()
		{
			foreach (Maid maid in getUndressMaidList()) {
				if (maid.Visible) pororiTop(maid);
			}
		}
		public void pororiBottom()
		{
			foreach (Maid maid in getUndressMaidList()) {
				if (maid.Visible) pororiBottom(maid);
			}
		}
		public void pororiTopReset()
		{
			foreach (Maid maid in getUndressMaidList()) {
				if (maid.Visible) pororiTopReset(maid);
			}
		}
		public void pororiBottomReset()
		{
			foreach (Maid maid in getUndressMaidList()) {
				if (maid.Visible) {
					if (PororiOff(maid, "panz",  MPN.panz)) maid.AllProcPropSeqStart();
				}
			}
		}
		public void pororiTop(Maid maid)
		{
			#if COM3D2_5
			//はだけ
			if (maid.IsCrcBody) {
				if (maid.mekureController.IsSupportedCostumeType(MaidExtension.MaidCostumeChangeController.CostumeType.Hadake)) {
					maid.mekureController.SetEnabledCostumeType(MaidExtension.MaidCostumeChangeController.CostumeType.Hadake, true, null);
				}
				return;
			}
			#endif

			//ぽろり上を実行 上着とブラは段階的に実行
			PororiTop(maid);
		}
		public void pororiTopReset(Maid maid)
		{
			#if COM3D2_5
			if (maid.IsCrcBody) {
				if (maid.mekureController.IsSupportedCostumeType(MaidExtension.MaidCostumeChangeController.CostumeType.Hadake)) {
					maid.mekureController.SetEnabledCostumeType(MaidExtension.MaidCostumeChangeController.CostumeType.Hadake, false, null);
				}
				return;
			}
			#endif
			bool changed = false;
			if (PororiOff(maid, "wear", MPN.wear)) changed = true;
			if (PororiOff(maid, "onepiece", MPN.onepiece)) changed = true;
			if (PororiOff(maid, "mizugi", MPN.mizugi)) changed = true;
			if (PororiOff(maid, "bra", MPN.bra)) changed = true;
			if (changed) maid.AllProcPropSeqStart();
		}

		public void pororiBottom(Maid maid)
		{
			PororiChange(maid, "panz", TBody.SlotID.panz, MPN.panz);
		}

		private bool isPorori(Maid maid, string name)
		{
			MaidProp prop = maid.GetProp(name);
			return prop.strFileName.Contains("_porori") || prop.strTempFileName.Contains("_porori");
		}

		//ぽろり上 上着とブラで2段階で切り替え
		private bool PororiTop(Maid maid)
		{
			if (!isPorori(maid, "wear")) {
				if (PororiChange(maid, "wear", TBody.SlotID.wear, MPN.wear)) return true;
			}
			if (!isPorori(maid, "onepiece")) {
				if (PororiChange(maid, "onepiece", TBody.SlotID.onepiece, MPN.onepiece)) return true;
			}
			if (!isPorori(maid, "mizugi")) {
				if (PororiChange(maid, "mizugi", TBody.SlotID.mizugi, MPN.mizugi)) return true;
			}
			if (!isPorori(maid, "bra")) {
				if (PororiChange(maid, "bra", TBody.SlotID.bra, MPN.bra)) return true;
			}
			return false;
		}

		//ぽろり処理 ぽろししたらtrue
		private bool PororiChange(Maid maid, string name, TBody.SlotID slotID, MPN mpn)
		{
			//非表示なら処理しない
			if (!maid.body0.GetSlotVisible(slotID)) return false;

			MaidProp prop = maid.GetProp(mpn);
			if (prop.nFileNameRID != 0 && !prop.strFileName.Contains("_porori")) { //本衣装でぽろりし無し
				if (ItemChangeTempSuffix(maid, name, "_porori", false)) {
					maid.AllProcPropSeqStart();
					return true;
				}
			} else if (prop.nTempFileNameRID != 0 && !prop.strTempFileName.Contains("_porori")) { //仮衣装でぽろり無し
				if (ItemChangeTempSuffix(maid, name, "_porori", true)) {
					maid.AllProcPropSeqStart();
					return true;
				}
			}
			return false;
		}
		//ぽろりを戻す
        private bool PororiOff(Maid maid, string name, MPN mpn)
        {
			MaidProp prop = maid.GetProp(name);
			if (prop.strFileName.Contains("_porori")) {
				maid.SetProp(mpn, maid.GetProp(mpn).strFileName.Replace("_porori", ""), 0, true, false);
				return true;
			} else if (prop.strTempFileName.Contains("_porori")) {
				maid.SetProp(mpn, maid.GetProp(mpn).strTempFileName.Replace("_porori", ""), 0, true, false);
				return true;
			}
			return false;
        }
		//メニューファイル名変更処理
        private bool ItemChangeTempSuffix(Maid maid, string mpn, string suffix, bool temp)
		{
          MaidProp prop = maid.GetProp(mpn);
          string filename = temp ?  prop.strTempFileName : prop.strFileName;
          filename = filename.Replace(suffix, "").Replace(".menu", suffix+".menu");
          //menuファイルがあれば変更
          if (GameUty.IsExistFile(filename, null)) {
            maid.SetProp(mpn, filename, 0, true, false);
            return true;
          }
          //ずらし等を解除してからmenuファイルがあるかチェック
          filename = temp ?  prop.strTempFileName : prop.strFileName;
          filename = filename.Replace("_zurashi", "").Replace("_mekure_back", "").Replace("_mekure", "");
          filename = filename.Replace(suffix, "").Replace(".menu", suffix+".menu");
          if (GameUty.IsExistFile(filename, null)) {
            maid.SetProp(mpn, filename, 0, true, false);
            return true;
          }
          return false;
        }


		//指定したMPNにアイテムをセット & スロット名でマスク
		private void setPropMask(PropInfo[] props, MaskInfo[] masks)
		{
			foreach (Maid maid in getUndressMaidList()) {
				if (maid.Visible) setPropMask(maid, props, masks);
			}
		}
		private void setPropMask(Maid maid, PropInfo[] props, MaskInfo[] masks)
		{
			if (props != null) { 
				foreach (PropInfo propInfo in props) {
					try {
						MPN mpn = (MPN)Enum.Parse(typeof(MPN), propInfo.mpn);
						if (propInfo.filename == null) {
							maid.ResetProp(propInfo.mpn, false);
						} else {
							//maid.SetProp(propInfo.mpn, propInfo.filename, propInfo.rid, propInfo.temp, propInfo.noscale);
							maid.SetProp(propInfo.mpn, propInfo.filename, 0, propInfo.temp, false);
						}
					} catch {
						Debug.LogError("指定されたプロパティ名がありません。" + propInfo.mpn);
					}
				}
				maid.AllProcPropSeqStart();
			}
			if (masks != null) {
				foreach (MaskInfo maskInfo in masks) {
					if (TBody.hashSlotName.ContainsKey(maskInfo.slot)) {
						int num = (int)TBody.hashSlotName[maskInfo.slot];
						if (maskInfo.visible != null) {
							bool mask = maskInfo.visible != "false";
							maid.body0.SetMask((TBody.SlotID)num, mask);
							if (maskInfo.visible == "force") maid.body0.goSlot[num].boVisible = true;
						}
					}
				}
			}
		}


		//竿表示
		public void manChinkoVisible(bool visible)
		{
			for (int i=0; i<GameMain.Instance.CharacterMgr.GetStockManCount(); i++) {
				Maid man = GameMain.Instance.CharacterMgr.GetStockMan(i);
				if (man.Visible) {
					man.body0.SetChinkoVisible(visible);
				}
		        #if COM3D2_5
				if (man.HasNewRealMan && man.pairMan.Visible) {
					man.pairMan.body0.SetChinkoVisible(visible);
				}
				#endif
			}
		}

		//男着衣
        #if COM3D2_5
		public void manDressAll()
		{
			for (int i=0; i<GameMain.Instance.CharacterMgr.GetStockManCount(); i++) {
				Maid man = GameMain.Instance.CharacterMgr.GetStockMan(i);
				if (man.HasNewRealMan && man.pairMan.Visible) {
					setMaskWear(man.pairMan, true);
					setMaskUnderware(man.pairMan, true);
					setMaskOther(man.pairMan, true);
				}
			}
		}
		public void manUndressAll()
		{
			for (int i=0; i<GameMain.Instance.CharacterMgr.GetStockManCount(); i++) {
				Maid man = GameMain.Instance.CharacterMgr.GetStockMan(i);
				if (man.HasNewRealMan && man.pairMan.Visible) {
					setMaskWear(man.pairMan, false);
					setMaskUnderware(man.pairMan, false);
					setMaskOther(man.pairMan, false);
				}
			}
		}
		public void manUndressWear()
		{
			for (int i=0; i<GameMain.Instance.CharacterMgr.GetStockManCount(); i++) {
				Maid man = GameMain.Instance.CharacterMgr.GetStockMan(i);
				if (man.HasNewRealMan && man.pairMan.Visible) {
					setMaskWear(man.pairMan, false);
				}
			}
		}
		public void manDressPants()
		{
			for (int i=0; i<GameMain.Instance.CharacterMgr.GetStockManCount(); i++) {
				Maid man = GameMain.Instance.CharacterMgr.GetStockMan(i);
				if (man.HasNewRealMan && man.pairMan.Visible) {
					man.pairMan.body0.SetMask(TBody.SlotID.panz, true);
				}
			}
		}
		public void manUndressPants()
		{
			for (int i=0; i<GameMain.Instance.CharacterMgr.GetStockManCount(); i++) {
				Maid man = GameMain.Instance.CharacterMgr.GetStockMan(i);
				if (man.HasNewRealMan && man.pairMan.Visible) {
					man.pairMan.body0.SetMask(TBody.SlotID.panz, false);
				}
			}
		}
		public void manDressShoes()
		{
			for (int i=0; i<GameMain.Instance.CharacterMgr.GetStockManCount(); i++) {
				Maid man = GameMain.Instance.CharacterMgr.GetStockMan(i);
				if (man.HasNewRealMan && man.pairMan.Visible) {
					man.pairMan.body0.SetMask(TBody.SlotID.shoes, true);
				}
			}
		}
		public void manUndressShoes()
		{
			for (int i=0; i<GameMain.Instance.CharacterMgr.GetStockManCount(); i++) {
				Maid man = GameMain.Instance.CharacterMgr.GetStockMan(i);
				if (man.HasNewRealMan && man.pairMan.Visible) {
					man.pairMan.body0.SetMask(TBody.SlotID.shoes, false);
				}
			}
		}
		#endif

		#endregion

		////////////////////////////////////////////////////////////////
		// テンキーウィンドウ
		#region GUI

		//GUIのスタイル情報
		class GUIInfo
		{
			public Rect rect;

			public GUIStyle gsWin;
			public GUIStyle gsLabel;
			public GUIStyle gsButton;

			public Texture2D bgTexture; //背景色
			public Texture2D lineTexture;
		}

		//GUI初期化
		private void initGUI()
		{
			this.gui = new GUIInfo();

			int buttonSize = cfg.buttonSize;
			int buttonMargin = cfg.buttonSize + 4;

			//パネル原点 位置 サイズ
			//rect = new Rect(-50, 50, 540, 28);
			gui.rect = new Rect(-50, 50, 2+buttonMargin*3+4+buttonSize+4+2, buttonMargin*5);
			gui.rect.x = UnityEngine.Screen.width-gui.rect.width + gui.rect.x; //右から
			gui.rect.x = Math.Min(Math.Max(0, gui.rect.x), UnityEngine.Screen.width-gui.rect.width);
			gui.rect.y = Math.Min(Math.Max(0, gui.rect.y), UnityEngine.Screen.height-gui.rect.height);

			//メインウィンドウ
			gui.gsWin = new GUIStyle("box");
			gui.gsWin.fontSize = cfg.fontSize;
			gui.gsWin.alignment = TextAnchor.UpperLeft;

			//テクスチャ
			gui.bgTexture = new Texture2D(1, 1);
			gui.bgTexture.SetPixel(0, 0, cfg.color);
			gui.bgTexture.Apply();
			gui.lineTexture = new Texture2D(1, 1);
			gui.lineTexture.SetPixel(0, 0, new Color32(255, 255, 255, 160));
			gui.lineTexture.Apply();

			//背景色設定
			gui.gsWin.onHover.background = gui.bgTexture;
			gui.gsWin.hover.background = gui.bgTexture;
			gui.gsWin.onFocused.background = gui.bgTexture;
			gui.gsWin.focused.background = gui.bgTexture;
			gui.gsWin.onHover.textColor = Color.white;
			gui.gsWin.hover.textColor = Color.white;
			gui.gsWin.onFocused.textColor = Color.white;
			gui.gsWin.focused.textColor = Color.white;

			gui.gsLabel = new GUIStyle("label");
			gui.gsLabel.fontSize = cfg.fontSize;
			gui.gsLabel.alignment = TextAnchor.MiddleLeft;

			gui.gsButton = new GUIStyle("button");
			gui.gsButton.fontSize = cfg.fontSize;
			gui.gsButton.alignment = TextAnchor.MiddleCenter;

		}

		//UI生成
		void drawGUI(int id)
		{
			int buttonSize = cfg.buttonSize;
			int buttonMargin = cfg.buttonSize + 4;

			//上にライン
			GUI.DrawTexture(new Rect(2, 2, 8, buttonSize), gui.lineTexture, ScaleMode.StretchToFill, true, 0);
			
			if (GUI.Button(new Rect (gui.rect.width-21, 1, 20, 20), "×", gui.gsButton)) {
				toggleKeyboard();
				return;
			}

			int left = 2; //左余白
			int top = 2; //上余白
			int x = left;
			int y = top;
			//数字キー 0-9
			x = left + buttonMargin; //0の位置
			y = top + buttonMargin*4;
			for (int i=0; i<10; i++) {
				if (GUI.Button(new Rect (x, y, buttonSize, buttonSize), ""+i, gui.gsButton)) { sendKey((ushort)(0x30+i)); }
				if (i == 0 || i == 3 || i == 6) { x = 2; y -= buttonMargin; }
				else x += buttonMargin;
			}
			//マイナス
			x = left;
			y = top + buttonMargin*4;
			if (GUI.Button(new Rect (x, y, buttonSize, buttonSize), "-", gui.gsButton)) { sendKey(Keys.Subtract); }
			//.
			x = left + buttonMargin*2;
			y = top + buttonMargin*4;
			if (GUI.Button(new Rect (x, y, buttonSize, buttonSize), ".", gui.gsButton)) { sendKey(Keys.Decimal); }

			x = left + buttonMargin;
			y = top;
			if (GUI.Button(new Rect (x, y, buttonSize+2, buttonSize), "←", gui.gsButton)) { sendKey(Keys.Left); }
			x += buttonMargin;
			if (GUI.Button(new Rect (x, y, buttonSize+2, buttonSize), "→", gui.gsButton)) { sendKey(Keys.Right); }
			x = left + buttonMargin*3 + 4;
			y = top + buttonMargin;
			//BS
			if (GUI.Button(new Rect (x, y, buttonSize+6, buttonSize), "bs", gui.gsButton)) { sendKey(Keys.Back); }
			y += buttonMargin;
			//DEL
			if (GUI.Button(new Rect (x, y, buttonSize+6, buttonSize), "del", gui.gsButton)) {
				guiStopPropagation = false;
				Invoke("sendDelete", 0.1f);
				//sendKey(Keys.Delete);
			}
			y += buttonMargin;
			//Enter
			if (GUI.Button(new Rect (x, y, buttonSize+6, buttonSize*2+4), "⏎", gui.gsButton)) {
				//遅延呼び出し
				guiStopPropagation = false;
				Invoke("sendEnter", 0.1f);
			}
			
			GUI.DragWindow();
		}

		#endregion

		////////////////////////////////////////////////////////////////
		// キー送信
		#region SendKey

		public void sendDelete()
		{
			sendKey(Keys.Delete);
		}

		//遅延してクリックを下に通してからキー呼び出し ModsSliderでEnterが効かないためフォーカス対策
		public void sendEnter()
		{
			sendKey(Keys.Enter);
			Invoke("endEnter", 0.1f);
		}
		public void endEnter()
		{
			guiStopPropagation = true;
		}

		//キー送信
		public void sendKey(Keys key)
		{
			sendKey((ushort)key, false, false, false, false);
		}
		public void sendKey(ushort key)
		{
			sendKey(key, false, false, false, false);
		}
		public void sendKey(Keys key, bool shift, bool ctrl, bool alt)
		{
			//Debug.Log("sendKey : "+key+(shift?" + shift":"")+(ctrl?" + ctrl":"")+(alt?" + alt":""));
			sendKey((ushort)key, shift, ctrl, alt, false);
		}
		public void sendExtendedKey(Keys key, bool shift, bool ctrl, bool alt)
		{
			sendKey((ushort)key, shift, ctrl, alt, true);
		}
		public void sendKey(ushort key, bool shift, bool ctrl, bool alt, bool extended)
		{
			if (alt) altDown();
			if (ctrl) ctrlDown();
			if (shift) shiftDown();
			keyDown(key, extended);
			keyUp(key, extended);
			//StartCoroutine(DelayKeyUp(0.05f, key, extended));
			//遅延実行しないと押したことにならない
			StartCoroutine(DelayScanUp(0.1f, shift, ctrl, alt));
		}

		private void keyDown(ushort key, bool extended)
		{
			INPUT[] inputs = new INPUT[1];
			inputs[0].Type = 1; //INPUT_KEYBOARD
			KEYBDINPUT ki = new KEYBDINPUT();
			ki.Time = 0;
			ki.ExtraInfo = GetMessageExtraInfo();
			ki.Vk = key;
			ki.Scan = 0;
			ki.Flags = KEYEVENTF_KEYDOWN;
			if (extended) ki.Flags |= KEYEVENTF_EXTENDEDKEY;
			inputs[0].U.ki = ki;

			SendInput((uint)1, inputs, Marshal.SizeOf(inputs[0]));
		}

		private void shiftDown() { scanDown((ushort)Keys.ShiftKey); }
		private void ctrlDown() { scanDown((ushort)Keys.ControlKey); }
		private void altDown() { scanDown((ushort)Keys.Menu); }
		private void scanDown(ushort key)
		{
			INPUT[] inputs = new INPUT[1];
			inputs[0].Type = 1; //INPUT_KEYBOARD
			KEYBDINPUT ki = new KEYBDINPUT();
			ki.Time = 0;
			ki.ExtraInfo = GetMessageExtraInfo();
			ki.Vk = key;
			ki.Scan = (ushort)MapVirtualKey(key, 0);
			ki.Flags = KEYEVENTF_SCANCODE;
			inputs[0].U.ki = ki;

			SendInput((uint)1, inputs, Marshal.SizeOf(inputs[0]));
		}

		private IEnumerator DelayKeyUp(float delay, ushort key, bool extended)
		{
			yield return new WaitForSeconds(delay);
			keyUp(key, extended);
		}
		private void keyUp(ushort key, bool extended)
		{
			INPUT[] inputs = new INPUT[1];
			inputs[0].Type = 1; //INPUT_KEYBOARD
			KEYBDINPUT ki = new KEYBDINPUT();
			ki.Time = 0;
			ki.ExtraInfo = GetMessageExtraInfo();
			ki.Vk = key;
			ki.Scan = 0;
			ki.Flags = KEYEVENTF_KEYUP;
			if (extended) ki.Flags |= KEYEVENTF_EXTENDEDKEY;
			inputs[0].U.ki = ki;

			SendInput((uint)1, inputs, Marshal.SizeOf(inputs[0]));
		}

		private IEnumerator DelayScanUp(float delay, bool shift, bool ctrl, bool alt		)
		{
			yield return new WaitForSeconds(delay);
			if (shift) shiftUp();
			if (ctrl) ctrlUp();
			if (alt) altUp();
		}
		private void shiftUp() { scanUp((ushort)Keys.ShiftKey); }
		private void ctrlUp() { scanUp((ushort)Keys.ControlKey); }
		private void altUp() { scanUp((ushort)Keys.Menu); }
		private void scanUp(ushort key)
		{
			INPUT[] inputs = new INPUT[1];
			inputs[0].Type = 1; //INPUT_KEYBOARD
			KEYBDINPUT ki = new KEYBDINPUT();
			ki.Time = 0;
			ki.ExtraInfo = GetMessageExtraInfo();
			ki.Vk = key;
			ki.Scan = (ushort)MapVirtualKey(key, 0);
			ki.Flags = KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP;
			inputs[0].U.ki = ki;

			SendInput((uint)1, inputs, Marshal.SizeOf(inputs[0]));
		}

		#endregion

		#region SendInput API

		private const uint KEYEVENTF_KEYDOWN = 0x0;
		private const uint KEYEVENTF_KEYUP = 0x2;
		private const uint KEYEVENTF_EXTENDEDKEY = 0x1;
		private const uint KEYEVENTF_SCANCODE = 0x8;

		[DllImport("user32.dll", EntryPoint = "SendInput", SetLastError = true)]
		static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
		[DllImport("user32.dll", EntryPoint = "GetMessageExtraInfo", SetLastError = true)]
		static extern IntPtr GetMessageExtraInfo();
		[DllImport("user32.dll", EntryPoint = "MapVirtualKey")]
		static extern int MapVirtualKey(int wCode, int wMapType);
		[StructLayout(LayoutKind.Explicit)]
		public struct INPUT {
			[FieldOffset(0)] public uint Type;
			[FieldOffset(8)] public InputUnion U;
		}
		[StructLayout(LayoutKind.Explicit )]
		public struct InputUnion {
			[FieldOffset(0)] public MOUSEINPUT mi;
			[FieldOffset(0)] public KEYBDINPUT ki;
			[FieldOffset(0)] public HARDWAREINPUT hi;
		}
		[StructLayout(LayoutKind.Sequential)]
		public struct MOUSEINPUT {
			public int dx;
			public int dy;
			public uint mouseData;
			public uint dwFlags;
			public uint time;
			public  IntPtr dwExtraInfo;
		}
		[StructLayout(LayoutKind.Sequential)]
		public struct KEYBDINPUT {
			public UInt16 Vk;
			public UInt16 Scan;
			public UInt32 Flags;
			public UInt32 Time;
			public IntPtr ExtraInfo;
		}
		[StructLayout(LayoutKind.Sequential)]
		public struct HARDWAREINPUT {
			public uint uMsg;
			public ushort wParamL;
			public ushort wParamH;
		}

		#endregion
	}
}
