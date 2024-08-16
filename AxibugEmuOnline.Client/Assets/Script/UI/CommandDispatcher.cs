using System;
using System.Collections.Generic;
using UnityEngine;

namespace AxibugEmuOnline.Client
{
    public class CommandDispatcher : MonoBehaviour
    {
        public static CommandDispatcher Instance { get; private set; }

        HashSet<MenuItemController> m_register = new HashSet<MenuItemController>();
        Dictionary<KeyCode, MenuItemController.EnumCommand> m_keyMapper = new Dictionary<KeyCode, MenuItemController.EnumCommand>();

        private void Awake()
        {
            Instance = this;

            m_keyMapper.Add(KeyCode.A, MenuItemController.EnumCommand.SelectItemLeft);
            m_keyMapper.Add(KeyCode.D, MenuItemController.EnumCommand.SelectItemRight);
            m_keyMapper.Add(KeyCode.W, MenuItemController.EnumCommand.SelectItemUp);
            m_keyMapper.Add(KeyCode.S, MenuItemController.EnumCommand.SelectItemDown);
            m_keyMapper.Add(KeyCode.K, MenuItemController.EnumCommand.Enter);
            m_keyMapper.Add(KeyCode.L, MenuItemController.EnumCommand.Back);
            m_keyMapper.Add(KeyCode.I, MenuItemController.EnumCommand.OptionMenu);

            m_keyMapper.Add(KeyCode.LeftArrow, MenuItemController.EnumCommand.SelectItemLeft);
            m_keyMapper.Add(KeyCode.RightArrow, MenuItemController.EnumCommand.SelectItemRight);
            m_keyMapper.Add(KeyCode.UpArrow, MenuItemController.EnumCommand.SelectItemUp);
            m_keyMapper.Add(KeyCode.DownArrow, MenuItemController.EnumCommand.SelectItemDown);
            m_keyMapper.Add(KeyCode.Return, MenuItemController.EnumCommand.Enter);
            m_keyMapper.Add(KeyCode.Escape, MenuItemController.EnumCommand.Back);
            m_keyMapper.Add(KeyCode.RightShift, MenuItemController.EnumCommand.OptionMenu);
            m_keyMapper.Add(KeyCode.LeftShift, MenuItemController.EnumCommand.OptionMenu);
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public void RegistController(MenuItemController controller)
        {
            m_register.Add(controller);
        }

        public void UnRegistController(MenuItemController menuItemController)
        {
            m_register.Remove(menuItemController);
        }

        private void Update()
        {
            foreach (var item in m_keyMapper)
            {
                if (Input.GetKeyDown(item.Key))
                {
                    foreach (var controller in m_register)
                        controller.ExecuteCommand(item.Value, false);
                }
                if (Input.GetKeyUp(item.Key))
                {
                    foreach (var controller in m_register)
                        controller.ExecuteCommand(item.Value, true);
                }
            }
        }
    }
}
