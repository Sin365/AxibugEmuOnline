using System;
using UnityEngine;
using VirtualNes.Core;

namespace AxibugEmuOnline.Client
{
    public class NesControllerMapper : ScriptableObject
    {
        public MapperSetter Player1 = new MapperSetter();
        public MapperSetter Player2 = new MapperSetter();
        public MapperSetter Player3 = new MapperSetter();
        public MapperSetter Player4 = new MapperSetter();

        public ControllerState CreateState()
        {
            var state1 = Player1.GetButtons();
            var state2 = Player2.GetButtons();
            var state3 = Player3.GetButtons();
            var state4 = Player4.GetButtons();

            return new ControllerState(state1, state2, state3, state4);
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
