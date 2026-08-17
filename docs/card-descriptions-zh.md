# 大锤猎手全部卡牌中文描述

> 来源：`HammerMod/localization/zhs/cards.json` 当前工作区版本。共78张卡牌；以下保留动态变量和官方条件语法，便于逐句精简。

## 1. 纵挥

- ID：`HAMMER_MOD_CARD_OVERHEAD_SMASH`
- 当前描述：

```text
造成{Damage:diff()}点伤害。
```

## 2. 翻滚

- ID：`HAMMER_MOD_CARD_ROLL`
- 当前描述：

```text
获得{Block:diff()}点[gold]格挡[/gold]。
```

## 3. 蓄力

- ID：`HAMMER_MOD_CARD_CHARGE`
- 当前描述：

```text
获得{Charge}级[gold]蓄力[/gold]。
```

## 4. 大地一击

- ID：`HAMMER_MOD_CARD_EARTH_STRIKE`
- 当前描述：

```text
根据[gold]蓄力[/gold]等级造成{DamageAt0:diff()}/{DamageAt1:diff()}/{DamageAt2:diff()}/{DamageAt3:diff()}点伤害。
造成{StunAt0:diff()}/{StunAt1:diff()}/{StunAt2:diff()}/{StunAt3:diff()}点[gold]晕眩[/gold]。{InCombat:
（造成{Damage:diff()}点伤害，造成{Stun:diff()}点[gold]晕眩[/gold]）|}
```

## 5. 蓄势纵挥

- ID：`HAMMER_MOD_CARD_CHARGED_OVERHEAD_SMASH`
- 当前描述：

```text
造成{NormalDamage:diff()}点伤害。
如果当前至少拥有2级[gold]蓄力[/gold]，则改为{ChargedDamage:diff()}点伤害。
```

## 6. 蓄势横挥

- ID：`HAMMER_MOD_CARD_CHARGED_SIDE_SMASH`
- 当前描述：

```text
对所有敌人造成{NormalDamage:diff()}点伤害。
如果当前至少拥有2级[gold]蓄力[/gold]，则改为{ChargedDamage:diff()}点伤害。
```

## 7. 浑身蓄力本垒

- ID：`HAMMER_MOD_CARD_MIGHTY_CHARGE_SLAM`
- 当前描述：

```text
根据[gold]蓄力[/gold]等级造成{DamageAt0:diff()}/{DamageAt1:diff()}/{DamageAt2:diff()}/{DamageAt3:diff()}点伤害。{InCombat:
（造成{Damage:diff()}点伤害）|}
```

## 8. 浑身蓄力回旋

- ID：`HAMMER_MOD_CARD_SILKBIND_SPINNING_BLUDGEON`
- 当前描述：

```text
根据[gold]蓄力[/gold]等级，对所有敌人造成{Damage:diff()}点伤害{HitsAt0}/{HitsAt1}/{HitsAt2}/{HitsAt3}次。{InCombat:
（造成{Damage:diff()}点伤害{Hits:diff()}次）|}
```

## 9. 浑身蓄力防守

- ID：`HAMMER_MOD_CARD_CHARGED_GUARD`
- 当前描述：

```text
根据[gold]蓄力[/gold]等级获得{BlockAt0:diff()}/{BlockAt1:diff()}/{BlockAt2:diff()}/{BlockAt3:diff()}点[gold]格挡[/gold]。{InCombat:
（获得{Block:diff()}点[gold]格挡[/gold]）|}
```

## 10. 蓄势待发

- ID：`HAMMER_MOD_CARD_READY_TO_CHARGE`
- 当前描述：

```text
提升{Charge}级[gold]蓄力[/gold]并抽{Cards:diff()}张牌，如果当前[gold]蓄力[/gold]等级最大则改为抽{FullCards:diff()}张牌。
```

## 11. 续力闪避

- ID：`HAMMER_MOD_CARD_KEEPING_SWAY`
- 当前描述：

```text
提升{Charge}级[gold]蓄力[/gold]并获得{Block:diff()}点[gold]格挡[/gold]，如果当前[gold]蓄力[/gold]等级最大则改为获得{FullBlock:diff()}点[gold]格挡[/gold]。
```

## 12. 回旋蓄势

- ID：`HAMMER_MOD_CARD_SPINNING_CHARGE`
- 当前描述：

```text
获得{BlockPerEnergy:diff()}点[gold]格挡[/gold]X次，并提升X级[gold]蓄力[/gold]。
超出上限的每级[gold]蓄力[/gold]额外转化为{ExcessBlock:diff()}点[gold]格挡[/gold]。{InCombat:
（获得{ResolvedBlock:diff()}点[gold]格挡[/gold]，提升{ResolvedCharge}级[gold]蓄力[/gold]）|}
```

## 13. 居合

- ID：`HAMMER_MOD_CARD_SHEATHE_AND_BREATHE`
- 当前描述：

```text
只有当[gold]蓄力[/gold]等级大于1级时才能打出。
失去所有[gold]蓄力[/gold]等级，每级[gold]蓄力[/gold]等级转化为{EnergyPerCharge:energyIcons()}。
```

## 14. 乘胜追击

- ID：`HAMMER_MOD_CARD_VICTORY_CHARGE`
- 当前描述：

```text
只有当有敌人处于[gold]击晕[/gold]状态时才能打出。
获得{Energy:energyIcons()}。
抽{Cards}张牌。
```

## 15. 超蓄力

- ID：`HAMMER_MOD_CARD_OVERCHARGE`
- 当前描述：

```text
本回合内免费打出[gold]释放蓄力[/gold]的牌，且打出时不清空[gold]蓄力[/gold]等级。{IfUpgraded:show:|
在你的下个回合开始时，获得{Backlash:diff()}层[gold]虚弱[/gold]和{Backlash:diff()}层[gold]易伤[/gold]。}
```

## 16. 集中

- ID：`HAMMER_MOD_CARD_FOCUS`
- 当前描述：

```text
在你的回合开始时，获得1级[gold]蓄力[/gold]，如果[gold]蓄力[/gold]已满，改为抽{FullCards:diff()}张牌。
```

## 17. 余势不绝

- ID：`HAMMER_MOD_CARD_ENDLESS_MOMENTUM`
- 当前描述：

```text
每当你以3级以上[gold]蓄力[/gold]等级打出[gold]释放蓄力[/gold]的牌时，获得{Energy:energyIcons()}，抽1张牌。
```

## 18. 蓄势坚守

- ID：`HAMMER_MOD_CARD_CHARGED_STAND`
- 当前描述：

```text
获得{NormalBlock:diff()}点[gold]格挡[/gold]。
如果当前至少拥有2级[gold]蓄力[/gold]，则改为获得{ChargedBlock:diff()}点[gold]格挡[/gold]。
```

## 19. 紧急回避

- ID：`HAMMER_MOD_CARD_EMERGENCY_EVADE`
- 当前描述：

```text
获得{Block:diff()}点[gold]格挡[/gold]。
失去当前所有[gold]蓄力[/gold]。
```

## 20. 踏步横挥

- ID：`HAMMER_MOD_CARD_STEP_SWEEP`
- 当前描述：

```text
造成{Damage:diff()}点伤害。
如果敌人的意图不是攻击，则获得{Charge}级[gold]蓄力[/gold]。
```

## 21. 高手立回

- ID：`HAMMER_MOD_CARD_DASH_JUICE`
- 当前描述：

```text
每当你提升[gold]蓄力[/gold]等级时，每提升1级，获得{Block:diff()}点[gold]格挡[/gold]。
```

## 22. 撩击

- ID：`HAMMER_MOD_CARD_SIDE_SMASH`
- 当前描述：

```text
造成{Damage:diff()}点伤害。
造成{Stun:diff()}点[gold]晕眩[/gold]。
```

## 23. 浑身蓄力升龙

- ID：`HAMMER_MOD_CARD_CHARGED_UPSWING`
- 当前描述：

```text
造成{Damage:diff()}点伤害。
根据[gold]蓄力[/gold]等级造成{StunAt0:diff()}/{StunAt1:diff()}/{StunAt2:diff()}/{StunAt3:diff()}点[gold]晕眩[/gold]。{InCombat:
（造成{Damage:diff()}点伤害，造成{Stun:diff()}点[gold]晕眩[/gold]）|}
```

## 24. 升龙锤

- ID：`HAMMER_MOD_CARD_RISING_DRAGON_HAMMER`
- 当前描述：

```text
造成{Damage:diff()}点伤害。
造成{Stun:diff()}点[gold]晕眩[/gold]。
```

## 25. 震地冲击

- ID：`HAMMER_MOD_CARD_GROUND_SHOCK`
- 当前描述：

```text
对所有敌人造成{Damage:diff()}点伤害。
对所有敌人造成{Stun:diff()}点[gold]晕眩[/gold]。
```

## 26. 山崩地裂

- ID：`HAMMER_MOD_CARD_EARTHSPLITTER_SHOCK`
- 当前描述：

```text
对所有敌人造成{Damage:diff()}点伤害。
对所有敌人造成{Stun:diff()}点[gold]晕眩[/gold]。
```

## 27. 震天

- ID：`HAMMER_MOD_CARD_FOCUS_BLOW_EARTHQUAKE`
- 当前描述：

```text
如果敌人已有[gold]晕眩[/gold]值或已被[gold]击晕[/gold]，则给予{VulnerablePower:diff()}层[gold]易伤[/gold]。
造成{Damage:diff()}点伤害。
造成{Stun:diff()}点[gold]晕眩[/gold]。
```

## 28. 本垒打

- ID：`HAMMER_MOD_CARD_HOME_RUN_SWING`
- 当前描述：

```text
造成{NormalDamage:diff()}点伤害。
如果该敌人已被[gold]击晕[/gold]，则改为{StunnedDamage:diff()}点伤害。{IsTargeting:
（造成{Damage:diff()}点伤害）|}
```

## 29. 捣年糕

- ID：`HAMMER_MOD_CARD_BIG_BANG_COMBO`
- 当前描述：

```text
造成{Damage:diff()}点伤害{BaseHits}次。
如果该敌人已被[gold]击晕[/gold]，则改为{StunnedHits}次。{IsTargeting:
（造成{Damage:diff()}点伤害{Hits:diff()}次）|}
```

## 30. 闪光锤

- ID：`HAMMER_MOD_CARD_FLASH_HAMMER`
- 当前描述：

```text
对所有敌人造成{Stun:diff()}点[gold]晕眩[/gold]。
```

## 31. 头晕跌倒

- ID：`HAMMER_MOD_CARD_DIZZY_FALL`
- 当前描述：

```text
所有敌人失去等同于其当前[gold]晕眩[/gold]值{Multiplier:diff()}倍的生命。
```

## 32. 震荡护身

- ID：`HAMMER_MOD_CARD_CONCUSSION_GUARD`
- 当前描述：

```text
获得等同于所有敌人[gold]晕眩[/gold]值总和的[gold]格挡[/gold]。{InCombat:
（获得{Block:diff()}点[gold]格挡[/gold]）|}
```

## 33. KO术

- ID：`HAMMER_MOD_CARD_FELYNE_KO_TECHNIQUE`
- 当前描述：

```text
你的攻击牌每实际消耗1{energyPrefix:energyIcons(1)}，额外造成1点[gold]晕眩[/gold]。{IfUpgraded:show:
额外再造成{BonusStun:diff()}点[gold]晕眩[/gold]。|}
```

## 34. 打桩高手

- ID：`HAMMER_MOD_CARD_PILE_DRIVER`
- 当前描述：

```text
攻击牌对有[gold]晕眩[/gold]值的敌人造成的伤害提高{StunBonusPercent:diff()}%，对被[gold]击晕[/gold]的敌人改为提高{KnockedOutBonusPercent:diff()}%。
```

## 35. 追头重击

- ID：`HAMMER_MOD_CARD_HEAD_HUNTER_SMASH`
- 当前描述：

```text
造成{Damage:diff()}点伤害。
该敌人每有1点[gold]晕眩[/gold]，就额外造成{StunMultiplier:diff()}点伤害。
```

## 36. 后劲

- ID：`HAMMER_MOD_CARD_AFTERSHOCK`
- 当前描述：

```text
造成{Damage:diff()}点伤害。
在你的下个回合开始时，该敌人受到{Stun:diff()}点[gold]晕眩[/gold]。
```

## 37. 震荡共鸣

- ID：`HAMMER_MOD_CARD_CONCUSSION_RESONANCE`
- 当前描述：

```text
每当你给予敌人[gold]虚弱[/gold]或[gold]易伤[/gold]时，使其受到{Stun:diff()}点[gold]晕眩[/gold]。
```

## 38. 冲击爆裂

- ID：`HAMMER_MOD_CARD_IMPACT_BURST`
- 当前描述：

```text
针对多段攻击，你的攻击牌可以额外造成等同于{StunPerHit:diff()}倍攻击段数的[gold]晕眩[/gold]。
```

## 39. 迎面相杀

- ID：`HAMMER_MOD_CARD_FACE_OFF`
- 当前描述：

```text
只有在本回合打出过攻击牌后才能打出。
在本回合中，该敌人对你造成的伤害降低至0。
获得{StrengthPower:diff()}点[gold]力量[/gold]。
```

## 40. 水面击

- ID：`HAMMER_MOD_CARD_WATER_STRIKE`
- 当前描述：

```text
造成{Damage:diff()}点伤害。
如果敌人的意图是攻击，则获得{Block:diff()}点[gold]格挡[/gold]。
```

## 41. 借力挥击

- ID：`HAMMER_MOD_CARD_LEVERAGED_SWING`
- 当前描述：

```text
造成{Damage:diff()}点伤害。
如果敌人的意图是攻击，则获得{Energy:energyIcons()}。
```

## 42. 预判走位

- ID：`HAMMER_MOD_CARD_PREDICTIVE_FOOTWORK`
- 当前描述：

```text
如果敌人的意图是攻击，则给予{Power:diff()}层[gold]虚弱[/gold]，否则给予{Power:diff()}层[gold]易伤[/gold]。
```

## 43. 扫腿重锤

- ID：`HAMMER_MOD_CARD_LEG_SWEEP_HAMMER`
- 当前描述：

```text
对所有敌人造成{Damage:diff()}点伤害。
对意图为攻击的敌人给予{WeakPower:diff()}层[gold]虚弱[/gold]。
```

## 44. 斗志激发

- ID：`HAMMER_MOD_CARD_DEEP_BREATH`
- 当前描述：

```text
获得{Energy:energyIcons()}。
如果敌人的意图是攻击，则改为获得{AttackEnergy:energyIcons()}。
```

## 45. 灭气怒吼

- ID：`HAMMER_MOD_CARD_UNLOADING_STANCE`
- 当前描述：

```text
获得{Block:diff()}点[gold]格挡[/gold]。
所有意图为攻击的敌人在本回合失去{StrengthLoss:diff()}点[gold]力量[/gold]。
```

## 46. 水面架势

- ID：`HAMMER_MOD_CARD_WATER_STANCE`
- 当前描述：

```text
获得{Block:diff()}点[gold]格挡[/gold]。
直到你的下个回合开始，获得{Thorns:diff()}点[gold]荆棘[/gold]。
```

## 47. 蓄力垫步

- ID：`HAMMER_MOD_CARD_CHARGE_STEP`
- 当前描述：

```text
获得{Block:diff()}点[gold]格挡[/gold]。
如果敌人的意图是攻击，则获得{Charge}级[gold]蓄力[/gold]。
```

## 48. 以牙还牙

- ID：`HAMMER_MOD_CARD_OFFSET_UPSWING`
- 当前描述：

```text
攻击段数等同于敌人的攻击段数。
每段造成{Damage:diff()}点伤害。
```

## 49. 破势锤击

- ID：`HAMMER_MOD_CARD_BREAK_MOMENTUM`
- 当前描述：

```text
造成{Damage:diff()}点伤害。
如果敌人的[gold]力量[/gold]大于0，则使其失去{StrengthPower:diff()}点[gold]力量[/gold]。
```

## 50. 相杀形态

- ID：`HAMMER_MOD_CARD_COUNTER_FORM`
- 当前描述：

```text
每当你对意图为攻击的敌人打出攻击牌时，获得{Block:diff()}点[gold]格挡[/gold]。
```

## 51. 开眠

- ID：`HAMMER_MOD_CARD_WAKE_UP_HIT`
- 当前描述：

```text
造成{NormalDamage:diff()}点伤害。
如果敌人的意图不是攻击，则改为{NonAttackDamage:diff()}点伤害。
如果敌人正在睡眠，则改为{SleepingDamage:diff()}点伤害。{IsTargeting:
（造成{Damage:diff()}点伤害）|}
```

## 52. 破壳重击

- ID：`HAMMER_MOD_CARD_SHELL_BREAKER`
- 当前描述：

```text
移除该敌人的所有[gold]格挡[/gold]。
造成{Damage:diff()}点伤害。
```

## 53. 锤柄打击

- ID：`HAMMER_MOD_CARD_SWITCH_GRIP_SWING`
- 当前描述：

```text
造成{Damage:diff()}点伤害。
抽{Cards:diff()}张牌。
```

## 54. 整备

- ID：`HAMMER_MOD_CARD_TOOL_SPECIALIST`
- 当前描述：

```text
获得{Block:diff()}点[gold]格挡[/gold]。
抽{Cards}张牌。
在你的下个回合开始时，抽{NextTurnCards}张牌。
```

## 55. 快捷调合

- ID：`HAMMER_MOD_CARD_QUICK_CRAFT`
- 当前描述：

```text
抽{Cards:diff()}张牌。
将[gold]手牌[/gold]中的一张牌放到[gold]抽牌堆[/gold]顶部。
```

## 56. 开场体操

- ID：`HAMMER_MOD_CARD_WARM_UP_EXERCISE`
- 当前描述：

```text
获得{Stats:diff()}点[gold]力量[/gold]。
获得{Stats:diff()}点[gold]敏捷[/gold]。
```

## 57. 天地大冲撞

- ID：`HAMMER_MOD_CARD_IMPACT_CRATER`
- 当前描述：

```text
根据[gold]蓄力[/gold]等级造成{DamageAt0:diff()}/{DamageAt1:diff()}/{DamageAt2:diff()}/{DamageAt3:diff()}点伤害。
造成{StunAt0:diff()}/{StunAt1:diff()}/{StunAt2:diff()}/{StunAt3:diff()}点[gold]晕眩[/gold]。{InCombat:
（造成{Damage:diff()}点伤害，造成{Stun:diff()}点[gold]晕眩[/gold]）|}
```

## 58. 重整架势

- ID：`HAMMER_MOD_CARD_REPOSITION`
- 当前描述：

```text
获得{Block:diff()}点[gold]格挡[/gold]。
将[gold]弃牌堆[/gold]中的一张牌放到[gold]抽牌堆[/gold]顶部。
```

## 59. 捞飞队友

- ID：`HAMMER_MOD_CARD_LAUNCH_TEAMMATE`
- 当前描述：

```text
选择一名其他玩家。
其获得{Block:diff()}点[gold]格挡[/gold]和{StrengthPower}点[gold]力量[/gold]。
在其下个回合开始时，获得{Energy:energyIcons()}。
将1张“[gold]重新起身[/gold]”加入其[gold]手牌[/gold]。
```

## 60. 鬼人粉尘

- ID：`HAMMER_MOD_CARD_DEMON_POWDER`
- 当前描述：

```text
所有玩家获得{StrengthPower:diff()}点[gold]力量[/gold]。
```

## 61. 硬化粉尘

- ID：`HAMMER_MOD_CARD_HARDSHELL_POWDER`
- 当前描述：

```text
所有玩家获得{DexterityPower:diff()}点[gold]敏捷[/gold]。
```

## 62. 重新起身

- ID：`HAMMER_MOD_CARD_GET_BACK_UP`
- 当前描述：

```text
抽{Cards}张牌。
“谁捞的我？”
```

## 63. 横挥二连

- ID：`HAMMER_MOD_CARD_DOUBLE_SIDE_SWING`
- 当前描述：

```text
造成{Damage:diff()}点伤害{BaseHits:diff()}次。{InCombat:
（造成{Damage:diff()}点伤害{Hits:diff()}次）|}
```

## 64. 铁虫追击

- ID：`HAMMER_MOD_CARD_IRONBUG_FOLLOW_UP`
- 当前描述：

```text
造成{Damage:diff()}点伤害{BaseHits}次。
抽{Cards:diff()}张牌。{InCombat:
（造成{Damage:diff()}点伤害{Hits:diff()}次）|}
```

## 65. 蓄势滑走

- ID：`HAMMER_MOD_CARD_SLIDING_COMBO`
- 当前描述：

```text
在本回合获得{NormalStrength:diff()}点[gold]力量[/gold]。
如果当前至少拥有{RequiredCharge}级[gold]蓄力[/gold]，则改为在本回合获得{ChargedStrength:diff()}点[gold]力量[/gold]。
```

## 66. 飞锤横扫

- ID：`HAMMER_MOD_CARD_SWEEPING_PREPARATION`
- 当前描述：

```text
对所有敌人造成{Damage:diff()}点伤害。
抽{Cards}张牌。
```

## 67. 连环冲击

- ID：`HAMMER_MOD_CARD_POUNDING_SMASH`
- 当前描述：

```text
造成{Damage:diff()}点伤害{BaseHits:diff()}次。
造成{StunPerHit:diff()}倍攻击段数的[gold]晕眩[/gold]值。{InCombat:
（造成{Damage:diff()}点伤害{Hits:diff()}次，造成{ResolvedStun:diff()}点[gold]晕眩[/gold]）|}
```

## 68. 龙卷风摧毁停车场

- ID：`HAMMER_MOD_CARD_TRUE_SPINNING_IMPACT`
- 当前描述：

```text
对所有敌人造成{Damage:diff()}点伤害{IfUpgraded:show:X+1|X}次。
对所有敌人造成{StunPerEnergy}X点[gold]晕眩[/gold]。{InCombat:
（造成{Damage:diff()}点伤害{Hits:diff()}次，造成{ResolvedStun:diff()}点[gold]晕眩[/gold]）|}
```

## 69. 挑战者

- ID：`HAMMER_MOD_CARD_CHALLENGER`
- 当前描述：

```text
在你的回合开始时，如果任意敌人的意图是攻击，则在本回合获得{StrengthPower:diff()}点[gold]力量[/gold]。
```

## 70. 翔虫续力

- ID：`HAMMER_MOD_CARD_WIREBUG_CONTINUATION`
- 当前描述：

```text
每当你打出一张耗能大于等于{RequiredEnergy:energyIcons()}的牌时，获得{Charge:diff()}级[gold]蓄力[/gold]。
```

## 71. 弱点特效

- ID：`HAMMER_MOD_CARD_WEAKNESS_EXPLOIT`
- 当前描述：

```text
攻击牌对[gold]易伤[/gold]敌人造成的伤害提高{BonusPercent:diff()}%。
```

## 72. 愈战愈勇

- ID：`HAMMER_MOD_CARD_CHARGE_SWITCH_COURAGE`
- 当前描述：

```text
每当你以3级以上[gold]蓄力[/gold]等级打出[gold]释放蓄力[/gold]的牌时，获得{StrengthPower}点[gold]力量[/gold]。
```

## 73. 破坏王

- ID：`HAMMER_MOD_CARD_PARTBREAKER`
- 当前描述：

```text
每有一次攻击造成未被格挡的伤害，就给予{VulnerablePower}层[gold]易伤[/gold]。
```

## 74. 超连击

- ID：`HAMMER_MOD_CARD_COMBO_BOOST`
- 当前描述：

```text
多段攻击牌额外增加{ExtraHits:diff()}次攻击段数。
```

## 75. 客制吸血

- ID：`HAMMER_MOD_CARD_BLOOD_RITE`
- 当前描述：

```text
每次攻击后，按每个敌人在这次攻击中失去的生命分别计算，每满10点回复1点生命。
```

## 76. 手摇拖拉机

- ID：`HAMMER_MOD_CARD_HAND_CRANKED_TRACTOR`
- 当前描述：

```text
你[gold]抽牌堆[/gold]中的一张随机[gold]释放蓄力[/gold]牌获得{Replay:diff()}层[gold]重放[/gold]。
```

## 77. 马拉松锤手

- ID：`HAMMER_MOD_CARD_MARATHON_HAMMERER`
- 当前描述：

```text
根据当前[gold]蓄力[/gold]等级，获得同等数值的[gold]力量[/gold]。{InCombat:
（获得{StrengthPower}点[gold]力量[/gold]）|}
```

## 78. 回复药

- ID：`HAMMER_MOD_CARD_RECOVERY_MEDICINE`
- 当前描述：

```text
只有当每名敌人的意图都不是攻击或攻击伤害为0时才能打出。
获得{RegenPower:diff()}点[gold]再生[/gold]。
```

# 卡面附加描述与选择提示

以下文本不属于主`description`字段，但会在对应交互中显示。

## HAMMER_MOD_CARD_QUICK_CRAFT.selectionPrompt

```text
选择1张手牌放到抽牌堆顶部。
```

## HAMMER_MOD_CARD_REPOSITION.selectionPrompt

```text
选择1张牌，将其放到抽牌堆顶部。
```
