﻿//TODO  VirituaNes097 / NES / Mapper / MapperFDS.cpp

////////////////////////////////////////////////////////////////////////////
//// Mapper020  Nintendo Disk System(FDS)                                 //
////////////////////////////////////////////////////////////////////////////
//using System;
//using Unity.Android.Gradle;
//using static VirtualNes.Core.CPU;
//using static VirtualNes.MMU;
//using BYTE = System.Byte;
//using INT = System.Int32;


//namespace VirtualNes.Core
//{
//    public class Mapper020 : Mapper
//    {
//        enum enum_1
//        {
//            BLOCK_READY = 0,
//            BLOCK_VOLUME_LABEL,
//            BLOCK_FILE_AMOUNT,
//            BLOCK_FILE_HEADER,
//            BLOCK_FILE_DATA,
//        };
//        enum enum_2
//        {
//            SIZE_VOLUME_LABEL = 56,
//            SIZE_FILE_AMOUNT = 2,
//            SIZE_FILE_HEADER = 16,
//        };
//        enum enum_3
//        {
//            OFFSET_VOLUME_LABEL = 0,
//            OFFSET_FILE_AMOUNT = 56,
//            OFFSET_FILE_HEADER = 58,
//            OFFSET_FILE_DATA = 74,
//        };

//        enum enum_4
//        {
//            MECHANICAL_SOUND_BOOT = 0,
//            MECHANICAL_SOUND_SEEKEND,
//            MECHANICAL_SOUND_MOTOR_ON,
//            MECHANICAL_SOUND_MOTOR_OFF,
//            MECHANICAL_SOUND_ALLSTOP,
//        };

//        bool bDiskThrottle;
//        bool DiskThrottleTime;

//        LPBYTE disk;
//        LPBYTE disk_w;

//        INT irq_counter, irq_latch; // $4020-$4021
//        BYTE irq_enable, irq_repeat;    // $4022
//        BYTE irq_occur;     // IRQ発生時に0以外になる
//        BYTE irq_transfer;      // 割り込み転送フラグ

//        BYTE disk_enable;       // Disk I/O enable
//        BYTE sound_enable;      // Sound I/O enable
//        BYTE RW_start;      // 読み書き可能になったらIRQ発生
//        BYTE RW_mode;       // 読み書きモード
//        BYTE disk_motor_mode;   // ディスクモーター
//        BYTE disk_eject;        // ディスクカードの挿入/非挿入
//        BYTE drive_ready;       // 読み書き中かどうか
//        BYTE drive_reset;       // ドライブリセット状態

//        INT block_point;
//        INT block_mode;
//        INT size_file_data;
//        INT file_amount;
//        INT point;
//        BYTE first_access;

//        BYTE disk_side;
//        BYTE disk_mount_count;

//        BYTE irq_type;

//        // For mechanical sound
//        BYTE sound_startup_flag;
//        INT sound_startup_timer;
//        INT sound_seekend_timer;
//        public Mapper020(NES parent) : base(parent)
//        {


//        }

//        public override bool IsStateSave()
//        {
//            return true;
//        }

//        override ma
//    }
//}
