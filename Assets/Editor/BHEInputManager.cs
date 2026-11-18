using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class BHEInputManager
{
    VisualElement root;
    VisualElement clickedElement;
    BulletHellEditor bulletHellEditor;

    public Dictionary<string, bool> keysPressed;
    bool initialized;
    public BHEInputManager(VisualElement _root, BulletHellEditor _bulletHellEditor)
    {
        Debug.Log("Input Manager Constructor");

        root = _root;
        bulletHellEditor = _bulletHellEditor;

        root.focusable = true;
        root.Focus();

        keysPressed = new();

        keysPressed.Add("Up", false);
        keysPressed.Add("Down", false);
        keysPressed.Add("Left", false);
        keysPressed.Add("Right", false);
        keysPressed.Add("Focus", false);

        _root.RegisterCallback<KeyDownEvent>(KeyDownEvent, TrickleDown.NoTrickleDown);

        _root.RegisterCallback<KeyUpEvent>(KeyUpEvent, TrickleDown.NoTrickleDown);

        _root.RegisterCallback<PointerDownEvent>(MouseDownEvent, TrickleDown.NoTrickleDown);

        initialized = true;
    }

    private void MouseDownEvent(PointerDownEvent evt)
    {
        clickedElement = root.panel.Pick(evt.position);

        if(clickedElement is PhaseRow row)
        {
            bulletHellEditor.TrySelectPhaseRow(row);
        }
        else
        {
            bulletHellEditor.DeselectPhaseRow();
        }
    }

    private void KeyDownEvent(KeyDownEvent evt)
    {
        if ((evt.keyCode == KeyCode.UpArrow) || (evt.keyCode == KeyCode.W))
        {
            keysPressed["Up"] = true;
            bulletHellEditor.TryMovePhaseRow(-1);
        }

        if ((evt.keyCode == KeyCode.DownArrow) || (evt.keyCode == KeyCode.S))
        {
            keysPressed["Down"] = true;
            bulletHellEditor.TryMovePhaseRow(1);
        }

        if ((evt.keyCode == KeyCode.LeftArrow) || (evt.keyCode == KeyCode.A))
        {
            keysPressed["Left"] = true;
        }

        if ((evt.keyCode == KeyCode.RightArrow) || (evt.keyCode == KeyCode.D))
        {
            keysPressed["Right"] = true;
        }

        if ((evt.keyCode == KeyCode.X))
        {
            keysPressed["Focus"] = true;
        }
    }

    private void KeyUpEvent(KeyUpEvent evt)
    {
        if ((evt.keyCode == KeyCode.UpArrow) || (evt.keyCode == KeyCode.W))
        {
            keysPressed["Up"] = false;
        }

        if ((evt.keyCode == KeyCode.DownArrow) || (evt.keyCode == KeyCode.S))
        {
            keysPressed["Down"] = false;
        }

        if ((evt.keyCode == KeyCode.LeftArrow) || (evt.keyCode == KeyCode.A))
        {
            keysPressed["Left"] = false;
        }

        if ((evt.keyCode == KeyCode.RightArrow) || (evt.keyCode == KeyCode.D))
        {
            keysPressed["Right"] = false;
        }

        if ((evt.keyCode == KeyCode.X))
        {
            keysPressed["Focus"] = false;
        }
    }

    public void ResetInputs()
    {
        //Its like this cause keysPressed is appearently being modified so you cant loop over the dictionary
        if (initialized)
        {
            foreach (string key in keysPressed.Keys.ToArray())
            {
                keysPressed[key] = false;
            }
        }
    }
}
