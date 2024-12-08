using System;
using System.IO;
using UnityEngine;
using VirtualNes.Core;

namespace AxibugEmuOnline.Client
{
    public class NesControllerMapper
    {
        private static readonly string ConfigFilePath = $"{CorePath.DataPath}/NES/ControllerMapper.json";

        public MapperSetter Player1 = new MapperSetter();
        public MapperSetter Player2 = new MapperSetter();
        public MapperSetter Player3 = new MapperSetter();
        public MapperSetter Player4 = new MapperSetter();

        public NesControllerMapper()
        {
            Player1.UP.keyCode = KeyCode.JoystickButton8;
            Player1.DOWN.keyCode = KeyCode.JoystickButton10;
            Player1.LEFT.keyCode = KeyCode.JoystickButton11;
            Player1.RIGHT.keyCode = KeyCode.JoystickButton9;
            Player1.B.keyCode = KeyCode.JoystickButton0;
            Player1.A.keyCode = KeyCode.JoystickButton1;
            Player1.SELECT.keyCode = KeyCode.JoystickButton6;
            Player1.START.keyCode = KeyCode.JoystickButton7;

			//dictKeyCfgs.Add(KeyCode.JoystickButton7, MotionKey.P1_GAMESTART);
			//dictKeyCfgs.Add(KeyCode.JoystickButton6, MotionKey.P1_INSERT_COIN);
			//dictKeyCfgs.Add(KeyCode.JoystickButton8, MotionKey.P1_UP);
			//dictKeyCfgs.Add(KeyCode.JoystickButton10, MotionKey.P1_DOWN);
			//dictKeyCfgs.Add(KeyCode.JoystickButton11, MotionKey.P1_LEFT);
			//dictKeyCfgs.Add(KeyCode.JoystickButton9, MotionKey.P1_RIGHT);
			//dictKeyCfgs.Add(KeyCode.JoystickButton2, MotionKey.P1_BTN_1);
			//dictKeyCfgs.Add(KeyCode.JoystickButton0, MotionKey.P1_BTN_2);
			//dictKeyCfgs.Add(KeyCode.JoystickButton1, MotionKey.P1_BTN_3);
			//dictKeyCfgs.Add(KeyCode.JoystickButton3, MotionKey.P1_BTN_4);
		}

        public void Save()
        {
            var jsonStr = JsonUtility.ToJson(this);
            File.WriteAllText(ConfigFilePath, jsonStr);
        }

        public ControllerState CreateState()
        {
            var state1 = Player1.GetButtons();
            var state2 = Player2.GetButtons();
            var state3 = Player3.GetButtons();
            var state4 = Player4.GetButtons();

            return new ControllerState(state1, state2, state3, state4);
        }

        private static NesControllerMapper s_setting;
        public static NesControllerMapper Get()
        {
            if (s_setting == null)
            {
                try
                {
                    var json = File.ReadAllText($"{CorePath.DataPath}/Nes/ControllerMapper.json");
                    s_setting = JsonUtility.FromJson<NesControllerMapper>(json);
                }
                catch
                {
                    s_setting = new NesControllerMapper();
                }
            }

            return s_setting;
        }

        [Serializable]
        public class Mapper
        {
            public EnumButtonType buttonType;
            public KeyCode keyCode;

            public Mapper(EnumButtonType buttonType)
            {
                this.buttonType = buttonType;
            }
        }

        [Serializable]
        public class MapperSetter
        {
            public Mapper UP = new Mapper(EnumButtonType.UP);
            public Mapper DOWN = new Mapper(EnumButtonType.DOWN);
            public Mapper LEFT = new Mapper(EnumButtonType.LEFT);
            public Mapper RIGHT = new Mapper(EnumButtonType.RIGHT);
            public Mapper A = new Mapper(EnumButtonType.A);
            public Mapper B = new Mapper(EnumButtonType.B);
            public Mapper SELECT = new Mapper(EnumButtonType.SELECT);
            public Mapper START = new Mapper(EnumButtonType.START);
            public Mapper MIC = new Mapper(EnumButtonType.MIC);

            public EnumButtonType GetButtons()
            {
                EnumButtonType res = 0;

                if (Input.GetKey(UP.keyCode))
                    res |= EnumButtonType.UP;

                if (Input.GetKey(DOWN.keyCode))
                    res |= EnumButtonType.DOWN;

                if (Input.GetKey(LEFT.keyCode))
                    res |= EnumButtonType.LEFT;

                if (Input.GetKey(RIGHT.keyCode))
                    res |= EnumButtonType.RIGHT;

                if (Input.GetKey(A.keyCode))
                    res |= EnumButtonType.A;

                if (Input.GetKey(B.keyCode))
                    res |= EnumButtonType.B;

                if (Input.GetKey(SELECT.keyCode))
                    res |= EnumButtonType.SELECT;

                if (Input.GetKey(START.keyCode))
                    res |= EnumButtonType.START;

                if (Input.GetKey(MIC.keyCode))
                    res |= EnumButtonType.MIC;

                return res;
            }
        }
    }
}
