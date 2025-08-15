/*Data for the table `accounts` */

insert  into `accounts`(`id`,`name`,`password`,`pin`,`pic`,`lastlogin`,`createdat`,`birthday`,`nxCredit`,`maplePoint`,`nxPrepaid`,`characterslots`,`gender`,`tos`,`nick`,`email`,`language`,`gmlevel`) values (1,'admin','C7AD44CBAD762A5DA0A452F9E854FDC1E0E7A52A38015F23F3EAB1D80B931DD472634DFAC71CD34EBC35D16AB7FB8A90C81F975113D6C7538DC69DD8DE9077EC','0000','000000','2021-05-24 08:00:01','2021-05-24 08:00:02','2005-05-11',1000000,1000000,1000000,3,0,1,NULL,NULL,2,6);

/*Data for the table `characters` */

insert  into `characters`(`id`,`accountid`,`world`,`name`,`level`,`exp`,`gachaexp`,`str`,`dex`,`luk`,`int`,`hp`,`mp`,`maxhp`,`maxmp`,`meso`,`hpMpUsed`,`job`,`skincolor`,`gender`,`fame`,`fquest`,`hair`,`face`,`ap`,`sp`,`map`,`spawnpoint`,`buddyCapacity`,`createdate`,`rank`,`rankMove`,`jobRank`,`jobRankMove`,`guildid`,`guildrank`,`mountlevel`,`mountexp`,`mounttiredness`,`omokwins`,`omoklosses`,`omokties`,`matchcardwins`,`matchcardlosses`,`matchcardties`,`equipslots`,`useslots`,`setupslots`,`etcslots`,`familyId`,`monsterbookcover`,`allianceRank`,`vanquisherStage`,`ariantPoints`,`dojoPoints`,`lastDojoStage`,`finishedDojoTutorial`,`vanquisherKills`,`summonValue`,`reborns`,`PQPoints`,`dataString`,`lastLogoutTime`,`lastExpGainTime`,`partySearch`,`jailexpire`) values (1,1,0,'Admin',1,0,0,12,5,4,4,50,5,50,5,0,0,0,0,0,0,0,30030,20000,0,'0,0,0,0,0,0,0,0,0,0',10000,2,25,'2021-05-24 08:00:03',1,0,1,0,0,5,1,0,0,0,0,0,0,0,0,24,24,24,24,-1,0,5,0,0,0,0,0,0,0,0,0,'','2021-05-24 08:00:04','2015-01-01 13:00:00',1,0);

/*Data for the table `inventoryequipment` */

insert  into `inventoryequipment`(`inventoryequipmentid`,`inventoryitemid`,`upgradeslots`,`level`,`str`,`dex`,`int`,`luk`,`hp`,`mp`,`watk`,`matk`,`wdef`,`mdef`,`acc`,`avoid`,`hands`,`speed`,`jump`,`locked`,`vicious`,`itemlevel`,`itemexp`,`ringid`) values (17,22,7,0,0,0,0,0,0,0,0,0,3,0,0,0,0,0,0,0,0,1,0,-1),(18,23,7,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,1,0,-1),(19,24,5,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,1,0,-1),(20,25,7,0,0,0,0,0,0,0,17,0,0,0,0,0,0,0,0,0,0,1,0,-1);

/*Data for the table `inventoryitems` */

insert  into `inventoryitems`(`inventoryitemid`,`type`,`characterid`,`accountid`,`itemid`,`inventorytype`,`position`,`quantity`,`owner`,`petid`,`flag`,`expiration`,`giftFrom`) values (21,1,1,NULL,4161001,4,1,1,'',-1,0,-1,''),(22,1,1,NULL,1040002,-1,-5,1,'',-1,0,-1,''),(23,1,1,NULL,1060002,-1,-6,1,'',-1,0,-1,''),(24,1,1,NULL,1072001,-1,-7,1,'',-1,0,-1,''),(25,1,1,NULL,1302000,-1,-11,1,'',-1,0,-1,'');

/*Data for the table `keymap` */

insert  into `keymap`(`id`,`characterid`,`key`,`type`,`action`) values (161,1,18,4,0),(162,1,65,6,106),(163,1,2,4,10),(164,1,23,4,1),(165,1,3,4,12),(166,1,4,4,13),(167,1,5,4,18),(168,1,6,4,24),(169,1,16,4,8),(170,1,17,4,5),(171,1,19,4,4),(172,1,25,4,19),(173,1,26,4,14),(174,1,27,4,15),(175,1,31,4,2),(176,1,34,4,17),(177,1,35,4,11),(178,1,37,4,3),(179,1,38,4,20),(180,1,40,4,16),(181,1,43,4,9),(182,1,44,5,50),(183,1,45,5,51),(184,1,46,4,6),(185,1,50,4,7),(186,1,56,5,53),(187,1,59,6,100),(188,1,60,6,101),(189,1,61,6,102),(190,1,62,6,103),(191,1,63,6,104),(192,1,64,6,105),(193,1,57,5,54),(194,1,48,4,22),(195,1,29,5,52),(196,1,7,4,21),(197,1,24,4,25),(198,1,33,4,26),(199,1,41,4,23),(200,1,39,4,27);