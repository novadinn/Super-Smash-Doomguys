using UnityEngine;

[System.Serializable]
public class Config {
    public enum ControlType {
		KEYBOARD,
		JOYSTICK
	}

    public ControlType control_type;

    public KeyCode jumpKey() {
        return control_type == ControlType.KEYBOARD ? KeyCode.Space : KeyCode.Joystick1Button4;
    }

    public KeyCode attackKey() {
        return control_type == ControlType.KEYBOARD ? KeyCode.Mouse0 : KeyCode.Joystick1Button5;
    }

    public KeyCode swapKey() {
        return control_type == ControlType.KEYBOARD ? KeyCode.C : KeyCode.Joystick1Button0;
    }

    public KeyCode destroyKey() {
        return control_type == ControlType.KEYBOARD ? KeyCode.V : KeyCode.Joystick1Button2;
    }

    public Vector2 getDirection(in Camera camera, Vector3 player_position) {
        return control_type == ControlType.KEYBOARD ? 
            (Vector2)(camera.ScreenToWorldPoint(Input.mousePosition) - player_position) : 
            new Vector2(Input.GetAxisRaw("HorizontalJR"), -Input.GetAxisRaw("VerticalJR"));
    }

    public string getHorizontalAxis() {
        return control_type == ControlType.KEYBOARD ? "HorizontalK" : "HorizontalJ";
    }

    public string getVerticalAxis() {
        return control_type == ControlType.KEYBOARD ? "VerticalK" : "VerticalJ" ;
    }
}
