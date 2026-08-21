# 大锤猎手全部卡牌中文描述

> 来源：`HammerMod/localization/zhs/cards.json` 当前工作区版本。共89张卡牌；以下保留动态变量和官方条件语法，便于逐句精简。

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

## 7. 浑身蓄力敲打

- ID：`HAMMER_MOD_CARD_MIGHTY_CHARGE_BONK`
- 当前描述：

```text
根据[gold]蓄力[/gold]等级造成{DamageAt0:diff()}/{DamageAt1:diff()}/{DamageAt2:diff()}/{DamageAt3:diff()}点伤害。{InCombat:
（造成{Damage:diff()}点伤害）|}
```

## 8. 浑身蓄力回旋

- ID：`HAMMER_MOD_CARD_MIGHTY_CHARGE_SPIN`
- 当前描述：

```text
根据[gold]蓄力[/gold]等级，对所有敌人造成{Damage:diff()}点伤害{HitsAt0}/{HitsAt1}/{HitsAt2}/{HitsAt3}次。{InCombat:
（造成{Damage:diff()}点伤害{Hits:diff()}次）|}
```

## 9. 浑身蓄力翻滚

- ID：`HAMMER_MOD_CARD_MIGHTY_CHARGE_ROLL`
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

## 11. 蓄力垫步

- ID：`HAMMER_MOD_CARD_CHARGE_STEP`
- 当前描述：

```text
提升{Charge}级[gold]蓄力[/gold]并获得{Block:diff()}点[gold]格挡[/gold]，如果当前[gold]蓄力[/gold]等级最大则改为获得{FullBlock:diff()}点[gold]格挡[/gold]。
```

## 12. 越转越稳

- ID：`HAMMER_MOD_CARD_STEADIER_WITH_EVERY_SPIN`
- 当前描述：

```text
获得{BlockPerEnergy:diff()}点[gold]格挡[/gold]X次，并提升X级[gold]蓄力[/gold]。
超出上限的每级[gold]蓄力[/gold]额外转化为{ExcessBlock:diff()}点[gold]格挡[/gold]。{InCombat:
（获得{ResolvedBlock:diff()}点[gold]格挡[/gold]，提升{ResolvedCharge}级[gold]蓄力[/gold]）|}
```

## 13. 收锤居合

- ID：`HAMMER_MOD_CARD_HAMMER_IAI`
- 当前描述：

```text
只有当[gold]蓄力[/gold]等级至少为1级时才能打出。
失去所有[gold]蓄力[/gold]等级，每级[gold]蓄力[/gold]等级转化为{EnergyPerCharge:energyIcons()}。
```

## 14. 乘胜追击

- ID：`HAMMER_MOD_CARD_PRESS_THE_ADVANTAGE`
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
本回合内免费打出[gold]释放蓄力[/gold]的牌，且打出时不清空[gold]蓄力[/gold]等级。
```

## 16. 边打边蓄

- ID：`HAMMER_MOD_CARD_CHARGE_AS_YOU_STRIKE`
- 当前描述：

```text
造成{Damage:diff()}点伤害。
{IfUpgraded:show:将[gold]蓄力[/gold]提升至最大等级。|提升{Charge}级[gold]蓄力[/gold]。}
```

## 17. 集中

- ID：`HAMMER_MOD_CARD_FOCUS`
- 当前描述：

```text
在你的回合开始时，获得1级[gold]蓄力[/gold]，如果[gold]蓄力[/gold]已满，改为抽{FullCards:diff()}张牌。
```

## 18. 余势不绝

- ID：`HAMMER_MOD_CARD_ENDLESS_MOMENTUM`
- 当前描述：

```text
每回合第一次以3级以上[gold]蓄力[/gold]等级打出[gold]释放蓄力[/gold]的牌时，获得{Energy:energyIcons()}，抽{Cards:diff()}张牌。
```

## 19. 架锤硬扛

- ID：`HAMMER_MOD_CARD_BRACE_WITH_THE_HAMMER`
- 当前描述：

```text
获得{NormalBlock:diff()}点[gold]格挡[/gold]。
如果当前至少拥有2级[gold]蓄力[/gold]，则改为获得{ChargedBlock:diff()}点[gold]格挡[/gold]。
```

## 20. 紧急回避

- ID：`HAMMER_MOD_CARD_EMERGENCY_EVADE`
- 当前描述：

```text
获得{Block:diff()}点[gold]格挡[/gold]。
失去所有[gold]蓄力[/gold]等级。
```

## 21. 见缝抡锤

- ID：`HAMMER_MOD_CARD_SWING_AT_EVERY_OPENING`
- 当前描述：

```text
造成{Damage:diff()}点伤害。
如果敌人的意图不是攻击，则获得{Charge}级[gold]蓄力[/gold]。
```

## 22. 高手立回

- ID：`HAMMER_MOD_CARD_MASTERFUL_POSITIONING`
- 当前描述：

```text
回合结束时，根据[gold]蓄力[/gold]等级，每级获得{Block:diff()}点[gold]格挡[/gold]。
```

## 23. 升龙

- ID：`HAMMER_MOD_CARD_UPSWING`
- 当前描述：

```text
造成{Damage:diff()}点伤害。
造成{Stun:diff()}点[gold]晕眩[/gold]。
```

## 24. 浑身蓄力升龙

- ID：`HAMMER_MOD_CARD_MIGHTY_CHARGE_UPPERCUT`
- 当前描述：

```text
造成{Damage:diff()}点伤害。
根据[gold]蓄力[/gold]等级造成{StunAt0:diff()}/{StunAt1:diff()}/{StunAt2:diff()}/{StunAt3:diff()}点[gold]晕眩[/gold]。{InCombat:
（造成{Damage:diff()}点伤害，造成{Stun:diff()}点[gold]晕眩[/gold]）|}
```

## 25. 强升龙

- ID：`HAMMER_MOD_CARD_MIGHTY_UPSWING`
- 当前描述：

```text
造成{Damage:diff()}点伤害。
造成{Stun:diff()}点[gold]晕眩[/gold]。
```

## 26. 震地冲击

- ID：`HAMMER_MOD_CARD_GROUND_SHOCK`
- 当前描述：

```text
对所有敌人造成{Damage:diff()}点伤害。
对所有敌人造成{Stun:diff()}点[gold]晕眩[/gold]。
```

## 27. 山崩地裂

- ID：`HAMMER_MOD_CARD_CATACLYSM`
- 当前描述：

```text
对所有敌人造成{Damage:diff()}点伤害。
对所有敌人造成{Stun:diff()}点[gold]晕眩[/gold]。
```

## 28. 震天

- ID：`HAMMER_MOD_CARD_FOCUS_BLOW_EARTHQUAKE`
- 当前描述：

```text
给予{VulnerablePower:diff()}层[gold]易伤[/gold]。
造成{Damage:diff()}点伤害。
造成{Stun:diff()}点[gold]晕眩[/gold]。
```

## 29. 本垒打

- ID：`HAMMER_MOD_CARD_HOME_RUN_SWING`
- 当前描述：

```text
造成{NormalDamage:diff()}点伤害。
如果该敌人已被[gold]击晕[/gold]，则改为{StunnedDamage:diff()}点伤害。{IsTargeting:
（造成{Damage:diff()}点伤害）|}
```

## 30. 捣年糕

- ID：`HAMMER_MOD_CARD_BIG_BANG_COMBO`
- 当前描述：

```text
造成{Damage:diff()}点伤害{BaseHits}次。
如果该敌人已被[gold]击晕[/gold]，则改为{StunnedHits}次。{IsTargeting:
（造成{Damage:diff()}点伤害{Hits:diff()}次）|}
```

## 31. 闪光锤

- ID：`HAMMER_MOD_CARD_FLASH_HAMMER`
- 当前描述：

```text
对所有敌人造成{Stun:diff()}点[gold]晕眩[/gold]。
```

## 32. 头重脚轻

- ID：`HAMMER_MOD_CARD_HEAD_OVER_HEELS`
- 当前描述：

```text
所有敌人失去等同于其当前[gold]晕眩[/gold]值{Multiplier:diff()}倍的生命。
```

## 33. 震荡护身

- ID：`HAMMER_MOD_CARD_CONCUSSION_GUARD`
- 当前描述：

```text
获得等同于所有敌人[gold]晕眩[/gold]值总和的[gold]格挡[/gold]。{InCombat:
（获得{Block:diff()}点[gold]格挡[/gold]）|}
```

## 34. KO术

- ID：`HAMMER_MOD_CARD_KO_TECHNIQUE`
- 当前描述：

```text
你的攻击牌额外造成等同于其{IfUpgraded:show:费用+1|费用}的[gold]晕眩[/gold]。
```

## 35. 打桩高手

- ID：`HAMMER_MOD_CARD_PILE_DRIVER`
- 当前描述：

```text
攻击牌对有[gold]晕眩[/gold]值的敌人造成的伤害提高{StunBonusPercent:diff()}%，对被[gold]击晕[/gold]的敌人改为提高{KnockedOutBonusPercent:diff()}%。
```

## 36. 狠狠砸头

- ID：`HAMMER_MOD_CARD_SMASH_THAT_HEAD`
- 当前描述：

```text
造成{Damage:diff()}点伤害。
该敌人每有1点[gold]晕眩[/gold]，就额外造成{StunMultiplier:diff()}点伤害。
```

## 37. 后劲

- ID：`HAMMER_MOD_CARD_AFTERSHOCK`
- 当前描述：

```text
造成{Damage:diff()}点伤害。
在你的下个回合开始时，该敌人受到{Stun:diff()}点[gold]晕眩[/gold]。
```

## 38. 精神抖擞

- ID：`HAMMER_MOD_CARD_CONCUSSION_RESONANCE`
- 当前描述：

```text
在每个回合开始时，失去{ChargeLoss:diff()}级[gold]蓄力[/gold]，获得{Energy:energyIcons()}。
```

## 39. 冲击爆裂

- ID：`HAMMER_MOD_CARD_IMPACT_BURST`
- 当前描述：

```text
针对多段攻击，你的攻击牌可以额外造成等同于{StunPerHit:diff()}倍攻击段数的[gold]晕眩[/gold]。
```

## 40. 迎面相杀

- ID：`HAMMER_MOD_CARD_FACE_OFF`
- 当前描述：

```text
只有在本回合打出过攻击牌后才能打出。
直到敌方回合结束，该敌人对你造成的伤害降低至0。
获得{StrengthPower:diff()}点[gold]力量[/gold]。
```

## 41. 水面击

- ID：`HAMMER_MOD_CARD_WATER_STRIKE`
- 当前描述：

```text
造成{Damage:diff()}点伤害。
如果敌人的意图是攻击，则获得{Block:diff()}点[gold]格挡[/gold]。
```

## 42. 借力挥击

- ID：`HAMMER_MOD_CARD_LEVERAGED_SWING`
- 当前描述：

```text
造成{Damage:diff()}点伤害。
如果敌人的意图是攻击，则获得{Energy:energyIcons()}。
```

## 43. 预判走位

- ID：`HAMMER_MOD_CARD_PREDICTIVE_FOOTWORK`
- 当前描述：

```text
如果敌人的意图是攻击，则给予{Power:diff()}层[gold]虚弱[/gold]，否则给予{Power:diff()}层[gold]易伤[/gold]。
```

## 44. 扫腿重锤

- ID：`HAMMER_MOD_CARD_LEG_SWEEP_HAMMER`
- 当前描述：

```text
对所有敌人造成{Damage:diff()}点伤害。
对意图为攻击的敌人给予{WeakPower:diff()}层[gold]虚弱[/gold]。
```

## 45. 斗志激发

- ID：`HAMMER_MOD_CARD_FIGHTING_SPIRIT`
- 当前描述：

```text
如果敌人的意图是攻击，则获得{AttackEnergy:energyIcons()}。
```

## 46. 灭气怒吼

- ID：`HAMMER_MOD_CARD_STAMINA_DRAINING_ROAR`
- 当前描述：

```text
获得{Block:diff()}点[gold]格挡[/gold]。
所有意图为攻击的敌人在本回合失去{StrengthLoss:diff()}点[gold]力量[/gold]。
```

## 47. 游走蹭刀

- ID：`HAMMER_MOD_CARD_WEAVE_AND_BONK`
- 当前描述：

```text
获得{Block:diff()}点[gold]格挡[/gold]。
直到你的下个回合开始，获得{Thorns:diff()}点[gold]荆棘[/gold]。
```

## 48. 借力蓄势

- ID：`HAMMER_MOD_CARD_BORROWED_MOMENTUM`
- 当前描述：

```text
获得{Block:diff()}点[gold]格挡[/gold]。
如果敌人的意图是攻击，则获得{Charge}级[gold]蓄力[/gold]。
```

## 49. 以锤还牙

- ID：`HAMMER_MOD_CARD_HAMMER_FOR_A_HAMMER`
- 当前描述：

```text
攻击段数等同于敌人的攻击段数。
每段造成{Damage:diff()}点伤害。
```

## 50. 破势锤击

- ID：`HAMMER_MOD_CARD_BREAK_MOMENTUM`
- 当前描述：

```text
造成{Damage:diff()}点伤害。
如果敌人的[gold]力量[/gold]大于0，则使其失去{StrengthPower:diff()}点[gold]力量[/gold]。
```

## 51. 相杀形态

- ID：`HAMMER_MOD_CARD_COUNTER_FORM`
- 当前描述：

```text
每当你对意图为攻击的敌人打出攻击牌时，获得{Block:diff()}点[gold]格挡[/gold]。
```

## 52. 开眠

- ID：`HAMMER_MOD_CARD_WAKE_UP_HIT`
- 当前描述：

```text
造成{NormalDamage:diff()}点伤害。
如果敌人的意图不是攻击，则改为{NonAttackDamage:diff()}点伤害。
如果敌人正在睡眠，则改为{SleepingDamage:diff()}点伤害。{IsTargeting:
（造成{Damage:diff()}点伤害）|}
```

## 53. 破壳重击

- ID：`HAMMER_MOD_CARD_SHELL_BREAKER`
- 当前描述：

```text
移除该敌人的所有[gold]格挡[/gold]。
造成{Damage:diff()}点伤害。
```

## 54. 锤柄打击

- ID：`HAMMER_MOD_CARD_HAMMER_HANDLE_STRIKE`
- 当前描述：

```text
造成{Damage:diff()}点伤害。
抽{Cards:diff()}张牌。
```

## 55. 整备

- ID：`HAMMER_MOD_CARD_TOOL_SPECIALIST`
- 当前描述：

```text
获得{Block:diff()}点[gold]格挡[/gold]。
抽{Cards}张牌。
在你的下个回合开始时，抽{NextTurnCards}张牌。
```

## 56. 快捷调合

- ID：`HAMMER_MOD_CARD_QUICK_CRAFT`
- 当前描述：

```text
抽{Cards:diff()}张牌。
将[gold]手牌[/gold]中的一张牌放到[gold]抽牌堆[/gold]顶部。
```

## 57. 开场体操

- ID：`HAMMER_MOD_CARD_WARM_UP_EXERCISE`
- 当前描述：

```text
获得{Stats:diff()}点[gold]力量[/gold]。
获得{Stats:diff()}点[gold]敏捷[/gold]。
```

## 58. 天地大冲撞

- ID：`HAMMER_MOD_CARD_IMPACT_CRATER`
- 当前描述：

```text
根据[gold]蓄力[/gold]等级造成{DamageAt0:diff()}/{DamageAt1:diff()}/{DamageAt2:diff()}/{DamageAt3:diff()}点伤害。
造成{StunAt0:diff()}/{StunAt1:diff()}/{StunAt2:diff()}/{StunAt3:diff()}点[gold]晕眩[/gold]。{InCombat:
（造成{Damage:diff()}点伤害，造成{Stun:diff()}点[gold]晕眩[/gold]）|}
```

## 59. 重整架势

- ID：`HAMMER_MOD_CARD_REPOSITION`
- 当前描述：

```text
获得{Block:diff()}点[gold]格挡[/gold]。
将[gold]弃牌堆[/gold]中的一张牌放到[gold]抽牌堆[/gold]顶部。
```

## 60. 捞飞队友

- ID：`HAMMER_MOD_CARD_LAUNCH_TEAMMATE`
- 当前描述：

```text
选择一名其他玩家。
其获得{Block:diff()}点[gold]格挡[/gold]和{StrengthPower}点[gold]力量[/gold]。
在其下个回合开始时，获得{Energy:energyIcons()}。
将1张“[gold]谁捞的我[/gold]”加入其[gold]手牌[/gold]。
```

## 61. 鬼人粉尘

- ID：`HAMMER_MOD_CARD_DEMON_POWDER`
- 当前描述：

```text
所有玩家获得{StrengthPower:diff()}点[gold]力量[/gold]。
```

## 62. 硬化粉尘

- ID：`HAMMER_MOD_CARD_HARDSHELL_POWDER`
- 当前描述：

```text
所有玩家获得{DexterityPower:diff()}点[gold]敏捷[/gold]。
```

## 63. 谁捞的我

- ID：`HAMMER_MOD_CARD_BACK_ON_YOUR_FEET`
- 当前描述：

```text
抽{Cards}张牌。
```

## 64. 连续横挥

- ID：`HAMMER_MOD_CARD_CONTINUOUS_SIDE_SWINGS`
- 当前描述：

```text
造成{Damage:diff()}点伤害{BaseHits:diff()}次。{InCombat:
（造成{Damage:diff()}点伤害{Hits:diff()}次）|}
```

## 65. 翔虫回旋

- ID：`HAMMER_MOD_CARD_WIREBUG_SPIN`
- 当前描述：

```text
造成{Damage:diff()}点伤害{BaseHits}次。
抽{Cards:diff()}张牌。{InCombat:
（造成{Damage:diff()}点伤害{Hits:diff()}次）|}
```

## 66. 滑走强化

- ID：`HAMMER_MOD_CARD_AFFINITY_SLIDING`
- 当前描述：

```text
在本回合获得{NormalStrength:diff()}点[gold]力量[/gold]。
如果当前至少拥有{RequiredCharge}级[gold]蓄力[/gold]，则改为在本回合获得{ChargedStrength:diff()}点[gold]力量[/gold]。
```

## 67. 横扫开路

- ID：`HAMMER_MOD_CARD_SWEEP_A_PATH`
- 当前描述：

```text
对所有敌人造成{Damage:diff()}点伤害。
抽{Cards}张牌。
```

## 68. 无敌风火轮

- ID：`HAMMER_MOD_CARD_INVINCIBLE_WIND_FIRE_WHEEL`
- 当前描述：

```text
造成{Damage:diff()}点伤害{BaseHits:diff()}次。
造成{StunPerHit:diff()}倍攻击段数的[gold]晕眩[/gold]值。{InCombat:
（造成{Damage:diff()}点伤害{Hits:diff()}次，造成{ResolvedStun:diff()}点[gold]晕眩[/gold]）|}
```

## 69. 龙卷风摧毁停车场

- ID：`HAMMER_MOD_CARD_TORNADO_DESTROYS_THE_PARKING_LOT`
- 当前描述：

```text
对所有敌人造成{Damage:diff()}点伤害{IfUpgraded:show:X+1|X}次。
对所有敌人造成{StunPerEnergy}X点[gold]晕眩[/gold]。{InCombat:
（造成{Damage:diff()}点伤害{Hits:diff()}次，造成{ResolvedStun:diff()}点[gold]晕眩[/gold]）|}
```

## 70. 挑战者

- ID：`HAMMER_MOD_CARD_CHALLENGER`
- 当前描述：

```text
在你的回合开始时，如果任意敌人的意图是攻击，则在本回合获得{StrengthPower:diff()}点[gold]力量[/gold]。
```

## 71. 翔虫续力

- ID：`HAMMER_MOD_CARD_WIREBUG_CONTINUATION`
- 当前描述：

```text
每当你打出一张耗能大于等于{RequiredEnergy:energyIcons()}的牌时，获得{Charge:diff()}级[gold]蓄力[/gold]。
```

## 72. 弱点特效

- ID：`HAMMER_MOD_CARD_WEAKNESS_EXPLOIT`
- 当前描述：

```text
攻击牌对[gold]易伤[/gold]敌人造成的伤害提高{BonusPercent:diff()}%。
```

## 73. 力大砖飞

- ID：`HAMMER_MOD_CARD_HARDER_WITH_EVERY_SMASH`
- 当前描述：

```text
每当你以3级以上[gold]蓄力[/gold]等级打出[gold]释放蓄力[/gold]的牌时，获得{StrengthPower}点[gold]力量[/gold]。
```

## 74. 破坏王

- ID：`HAMMER_MOD_CARD_PARTBREAKER`
- 当前描述：

```text
每有一次攻击造成未被格挡的伤害，就给予{VulnerablePower}层[gold]易伤[/gold]。
```

## 75. 再来一锤

- ID：`HAMMER_MOD_CARD_ONE_MORE_BONK`
- 当前描述：

```text
多段攻击牌额外增加{ExtraHits:diff()}次攻击段数。
```

## 76. 客制吸血

- ID：`HAMMER_MOD_CARD_CUSTOM_LIFESTEAL`
- 当前描述：

```text
每次攻击后，按每个敌人在这次攻击中失去的生命分别计算，每满10点回复1点生命。
```

## 77. 手摇拖拉机

- ID：`HAMMER_MOD_CARD_HAND_CRANKED_TRACTOR`
- 当前描述：

```text
你[gold]抽牌堆[/gold]中的一张随机[gold]释放蓄力[/gold]牌获得{Replay:diff()}层[gold]重放[/gold]。
```

## 78. 蓄力变化·武

- ID：`HAMMER_MOD_CARD_CHARGE_SWITCH_STRENGTH`
- 当前描述：

```text
获得当前[gold]蓄力[/gold]等级+1点[gold]力量[/gold]。{InCombat:
（获得{StrengthPower}点[gold]力量[/gold]）|}
```

## 79. 回复药

- ID：`HAMMER_MOD_CARD_RECOVERY_MEDICINE`
- 当前描述：

```text
给予自己{VulnerablePower:diff()}层[gold]易伤[/gold]。
获得{RegenPower:diff()}点[gold]再生[/gold]。
```

## 80. 砥石

- ID：`HAMMER_MOD_CARD_WHETSTONE`
- 当前描述：

```text
选择1张其他[gold]手牌[/gold]，将其[gold]消耗[/gold]。
抽{Cards:diff()}张牌。
提升{Charge:diff()}级[gold]蓄力[/gold]。
```

## 81. 灭气重锤

- ID：`HAMMER_MOD_CARD_STAMINA_DRAINING_HAMMER`
- 当前描述：

```text
造成{Damage:diff()}点伤害。
造成{Stun:diff()}点[gold]晕眩[/gold]。
给予{WeakPower:diff()}层[gold]虚弱[/gold]。
给予{VulnerablePower:diff()}层[gold]易伤[/gold]。
```

## 82. 寻找斜坡

- ID：`HAMMER_MOD_CARD_FIND_A_SLOPE`
- 当前描述：

```text
选择你的[gold]抽牌堆[/gold]中的{Cards:diff()}张牌，将其放到[gold]抽牌堆[/gold]顶部。
```

## 83. 翔虫受身

- ID：`HAMMER_MOD_CARD_WIREFALL`
- 当前描述：

```text
在下个敌方回合中，第一次受到未被格挡的攻击伤害后，后续所有攻击对你造成的伤害降低至0。
```

## 84. 回家玉

- ID：`HAMMER_MOD_CARD_FARCASTER`
- 当前描述：

```text
移除你的所有状态。
结束你的回合。
下个敌方回合中，你不会受到任何伤害，不会被添加状态，也不会被塞牌。
```

## 85. 转祸为福

- ID：`HAMMER_MOD_CARD_COALESCENCE`
- 当前描述：

```text
分别减少自身至多{MaxReduction}层[gold]虚弱[/gold]、[gold]易伤[/gold]与[gold]脆弱[/gold]。
每减少1层，获得{StrengthPower:diff()}点[gold]力量[/gold]。
```

## 86. 满足感

- ID：`HAMMER_MOD_CARD_FREE_MEAL`
- 当前描述：

```text
你下一瓶使用的药水不会被消耗。
```

## 87. 激运票

- ID：`HAMMER_MOD_CARD_LUCKY_VOUCHER`
- 当前描述：

```text
战斗结束时，可以重掷卡牌奖励。
```

## 88. 蓄力变化·勇

- ID：`HAMMER_MOD_CARD_CHARGE_SWITCH_COURAGE`
- 当前描述：

```text
本回合内，每打出一张攻击牌，提升{Charge:diff()}级[gold]蓄力[/gold]。
```

## 89. 勇气风格

- ID：`HAMMER_MOD_CARD_VALOR_STYLE`
- 当前描述：

```text
将[gold]蓄力[/gold]提升至最大等级。
此后，[gold]释放蓄力[/gold]时改为只失去1级[gold]蓄力[/gold]。
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

## HAMMER_MOD_CARD_FIND_A_SLOPE.selectionPrompt

```text
选择要放到抽牌堆顶部的牌。
```
