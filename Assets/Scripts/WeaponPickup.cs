using UnityEngine;

public class WeaponPickup : Pickup {

    const float kFlightAmount = 0.25f;

    [SerializeField] Weapon weapon_;

    Vector3 start_position_;
    float flight_position_y_ = 0.0f;

    void Start() {
        start_position_ = transform.position;
    }
    
    protected override void Update() {
        base.Update();

        flight_position_y_ += Time.deltaTime;
        transform.position = start_position_ + new Vector3(0, Mathf.Sin(flight_position_y_) * 0.25f, 0);
    }

    protected override void reactToCollision(RaycastHit2D hit) {
        Player player = hit.collider.GetComponent<Player>();
        int reaction = player.pickWeaponPickup(weapon_);
        if(reaction == -1) {
            Destroy(gameObject);
        } else if(reaction == 1) {
            weapon_ = Instantiate(weapon_, transform.position, Quaternion.identity);
            player.pickWeapon(weapon_);
            Destroy(gameObject);
        }
    }
}
