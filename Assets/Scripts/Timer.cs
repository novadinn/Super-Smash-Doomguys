public class Timer {
    float expiration_time_;
    float current_time_;

    public Timer(float expiration_time, bool start_expired=false) {
        expiration_time_ = expiration_time;
        current_time_ = start_expired ? expiration_time : 0.0f;
    }

    public void reset() { current_time_ = 0.0f; }
    public bool active() { return current_time_ < expiration_time_; }
    public void deactivate(float multiplier=1) { current_time_ = expiration_time_*multiplier; }

    public float current_time() { return current_time_; }

    public void update(float elapsed_time) {
        if(active()) {
            current_time_ += elapsed_time;
        }
    }
}
