/*
 Navicat Premium Data Transfer

 Source Server         : localhost_3306
 Source Server Type    : MariaDB
 Source Server Version : 100311
 Source Host           : localhost:3306
 Source Schema         : haoyue_emu

 Target Server Type    : MariaDB
 Target Server Version : 100311
 File Encoding         : 65001

 Date: 05/02/2025 11:46:40
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for rom_stars
-- ----------------------------
DROP TABLE IF EXISTS `rom_stars`;
CREATE TABLE `rom_stars`  (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `uid` int(11) NOT NULL,
  `romid` int(11) NOT NULL,
  `logdate` datetime NOT NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `uid_romid`(`uid`, `romid`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 25 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Fixed;

-- ----------------------------
-- Table structure for romlist
-- ----------------------------
DROP TABLE IF EXISTS `romlist`;
CREATE TABLE `romlist`  (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `PlatformType` int(11) NOT NULL,
  `Name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `RomUrl` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `ImgUrl` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `GameType` varchar(5) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `Note` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `Hash` varchar(60) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `playcount` int(11) NOT NULL DEFAULT 0,
  `stars` int(11) NOT NULL DEFAULT 0,
  `parentids` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 10940 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for room_log
-- ----------------------------
DROP TABLE IF EXISTS `room_log`;
CREATE TABLE `room_log`  (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `uid` int(11) NOT NULL,
  `platform` int(11) NOT NULL,
  `romid` int(11) NOT NULL,
  `roomid` int(11) NULL DEFAULT NULL,
  `state` int(11) NOT NULL,
  `logdate` datetime NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 867 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Fixed;

-- ----------------------------
-- Table structure for room_log_state
-- ----------------------------
DROP TABLE IF EXISTS `room_log_state`;
CREATE TABLE `room_log_state`  (
  `id` int(11) NOT NULL,
  `name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = MyISAM CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for user_devices
-- ----------------------------
DROP TABLE IF EXISTS `user_devices`;
CREATE TABLE `user_devices`  (
  `device` varchar(80) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `devicetype` int(11) NOT NULL,
  `uid` int(11) NOT NULL
) ENGINE = MyISAM CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for user_gamesavedata
-- ----------------------------
DROP TABLE IF EXISTS `user_gamesavedata`;
CREATE TABLE `user_gamesavedata`  (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `uid` int(11) NOT NULL,
  `romid` int(11) NOT NULL,
  `savidx` int(11) NOT NULL DEFAULT current_timestamp(),
  `savName` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `savNote` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `savUrl` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `savImgUrl` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `savDate` datetime NOT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 2 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for users
-- ----------------------------
DROP TABLE IF EXISTS `users`;
CREATE TABLE `users`  (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `account` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `password` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `regdate` datetime NOT NULL DEFAULT current_timestamp(),
  `nikename` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `lastlogindate` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`uid`) USING BTREE
) ENGINE = MyISAM AUTO_INCREMENT = 8 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

SET FOREIGN_KEY_CHECKS = 1;
