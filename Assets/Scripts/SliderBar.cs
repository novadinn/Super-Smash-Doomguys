using UnityEngine;

public class SliderBar : MonoBehaviour {

    [SerializeField] float bar_size_ = 3.25f;
    [SerializeField] GameObject fill_object_;

    public void setFill(float value, float max_size) {
        float t = Mathf.InverseLerp(0, max_size, value);
        float x_scale = Mathf.Lerp(0, bar_size_, t);
        fill_object_.transform.localScale = new Vector3(x_scale,fill_object_.transform.localScale.y,fill_object_.transform.localScale.z);
    }
}
