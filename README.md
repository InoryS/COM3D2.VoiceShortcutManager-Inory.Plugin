# COM3D2.VoiceShortcutManager
A VoiceShortcutManager fork for personal purposes

This repository is for personal purposes, I added some custom modifications.

Only for COM3D2, sorry

<br>

- added more slots support
- added shape key support

<details>

<summary>example</summary>

Use voice to control my [facehugger mod](https://mega.nz/folder/U6Jy0a6a#Pv5G9G_J5zoYc46TVmz6iA): 

just say `butt facehugger` to wear, and say `Facehugger ready` and `Facehugger animation` to let it move

VoiceConfig.xml
```
    <propVoiceList>
        <PropVoiceInfo>
            <voice>
                <string>butt facehugger</string>
            </voice>
            <props>
                <PropInfo>
                    <mpn>accanl</mpn>
                    <filename>InoryS_FaceHugger_蟲姦_accanl_z1.menu</filename>
                    <temp>false</temp>
                </PropInfo>
            </props>
            <masks>
                <MaskInfo>
                    <slot>accAnl</slot>
                    <visible>true</visible>
                </MaskInfo>
            </masks>
        </PropVoiceInfo>
        <PropVoiceInfo>
            <voice>
                <string>cancle butt facehugger</string>
            </voice>
            <props>
                <PropInfo>
                    <mpn>accanl</mpn>
                    <filename>_i_accanl_del.menu</filename>
                    <temp>false</temp>
                </PropInfo>
            </props>
            <masks>
                <MaskInfo>
                    <slot>accAnl</slot>
                    <visible>true</visible>
                </MaskInfo>
            </masks>
        </PropVoiceInfo>
    </propVoiceList>

<!-- Features added in this version -->
    <shapeKeyList>
        <ShapeKeyInfo>
            <shapeKey>facehugger_ready</shapeKey>
            <value>1</value>
            <voice>
                <string>Facehugger ready</string>
                <string>Facehugger ready to insert</string>
            </voice>
        </ShapeKeyInfo>
        <ShapeKeyInfo>
            <shapeKey>facehugger_ready</shapeKey>
            <value>0</value>
            <voice>
                <string>Facehugger cancel ready</string>
                <string>Facehugger cancel ready to insert</string>
                <string>Cancel Facehugger ready to insert</string>
            </voice>
        </ShapeKeyInfo>
        <ShapeKeyInfo>
            <shapeKey>facehugger_insert</shapeKey>
            <value>1</value>
            <voice>
                  <string>Facehugger insert</string>
            </voice>
        </ShapeKeyInfo>
        <ShapeKeyInfo>
            <shapeKey>facehugger_insert</shapeKey>
            <value>0</value>
            <voice>
                <string>Cancel facehugger insert</string>
            </voice>
        </ShapeKeyInfo>
    </shapeKeyList>


    <shapeKeyAnimationList>
        <ShapeKeyAnimationInfo>
            <shapeKey>facehugger_insert</shapeKey>
            <min>0</min>
            <max>1</max>
            <speed>0.5</speed>
            <startVoice>
                <string>Facehugger insert animation</string>
                <string>Facehugger animation</string>
            </startVoice>
            <stopVoice>
                <string>Cancel facehugger insert animation</string>
                <string>Cancel facehugger animation</string>
            </stopVoice>
        </ShapeKeyAnimationInfo>
    </shapeKeyAnimationList>
```

</details>



<br>


This plugin is made by twitter [@ankokusamochi](https://x.com/ankokusamochi), release at [https://x.com/ankokusamochi/status/1857758072518705453](https://x.com/ankokusamochi/status/1857758072518705453), you can find original description there.

<br>

The readme says:
```
## ソースコードについて
- ソースコードの流用、改変、再配布用は自由に行ってもらって構いません
- GearMenu.cs はラベル開業対応とアイコン画像の切替ができるように一部改修しています
- アイコン画像は以下のフリーアイコンを利用しています
  https://icooon-mono.com/license/
  https://icon-rainbow.com/
```
So I suppose free to use, modify, and redistribute the source code.

But, it the author don't want me to do this, please let me know and I will delete this repo.

All rights belong to the original author @ankokusamochi
