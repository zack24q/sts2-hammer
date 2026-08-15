# 大锤猎人 Mod 测试包

这是开发阶段测试包，不是正式发布版。准确版本信息见同目录的 `build-info.json`。

## 安装

1. 完全退出《杀戮尖塔 2》。
2. 找到游戏可执行文件所在目录：
   - Windows：Steam 中选择游戏，打开“管理 -> 浏览本地文件”。
   - macOS：在 Steam 中浏览本地文件，右键 `SlayTheSpire2.app`，选择“显示包内容”，进入 `Contents/MacOS/`。
3. 把测试包里的整个 `mods` 文件夹复制到该目录。最终目录必须是：

```text
mods/
├── HammerMod/
│   ├── HammerMod.dll
│   ├── HammerMod.json
│   └── HammerMod.pck
└── STS2-RitsuLib/
    ├── STS2-RitsuLib.dll
    └── mod_manifest.json
```

4. 如果已有同名旧版本，请用测试包中的文件替换对应目录内容，不要保留旧 DLL 或 PCK。
5. 启动游戏并确认启用 `RitsuLib` 和 `The Hammer / 大锤`。首次启用后若游戏提示重启，请完整退出并重新启动。

测试包固定绑定一个游戏 API 版本。游戏版本低于 `build-info.json` 的 `min_game_version` 时不要加载；游戏更新后也应先向开发者确认兼容性。

## 建议测试内容

- 创建独立测试档案，确认角色选择页显示“大锤猎人”。
- 进入战斗检查人物、血条、蓄力 UI、粉色卡框和动态数值。
- 检查蓄力 0 至 3 级、受伤清空、释放牌和晕眩阈值。
- 分别测试普通战斗、精英、Boss、商店、休息处、遗物和药水。
- 联机测试时确认联机限定牌、队友目标和各玩家承伤效果。

## 反馈格式

请一并提供：

- `build-info.json` 的内容；
- 操作系统、游戏版本、单人或联机；
- 可重复的操作步骤、预期结果和实际结果；
- 截图或短视频；
- 本次游戏会话的 `godot.log`，以及同时启用的其他 Mod 列表。
