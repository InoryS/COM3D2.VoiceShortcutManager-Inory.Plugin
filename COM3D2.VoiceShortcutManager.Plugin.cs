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

build :
C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:library /lib:"..\COM3D2\Sybaris" /lib:"..\COM3D2\Sybaris\UnityInjector" /lib:"..\COM3D2\COM3D2x64_Data\Managed" /r:UnityEngine.dll /r:UnityEngine.VR.dll /r:UnityInjector.dll /r:Assembly-CSharp.dll /r:Assembly-CSharp-firstpass.dll COM3D2.VoiceShortcutManager.Plugin.cs VoiceConfig.cs GearMenu.cs
*/

#if COM3D2_5
[assembly: AssemblyTitle("VoiceShortcutManager COM3D2.5")]
#else
[assembly: AssemblyTitle("VoiceShortcutManager COM3D2")]
#endif
[assembly: AssemblyVersion("1.2.0.0")]

namespace COM3D2.VoiceShortcutManager.Plugin
{
	[
		PluginName("COM3D2.VoiceShortcutManager"), PluginVersion("1.2")
	]
	public class VoiceShortcutManager : PluginBase
	{
		const string VERSION = "1.2";

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
		//イベントを透過させない Enterのみ遅延実行で投下させる制御に利用
		private bool guiStopPropagation = true;

		//ギアメニューに配置するボタン
		private GameObject keyboardButton;
		private GameObject micButton;
		private GameObject[] gearMenuButton;

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

		//設定ファイルパス
		const string configPath = @"Sybaris\UnityInjector\Config\VoiceShortcutManager\";
		const string configFile =  configPath+"VoiceShortcutManager.xml";
		const string voiceConfigFile =  configPath+"VoiceConfig.xml";

		//設定ファイルの更新日時 再取得チェック用
		DateTime voiceConfigFileTime;

		//設定ファイル
		public class Config
		{
			//バージョン番号 設定ファイル上書き保存判定に利用
			public string version = "";

			//起動時に音声認識をONにする
			public bool micActivateOnStart = false;

			//音声認識を利用するならtrue
			public bool micEnabled = true;
			public string micLabelON = "音声認識 ON";
			public string micLabelOFF = "音声認識 無効";

			//テンキー表示を利用するならtrue
			public bool keyboardEnabled = true;
			public string keyboardLabelON = "テンキー表示 ON";
			public string keyboardLabelOFF = "テンキー表示";

			//テンキーのサイズと色
			public int buttonSize = 32;
			public int fontSize = 16;
			public Color32 color = new Color32(96, 96, 96, 240);

			//夜伽コマンドが無効状態でも音声で実行する
			public bool forceVoiceYotogiCommand = false;

			//ショートカットボタンリスト
			public List<MenuInfo> menuList = new List<MenuInfo>();

			public Config() {}
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
			//設定ファイル読み込み 読み込み時に引数なしでnewされる
			if (System.IO.File.Exists(configFile)) {
				try {
					System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Config));
					using (System.IO.StreamReader sr = new System.IO.StreamReader(configFile, new System.Text.UTF8Encoding(false))) {
						cfg = (Config)serializer.Deserialize(sr); //XMLファイルから読み込み
					}
					//バージョンが変わっていたら保存
					if (cfg.version != VERSION) {
						cfg.version = VERSION;
						using (System.IO.StreamWriter sw = new System.IO.StreamWriter(configFile, false, new System.Text.UTF8Encoding(false))) {
							serializer.Serialize(sw, cfg); //XMLファイルに保存
						}
					}
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
			}

			//ボタンオブジェクト格納用配列
			this.gearMenuButton = new GameObject[cfg.menuList.Count()];

			//アイコン読み込み
			string[] buttons = {"Keyboard", "MicON", "MicOFF", "MicDisabled"};
			foreach (string button in buttons) {
				if (System.IO.File.Exists(configPath+button+".png")) {
					byte[] bytes = System.IO.File.ReadAllBytes(configPath+button+".png");
					if (bytes != null && bytes.Length > 0) this.icons.Add(button, bytes);
				}
			}
			foreach (MenuInfo menu in cfg.menuList) {
				if (System.IO.File.Exists(configPath+menu.name+".png")) {
					byte[] bytes = System.IO.File.ReadAllBytes(configPath+menu.name+".png");
					if (bytes != null && bytes.Length > 0) this.icons.Add(menu.name, bytes);
				}
				//ついでにsceneIdの先頭の^と末尾の$を除去
				if (menu.sceneId != null) {
					menu.sceneId = menu.sceneId.TrimStart('^');
					menu.sceneId = menu.sceneId.TrimEnd('$');
				}
			}

			initGUI();

			//音声設定ファイル読み込み
			if (System.IO.File.Exists(voiceConfigFile)) {
				try {
					loadVoiceConfig();
				} catch (Exception e) {
					Debug.LogError("[VoiceShortcutManager] VoiceConfig.xml Error : "+e);
				}
				//バージョンが上がっていても保存しない（改行されるため）
				//if (newVersion) saveVoiceConfig();
			} else {
				voiceCfg = new VoiceConfig(); //初期化
				voiceCfg.initDefault();
				saveVoiceConfig();
			}
			createVoiceCache();
		}

		//シーンロード時に呼ばれる
		void OnLevelWasLoaded(int level)
		{
			this.sceneLevel = level;
			this.sceneName = GameMain.Instance.GetNowSceneName();

			//夜伽シーンならチェック用コルーチン開始 
			if (YotogiManager.instans || YotogiOldManager.instans) {
				//コルーチン開始
				if (this.yotogiCheckCoroutine == null) this.yotogiCheckCoroutine = StartCoroutine(yotogiCheck());
			} else {
				//コルーチン停止
				if (this.yotogiCheckCoroutine != null) {
					StopCoroutine(this.yotogiCheckCoroutine);
					this.yotogiCheckCoroutine = null;
				}
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

		//毎フレーム呼ばれる 更新処理
		void Update()
		{
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
		// システムショートカット

		//テンキーのトグル
		public void toggleKeyboard()
		{
			keyboardVisible = !keyboardVisible;
			guiStopPropagation = keyboardVisible;
			GearMenu.Buttons.SetText(keyboardButton, (keyboardVisible ? cfg.keyboardLabelON : cfg.keyboardLabelOFF));

			//位置変更 すぐドラッグ移動できるようにマウスの下に表示 ツールバーより下にする
			Vector2 pos = new Vector2(Input.mousePosition.x, (float)UnityEngine.Screen.height - Input.mousePosition.y + 50);
			gui.rect.x = pos.x-10;
			gui.rect.y = pos.y-10;
		}

		//マイクボタンのトグル
		public void toggleMic()
		{
			micON = !micON;
			//音声認識初期化
			if (micON) {
				enableMic();
			}
			//音声認識無効化
			else {
				disableMic();
			}
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
					this.gearMenuButton[idx] = GearMenu.Buttons.Add(menu.name, menu.label, icon, (go) => sendKey(menu.key, menu.shift, menu.ctrl, menu.alt));
				}
			}
		}

		////////////////////////////////////////////////////////////////
		// 音声関連
		
		//音声設定ファイル読み込み 読み込み時に引数なしでnewされる
		private void loadVoiceConfig()
		{
			Debug.Log("[VoiceShortcutManager] Load VoiceConfig");
			System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(VoiceConfig));
			using (System.IO.StreamReader sr = new System.IO.StreamReader(voiceConfigFile, new System.Text.UTF8Encoding(false))) {
				voiceCfg = (VoiceConfig)serializer.Deserialize(sr); //XMLファイルから読み込み
			}
			//更新日時
			voiceConfigFileTime = System.IO.File.GetLastWriteTime(voiceConfigFile);
		}
		//音声設定ファイル保存 基本ファイルがない場合のみ
		private void saveVoiceConfig()
		{
			Debug.Log("[VoiceShortcutManager] Save VoiceConfig");
			System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(VoiceConfig));
			using (System.IO.StreamWriter sw = new System.IO.StreamWriter(voiceConfigFile, false, new System.Text.UTF8Encoding(false))) {
				serializer.Serialize(sw, voiceCfg); //XMLファイルに保存
			}
			//更新日時
			voiceConfigFileTime = System.IO.File.GetLastWriteTime(voiceConfigFile);
		}
		//音声設定ファイルからDictionary作成
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

		//音声認識有効化
		private void enableMic()
		{
			if (System.IO.File.Exists(voiceConfigFile)) {
				//ファイルが更新されていたら読み込み
				if (voiceConfigFileTime < System.IO.File.GetLastWriteTime(voiceConfigFile)) {
					try {
						loadVoiceConfig();
						createVoiceCache();
					} catch (Exception e) {
						Debug.LogError("[VoiceShortcutManager] VoiceConfig.xml Error : "+e);
					}
				}
			} else {
				//ファイルがなくなっていたら初期化して出力
				voiceCfg = new VoiceConfig();
				voiceCfg.initDefault();
				saveVoiceConfig();
				createVoiceCache();
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
								sendKey(menu.key, menu.shift, menu.ctrl, menu.alt);
							} else {
								Debug.Log("Voice sendKey : 無効なシーンです");
							}
						});
					}
				}
			}

			//夜伽シーンキーワード設定
			setYotogiKeywords();

			//視線 顔向き
			setFaceKeywoards();

			//脱衣
			setUndressKeywoards();

			//VYM連携


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
									string a = alias; //スコープ対策
									if (aliasLabel == "") aliasLabel = alias;
									else aliasLabel += "  |  "+alias;
									keywords.Add(alias, () => {
										Debug.Log("YotogiCommand : "+keyword+" ("+a+")");
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

		#region Face

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
			foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList()) {
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
			foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList()) {
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
			foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList()) {
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
			foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList()) {
				if (maid.Visible) {
					maid.EyeToCamera(Maid.EyeMoveType.顔をそらす, 1.0f);
				}
			}			
		}

		#endregion

		#region Undressing

		//脱衣関連

		//音声キーワード設定
		private void setUndressKeywoards()
		{
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
		}

		//UnitType { 全脱衣, 全着衣, トップス, ボトムス, ブラジャー, パンツ, ソックス, シューズ, ヘッドドレス, メガネ, 背中, 手袋, Max }
		//夜伽シーンの脱衣パネル取得
		private UndressingManager getUndressingManager()
		{
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

		//全部着る
		public void dressAll()
		{
			//すべてのメイド（仮）
			foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList()) {
				if (maid.Visible) dressAll(maid);
			}
		}
		public void dressAll(Maid maid)
		{
			UndressingManager undressManager = getUndressingManager();
			if (undressManager != null) {
				undressManager.SetMaskMode(UndressingManager.UnitType.全着衣, UndressingManager.MaskStatus.On); //Onで実行
			} else {
				maid.body0.SetMask(TBody.SlotID.wear, true);
				maid.body0.SetMask(TBody.SlotID.mizugi, true);
				maid.body0.SetMask(TBody.SlotID.onepiece, true);
				maid.body0.SetMask(TBody.SlotID.bra, true);
				maid.body0.SetMask(TBody.SlotID.skirt, true);
				maid.body0.SetMask(TBody.SlotID.panz, true);
				maid.body0.SetMask(TBody.SlotID.glove, true);
				maid.body0.SetMask(TBody.SlotID.accUde, true);
				maid.body0.SetMask(TBody.SlotID.stkg, true);
				maid.body0.SetMask(TBody.SlotID.shoes, true);
				maid.body0.SetMask(TBody.SlotID.accKubi, true);
				maid.body0.SetMask(TBody.SlotID.accKubiwa, true);
			}
		}
		//全部脱がす
		public void undressAll()
		{
			UndressingManager undressManager = getUndressingManager();
			//すべてのメイド（仮）
			foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList()) {
				if (maid.Visible) {
					if (undressManager != null) {
						undressManager.SetMaskMode(UndressingManager.UnitType.全脱衣, UndressingManager.MaskStatus.On); //Onで実行
					} else {
						maid.body0.SetMask(TBody.SlotID.wear, false);
						maid.body0.SetMask(TBody.SlotID.mizugi, false);
						maid.body0.SetMask(TBody.SlotID.onepiece, false);
						maid.body0.SetMask(TBody.SlotID.bra, false);
						maid.body0.SetMask(TBody.SlotID.skirt, false);
						maid.body0.SetMask(TBody.SlotID.panz, false);
						maid.body0.SetMask(TBody.SlotID.glove, false);
						maid.body0.SetMask(TBody.SlotID.accUde, false);
						maid.body0.SetMask(TBody.SlotID.stkg, false);
						maid.body0.SetMask(TBody.SlotID.shoes, false);
						maid.body0.SetMask(TBody.SlotID.accKubi, false);
						maid.body0.SetMask(TBody.SlotID.accKubiwa, false);
					}
				}
			}
		}

		//服だけ脱がす
		public void undressWear()
		{
			UndressingManager undressManager = getUndressingManager();
			//すべてのメイド（仮）
			foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList()) {
				if (maid.Visible) {
					if (undressManager != null) {
						undressManager.SetMaskMode(UndressingManager.UnitType.トップス, UndressingManager.MaskStatus.On); //Onが非表示
						undressManager.SetMaskMode(UndressingManager.UnitType.ボトムス, UndressingManager.MaskStatus.On); //Onが非表示
						undressManager.SetMaskMode(UndressingManager.UnitType.シューズ, UndressingManager.MaskStatus.On); //Onが非表示
					} else {
						maid.body0.SetMask(TBody.SlotID.wear, false);
						maid.body0.SetMask(TBody.SlotID.onepiece, false);
						maid.body0.SetMask(TBody.SlotID.skirt, false);
						maid.body0.SetMask(TBody.SlotID.shoes, false);
					}
				}
			}
		}
		//服の上だけ脱がす
		public void undressTop()
		{
			UndressingManager undressManager = getUndressingManager();
			//すべてのメイド（仮）
			foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList()) {
				if (maid.Visible) {
					if (undressManager != null) {
						undressManager.SetMaskMode(UndressingManager.UnitType.トップス, UndressingManager.MaskStatus.On); //Onが非表示
					} else {
						maid.body0.SetMask(TBody.SlotID.wear, false);
						maid.body0.SetMask(TBody.SlotID.onepiece, false);
					}
				}
			}
		}
		//服の下だけ脱がす
		public void undressBottom()
		{
			UndressingManager undressManager = getUndressingManager();
			//すべてのメイド（仮）
			foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList()) {
				if (maid.Visible) {
					if (undressManager != null) {
						undressManager.SetMaskMode(UndressingManager.UnitType.ボトムス, UndressingManager.MaskStatus.On); //Onが非表示
					} else {
						maid.body0.SetMask(TBody.SlotID.skirt, false);
					}
				}
			}
		}

		//水着着て
		public void dressMizugi()
		{
			UndressingManager undressManager = getUndressingManager();
			//すべてのメイド（仮）
			foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList()) {
				if (maid.Visible) {
					if (undressManager != null) {
						undressManager.SetMaskMode(UndressingManager.UnitType.トップス, UndressingManager.MaskStatus.On); //Onが非表示
					} else {
						maid.body0.SetMask(TBody.SlotID.mizugi, true);
					}
				}
			}
		}
		//水着脱いで
		public void undressMizugi()
		{
			UndressingManager undressManager = getUndressingManager();
			//すべてのメイド（仮）
			foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList()) {
				if (maid.Visible) {
					if (undressManager != null) {
						undressManager.SetMaskMode(UndressingManager.UnitType.トップス, UndressingManager.MaskStatus.On); //Onが非表示
					} else {
						maid.body0.SetMask(TBody.SlotID.mizugi, false);
					}
				}
			}
		}

		//下着つけて 夜伽以外は水着も含む
		public void dressUnderWear()
		{
			UndressingManager undressManager = getUndressingManager();
			//すべてのメイド（仮）
			foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList()) {
				if (maid.Visible) {
					if (undressManager != null) {
						undressManager.SetMaskMode(UndressingManager.UnitType.ブラジャー, UndressingManager.MaskStatus.Off); //Offが表示
						undressManager.SetMaskMode(UndressingManager.UnitType.パンツ, UndressingManager.MaskStatus.Off); //Offが表示
					} else {
						maid.body0.SetMask(TBody.SlotID.mizugi, true);
						maid.body0.SetMask(TBody.SlotID.bra, true);
						maid.body0.SetMask(TBody.SlotID.panz, true);
					}
				}
			}
		}
		//下着脱いで 夜伽以外は水着も含む
		public void undressUnderWear()
		{
			UndressingManager undressManager = getUndressingManager();
			//すべてのメイド（仮）
			foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList()) {
				if (maid.Visible) {
					if (undressManager != null) {
						undressManager.SetMaskMode(UndressingManager.UnitType.ブラジャー, UndressingManager.MaskStatus.On); //Onが非表示
						undressManager.SetMaskMode(UndressingManager.UnitType.パンツ, UndressingManager.MaskStatus.On); //Onが非表示
					} else {
						maid.body0.SetMask(TBody.SlotID.mizugi, false);
						maid.body0.SetMask(TBody.SlotID.bra, false);
						maid.body0.SetMask(TBody.SlotID.panz, false);
					}
				}
			}
		}
		//ブラつけて
		public void dressBra()
		{
			UndressingManager undressManager = getUndressingManager();
			//すべてのメイド（仮）
			foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList()) {
				if (maid.Visible) {
					if (undressManager != null) {
						undressManager.SetMaskMode(UndressingManager.UnitType.ブラジャー, UndressingManager.MaskStatus.Off); //Offが表示
					} else {
						maid.body0.SetMask(TBody.SlotID.bra, true);
					}
				}
			}
		}
		//ブラ外して
		public void undressBra()
		{
			UndressingManager undressManager = getUndressingManager();
			//すべてのメイド（仮）
			foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList()) {
				if (maid.Visible) {
					if (undressManager != null) {
						undressManager.SetMaskMode(UndressingManager.UnitType.ブラジャー, UndressingManager.MaskStatus.On); //Onが非表示
					} else {
						maid.body0.SetMask(TBody.SlotID.bra, false);
					}
				}
			}
		}
		//パンツ履いて
		public void dressPants()
		{
			UndressingManager undressManager = getUndressingManager();
			//すべてのメイド（仮）
			foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList()) {
				if (maid.Visible) {
					if (undressManager != null) {
						undressManager.SetMaskMode(UndressingManager.UnitType.パンツ, UndressingManager.MaskStatus.Off); //Offが表示
					} else {
						maid.body0.SetMask(TBody.SlotID.panz, true);
					}
				}
			}
		}
		//パンツ脱いで
		public void undressPants()
		{
			UndressingManager undressManager = getUndressingManager();
			//すべてのメイド（仮）
			foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList()) {
				if (maid.Visible) {
					if (undressManager != null) {
						undressManager.SetMaskMode(UndressingManager.UnitType.パンツ, UndressingManager.MaskStatus.On); //Onが非表示
					} else {
						maid.body0.SetMask(TBody.SlotID.panz, false);
					}
				}
			}
		}
		//靴履いて
		public void dressShoes()
		{
			UndressingManager undressManager = getUndressingManager();
			//すべてのメイド（仮）
			foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList()) {
				if (maid.Visible) {
					if (undressManager != null) {
						undressManager.SetMaskMode(UndressingManager.UnitType.シューズ, UndressingManager.MaskStatus.Off); //Offが表示
					} else {
						maid.body0.SetMask(TBody.SlotID.shoes, true);
					}
				}
			}
		}
		//靴脱いで
		public void undressShoes()
		{
			UndressingManager undressManager = getUndressingManager();
			//すべてのメイド（仮）
			foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList()) {
				if (maid.Visible) {
					if (undressManager != null) {
						undressManager.SetMaskMode(UndressingManager.UnitType.シューズ, UndressingManager.MaskStatus.On); //Onが非表示
					} else {
						maid.body0.SetMask(TBody.SlotID.shoes, false);
					}
				}
			}
		}
		//パンツ戻す
		public void modoshiPants()
		{
			//すべてのメイド（仮）
			foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList()) {
				if (maid.Visible) modoshiPants(maid);
			}
		}
		public void modoshiPants(Maid maid)
		{
			MPN mpn;
            if (maid.body0.GetSlotVisible(TBody.SlotID.panz)) mpn = MPN.panz;
			else if (maid.body0.GetSlotVisible(TBody.SlotID.mizugi)) mpn = MPN.mizugi;
			else return; //見えてなければ終了

			MaidProp prop = maid.GetProp(mpn);
			//仮衣装
			if (prop.nTempFileNameRID != 0) {
				//zurashiを除去した衣装に変更
				maid.SetProp(mpn, maid.GetProp(mpn).strTempFileName.Replace("_zurashi", ""), 0, true, false);
			}
			//本衣装
			else if (prop.nFileNameRID != 0) {
				//本衣装に戻す
				maid.ResetProp(mpn, false);
			}
			maid.AllProcPropSeqStart();
		}
		//パンツずらす
		public void zurashiPants()
		{
			//すべてのメイド（仮）
			foreach (Maid maid in GameMain.Instance.CharacterMgr.GetStockMaidList()) {
				if (maid.Visible) zurashiPants(maid);
			}
		}
		public void zurashiPants(Maid maid)
		{
			MPN mpn; 
            if (maid.body0.GetSlotVisible(TBody.SlotID.panz)) mpn = MPN.panz;
			else if (maid.body0.GetSlotVisible(TBody.SlotID.mizugi)) mpn = MPN.mizugi;
			else return; //見えてなければ終了

			SortedDictionary<string, string> sortedDictionary;
			//ずらしファイル名
			string filename = "";
			MaidProp prop = maid.GetProp(mpn);
			//仮衣装状態
			if (prop.nTempFileNameRID != 0) {
				//衣装なし
				if (prop.strTempFileName.Contains("del.menu")) return;
				//すでにずれている
				if (prop.strTempFileName.Contains("_zurashi")) return;
				//仮衣装のずらしファイルを取得
				if (Menu.m_dicResourceRef.TryGetValue(prop.nTempFileNameRID, out sortedDictionary) && sortedDictionary.TryGetValue("パンツずらし", out filename)) {
					//仮衣装がすでにずれている
					if (filename.Equals(prop.strTempFileName)) return;
					//ずらし衣装に設定
					maid.SetProp(mpn, filename, 0, true, false);
				}
			}
			//本衣装 仮衣装状態でない
			else if (prop.nFileNameRID != 0) {
				//すでにずれている
				if (prop.strTempFileName.Contains("_zurashi")) return;
				//本衣装のずらしファイルを取得
				if (Menu.m_dicResourceRef.TryGetValue(prop.nFileNameRID, out sortedDictionary) && sortedDictionary.TryGetValue("パンツずらし", out filename)) {
					//すでにずれている
					if (filename.Equals(prop.strTempFileName)) return;
					//ずらし衣装に設定
					maid.SetProp(mpn, filename, 0, true, false);
				}
			}
			maid.AllProcPropSeqStart();
		}

		#endregion

		#region GUI

		//テンキーウィンドウ

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

		//初期化
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
