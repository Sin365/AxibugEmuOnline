using System;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using AxibugEmuOnline.Client.UNES.Controller;
using AxibugEmuOnline.Client.UNES.Input;
using AxibugEmuOnline.Client.UNES.Renderer;
using AxibugEmuOnline.Client.ClientCore;
using AxibugEmuOnline.Client.Sample;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace AxibugEmuOnline.Client.UNES
{
    public class UNESBehaviour : MonoBehaviour
    {
        [Header("Render")]
        public RenderTexture RenderTexture;
        public const int GameWidth = 256;
        public const int GameHeight = 240;
        public FilterMode FilterMode = FilterMode.Point;
        public bool LogicThread = true;

        [Header("Input")] 
        public KeyConfig KeyConfig;

        public BaseInput Input { get; set; }
        public IRenderer Renderer { get; set; }
        public uint[] RawBitmap { get; set; } = new uint[GameWidth * GameHeight];
        public bool Ready { get; set; }
        public bool GameStarted { get; set; }

        private bool _rendererRunning = true;
        private IController _controller;
        private Emulator _emu;
        private bool _suspended;
        private Thread _renderThread;
        private int _activeSpeed = 1;

        public void Boot(byte[] romData)
        {
            InitInput();
            InitRenderer();
            BootCartridge(romData);
        }

        public void Boot_Obs()
        {
            InitInput();
            InitRenderer();
            BootCartridge_Obs();
        }

        public void LoadSaveData(byte[] saveData)
        {
            _emu?.Mapper.LoadSaveData(saveData);
        }

        public byte[] GetSaveData()
        {
            return _emu?.Mapper.GetSaveData();
        }

        Queue<byte[]> _queue = new Queue<byte[]>();


        private void BootCartridge(byte[] romData)
        {
            _emu = new Emulator(romData, _controller);
            if (LogicThread)
            {
                _renderThread = new Thread(() =>
                {
                    GameStarted = true;
                    var s = new Stopwatch();
                    var s0 = new Stopwatch();
                    while (_rendererRunning)
                    {
                        if (_suspended)
                        {
                            Thread.Sleep(100);
                            continue;
                        }

                        s.Restart();
                        for (var i = 0; i < 60 && !_suspended; i++)
                        {
                            s0.Restart();
                            lock (RawBitmap)
                            {
                                _emu.PPU.ProcessFrame();
                                RawBitmap = _emu.PPU.RawBitmap;
                                //塞进发送队列
                                _queue.Enqueue(_emu.PPU.RawBitmap_paletteIdxCache);
                            }

                            s0.Stop();
                            Thread.Sleep(Math.Max((int)(980 / 60.0 - s0.ElapsedMilliseconds), 0) / _activeSpeed);
                        }

                        s.Stop();
                    }
                });

                _renderThread.Start();
            }
            else
            {
                GameStarted = true;
            }
        }

        private void BootCartridge_Obs()
        {
            if (LogicThread)
            {
                _renderThread = new Thread(() =>
                {
                    GameStarted = true;
                    var s = new Stopwatch();
                    var s0 = new Stopwatch();
                    while (_rendererRunning)
                    {
                        if (_suspended)
                        {
                            Thread.Sleep(100);
                            continue;
                        }

                        s.Restart();
                        for (var i = 0; i < 60 && !_suspended; i++)
                        {
                            s0.Restart();
                            lock (RawBitmap)
                            {
                                RawBitmap = AppAxibugEmuOnline.game.RawBitmap;
                            }

                            s0.Stop();
                            Thread.Sleep(Math.Max((int)(980 / 60.0 - s0.ElapsedMilliseconds), 0) / _activeSpeed);
                        }

                        s.Stop();
                    }
                });

                _renderThread.Start();
            }
            else
            {
                GameStarted = true;
            }
        }

        #region Monobehaviour

        public void Awake()
        {
        }

        public void OnEnable()
        {
            
        }

        public void OnDisable()
        {
            _rendererRunning = false;
            Renderer?.End();
        }

        public void Update()
        {
            if (!GameStarted) return;
            UpdateInput();

            if (UNESTest.instance.PlayerP1)
            {
                UpdateRender();
            }
            else
            {
                UpdateRender_Obs();
            }
        }

        #endregion

        #region Input

        private void InitInput()
        {
            _controller = new NesController(this);
            Input = new DefaultInput();
        }

        public void UpdateInput()
        {
            Input.HandlerKeyDown(keyCode =>
            {
                switch (keyCode)
                {
                    case KeyCode.F2:
                        _suspended = false;
                        break;
                    case KeyCode.F3:
                        _suspended = true;
                        break;
                    default:
                        _controller.PressKey(keyCode);
                        break;
                }
            });

            Input.HandlerKeyUp(keyCode =>
            {
                _controller.ReleaseKey(keyCode);
            });
        }

        #endregion

        #region Render

        public void UpdateRender()
        {
            while (_queue.Count() > 0)
            {
                //发送
                AppAxibugEmuOnline.game.SendScreen(_queue.Dequeue());
            }
            if (!_rendererRunning) return;
            if (_suspended) return;

            if (!LogicThread)
            {
                _emu.PPU.ProcessFrame();
                RawBitmap = _emu.PPU.RawBitmap;
                Renderer.HandleRender();

            }
            else
            {
                lock (RawBitmap)
                {
                    Renderer.HandleRender();
                }
            }
        }

        public void UpdateRender_Obs()
        {
            if (!_rendererRunning) return;
            if (_suspended) return;

            if (!LogicThread)
            {
                RawBitmap = AppAxibugEmuOnline.game.RawBitmap;
                Renderer.HandleRender();
            }
            else
            {
                lock (RawBitmap)
                {
                    Renderer.HandleRender();
                }
            }
        }

        private void InitRenderer()
        {
            Renderer?.End();

            Renderer = new UnityRenderer();
            Renderer.Init(this);
        } 

        #endregion
    }
}
