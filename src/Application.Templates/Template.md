### Item.wz

#### Cash 501 - 599

501：装饰  
502：装饰  
503：雇佣商人`HiredMerchantItemTemplate`  
504：缩地石  
505：洗点卷轴  
506：特殊消耗道具  
507：喇叭  
508：风筝  
509：消息  
510：音乐盒  
512：场景特效，场景buff`MapBuffItemTemplate`  
513：护符（特殊消耗道具）`SafetyCharmItemTemplate`  
514：个人商店  
515：美容美发卡（特殊消耗道具）  
516：表情（装饰）  
517：宠物改名（特殊消耗道具）  
518：生命水（特殊消耗道具）  
519：宠物技能（特殊消耗道具）`WaterOfLifeItemTemplate`  
520：金币袋`MesoBagItemTemplate`  
521：经验倍率`CouponItemTemplate`  
522：各种门票（特殊消耗道具）  
523：猫头鹰  
524：宠物饲料`CashPetFoodItemTemplate`  
525：婚礼道具 （特殊消耗道具）  
528：范围特效`AreaEffectItemTemplate`  
530：变身`MorphItemTemplate`  
536：爆率倍率`CouponItemTemplate`  
538：进化之石  
550：魔法沙漏`ExtendItemTimeItemTemplate`  
553：`CashPackagedItemTemplate`  
557：金锤子

##### 节点

stateChangeItem: 触发道具效果  
protectTime: 封印时长（封印之锁）  
soldInform: 售出后通知？（雇佣商人）  
info/rate: 倍率  
spec/expR: 名称上看是经验倍率，但是有些与`info/rate`不同，比如`05211048`，String.wz中提到是双倍，但是`spec/expR`是3  


#### Consume 200 - 245

201：药水 `PotionItemTemplate`  
202：药水，箱子 `PotionItemTemplate`  
203：传送卷轴`TownScrollItemTemplate`  
204：属性卷轴`ScrollItemTemplate`  
205：解除debuff状态、特殊道具`PotionItemTemplate`  
206：弓矢`BulletItemTemplate`  
207：飞镖`BulletItemTemplate`  
210：怪物召唤包`SummonMobItemTemplate`   
212：宠物饲料`PetFoodItemTemplate`  
216：新年贺卡  
219：测谎仪  
221：变身药水  `MorphItemTemplate`  
224：结婚道具  
226：坐骑回复疲劳`OtherConsumeItemTemplate`  
227：捕捉怪物道具`CatchMobItemTemplate`  
228：技能册`MasteryItemTemplate`  
229：能手册`MasteryItemTemplate`  
233：子弹`BulletItemTemplate`  
234：祝福卷轴  
236：幽灵变身`GhostItemTemplate`  
237：经验书`SolomenItemTemplate`  
238：怪物卡片`MonsterCardItemTemplate`  
243：任务用到的消耗道具？`ScriptItemTemplate`  
245：幸运的狩猎`OtherConsumeItemTemplate`  


#### Etc 400 - 431

401：矿石  
402：母矿  
403、408：小游戏道具  
413：制作辅助剂  
425：制作技能道具  
426：怪物结晶  

#### 节点作用

hpR: 回复HP比例  
mhpR: 最大HP提升比例  
mhpRRate: 回复频率？  
respectPimmune: 无视物理抵抗  
respectMimmune: 无视魔法抵抗  
respectFS: 伤害加成  
defenseAtt: 属性攻击抗性  
defenseState: 暗属性、诅咒、昏迷、虚弱抗性  
thaw: ？  
timeLimited: 时间限制  

### Skill.wz


#### 节点作用

mpCon: 消耗MP  
hpCon: 消耗HP  